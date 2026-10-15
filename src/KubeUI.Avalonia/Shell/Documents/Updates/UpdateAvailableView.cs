using Avalonia.Controls.Primitives;
using KubeUI.Avalonia.Infrastructure;
using LiveMarkdown.Avalonia;

namespace KubeUI.Avalonia.Shell.Documents.Updates;

public sealed class UpdateAvailableView : ViewBase<UpdateAvailableViewModel>
{
    protected override object Build(UpdateAvailableViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*,Auto,Auto,Auto")
            .Margin(12)
            .Children(
                new TextBlock()
                    .FontSize(20)
                    .FontWeight(FontWeight.SemiBold)
                    .Text(vm, x => x.VersionSummary)
                    .Margin(0, 0, 0, 12)
                    .Row(0),
                new ScrollViewer()
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
                    .Content(new MarkdownRenderer
                    {
                        MarkdownBuilder = vm.MarkdownBuilder,
                        LinkCommand = vm.OpenReleaseNoteLinkCommand
                    })
                    .Row(1),
                new TextBlock()
                    .Text(vm, x => x.DownloadProgressText)
                    .IsVisible(vm, x => x.IsUpdating)
                    .Row(2),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(vm, x => x.DownloadProgress)
                    .IsVisible(vm, x => x.IsUpdating)
                    .Row(3),
                new Button()
                    .Content(Assets.Resources.UpdateAvailableView_Update)
                    .Command(vm, x => x.UpdateCommand)
                    .IsEnabled(vm, x => !x.IsUpdating)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Margin(0, 12, 0, 0)
                    .Row(4));
    }
}
