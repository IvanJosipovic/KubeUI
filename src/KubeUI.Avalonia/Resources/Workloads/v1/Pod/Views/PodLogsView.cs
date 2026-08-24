using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Styles;
using Ursa.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

/// <summary>Displays streamed pod logs with pod and container selection.</summary>
public sealed partial class PodLogsView : ViewBase<PodLogsViewModel>
{
    private static readonly FuncValueConverter<string?, bool> HasErrorConverter = new(
        value => !string.IsNullOrWhiteSpace(value));

    protected override object Build(PodLogsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children(
                CreateTopBar(vm),
                RegisterName("TextEditorControl", CreateEditor(vm)));
    }

    private StackPanel CreateTopBar(PodLogsViewModel vm)
    {
        return RegisterName(
            "TopBar",
            new StackPanel()
                .Row(0)
                .Height(32)
                .ClipToBounds(true)
                .Orientation(Orientation.Horizontal)
                .Children(
                    new Label()
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Content(Assets.Resources.PodLogsView_PodLabel),
                    RegisterName("PodSelectionComboBox", CreatePodSelector(vm)),
                    new Label()
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Content(Assets.Resources.PodLogsView_ContainerLabel),
                    RegisterName("ContainerSelectionComboBox", CreateContainerSelector(vm)),
                    new TextBlock()
                        .MaxWidth(400)
                        .Text(vm, x => x.ConnectionError)
                        .IsVisible(vm, x => x.ConnectionError, converter: HasErrorConverter)
                        .TextTrimming(TextTrimming.CharacterEllipsis),
                    new Button()
                        .Command(vm, x => x.ClearCommand)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_Clear)
                        .Content(new FluentIcon().Icon(Icon.Delete)),
                    new Button()
                        .Command(vm, x => x.JumpToPresentCommand)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_JumpToPresent)
                        .Content(new FluentIcon().Icon(Icon.ArrowDown)),
                    new Button()
                        .Command(vm, x => x.DownloadLogsCommand)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_Download)
                        .Content(new FluentIcon().Icon(Icon.Save)),
                    new ToggleButton()
                        .IsChecked(vm, x => x.ShowResourceNames, BindingMode.TwoWay)
                        .IsEnabled(vm, x => x.CanShowResourceNames)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_ShowResourceNames)
                        .Content(new FluentIcon().Icon(Icon.Tag)),
                    new Button()
                        .Command(vm, x => x.JumpToControlledByLogsCommand)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_Controller)
                        .Content(new FluentIcon().Icon(Icon.ArrowUp)),
                    new ToggleButton()
                        .IsChecked(vm, x => x.Previous, BindingMode.TwoWay)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_Previous)
                        .Content(new FluentIcon().Icon(Icon.ArrowUndo)),
                    new ToggleButton()
                        .IsChecked(vm, x => x.Timestamps, BindingMode.TwoWay)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_Timestamps)
                        .Content(new FluentIcon().Icon(Icon.Timer)),
                    new ToggleButton()
                        .IsChecked(vm, x => x.WordWrap, BindingMode.TwoWay)
                        .ToolTip_Tip(Assets.Resources.PodLogsView_WordWrap)
                        .Content(new FluentIcon().Icon(Icon.TextWrap))));
    }

    private static MultiComboBox CreatePodSelector(PodLogsViewModel vm)
    {
        FuncDataTemplate<PodLogPodSelectionItem> template = new(
            (item, _) => new TextBlock().Text(item, x => x.DisplayName));

        return new MultiComboBox()
            .Width(220)
            .Height(24)
            .MaxHeight(24)
            .Margin(0, 0, 8, 0)
            .VerticalAlignment(VerticalAlignment.Center)
            .ItemsSource(vm, x => x.PodSelectionItems)
            .SelectedItems(vm, x => x.SelectedPodItems, BindingMode.TwoWay)
            .ToolTip_Tip(Assets.Resources.PodLogsView_PodLabel)
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
    }

    private static MultiComboBox CreateContainerSelector(PodLogsViewModel vm)
    {
        FuncDataTemplate<PodLogContainerSelectionItem> template = new(
            (item, _) => new TextBlock().Text(item, x => x.DisplayName));

        return new MultiComboBox()
            .Width(180)
            .Height(24)
            .MaxHeight(24)
            .Margin(0, 0, 8, 0)
            .VerticalAlignment(VerticalAlignment.Center)
            .ItemsSource(vm, x => x.ContainerSelectionItems)
            .SelectedItems(vm, x => x.SelectedContainerItems, BindingMode.TwoWay)
            .ToolTip_Tip(Assets.Resources.PodLogsView_ContainerLabel)
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
    }

    private TextEditor CreateEditor(PodLogsViewModel vm)
    {
        PodLogsEditorBehavior behavior = new();
        behavior.AutoScrollToBottom(vm, x => x.AutoScrollToBottom);
        behavior.JumpToPresentRequested(vm, x => x.JumpToPresentRequested);
        behavior.ScrollOffset(vm, x => x.ScrollOffset);

        var editor = new TextEditor()
            .Row(1)
            .Background(new DynamicResourceExtension("SystemAltHighColor"))
            .Document(vm, x => x.Logs)
            .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
            .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
            .FontWeight(FontWeight.Normal)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .IsReadOnly(true)
            .ShowLineNumbers(false)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .WordWrap(vm, x => x.WordWrap)
            .Options(new TextEditorOptions
            {
                AllowScrollBelowDocument = false,
                EnableEmailHyperlinks = false,
                EnableHyperlinks = false,
                ShowBoxForControlCharacters = false,
            })
            .Behaviors(behavior);

        editor.ContextMenu = CreateContextMenu(editor);
        return editor;
    }

    private static ContextMenu CreateContextMenu(TextEditor editor)
    {
        return new ContextMenu()
            .Items(
                new MenuItem()
                    .Header(Assets.Resources.Shared_Action_Copy)
                    .HotKey(new KeyGesture(Key.C, KeyModifiers.Control))
                    .Icon(new FluentIcon().Icon(Icon.Copy))
                    .Behaviors(new EventTriggerBehavior
                    {
                        EventName = nameof(MenuItem.Click),
                        Actions =
                        [
                            new CallMethodAction
                            {
                                TargetObject = editor,
                                MethodName = nameof(TextEditor.Copy),
                            },
                        ],
                    }));
    }

    private TControl RegisterName<TControl>(string name, TControl control)
        where TControl : Control
    {
        control.Name = name;
        Scope.Register(name, control);
        return control;
    }
}
