using k8s.Models;
using KubeUI.Avalonia.Infrastructure.DataGrid;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.DataGrid;

public sealed class IdentityPreservingSelectionModelTests
{
    [Fact]
    public void Restores_selection_by_uid_when_same_name_resources_reorder()
    {
        V1Pod first = Pod("uid-first");
        V1Pod second = Pod("uid-second");
        List<V1Pod> source = [first, second];

        using var model = new IdentityPreservingSelectionModel<V1Pod>(static pod => pod.Uid())
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

    private static V1Pod Pod(string uid)
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "default",
                Name = "same-name",
                Uid = uid,
            },
        };
    }
}
