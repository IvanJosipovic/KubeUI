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

    /// <summary>Gets the optional served API version handled by this config.</summary>
    public virtual string? ApiVersion { get; }

    internal bool IsFallback => ApiGroup is null && KindName is null && ApiVersion is null;

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

        var storageVersion = spec.Versions?.FirstOrDefault(static version => version.Storage && version.Served)?.Name;
        return string.Equals(ApiVersion, storageVersion, StringComparison.Ordinal);
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
        if (matching.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one CustomResourceConfig matches CRD '{crd.Name()}'.");
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

    public override bool IsNamespaced => _isNamespaced;

    public override GroupApiVersionKind Kind => _kind;

    public override string Name => _resourceName ?? base.Name;

    /// <inheritdoc />
    public override ResourceOrigin Origin => ResourceOrigin.CustomResourceDefinition;

    /// <summary>Gets the CRD that defines this resource kind.</summary>
    public V1CustomResourceDefinition Definition { get; private set; } = new();

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
    public void Configure(V1CustomResourceDefinition crd)
    {
        Definition = crd;
        _columns.Clear();
        if (!crd.TryGetResourceKind(out _kind) || crd.Spec is not { } spec)
        {
            throw new InvalidOperationException("CRD has no served storage version.");
        }

        var version = spec.Versions.First(candidate => candidate.Served && candidate.Storage);
        _resourceName = spec.Names!.Kind.Humanize(LetterCasing.Title).Pluralize();
        _groups = NavigationTreeEditor.ConstructCustomResourceGroups(spec.Group);

        if (spec.Scope == "Namespaced")
        {
            _isNamespaced = true;
        }
        else
        {
            _isNamespaced = false;
        }

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

        if (!ReplaceAdditionalPrinterColumns)
        {
            _columns.AddRange(crdPrinterColumns);
        }

        _columns.AddRange(CreateAdditionalColumns(crd));

        _columns.Add(AgeColumn());
    }

    /// <summary>Gets whether CRD printer columns should be omitted for this kind.</summary>
    protected virtual bool ReplaceAdditionalPrinterColumns => false;

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
        return _columns;
    }

    public override Task EvaluateListWatchAccessAsync()
    {
        PermissionsLoaded = false;
        CanListAndWatch = false;
        CanListAndWatch = Cluster.Runtime.Permissions.CanIAnyNamespace(
            Kind,
            IsNamespaced,
            Verb.List)
            && Cluster.Runtime.Permissions.CanIAnyNamespace(
                Kind,
                IsNamespaced,
                Verb.Watch);
        PermissionsLoaded = true;
        return Task.CompletedTask;
    }

    private static string CreateColumnKey(string name)
    {
        return Regex.Replace(name.Trim().ToLowerInvariant(), @"\W+", "-").Trim('-');
    }
}
