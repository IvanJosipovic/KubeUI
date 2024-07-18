using AvaloniaEdit.TextMate;
using KubeUI.ViewModels;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public partial class PodLogsView : UserControl
{
    private readonly Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public PodLogsView()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        _textMateInstallation = Editor.InstallTextMate(_registryOptions, false);

        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if ((DataContext as PodLogsViewModel)?.AutoScrollToBottom == true)
            Editor.ScrollToEnd();
    }
}
