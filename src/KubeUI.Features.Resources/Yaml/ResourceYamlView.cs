using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Styles;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Settings;
using KubeUI.Features.Resources.Yaml.Behaviors;
using SharedConverters = KubeUI.Controls.Converters.Converters;


namespace KubeUI.Features.Resources.Yaml;

public sealed partial class ResourceYamlView : KubeUIView<ResourceYamlViewModel>
{
    public ResourceYamlView()
    {
        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    [RequiresUnreferencedCode("Declarative YAML view construction may use members removed by trimming.")]
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
                    .IsVisible(vm, x => x.IsReadOnlyLocked, BindingMode.OneWay, SharedConverters.Not)
                    .Command(vm, x => x.SetEditModeCommand)
                    .ToolTip_Tip(YamlDocumentationViewFactory.CreateCodeText(Assets.Resources.ResourceYamlView_Edit))
                    .Content(new FluentIcon().Icon(Icon.DocumentEdit)),
                new ToggleButton()
                    .Name("HideNoisyFieldsToggle", Scope)
                    .IsVisible(vm, x => x.IsReadOnlyLocked, BindingMode.OneWay, SharedConverters.Not)
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

    [RequiresUnreferencedCode("Calls KubeUI.Features.Resources.Yaml.ResourceYamlView.CreateEditorContextMenu(ResourceYamlViewModel, TextEditor)")]
    private static TextEditor CreateEditor(ResourceYamlViewModel vm, Popup hoverPopup)
    {
        var editor = new TextEditor()
            .Row(2)
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
            });

        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(new YamlEditorBehavior());
        behaviors.Add(new YamlSyntaxHighlightingBehavior());
        behaviors.Add(new YamlDiagnosticRenderingBehavior());
        behaviors.Add(new YamlHoverToolTipBehavior(hoverPopup));
        behaviors.Add(new YamlEditorScrollBehavior());

        foreach (var keyBinding in CodeFontSizeKeyBindings.Create(
            ((IServiceProviderHost)Application.Current!).Services.GetRequiredService<IResourceAppearanceSettings>()))
        {
            editor.KeyBindings.Add(keyBinding);
        }

        editor.KeyBindings.Add(new KeyBinding { Command = vm.RequestCompletionCommand, Gesture = new KeyGesture(Key.Space, KeyModifiers.Control) });
        editor.KeyBindings.Add(new KeyBinding { Command = vm.UndoCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Control) });
        editor.KeyBindings.Add(new KeyBinding
        {
            Command = new RelayCommand(
                () =>
                {
                    if (vm.EditMode)
                    {
                        editor.Redo();
                    }
                },
                () => vm.EditMode && editor.Document?.UndoStack.CanRedo == true),
            Gesture = new KeyGesture(Key.Y, KeyModifiers.Control)
        });

        editor.ContextMenu = CreateEditorContextMenu(vm, editor);
        return editor;
    }

    [RequiresUnreferencedCode("Calls KubeUI.Features.Resources.Yaml.ResourceYamlView.CreateEditorMenuItem(ResourceYamlViewModel, TextEditor, String, KeyGesture, Icon, String, Boolean)")]
    private static ContextMenu CreateEditorContextMenu(ResourceYamlViewModel vm, TextEditor editor)
    {
        return new ContextMenu()
            .Items(
                CreateEditorMenuItem(vm, Assets.Resources.ResourceYamlView_Action_Cut, new KeyGesture(Key.X, KeyModifiers.Control), Icon.Cut, editor.Cut, requiresEditMode: true),
                CreateEditorMenuItem(vm, SharedAssets.Resources.Shared_Action_Copy, new KeyGesture(Key.C, KeyModifiers.Control), Icon.Copy, editor.Copy, requiresEditMode: false),
                CreateEditorMenuItem(vm, Assets.Resources.ResourceYamlView_Action_Paste, new KeyGesture(Key.V, KeyModifiers.Control), Icon.ClipboardPaste, editor.Paste, requiresEditMode: true),
                CreateEditorMenuItem(vm, Assets.Resources.ResourceYamlView_Action_Delete, null, Icon.Delete, editor.Delete, requiresEditMode: true),
                new Separator().IsVisible(vm, x => x.EditMode),
                CreateEditorMenuItem(vm, Assets.Resources.ResourceYamlView_Action_Undo, new KeyGesture(Key.Z, KeyModifiers.Control), Icon.ArrowUndo, () => editor.Undo(), requiresEditMode: true),
                CreateEditorMenuItem(vm, Assets.Resources.ResourceYamlView_Action_Redo, new KeyGesture(Key.Y, KeyModifiers.Control), Icon.ArrowRedo, () => editor.Redo(), requiresEditMode: true));
    }

    [RequiresUnreferencedCode("Calls Avalonia.Xaml.Interactions.Core.EventTriggerBehavior.EventTriggerBehavior()")]
    private static MenuItem CreateEditorMenuItem(ResourceYamlViewModel vm, string header, KeyGesture? hotKey, Icon icon, System.Action execute, bool requiresEditMode)
    {
        var item = new MenuItem()
            .Header(header)
            .Icon(new FluentIcon().Icon(icon))
            .Command(new RelayCommand(execute))
            .InputGesture(hotKey);


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
