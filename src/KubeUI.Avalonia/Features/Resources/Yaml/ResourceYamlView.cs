using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using SharedConverters = KubeUI.Avalonia.Converters.Converters;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

public sealed partial class ResourceYamlView : ViewBase<ResourceYamlViewModel>
{
    public ResourceYamlView()
    {
        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    protected override object Build(ResourceYamlViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var hoverPopup = new Popup();

        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,Auto,*")
            .Children(
                CreateReadOnlyToolbar(vm),
                CreateEditToolbar(vm),
                new FAInfoBar()
                    .Name("ActionResultBar", Scope)
                    .Classes("YamlActionResultBar")
                    .Styles(
                        new Style<FAInfoBarPanel>(selector =>
                                selector.OfType<FAInfoBar>()
                                    .Class("YamlActionResultBar")
                                    .Template()
                                    .Descendant()
                                    .OfType<FAInfoBarPanel>())
                            .Setter(FAInfoBarPanel.VerticalOrientationPaddingProperty, new Thickness(0, 14, 0, 14)))
                    .Row(1)
                    .Title(vm, x => x.ActionResultTitle)
                    .Margin(4)
                    .CloseButtonCommand(vm, x => x.DismissActionResultCommand)
                    .IsClosable(true)
                    .IsOpen(vm, x => x.HasActionResult)
                    .IsVisible(vm, x => x.HasActionResult)
                    .Message(vm, x => x.ActionResultMessage)
                    .Severity(vm, x => x.ActionResultSeverity),
                CreateEditor(vm, hoverPopup).Name("Editor", Scope),
                hoverPopup);
    }

    private StackPanel CreateReadOnlyToolbar(ResourceYamlViewModel vm)
    {
        return new StackPanel()
            .Row(0)
            .IsVisible(vm, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)
            .Orientation(Orientation.Horizontal)
            .Children(
                new Button()
                    .Command(vm, x => x.SetEditModeCommand)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_Edit))
                    .Content(new FluentIcon().Icon(Icon.DocumentEdit)),
                new ToggleButton()
                    .Name("HideNoisyFieldsToggle", Scope)
                    .IsChecked(vm, x => x.HideNoisyFields, BindingMode.TwoWay)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_HideNoisyFields))
                    .Content(new FluentIcon().Icon(Icon.EyeOff)),
                CreateWordWrapToggle(vm));
    }

    private static StackPanel CreateEditToolbar(ResourceYamlViewModel vm)
    {
        return new StackPanel()
            .Row(0)
            .IsVisible(vm, x => x.EditMode)
            .Orientation(Orientation.Horizontal)
            .Children(
                new Button()
                    .Command(vm, x => x.SaveCommand)
                    .IsEnabled(vm, x => x.CanSaveAction)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_Save))
                    .Content(new FluentIcon().Icon(Icon.Save)),
                new Button()
                    .Command(vm, x => x.DryRunCommand)
                    .IsEnabled(vm, x => x.CanDryRunAction)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_DryRun))
                    .Content(new FluentIcon().Icon(Icon.CheckmarkCircle)),
                new Button()
                    .Command(vm, x => x.SetEditModeCommand)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_Cancel))
                    .Content(new FluentIcon().Icon(Icon.Dismiss)),
                CreateWordWrapToggle(vm));
    }

    private static ToggleButton CreateWordWrapToggle(ResourceYamlViewModel vm)
    {
        return new ToggleButton()
            .IsChecked(vm, x => x.WordWrap, BindingMode.TwoWay)
            .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_WordWrap))
            .Content(new FluentIcon().Icon(Icon.TextWrap));
    }

    private static TextEditor CreateEditor(ResourceYamlViewModel vm, Popup hoverPopup)
    {
        var editor = new TextEditor()
            .Row(2)
            .Background(new DynamicResourceExtension("SystemAltHighColor"))
            .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
            .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
            .FontWeight(FontWeight.Normal)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .IsReadOnly(vm, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)
            .ShowLineNumbers(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .WordWrap(vm, x => x.WordWrap)
            .Options(new TextEditorOptions
            {
                ConvertTabsToSpaces = true,
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false,
                HighlightCurrentLine = true,
                IndentationSize = 2,
                ShowBoxForControlCharacters = false
            })
            .Behaviors(
                new YamlEditorBehavior(),
                new YamlDiagnosticRenderingBehavior(),
                new YamlHoverToolTipBehavior(hoverPopup),
                new YamlEditorScrollBehavior())
            .KeyBindings(
                new KeyBinding { Command = vm.RequestCompletionCommand, Gesture = new KeyGesture(Key.Space, KeyModifiers.Control) },
                new KeyBinding { Command = vm.UndoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Control) });

        editor.ContextMenu = CreateEditorContextMenu(vm, editor);
        return editor;
    }

    private static ContextMenu CreateEditorContextMenu(ResourceYamlViewModel vm, TextEditor editor)
    {
        return new ContextMenu()
            .Items(
                CreateEditorMenuItem(vm, editor, Assets.Resources.ResourceYamlView_Action_Cut, new KeyGesture(Key.X, KeyModifiers.Control), Icon.Cut, "Cut", requiresEditMode: true),
                CreateEditorMenuItem(vm, editor, Assets.Resources.Shared_Action_Copy, new KeyGesture(Key.C, KeyModifiers.Control), Icon.Copy, "Copy", requiresEditMode: false),
                CreateEditorMenuItem(vm, editor, Assets.Resources.ResourceYamlView_Action_Paste, new KeyGesture(Key.V, KeyModifiers.Control), Icon.ClipboardPaste, "Paste", requiresEditMode: true),
                CreateEditorMenuItem(vm, editor, Assets.Resources.ResourceYamlView_Action_Delete, null, Icon.Delete, "Delete", requiresEditMode: true),
                new Separator().IsVisible(vm, x => x.EditMode),
                CreateEditorMenuItem(vm, editor, Assets.Resources.ResourceYamlView_Action_Undo, new KeyGesture(Key.Z, KeyModifiers.Control), Icon.ArrowUndo, "Undo", requiresEditMode: true),
                CreateEditorMenuItem(vm, editor, Assets.Resources.ResourceYamlView_Action_Redo, new KeyGesture(Key.Y, KeyModifiers.Control), Icon.ArrowRedo, "Redo", requiresEditMode: true));
    }

    private static MenuItem CreateEditorMenuItem(ResourceYamlViewModel vm, TextEditor editor, string header, KeyGesture? hotKey, Icon icon, string methodName, bool requiresEditMode)
    {
        var item = new MenuItem()
            .Header(header)
            .Icon(new FluentIcon().Icon(icon))
            .Behaviors(new EventTriggerBehavior
            {
                EventName = nameof(MenuItem.Click),
                Actions =
                [
                    new CallMethodAction
                    {
                        TargetObject = editor,
                        MethodName = methodName
                    }
                ]
            })
            .HotKey(hotKey);


        return requiresEditMode
            ? item.IsVisible(vm, x => x.EditMode)
            : item;
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        var cluster = await DesignTimePreview.CreateClusterAsync<V1Pod>();
        var vm = DesignTimePreview.Get<ResourceYamlViewModel>();

        var obj = new V1Pod()
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default",
            },
            Spec = new()
            {
                Containers = [
                    new(){
                        Image = "nginx",
                        ImagePullPolicy = "Always"
                    }
                ]
            }
        };

        vm.Initialize(cluster, obj);

        DataContext = vm;
    }
}
