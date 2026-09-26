using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public sealed class ResourceEditorView : ViewBase<ResourceEditorViewModel>
{
    public ResourceEditorView()
    {
    }

    protected override object Build(ResourceEditorViewModel vm)
    {
        var editorHost = new ContentControl()
            .ContentTemplate(new FuncDataTemplate<ResourceEditorNodeViewModel>(
                static (node, _) => node is null ? null : new ResourceEditorNodeControl(node)));
        editorHost.Bind(
            ContentControl.ContentProperty,
            CompiledBinding.Create<ResourceEditorViewModel, ResourceEditorNodeViewModel?>(x => x.EditorRoot));

        var save = new Button()
            .ToolTip_Tip(Assets.Resources.ResourceEditorView_Save)
            .Content(new FluentIcon().Icon(Icon.Save));
        save.Bind(
            Button.CommandProperty,
            CompiledBinding.Create<ResourceEditorViewModel, System.Windows.Input.ICommand?>(x => x.SaveCommand));
        var validate = new Button()
            .ToolTip_Tip(Assets.Resources.ResourceEditorView_Validate)
            .Content(new FluentIcon().Icon(Icon.CheckmarkCircle));
        validate.Bind(
            Button.CommandProperty,
            CompiledBinding.Create<ResourceEditorViewModel, System.Windows.Input.ICommand?>(x => x.ValidateNowCommand));
        var reset = new Button()
            .ToolTip_Tip(Assets.Resources.ResourceEditorView_Reset)
            .Content(new FluentIcon().Icon(Icon.ArrowReset));
        reset.Bind(
            Button.CommandProperty,
            CompiledBinding.Create<ResourceEditorViewModel, System.Windows.Input.ICommand?>(x => x.ResetCommand));

        var actionBar = new FAInfoBar()
            .Title(vm, x => x.ActionResultTitle)
            .Margin(4)
            .CloseButtonCommand(vm, x => x.DismissActionResultCommand)
            .IsClosable(true)
            .IsOpen(vm, x => x.HasActionResult)
            .IsVisible(vm, x => x.HasActionResult)
            .Message(vm, x => x.ActionResultMessage)
            .Severity(vm, x => x.ActionResultSeverity);

        return new Grid().Rows("Auto,Auto,*").Children(
            new StackPanel()
                .Row(0)
                .Orientation(Orientation.Horizontal)
                .Children(save, validate, reset),
            actionBar.Row(1),
            new ScrollViewer()
                .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
                .Content(editorHost)
                .Row(2));
    }
}
