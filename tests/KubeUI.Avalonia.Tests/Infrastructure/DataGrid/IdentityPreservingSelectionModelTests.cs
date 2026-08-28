using k8s.Models;
using KubeUI.Avalonia.Infrastructure.DataGrid;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.DataGrid;

public sealed class IdentityPreservingSelectionModelTests
{
    [Fact]
    public void Restores_selection_by_namespace_and_name_when_resources_reorder()
    {
        V1Pod first = Pod("namespace-a", "same-name");
        V1Pod second = Pod("namespace-b", "same-name");
        List<V1Pod> source = [first, second];

        using var model = new IdentityPreservingSelectionModel<V1Pod, ResourceCacheKey>(ResourceCacheKey.From)
        {
            Source = source
        };
        model.SetIdentitySource(source);
        model.Select(0);

        source[0] = second;
        source[1] = first;
        model.SetIdentitySource(source);

        model.SelectedIndexes.ShouldBe([1]);
        model.SelectedItem.ShouldBe(first);
    }

    private static V1Pod Pod(string @namespace, string name)
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = @namespace,
                Name = name,
            },
        };
    }
}
