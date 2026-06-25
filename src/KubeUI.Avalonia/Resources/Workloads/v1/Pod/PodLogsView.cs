using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using AvaloniaEdit;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class PodLogsView : ViewBase<PodLogsViewModel>
{
    private Installation? _textMateInstallation;
    private readonly RegistryOptions _registryOptions;

    private TextEditor? _textEditorControl;

    public PodLogsView()
    {
        _registryOptions = new RegistryOptions(Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        Application.Current.ActualThemeVariantChanged += Current_ActualThemeVariantChanged;
    }

    protected override object Build(PodLogsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var textEditor = new TextEditor()
            .Row(1)
            .OnTextChanged((e) => TextEditorControl_TextChanged(e))
            .Background(Brushes.Transparent)
            .Document(vm, x => x.Logs)
            .FontSize(vm, x => (double)x.SettingsService.Appearance.FontSize)
            .WordWrap(vm, x => x.WordWrap)
            .FontFamily(new FontFamily("Cascadia Mono"))
            .FontWeight(FontWeight.Normal)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .IsReadOnly(true)
            .ShowLineNumbers(false)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .ContextMenu(new ContextMenu()
                .Items(
                    new MenuItem()
                        .Header(Assets.Resources.Shared_Action_Copy)
                        .HotKey(new KeyGesture(Key.C, KeyModifiers.Control))
                        .Icon(new FluentIcon().Icon(Icon.Copy))
                        .OnClick((_) => _textEditorControl?.Copy())));

        _textEditorControl = textEditor;

        EnsureTextMateInstallation();
        ApplyThemeVariant();

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
                            .Content($"{vm.Object?.Metadata?.NamespaceProperty}/{vm.Object?.Metadata?.Name}/{vm.ContainerName}"),
                        new Button()
                            .Command(vm, x => x.ClearCommand)
                            .ToolTip_Tip(Assets.Resources.PodLogsView_Clear)
                            .Content(new FluentIcon().Icon(Icon.Delete)),
                        new ToggleButton()
                            .IsChecked(vm, x => x.Previous, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.PodLogsView_Previous)
                            .Content(new FluentIcon().Icon(Icon.ArrowUndo)),
                        new ToggleButton()
                            .IsChecked(vm, x => x.Timestamps, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.PodLogsView_Timestamps)
                            .Content(new FluentIcon().Icon(Icon.Timer)),
                        new ToggleButton()
                            .IsChecked(vm, x => x.AutoScrollToBottom, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.PodLogsView_AutoScrollToBottom)
                            .Content(new FluentIcon().Icon(Icon.PaddingDown)),
                        new ToggleButton()
                            .IsChecked(vm, x => x.WordWrap, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.PodLogsView_WordWrap)
                            .Content(new FluentIcon().Icon(Icon.TextWrap))),
                textEditor);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetOffset();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        GetOffset();
        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }

    private void Current_ActualThemeVariantChanged(object? sender, EventArgs e) => ApplyThemeVariant();

    private void EnsureTextMateInstallation()
    {
        if (_textEditorControl == null || _textMateInstallation != null)
        {
            return;
        }

        _textMateInstallation = _textEditorControl.InstallTextMate(_registryOptions, false);
    }

    private void ApplyThemeVariant()
    {
        if (_textMateInstallation is null)
        {
            return;
        }

        _textMateInstallation.SetTheme(Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? _registryOptions.LoadTheme(ThemeName.Light)
            : _registryOptions.LoadTheme(ThemeName.DarkPlus));
    }

    public void SetOffset()
    {
        if (ViewModel != null && _textEditorControl?.GetScrollViewer() is ScrollViewer sc)
        {
            sc.Offset = ViewModel.ScrollOffset;
        }
    }

    public void GetOffset()
    {
        if (ViewModel != null && _textEditorControl != null)
        {
            ViewModel.ScrollOffset = new Vector(_textEditorControl.HorizontalOffset, _textEditorControl.VerticalOffset);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetOffset();
    }

    private void TextEditorControl_TextChanged(EventArgs e)
    {
        if (ViewModel?.AutoScrollToBottom == true)
        {
            var sc = _textEditorControl?.GetScrollViewer();
            sc?.Offset = new Vector(sc.Offset.X, sc.ScrollBarMaximum.Y);
        }
    }

}
