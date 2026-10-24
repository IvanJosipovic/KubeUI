using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Humanizer;
using JsonPathLINQ;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Resources;

/// <summary>Configures a dynamically discovered Kubernetes resource from its CRD definition.</summary>
public class CustomResourceConfig : ResourceConfig<GenericKubernetesObject>
{
    private readonly Lock _configureLock = new();
    private bool _isNamespaced = true;
    private string? _resourceName;
    private GroupApiVersionKind _kind;
    private IReadOnlyList<NavigationGroup> _groups = [];

    /// <summary>Creates a kind-specific custom resource config with identity supplied by overrides.</summary>
    protected CustomResourceConfig()
        : base()
    {
    }

    /// <summary>Creates a dynamic resource configuration.</summary>
    /// <param name="serviceProvider">Application services used by shared resource actions.</param>
    /// <param name="apiGroup">CRD API group; null together with kind and version means generic fallback.</param>
    /// <param name="kindName">CRD kind handled by this config.</param>
    /// <param name="apiVersion">Optional storage API version handled by this config.</param>
    public CustomResourceConfig(
        IServiceProvider serviceProvider,
        string? apiGroup = null,
        string? kindName = null,
        string? apiVersion = null)
        : base(serviceProvider)
    {
        if ((apiGroup is null) != (kindName is null)
            || (apiVersion is not null && apiGroup is null))
        {
            throw new ArgumentException("Custom resource identity requires both API group and kind; version also requires that identity.");
        }

        ApiGroup = apiGroup;
        KindName = kindName;
        ApiVersion = apiVersion;
    }

    /// <summary>Gets the CRD API group handled by this config, or null for the generic fallback.</summary>
    public virtual string? ApiGroup { get; }

    /// <summary>Gets the CRD kind handled by this config, or null for the generic fallback.</summary>
    public virtual string? KindName { get; }

    /// <summary>Gets the optional served API version handled by this config; null matches any served version.</summary>
    public virtual string? ApiVersion { get; }

    internal bool IsFallback => ApiGroup is null && KindName is null && ApiVersion is null;

    /// <summary>Gets whether the CRD serves the configured API version, if one was specified.</summary>
    internal bool Matches(V1CustomResourceDefinition crd)
    {
        var spec = crd.Spec;
        if (IsFallback || spec is null
            || !string.Equals(ApiGroup, spec.Group, StringComparison.Ordinal)
            || !string.Equals(KindName, spec.Names?.Kind, StringComparison.Ordinal))
        {
            return false;
        }

        if (ApiVersion is null)
        {
            return true;
        }

        return spec.Versions?.Any(version => version.Served
            && string.Equals(version.Name, ApiVersion, StringComparison.Ordinal)) == true;
    }

    internal static CustomResourceConfig Select(
        V1CustomResourceDefinition crd,
        IEnumerable<CustomResourceConfig> configurations)
    {
        ArgumentNullException.ThrowIfNull(crd);
        ArgumentNullException.ThrowIfNull(configurations);

        var available = configurations
            .DistinctBy(static configuration => (
                configuration.GetType(),
                configuration.ApiGroup,
                configuration.KindName,
                configuration.ApiVersion))
            .ToArray();
        var matching = available
            .Where(configuration => configuration.Matches(crd))
            .ToArray();

        // If multiple configs match, prioritize:
        // 1. Exact storage version match (ApiVersion specified and matches storage version)
        // 2. Exact served version match (ApiVersion specified and matches any served version)
        // 3. Version-agnostic config (ApiVersion == null)
        // 4. Fallback
        if (matching.Length > 1)
        {
            var spec = crd.Spec;
            var storageVersion = spec?.Versions?.FirstOrDefault(v => v.Storage && v.Served)?.Name;

            var prioritized = matching
                .Select(config => (config, priority: GetConfigPriority(config, storageVersion)))
                .OrderBy(x => x.priority)
                .Select(x => x.config)
                .ToArray();

            // If top priority is unique, use it
            var topPriority = prioritized.First();
            var topPriorityValue = GetConfigPriority(topPriority, storageVersion);
            var samePriorityCount = prioritized.Count(c => GetConfigPriority(c, storageVersion) == topPriorityValue);

            if (samePriorityCount == 1)
            {
                return topPriority;
            }

            // If still ambiguous at top priority, throw with details
            var ambiguous = prioritized.Where(c => GetConfigPriority(c, storageVersion) == topPriorityValue).ToArray();
            throw new InvalidOperationException(
                $"More than one CustomResourceConfig matches CRD '{crd.Name()}' with equal priority: {string.Join(", ", ambiguous.Select(c => c.GetType().Name + (c.ApiVersion != null ? $" (ApiVersion={c.ApiVersion})" : " (version-agnostic)")))}.");
        }

        if (matching.Length == 1)
        {
            return matching[0];
        }

        var fallback = available.Where(static configuration => configuration.IsFallback).ToArray();
        return fallback.Length switch
        {
            1 => fallback[0],
            0 => throw new InvalidOperationException("No generic CustomResourceConfig fallback is registered."),
            _ => throw new InvalidOperationException("More than one generic CustomResourceConfig fallback is registered."),
        };
    }

