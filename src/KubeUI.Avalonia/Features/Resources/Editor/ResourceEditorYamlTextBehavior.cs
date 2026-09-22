using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.TextMate;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Styles;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Features.Resources.Editor;

internal sealed class ResourceEditorYamlTextBehavior : Behavior<TextEditor>
{
    private static readonly IIndentationStrategy s_yamlIndentationStrategy = new YamlIndentationStrategy();
    private Installation? _textMateInstallation;
    private RegistryOptions? _registryOptions;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not null)
        {
            ConfigureEditor(AssociatedObject);
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.AddHandler(
                Control.RequestBringIntoViewEvent,
                OnRequestBringIntoView,
                RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            Application.Current!.ActualThemeVariantChanged += OnThemeChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.RemoveHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView);
            Application.Current!.ActualThemeVariantChanged -= OnThemeChanged;
        }
        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
        _registryOptions = null;
        base.OnDetaching();
    }

    private void ConfigureEditor(TextEditor editor)
    {
        editor.Options = new TextEditorOptions
        {
            ConvertTabsToSpaces = true,
            EnableEmailHyperlinks = false,
            EnableHyperlinks = false,
            HighlightCurrentLine = true,
            IndentationSize = 2,
            ShowBoxForControlCharacters = false,
        };
        editor.TextArea.IndentationStrategy = s_yamlIndentationStrategy;
        _registryOptions = new RegistryOptions(GetThemeName());
        _textMateInstallation = editor.InstallTextMate(_registryOptions, true);
        _textMateInstallation.SetGrammar(_registryOptions
            .GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (_textMateInstallation is null || _registryOptions is null)
            return;
        _textMateInstallation.SetTheme(_registryOptions.LoadTheme(GetThemeName()));
    }

    private static ThemeName GetThemeName()
        => Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus;

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceEditorNodeViewModel node)
            node.YamlValue = AssociatedObject.Text;
    }

    private static void OnRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }
}
