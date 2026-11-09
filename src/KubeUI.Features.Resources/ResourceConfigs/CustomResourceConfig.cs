using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Humanizer;
using JsonPathLINQ;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Features.Resources;

/// <summary>Configures a dynamically discovered Kubernetes resource from its CRD definition.</summary>
public class CustomResourceConfig : ResourceConfig<GenericKubernetesObject>, IClusterCustomResourceConfig, IClusterCustomResourcePrinterColumnConsumer
{
    private readonly record struct PrinterColumnSpec(string Name, string JsonPath, string Type, string? Format);

    private readonly Lock _configureLock = new();
    private bool _isNamespaced = true;
    private string? _resourceName;
    private GroupApiVersionKind _kind;
    private IReadOnlyList<NavigationGroup> _groups = [];
    private IReadOnlyList<IResourceListColumn> _discoveryAdditionalColumns = [];
    private IReadOnlyList<IResourceListColumn> _discoveryPrinterColumns = [];
    private IReadOnlyList<IResourceListColumn> _discoveryDefinitionColumns = [];
    private IReadOnlyList<PrinterColumnSpec> _discoveryPrinterColumnSpecs = [];
    private string? _discoveryPrinterColumnDefinitionName;
    private long? _discoveryPrinterColumnDefinitionGeneration;
    private ResourceOrigin _origin = ResourceOrigin.CustomResourceDefinition;

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

    internal bool Matches(ApiDiscoveryResource resource)
        => !IsFallback
            && string.Equals(ApiGroup, resource.Kind.Group, StringComparison.Ordinal)
            && string.Equals(KindName, resource.Kind.Kind, StringComparison.Ordinal)
            && (ApiVersion is null || string.Equals(ApiVersion, resource.Kind.ApiVersion, StringComparison.Ordinal));

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

    internal static CustomResourceConfig Select(
        ApiDiscoveryResource resource,
        IEnumerable<CustomResourceConfig> configurations)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(configurations);

        var available = configurations
            .DistinctBy(static configuration => (
                configuration.GetType(),
                configuration.ApiGroup,
                configuration.KindName,
                configuration.ApiVersion))
            .ToArray();
        var matching = available.Where(configuration => configuration.Matches(resource)).ToArray();
        if (matching.Length == 0)
        {
            return GetFallback(available);
        }

