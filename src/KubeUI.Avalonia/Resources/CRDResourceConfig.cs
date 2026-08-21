using System.Text.Json;
using System.Text.RegularExpressions;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using JsonPathLINQ;

namespace KubeUI.Avalonia.Resources;

public sealed class CRDResourceConfig : ResourceConfigBase<GenericKubernetesObject>
{
    private bool _isNamespaced = true;
    private string? _resourceName;
    private GroupApiVersionKind _kind;

    public CRDResourceConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => _isNamespaced;

    public override GroupApiVersionKind Kind => _kind;

    public override bool IsCustomResource => true;

    public override string Name => _resourceName ?? base.Name;

    private readonly List<IResourceListColumn> _columns = [];

    public void Configure(V1CustomResourceDefinition crd)
    {
        _columns.Clear();
        if (!crd.TryGetResourceKind(out _kind) || crd.Spec is not { } spec)
        {
            throw new InvalidOperationException("CRD has no served storage version.");
        }

        var version = spec.Versions.First(candidate => candidate.Served && candidate.Storage);
        _resourceName = spec.Names!.Kind.Humanize(LetterCasing.Title).Pluralize();

        _columns.Add(NameColumn(SortDirection.Ascending));

        if (spec.Scope == "Namespaced")
        {
            _isNamespaced = true;
            _columns.Add(NamespaceColumn());
        }
        else
        {
            _isNamespaced = false;
        }

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
                            _columns.Add(CreateColumn<long>(item.Name, item.JsonPath));
                            break;
                        case ("integer", _):
                            _columns.Add(CreateColumn<int>(item.Name, item.JsonPath));
                            break;
                        case ("number", _):
                            _columns.Add(CreateColumn<double>(item.Name, item.JsonPath));
                            break;
                        case ("boolean", _):
                            _columns.Add(CreateColumn<bool>(item.Name, item.JsonPath));
                            break;
                        case ("date", _):
                            _columns.Add(CreateDateColumn(item.Name, item.JsonPath));
                            break;
                        default:
                            _columns.Add(CreateStringColumn(item.Name, item.JsonPath));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to generate generic resource column: {Name} in {Resource}", item.Name, crd.Name());
                }
            }
        }

        _columns.Add(AgeColumn());
    }

    private static ResourceListColumn<GenericKubernetesObject, TValue?> CreateColumn<TValue>(
        string name,
        string jsonPath)
        where TValue : struct
    {
        var getter = JsonPath.GetExpression<GenericKubernetesObject, TValue?>(jsonPath, addNullChecks: true).Compile();
        return new ResourceListColumn<GenericKubernetesObject, TValue?>
        {
            Key = CreateColumnKey(name),
            Name = name,
            Field = resource => getter(resource)
        };
    }

    private static ResourceListColumn<GenericKubernetesObject, string> CreateStringColumn(
        string name,
        string jsonPath)
    {
        var getter = JsonPath.GetExpression<GenericKubernetesObject, string?>(jsonPath, addNullChecks: true).Compile();
        return new ResourceListColumn<GenericKubernetesObject, string>
        {
            Key = CreateColumnKey(name),
            Name = name,
            Field = resource => getter(resource) ?? string.Empty
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

    public override IList<IResourceListColumn> Columns()
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
