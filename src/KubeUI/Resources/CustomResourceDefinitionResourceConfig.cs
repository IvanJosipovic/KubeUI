using k8s.Models;
using k8s;
using KubeUI.Client;
using System.Text.RegularExpressions;

namespace KubeUI.Resources;

public partial class CustomResourceDefinitionResourceConfig<T> : ResourceConfigBase<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public CustomResourceDefinitionResourceConfig(V1CustomResourceDefinition crd)
    {

    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        throw new NotImplementedException();
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        throw new NotImplementedException();
    }

    public override Control[]? Properties(T resource)
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex("types '(.+)' and '(.+)'", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TypeErrorRegex();
}
