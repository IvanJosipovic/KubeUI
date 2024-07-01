using System;
using Avalonia.Controls;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public partial class ResourceYamlView : UserControl
{
    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public ResourceYamlView()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(ThemeName.Dark);

        _textMateInstallation = Editor.InstallTextMate(_registryOptions, false);

        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));

        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
    }
}
