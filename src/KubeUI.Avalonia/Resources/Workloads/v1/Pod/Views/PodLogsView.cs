using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
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
    private const double SourcesWidth = 300;
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
            .Rows("Auto")
            .Children(CreateLogControlsBar(vm));
    }

    private Grid CreateLogControlsBar(PodLogsViewModel vm)
    {
        return new Grid()
            .Row(0)
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
               CreateSourcesSelector(vm),
               new TextBlock()
                    .MaxWidth(400)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(vm, x => x.ConnectionError)
                    .IsVisible(vm, x => x.ConnectionError, converter: HasErrorConverter)
                    .TextTrimming(TextTrimming.CharacterEllipsis),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .IsVisible(vm, x => x.StreamLimitWarning, converter: HasErrorConverter)
                    .Children(
                        new FluentIcon().Icon(Icon.Warning),
                        new TextBlock()
                            .MaxWidth(320)
                            .Text(vm, x => x.StreamLimitWarning)
                            .TextTrimming(TextTrimming.CharacterEllipsis)));
    }

    private static TreeComboBox CreateSourcesSelector(PodLogsViewModel vm)
    {
        FuncTreeDataTemplate<PodLogSourceTreeNode> template = new(
            (node, _) => new CheckBox()
                .VerticalAlignment(VerticalAlignment.Center)
                .Content(node.DisplayName)
                .IsChecked(node, x => x.IsChecked, BindingMode.TwoWay),
            node => node.Children);

        ControlTheme itemTheme = new(typeof(TreeComboBoxItem))
        {
            BasedOn = Application.Current?.FindResource(typeof(TreeComboBoxItem)) as ControlTheme,
        };
        itemTheme.Setters.Add(new Setter(Layoutable.MinHeightProperty, 20d));
        itemTheme.Setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(0)));

        return new TreeComboBox
        {
            ItemContainerTheme = itemTheme,
        }
            .Width(SourcesWidth)
            .MaxHeight(32)
            .MaxDropDownHeight(500)
            .VerticalAlignment(VerticalAlignment.Center)
            .PlaceholderText(Assets.Resources.PodLogsView_Sources)
            .ItemsSource(vm, x => x.SourceTreeItems)
            .ItemTemplate(template)
            .Styles(
                new Style<TreeComboBoxItem>()
                    .Setter(TreeComboBoxItem.IsExpandedProperty, true)
                    .Setter(TreeComboBoxItem.IsSelectableProperty, false));
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
                new ToggleButton()
                    .IsChecked(vm, x => x.AutoScrollToBottom, BindingMode.TwoWay)
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
