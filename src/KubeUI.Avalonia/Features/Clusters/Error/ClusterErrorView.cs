namespace KubeUI.Avalonia.Features.Clusters.Error;

public sealed partial class ClusterErrorView : ViewBase<ClusterErrorViewModel>
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
                    .Text(Assets.Resources.ClusterErrorView_Header),
                new SelectableTextBlock()
                    .Row(1)
                    .Padding(5, 0, 0, 0)
                    .Text(vm, x => x.Error)
                    .TextWrapping(TextWrapping.Wrap));
    }
}
