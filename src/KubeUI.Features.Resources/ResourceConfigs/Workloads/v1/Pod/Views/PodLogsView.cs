using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Styles;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Ursa.Controls;


namespace KubeUI.Features.Resources.Workloads.v1.Pod.Views;

/// <summary>Displays streamed pod logs with pod and container selection.</summary>
public sealed partial class PodLogsView : KubeUIView<PodLogsViewModel>
{
    private const double SourcesWidth = 300;
    private static readonly StringNotNullOrEmptyConverter HasErrorConverter = new();

    [RequiresUnreferencedCode("Declarative pod-log view construction may use members removed by trimming.")]
    protected override object Build(PodLogsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var root = new Grid();
        foreach (var keyBinding in CodeFontSizeKeyBindings.Create(vm.SettingsService))
        {
            root.KeyBindings.Add(keyBinding);
        }

        return root
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
            .Children(
                new ScrollViewer()
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Disabled)
                    .Content(CreateLogControlsBar(vm)));
    }

    private Grid CreateLogControlsBar(PodLogsViewModel vm)
    {
        return new Grid()
            .Row(0)
            .Height(32)
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
                            .TextTrimming(TextTrimming.CharacterEllipsis)),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .IsVisible(vm, x => x.IsDisplayPaused)
                    .Children(
                        new FluentIcon().Icon(Icon.Warning),
                        new TextBlock()
                            .MaxWidth(320)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.LogUpdateStatus)
                            .TextTrimming(TextTrimming.CharacterEllipsis),
                        new Button()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Command(vm, x => x.FollowLogsCommand)
                            .Content(ResourcesLanguageManager.Instance.PodLogsView_ResumeFollowing.ToBinding())));
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
        itemTheme.Setters.Add(new Setter(MinHeightProperty, 20d));
        itemTheme.Setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(0)));

        return new TreeComboBox
        {
            ItemContainerTheme = itemTheme,
        }
            .Width(SourcesWidth)
            .MaxHeight(32)
            .MaxDropDownHeight(500)
            .VerticalAlignment(VerticalAlignment.Center)
            .PlaceholderText(ResourcesLanguageManager.Instance.PodLogsView_Sources.ToBinding())
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
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_Clear.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.Broom)),
                new Button()
                    .Command(vm, x => x.DownloadLogsCommand)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_Download.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.Save)),
                new ToggleButton()
                    .IsChecked(vm, x => x.AutoScrollToBottom, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_FollowLogs.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.ArrowDownload)),
                new Button()
                    .Command(vm, x => x.JumpToControlledByLogsCommand)
                    .IsVisible(vm, x => x.CanJumpToController)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_Controller.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.ArrowUp)),
                new ToggleButton()
                    .IsChecked(vm, x => x.Previous, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_Previous.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.ArrowUndo)),
                new ToggleButton()
                    .IsChecked(vm, x => x.Timestamps, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_Timestamps.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.Timer)),
                new ToggleButton()
                    .IsChecked(vm, x => x.ShowResourceNames, BindingMode.TwoWay)
                    .IsEnabled(vm, x => x.CanShowResourceNames)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_ShowResourceNames.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.Tag)),
                new ToggleButton()
                    .IsChecked(vm, x => x.WordWrap, BindingMode.TwoWay)
                    .ToolTip_Tip(ResourcesLanguageManager.Instance.PodLogsView_WordWrap.ToBinding())
                    .Content(new FluentIcon().Icon(Icon.TextWrap)));
    }

    [RequiresUnreferencedCode("Calls KubeUI.Features.Resources.Workloads.v1.Pod.Views.PodLogsView.CreateContextMenu(TextEditor)")]
    private TextEditor CreateEditor(PodLogsViewModel vm)
    {
        PodLogsEditorBehavior behavior = new();
        behavior.AutoScrollToBottom(vm, x => x.AutoScrollToBottom);
        behavior.FollowLogsRequested(vm, x => x.FollowLogsRequested);
        behavior.ScrollOffset(vm, x => x.ScrollOffset);

        var editor = new TextEditor()
            .Row(1)
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
            });

        Interaction.GetBehaviors(editor).Add(behavior);

        editor.KeyBindings.Add(new KeyBinding
        {
            Command = new RelayCommand(editor.Copy),
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control)
        });

        editor.ContextMenu = CreateContextMenu(editor);
        return editor;
    }

    private static ContextMenu CreateContextMenu(TextEditor editor)
    {
        var item = new MenuItem()
            .Header(FoundationLanguageManager.Instance.Shared_Action_Copy.ToBinding())
            .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
            .Icon(new FluentIcon().Icon(Icon.Copy));

        item.Command = new RelayCommand(editor.Copy);

        return new ContextMenu().Items(item);
    }

}
