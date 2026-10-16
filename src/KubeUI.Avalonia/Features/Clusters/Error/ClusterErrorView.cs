using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Presentation;


namespace KubeUI.Avalonia.Features.Clusters.Error;

public sealed partial class ClusterErrorView : KubeUIView<ClusterErrorViewModel>
{
    public ClusterErrorView()
    {
        if (DesignTimePreview.IsDesignMode)
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