    private static int GetConfigPriority(CustomResourceConfig config, string? storageVersion)
    {
        // Priority 0: Exact storage version match
        if (config.ApiVersion != null && storageVersion != null && string.Equals(config.ApiVersion, storageVersion, StringComparison.Ordinal))
        {
            return 0;
        }
        // Priority 1: Exact served version match (non-storage)
        if (config.ApiVersion != null)
        {
            return 1;
        }
        // Priority 2: Version-agnostic
        return 2;
    }

    public override bool IsNamespaced => _isNamespaced;

    public override GroupApiVersionKind Kind => _kind;

    public override string Name => _resourceName ?? base.Name;

    /// <inheritdoc />
    public override ResourceOrigin Origin => ResourceOrigin.CustomResourceDefinition;

    /// <summary>Gets the CRD that defines this resource kind, or null before it is configured.</summary>
    public V1CustomResourceDefinition? Definition { get; private set; }

    /// <inheritdoc />
    public override V1CustomResourceDefinition? CustomResourceDefinition => Definition;

    /// <inheritdoc />
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.CustomResourcesId,
        _groups);

    /// <inheritdoc />
    public override Task SeedResource(bool waitForReady, CancellationToken cancellationToken)
        => Cluster.Runtime.SeedResource(Kind, waitForReady, cancellationToken);

    /// <inheritdoc />
    protected override void RegisterResourceModel(ClusterWorkspace cluster)
    {
        // CRD models are registered from their live definitions, not as statically typed resources.
    }

    /// <summary>Gets this kind's optional YAML starter template; configured API identity is applied after parsing.</summary>
    protected virtual string? Template => null;

    /// <inheritdoc />
    protected override GenericKubernetesObject CreateInitialResourceTemplate()
        => string.IsNullOrWhiteSpace(Template)
            ? new GenericKubernetesObject()
            : KubernetesYaml.Deserialize<GenericKubernetesObject>(Template);

    private readonly List<IResourceListColumn> _columns = [];

    /// <summary>Configures this instance from a current CRD definition.</summary>
    /// <param name="crd">The valid CRD defining the dynamic resource.</param>
    /// <exception cref="InvalidOperationException">
    /// The definition does not expose a served storage version.
    /// </exception>
    public void Configure(V1CustomResourceDefinition crd)
    {
        ArgumentNullException.ThrowIfNull(crd);

        lock (_configureLock)
        {
            _columns.Clear();
            if (!crd.TryGetResourceKind(out _kind) || crd.Spec is not { } spec)
            {
                throw new InvalidOperationException("CRD has no served storage version.");
            }

            // A definition whose storage version is not served cannot be listed or watched. The
            // caller treats that as a temporary condition, so the failure must be explicit
            // instead of an unhandled sequence access.
            var version = spec.Versions?.FirstOrDefault(candidate => candidate.Served && candidate.Storage)
                ?? throw new InvalidOperationException("CRD has no served storage version.");

            Definition = crd;
            _resourceName = spec.Names!.Kind.Humanize(LetterCasing.Title).Pluralize();
            _groups = NavigationTreeEditor.ConstructCustomResourceGroups(spec.Group);

            _isNamespaced = spec.Scope == "Namespaced";

            _columns.Add(NameColumn(SortDirection.Ascending));
            if (IsNamespaced)
            {
                _columns.Add(NamespaceColumn());
            }

            List<IResourceListColumn> crdPrinterColumns = [];

            if (version.AdditionalPrinterColumns != null)
            {
                foreach (var item in version.AdditionalPrinterColumns)
                {
                    if (string.Equals(item.JsonPath, ".metadata.creationTimestamp", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    try
                    {
                        switch (item.Type, item.Format)
                        {
                            case ("integer", "int64"):
                                crdPrinterColumns.Add(CreateColumn(item.Name, item.JsonPath, ConvertInt64));
                                break;
                            case ("integer", _):
                                crdPrinterColumns.Add(CreateColumn(item.Name, item.JsonPath, ConvertInt32));
                                break;
                            case ("number", _):
                                crdPrinterColumns.Add(CreateColumn(item.Name, item.JsonPath, ConvertDouble));
                                break;
                            case ("boolean", _):
                                crdPrinterColumns.Add(CreateColumn(item.Name, item.JsonPath, ConvertBoolean));
                                break;
                            case ("date", _):
                                crdPrinterColumns.Add(CreateDateColumn(item.Name, item.JsonPath));
                                break;
                            default:
                                crdPrinterColumns.Add(CreateStringColumn(item.Name, item.JsonPath));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to generate generic resource column: {Name} in {Resource}", item.Name, crd.Name());
                    }
                }
            }

            if (IncludeAdditionalPrinterColumns)
            {
                _columns.AddRange(crdPrinterColumns);
            }

            _columns.AddRange(CreateAdditionalColumns(crd));

            _columns.Add(AgeColumn());
        }
    }

    /// <summary>Gets whether CRD printer columns should be included for this kind.</summary>
    protected virtual bool IncludeAdditionalPrinterColumns => true;

    /// <summary>Creates columns specific to this CRD kind.</summary>
    /// <param name="crd">CRD being configured.</param>
    protected virtual IEnumerable<IResourceListColumn> CreateAdditionalColumns(V1CustomResourceDefinition crd) => [];

    private static ResourceListColumn<GenericKubernetesObject, TValue?> CreateColumn<TValue>(
        string name,
        string jsonPath,
        Func<object?, TValue?> convert)
        where TValue : struct
    {
        var getter = JsonPath.GetExpression<GenericKubernetesObject, object?>(jsonPath, addNullChecks: true).Compile();
        return new ResourceListColumn<GenericKubernetesObject, TValue?>
        {
            Key = CreateColumnKey(name),
            Name = name,
            Field = resource => convert(getter(resource))
        };
    }

    private static long? ConvertInt64(object? value)
        => long.TryParse(GetValueText(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out var converted)
            ? converted
            : null;

    private static int? ConvertInt32(object? value)
        => int.TryParse(GetValueText(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out var converted)
            ? converted
            : null;

    private static double? ConvertDouble(object? value)
        => double.TryParse(GetValueText(value), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var converted)
            ? converted
            : null;

    private static bool? ConvertBoolean(object? value)
        => bool.TryParse(GetValueText(value), out var converted) ? converted : null;

    private static string? GetValueText(object? value)
        => value switch
        {
            null => null,
            JsonElement json => json.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };

    private static ResourceListColumn<GenericKubernetesObject, string> CreateStringColumn(
        string name,
        string jsonPath)
    {
        var getter = JsonPath.GetExpression<GenericKubernetesObject, object?>(jsonPath, addNullChecks: true).Compile();
        return new ResourceListColumn<GenericKubernetesObject, string>
        {
            Key = CreateColumnKey(name),
            Name = name,
            Field = resource =>
            {
                try
                {
                    return getter(resource)?.ToString() ?? string.Empty;
                }
                catch (KeyNotFoundException)
                {
                    return string.Empty;
                }
            }
        };
    }

    private static ResourceListColumn<GenericKubernetesObject, DateTime?> CreateDateColumn(string name, string jsonPath)
    {
        var getter = JsonPath.GetExpression<GenericKubernetesObject, string?>(jsonPath, addNullChecks: true).Compile();
        return new ResourceListColumn<GenericKubernetesObject, DateTime?>
        {
            Key = CreateColumnKey(name),
            Name = name,
            Field = resource => DateTime.TryParse(getter(resource), out var value) ? value : null
        };
    }

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return _columns.ToArray();
    }

    private static string CreateColumnKey(string name)
    {
        return Regex.Replace(name.Trim().ToLowerInvariant(), @"\W+", "-").Trim('-');
    }
}
