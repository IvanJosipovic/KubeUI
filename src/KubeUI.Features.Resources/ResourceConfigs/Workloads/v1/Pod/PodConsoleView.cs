using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactivity;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Features.Resources.Settings;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Controls.Styles;
using SvcSystems.UI.Terminal;


namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed class PodConsoleView : KubeUIView<PodConsoleViewModel>
{
    protected override object Build(PodConsoleViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        Interaction.GetBehaviors(this).Add(new PodConsoleConnectionBehavior());

        var root = new Grid();
        foreach (var keyBinding in CodeFontSizeKeyBindings.Create(
            ((IServiceProviderHost)Application.Current!).Services.GetRequiredService<IResourceAppearanceSettings>()))
        {
            root.KeyBindings.Add(keyBinding);
        }

        return root
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new Label()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(Assets.Resources.PodLogsView_PodLabel),
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
