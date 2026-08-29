using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Styles;
using Ursa.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

/// <summary>Displays streamed pod logs with pod and container selection.</summary>
public sealed partial class PodLogsView : ViewBase<PodLogsViewModel>
{
    private const double SelectorWidth = 220;
    private static readonly StringNotNullOrEmptyConverter HasErrorConverter = new();

    protected override object Build(PodLogsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children(
                CreateTopBar(vm),
                CreateEditor(vm));
    }

    private Grid CreateTopBar(PodLogsViewModel vm)
    {
        return new Grid()
            .Row(0)
            .Rows("Auto,Auto")
            .Children(
                CreateScopeIdentityBar(vm),
                CreateLogControlsBar(vm));
    }

    private StackPanel CreateScopeIdentityBar(PodLogsViewModel vm)
    {
        return new StackPanel()
            .Row(0)
            .Height(24)
            .ClipToBounds(true)
            .Orientation(Orientation.Horizontal)
            .Spacing(16)
            .Margin(2, 0)
            .Children(
                new StackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(4)
                    .Margin(4, 0, 0, 0)
                    .Children(
                        new TextBlock()
                            .FontWeight(FontWeight.SemiBold)
                            .Text($"{Assets.Resources.ResourcePropertiesView_Name}:"),
                        new TextBlock()
                            .FontWeight(FontWeight.Normal)
                            .Text(vm, x => x.ScopeResourceName)
                            .TextTrimming(TextTrimming.CharacterEllipsis)),
                new StackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(4)
                    .IsVisible(vm, x => x.HasScopeNamespace)
                    .Children(
                        new TextBlock()
                            .FontWeight(FontWeight.SemiBold)
                            .Text($"{Assets.Resources.ResourcePropertiesView_Namespace}:"),
                        new TextBlock()
                            .FontWeight(FontWeight.Normal)
                            .Text(vm, x => x.ScopeNamespace)
                            .TextTrimming(TextTrimming.CharacterEllipsis)));
    }

    private Grid CreateLogControlsBar(PodLogsViewModel vm)
    {
        return new Grid()
            .Row(1)
            .Height(32)
            .ClipToBounds(true)
            .Cols("*,Auto")
            .Margin(2, 0)
            .Children(
                CreateSelectionControls(vm),
                CreateActionControls(vm));
    }

    private StackPanel CreateSelectionControls(PodLogsViewModel vm)
    {
        return new StackPanel()
            .Col(0)
            .Orientation(Orientation.Horizontal)
            .Spacing(4)
            .Children(
                CreatePodSelector(vm),
                CreateContainerSelector(vm),
                new TextBlock()
                    .MaxWidth(400)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(vm, x => x.ConnectionError)
                    .IsVisible(vm, x => x.ConnectionError, converter: HasErrorConverter)
                    .TextTrimming(TextTrimming.CharacterEllipsis));
    }

    private StackPanel CreateActionControls(PodLogsViewModel vm)
    {
        return new StackPanel()
            .Col(1)
            .Orientation(Orientation.Horizontal)
            .Spacing(2)
            .Children(
                new Button()
                    .Command(vm, x => x.ClearCommand)
                    .ToolTip_Tip(Assets.Resources.PodLogsView_Clear)
                    .Content(new FluentIcon().Icon(Icon.Broom)),
                new Button()
                    .Command(vm, x => x.DownloadLogsCommand)
                    .ToolTip_Tip(Assets.Resources.PodLogsView_Download)
                    .Content(new FluentIcon().Icon(Icon.Save)),
                new Button()
                    .Command(vm, x => x.FollowLogsCommand)
                    .IsEnabled(vm, x => x.CanFollowLogs)
                    .ToolTip_Tip(Assets.Resources.PodLogsView_FollowLogs)
                    .Content(new FluentIcon().Icon(Icon.ArrowDownload)),
                new Button()
                    .Command(vm, x => x.JumpToControlledByLogsCommand)
                    .IsVisible(vm, x => x.CanJumpToController)
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
                    .IsChecked(vm, x => x.ShowResourceNames, BindingMode.TwoWay)
                    .IsEnabled(vm, x => x.CanShowResourceNames)
                    .ToolTip_Tip(Assets.Resources.PodLogsView_ShowResourceNames)
                    .Content(new FluentIcon().Icon(Icon.Tag)),
                new ToggleButton()
                    .IsChecked(vm, x => x.WordWrap, BindingMode.TwoWay)
                    .ToolTip_Tip(Assets.Resources.PodLogsView_WordWrap)
                    .Content(new FluentIcon().Icon(Icon.TextWrap)));
    }

    private static MultiComboBox CreatePodSelector(PodLogsViewModel vm)
    {
        FuncDataTemplate<PodLogPodSelectionItem> template = new(
            (item, _) => new TextBlock().Text(item, x => x.DisplayName));

        return new MultiComboBox()
            .Width(SelectorWidth)
            .MaxHeight(20)
            .Margin(0, 0, 2, 0)
            .VerticalAlignment(VerticalAlignment.Center)
            .Classes("ClearButton")
            .IsVisible(vm, x => x.IsControllerScope)
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
            .Width(SelectorWidth)
            .MaxHeight(20)
            .Margin(0, 0, 2, 0)
            .VerticalAlignment(VerticalAlignment.Center)
            .Classes("ClearButton")
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
        behavior.FollowLogsRequested(vm, x => x.FollowLogsRequested);
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

}