        var preferred = matching
            .OrderBy(configuration => configuration.ApiVersion is null ? 1 : 0)
            .ToArray();
        var bestPriority = preferred[0].ApiVersion is null ? 1 : 0;
        var bestMatches = preferred.Where(configuration => (configuration.ApiVersion is null ? 1 : 0) == bestPriority).ToArray();
        return bestMatches.Length switch
        {
            1 => bestMatches[0],
            _ => throw new InvalidOperationException(
                $"More than one CustomResourceConfig matches discovered resource '{resource.Kind}' with equal priority: {string.Join(", ", bestMatches.Select(configuration => configuration.GetType().Name))}.")
        };
    }

    private static CustomResourceConfig GetFallback(IEnumerable<CustomResourceConfig> configurations)
    {
        var fallback = configurations.Where(static configuration => configuration.IsFallback).ToArray();
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
    public override ResourceOrigin Origin => _origin;

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
            : ServiceProvider.GetRequiredService<IKubernetesYamlSerializer>().Deserialize<GenericKubernetesObject>(Template);

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
            _origin = ResourceOrigin.CustomResourceDefinition;
            _discoveryAdditionalColumns = [];
            _discoveryPrinterColumns = [];
            _discoveryDefinitionColumns = [];
            _discoveryPrinterColumnSpecs = [];
            _discoveryPrinterColumnDefinitionName = null;
            _discoveryPrinterColumnDefinitionGeneration = null;
            _resourceName = spec.Names!.Kind.Humanize(LetterCasing.Title).Pluralize();
            _groups = NavigationGroupPath.CreateCustomResourceGroups(spec.Group);

            _isNamespaced = spec.Scope == "Namespaced";

            _columns.Add(NameColumn(SortDirection.Ascending));
            if (IsNamespaced)
            {
                _columns.Add(NamespaceColumn());
            }

            var crdPrinterColumns = CreatePrinterColumns(crd, version.AdditionalPrinterColumns);

            if (IncludeAdditionalPrinterColumns)
            {
                _columns.AddRange(crdPrinterColumns);
            }

            _columns.AddRange(CreateAdditionalColumns(crd));

            _columns.Add(AgeColumn());
        }
    }

    /// <summary>Configures this instance from a discovered resource endpoint.</summary>
    /// <param name="resource">The resource endpoint from API discovery.</param>
    public void Configure(ApiDiscoveryResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!IsFallback && !Matches(resource))
        {
            throw new InvalidOperationException($"Discovered resource '{resource.Kind}' does not match this configuration.");
        }

        lock (_configureLock)
        {
            Definition = null;
            _kind = resource.Kind;
            _resourceName = _kind.PluralName.Humanize(LetterCasing.Title);
            _groups = NavigationGroupPath.CreateCustomResourceGroups(_kind.Group);
            _isNamespaced = resource.IsNamespaced;
            _origin = ResourceOrigin.ApiDiscovery;
            _discoveryAdditionalColumns = CreateAdditionalColumns(resource).ToArray();
            _discoveryPrinterColumns = [];
            _discoveryDefinitionColumns = [];
            _discoveryPrinterColumnSpecs = [];
            _discoveryPrinterColumnDefinitionName = null;
            _discoveryPrinterColumnDefinitionGeneration = null;
            RebuildDiscoveryColumns();
        }
    }

    /// <inheritdoc />
    public void ConfigurePrinterColumns(V1CustomResourceDefinition? definition)
    {
        lock (_configureLock)
        {
            if (_origin != ResourceOrigin.ApiDiscovery)
            {
                return;
            }

            var matchingDefinition = MatchesDiscoveryKind(definition) ? definition : null;
            var version = matchingDefinition?.Spec?.Versions?.FirstOrDefault(candidate =>
                candidate.Served && string.Equals(candidate.Name, _kind.ApiVersion, StringComparison.Ordinal));
            var specs = version?.AdditionalPrinterColumns?
                .Where(static column => !string.IsNullOrWhiteSpace(column.Name)
                    && !string.IsNullOrWhiteSpace(column.JsonPath)
                    && !string.Equals(column.JsonPath, ".metadata.creationTimestamp", StringComparison.Ordinal))
                .Select(static column => new PrinterColumnSpec(
                    column.Name,
                    column.JsonPath,
                    column.Type ?? "string",
                    column.Format))
                .ToArray() ?? [];
            var definitionName = version is null ? null : matchingDefinition!.Metadata?.Name;
            var definitionGeneration = version is null ? null : matchingDefinition!.Metadata?.Generation;

            if (string.Equals(_discoveryPrinterColumnDefinitionName, definitionName, StringComparison.Ordinal)
                && _discoveryPrinterColumnDefinitionGeneration == definitionGeneration
                && _discoveryPrinterColumnSpecs.SequenceEqual(specs))
            {
                return;
            }

            _discoveryPrinterColumnDefinitionName = definitionName;
            _discoveryPrinterColumnDefinitionGeneration = definitionGeneration;
            _discoveryPrinterColumnSpecs = specs;
            _discoveryPrinterColumns = IncludeAdditionalPrinterColumns && matchingDefinition is not null && version is not null
                ? CreatePrinterColumns(matchingDefinition, version.AdditionalPrinterColumns)
                : [];
            _discoveryDefinitionColumns = matchingDefinition is not null && version is not null
                ? CreateAdditionalColumns(matchingDefinition).ToArray()
                : [];
            RebuildDiscoveryColumns();
        }

        NotifyColumnsChanged();
    }

    private bool MatchesDiscoveryKind(V1CustomResourceDefinition? definition)
        => definition?.Spec is { } spec
            && string.Equals(spec.Group, _kind.Group, StringComparison.Ordinal)
            && string.Equals(spec.Names?.Kind, _kind.Kind, StringComparison.Ordinal)
            && string.Equals(spec.Names?.Plural, _kind.PluralName, StringComparison.Ordinal);

    /// <summary>Gets whether CRD printer columns should be included for this kind.</summary>
    protected virtual bool IncludeAdditionalPrinterColumns => true;

    /// <summary>Creates columns specific to this CRD kind.</summary>
    /// <param name="crd">CRD being configured.</param>
    protected virtual IEnumerable<IResourceListColumn> CreateAdditionalColumns(V1CustomResourceDefinition crd) => [];

    /// <summary>Creates columns specific to a discovered resource endpoint.</summary>
    /// <param name="resource">Discovered endpoint being configured.</param>
    protected virtual IEnumerable<IResourceListColumn> CreateAdditionalColumns(ApiDiscoveryResource resource) => [];

    private List<IResourceListColumn> CreatePrinterColumns(
        V1CustomResourceDefinition crd,
        IEnumerable<V1CustomResourceColumnDefinition>? definitions)
    {
        List<IResourceListColumn> columns = [];
        if (definitions is null)
        {
            return columns;
        }

        foreach (var item in definitions)
        {
            if (string.IsNullOrWhiteSpace(item.JsonPath)
                || string.Equals(item.JsonPath, ".metadata.creationTimestamp", StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                switch (item.Type, item.Format)
                {
                    case ("integer", "int64"):
                        columns.Add(CreateColumn(item.Name, item.JsonPath, ConvertInt64));
                        break;
                    case ("integer", _):
                        columns.Add(CreateColumn(item.Name, item.JsonPath, ConvertInt32));
                        break;
                    case ("number", _):
                        columns.Add(CreateColumn(item.Name, item.JsonPath, ConvertDouble));
                        break;
                    case ("boolean", _):
                        columns.Add(CreateColumn(item.Name, item.JsonPath, ConvertBoolean));
                        break;
                    case ("date", _):
                        columns.Add(CreateDateColumn(item.Name, item.JsonPath));
                        break;
                    default:
                        columns.Add(CreateStringColumn(item.Name, item.JsonPath));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to generate generic resource column: {Name} in {Resource}", item.Name, crd.Name());
            }
        }

        return columns;
    }

    private void RebuildDiscoveryColumns()
    {
        _columns.Clear();
        _columns.Add(NameColumn(SortDirection.Ascending));
        if (IsNamespaced)
        {
            _columns.Add(NamespaceColumn());
        }

        _columns.AddRange(_discoveryPrinterColumns);
        _columns.AddRange(_discoveryDefinitionColumns);
        _columns.AddRange(_discoveryAdditionalColumns);
        _columns.Add(AgeColumn());
    }

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
        lock (_configureLock)
        {
            return _columns.ToArray();
        }
    }

    private static string CreateColumnKey(string name)
    {
        return Regex.Replace(name.Trim().ToLowerInvariant(), @"\W+", "-").Trim('-');
    }
}
