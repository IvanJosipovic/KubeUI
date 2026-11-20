using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using AvaloniaEdit;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Converters;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Settings;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
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
                CreateEditor(vm).Row(3));
    }

    private static TextEditor CreateEditor(ContainerFilePreviewViewModel vm)
    {
        var editor = new TextEditor()
            .Document(vm, x => x.Document)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .IsReadOnly(true)
            .ShowLineNumbers(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .WordWrap(false)
            .WithCodeFontSizeKeyBindings(
                ((IServiceProviderHost)Application.Current!).Services.GetRequiredService<IResourceAppearanceSettings>())
            .Behaviors(new ContainerFileSyntaxHighlightingBehavior(vm.Path));

        return editor;
    }
}
