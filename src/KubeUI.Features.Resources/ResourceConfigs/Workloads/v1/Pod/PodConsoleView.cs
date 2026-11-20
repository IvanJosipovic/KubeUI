using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Styles;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Settings;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
using SvcSystems.UI.Terminal;


namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed class PodConsoleView : KubeUIView<PodConsoleViewModel>
{
    protected override object Build(PodConsoleViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        this.Behaviors(new PodConsoleConnectionBehavior());

        var root = new Grid().
            WithCodeFontSizeKeyBindings(
                ((IServiceProviderHost)Application.Current!).Services.GetRequiredService<IResourceAppearanceSettings>());

        return root
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new Label()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(ResourcesLanguageManager.Instance.PodLogsView_PodLabel.ToBinding()),
                        new Label()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content($"{vm.Object?.Metadata?.NamespaceProperty}/{vm.Object?.Metadata?.Name}/{vm.ContainerName}")),
                new TerminalControl()
                    .Model(vm, x => x.Model)
                    .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
                    .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
                    .RightClickAction(RightClickAction.CopyOrPaste)
                    .Row(1));
    }
}
