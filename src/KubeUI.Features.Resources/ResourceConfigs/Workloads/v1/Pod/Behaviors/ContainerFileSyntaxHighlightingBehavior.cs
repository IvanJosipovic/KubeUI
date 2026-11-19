using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;

/// <summary>Applies theme-aware TextMate highlighting chosen from a container file's extension.</summary>
internal sealed class ContainerFileSyntaxHighlightingBehavior(string? fileName) : Behavior<TextEditor>
{
    private TextMate.Installation? _installation;
    private RegistryOptions? _registry;
    private IRawTheme? _lightTheme;
    private IRawTheme? _darkTheme;

    /// <summary>Whether TextMate is attached to the editor.</summary>
    internal bool IsInstalled => _installation is not null;

    /// <summary>The TextMate scope chosen for the file extension, or null when it has no known grammar.</summary>
    internal string? Scope { get; private set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        _registry = new RegistryOptions(GetTheme());
        _lightTheme = _registry.LoadTheme(ThemeName.Light);
        _darkTheme = _registry.LoadTheme(ThemeName.DarkPlus);
        _installation = AssociatedObject.InstallTextMate(_registry, true);
        _installation.SetTheme(GetThemeObject());

        // Extensions that TextMate does not know stay plain text instead of guessing a grammar.
        var extension = Path.GetExtension(fileName ?? string.Empty);
        var language = extension.Length == 0 ? null : _registry.GetLanguageByExtension(extension);
        Scope = language is null ? null : _registry.GetScopeByLanguageId(language.Id);
        if (!string.IsNullOrEmpty(Scope))
        {
            _installation.SetGrammar(Scope);
        }

        Application.Current!.ActualThemeVariantChanged += ThemeChanged;
    }

    protected override void OnDetaching()
    {
        Application.Current!.ActualThemeVariantChanged -= ThemeChanged;
        _installation?.Dispose();
        _installation = null;
        _registry = null;
        _lightTheme = null;
        _darkTheme = null;
        base.OnDetaching();
    }

    private static ThemeName GetTheme()
        => Application.Current!.ActualThemeVariant == ThemeVariant.Light ? ThemeName.Light : ThemeName.DarkPlus;

    private IRawTheme GetThemeObject()
        => GetTheme() == ThemeName.Light ? _lightTheme! : _darkTheme!;

    private void ThemeChanged(object? sender, EventArgs args)
        => _installation?.SetTheme(GetThemeObject());
}
