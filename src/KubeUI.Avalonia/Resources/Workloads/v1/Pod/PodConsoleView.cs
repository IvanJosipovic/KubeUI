using Avalonia.Markup.Declarative;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;
using SvcSystems.UI.Terminal;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PodConsoleView : ViewBase<PodConsoleViewModel>
{
    protected override object Build(PodConsoleViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        this.AddBehaviors(new PodConsoleConnectionBehavior());

        return new Grid()
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
                    .RightClickAction(RightClickAction.CopyOrPaste)
                    .Row(1));
    }
}
