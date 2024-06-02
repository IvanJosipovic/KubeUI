using Avalonia.Controls;
using AvaloniaEdit.TextMate;
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

        _textMateInstallation = Editor.InstallTextMate(_registryOptions);

        //Language yamlLanguage = _registryOptions.GetLanguageByExtension(".yaml");

        //_textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(yamlLanguage.Id));

        Editor.TextChanged += Editor_TextChanged;

        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
    }

    private void Editor_TextChanged(object? sender, System.EventArgs e)
    {
        Editor.ScrollToEnd();
    }
}
