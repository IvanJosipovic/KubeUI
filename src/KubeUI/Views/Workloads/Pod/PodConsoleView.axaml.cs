using Avalonia.Controls;
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

        _textMateInstallation = Editor.InstallTextMate(_registryOptions);

        //Language yamlLanguage = _registryOptions.GetLanguageByExtension(".yaml");

        //_textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(yamlLanguage.Id));

        Editor.TextChanged += Editor_TextChanged;

        Editor.KeyUp += Editor_KeyUp;

        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
    }

    private void Editor_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (DataContext is PodConsoleViewModel dc)
        {
            dc.KeyUp(e);
        }
    }

    private void Editor_TextChanged(object? sender, System.EventArgs e)
    {
        Editor.ScrollToEnd();
    }
}
