using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.TextMate;
using KubeUI.Avalonia.Features.Resources.Yaml;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Features.Resources.Editor;

internal sealed class ResourceEditorYamlTextBehavior : Behavior<TextEditor>
{
    private static readonly IIndentationStrategy s_yamlIndentationStrategy = new YamlIndentationStrategy();
    private Installation? _textMateInstallation;
    private RegistryOptions? _registryOptions;
    private Application? _application;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not null)
        {
            _application = Application.Current;
            ConfigureEditor(AssociatedObject);
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.AddHandler(
                Control.RequestBringIntoViewEvent,
                OnRequestBringIntoView,
                RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            if (_application is not null)
                _application.ActualThemeVariantChanged += OnThemeChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.RemoveHandler(Control.RequestBringIntoViewEvent, OnRequestBringIntoView);
        }
        if (_application is not null)
        {
            _application.ActualThemeVariantChanged -= OnThemeChanged;
            _application = null;
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

    private ThemeName GetThemeName()
        => _application?.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus;

    private static void OnRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceEditorNodeViewModel node)
            node.YamlValue = AssociatedObject.Text;
    }
}
