using Avalonia.Styling;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

public sealed partial class PodLogsView : UserControl
{
    private readonly Installation _textMateInstallation;
    private readonly RegistryOptions _registryOptions;

    public PodLogsViewModel? ViewModel => DataContext as PodLogsViewModel;

    public PodLogsView()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        _textMateInstallation = TextEditorControl.InstallTextMate(_registryOptions, false);

        ApplyThemeVariant();

        Application.Current.ActualThemeVariantChanged += Current_ActualThemeVariantChanged;
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

    private void ApplyThemeVariant()
    {
        if (_textMateInstallation is null)
            return;

        _textMateInstallation.SetTheme(Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? _registryOptions.LoadTheme(ThemeName.Light)
            : _registryOptions.LoadTheme(ThemeName.DarkPlus));
    }

    public void SetOffset()
    {
        if (ViewModel != null && TextEditorControl?.GetScrollViewer() is ScrollViewer sc)
            sc.Offset = ViewModel.ScrollOffset;
    }

    public void GetOffset()
    {
        ViewModel?.ScrollOffset = new Vector(TextEditorControl.HorizontalOffset, TextEditorControl.VerticalOffset);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetOffset();
    }

    private void CopyMenuItem_Click(object? sender, RoutedEventArgs e) => TextEditorControl?.Copy();

    private void TextEditorControl_TextChanged(object? sender, System.EventArgs e)
    {
        if (ViewModel?.AutoScrollToBottom == true)
        {
            var sc = TextEditorControl.GetScrollViewer();
            sc?.Offset = new Vector(sc.Offset.X, sc.ScrollBarMaximum.Y);
        }
    }
}


