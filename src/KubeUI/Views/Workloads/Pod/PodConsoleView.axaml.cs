using Avalonia.Input;
using AvaloniaEdit.TextMate;
using KubeUI.ViewModels;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public partial class PodConsoleView : UserControl
{
    private readonly Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public PodConsoleView()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        _textMateInstallation = Editor.InstallTextMate(_registryOptions, false);

        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
    }

    private async void Editor_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers is KeyModifiers.Control)
        {
            if (e.Key == Key.V && DataContext is PodConsoleViewModel dc)
            {
                await dc.Paste();
                return;
            }
        }

        if (DataContext is PodConsoleViewModel dc1)
        {
            dc1.Send(e.KeySymbol);
        }
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        Editor.ScrollToEnd();
    }
}
