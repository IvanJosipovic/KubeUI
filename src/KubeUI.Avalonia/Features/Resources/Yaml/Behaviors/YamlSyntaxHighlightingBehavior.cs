using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

/// <summary>Applies theme-aware TextMate YAML syntax highlighting to an editor.</summary>
public sealed class YamlSyntaxHighlightingBehavior : Behavior<TextEditor>
{
    private TextMate.Installation? _installation;
    private RegistryOptions? _registry;

    internal bool IsInstalled => _installation is not null;

    protected override void OnAttached()
    {
        base.OnAttached();
        _registry = new RegistryOptions(GetTheme());
        _installation = AssociatedObject.InstallTextMate(_registry, true);
        _installation.SetGrammar(_registry.GetScopeByLanguageId(_registry.GetLanguageByExtension(".yaml").Id));
        Application.Current!.ActualThemeVariantChanged += ThemeChanged;
    }

    protected override void OnDetaching()
    {
        Application.Current!.ActualThemeVariantChanged -= ThemeChanged;
        _installation!.Dispose();
        _installation = null;
        _registry = null;
        base.OnDetaching();
    }

    private static ThemeName GetTheme()
        => Application.Current!.ActualThemeVariant == ThemeVariant.Light ? ThemeName.Light : ThemeName.DarkPlus;

    private void ThemeChanged(object? sender, EventArgs args)
        => _installation!.SetTheme(_registry!.LoadTheme(GetTheme()));
}
