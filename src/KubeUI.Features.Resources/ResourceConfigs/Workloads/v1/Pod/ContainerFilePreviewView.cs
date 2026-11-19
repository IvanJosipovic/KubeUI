using Avalonia.Layout;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

/// <summary>Shows a container file's contents in the bottom tool dock.</summary>
public sealed class ContainerFilePreviewView : KubeUIView<ContainerFilePreviewViewModel>
{
    protected override object Build(ContainerFilePreviewViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,Auto,Auto,*")
            .Children(
                new CompactToolbar
                {
                    Leading = CompactToolbar.Group(
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Path)
                            .TextTrimming(TextTrimming.CharacterEllipsis),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Identity)
                            .Opacity(0.7)
                            .TextTrimming(TextTrimming.CharacterEllipsis)),
                    Trailing = CompactToolbar.Group(
                        new Button()
                            .Content(new FluentIcon().Icon(Icon.ArrowClockwise))
                            .Command(vm, x => x.RefreshCommand)),
                }.Row(0),
                new TextBlock()
                    .Row(1)
                    .Text(ResourcesLanguageManager.Instance.ContainerFileBrowserView_PreviewTruncated.ToBinding())
                    .IsVisible(vm, x => x.IsTruncated)
                    .TextWrapping(TextWrapping.Wrap)
                    .Margin(0, 4),
                new TextBlock()
                    .Row(2)
                    .Text(vm, x => x.Message)
                    .IsVisible(vm, x => x.Message, converter: new StringNotNullOrEmptyConverter())
                    .TextWrapping(TextWrapping.Wrap)
                    .Margin(0, 4),
                new ScrollViewer()
                    .Row(3)
                    .Content(
                        new TextBox()
                            .Text(vm, x => x.Text)
                            .IsReadOnly(true)
                            .AcceptsReturn(true)
                            .TextWrapping(TextWrapping.NoWrap)));
    }
}
