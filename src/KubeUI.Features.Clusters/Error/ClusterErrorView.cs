#pragma warning disable CA1716 // Public namespace is an API contract; renaming would be a breaking change.
namespace KubeUI.Features.Clusters.Error;
#pragma warning restore CA1716

public sealed partial class ClusterErrorView : KubeUIView<ClusterErrorViewModel>
{
    public ClusterErrorView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = new ClusterErrorViewModel
            {
                Error = "Design-time error sample"
            };
        }
    }

    protected override object Build(ClusterErrorViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new TextBlock()
                    .Row(0)
                    .Padding(5, 0, 0, 0)
                    .Text(ClustersLanguageManager.Instance.ClusterErrorView_Header.ToBinding()),
                new SelectableTextBlock()
                    .Row(1)
                    .Padding(5, 0, 0, 0)
                    .Text(vm, x => x.Error)
                    .TextWrapping(TextWrapping.Wrap));
    }
}
