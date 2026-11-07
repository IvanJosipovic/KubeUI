using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace KubeUI.Foundation.Styles;

/// <summary>Provides dynamic, domain-neutral success, error, and muted brushes for light and dark themes.</summary>
public sealed class SemanticThemeStyles : global::Avalonia.Styling.Styles
{
    /// <summary>Creates shared semantic brushes without requiring App or feature resources.</summary>
    public SemanticThemeStyles()
    {
        Resources = new ResourceDictionary
        {
            ThemeDictionaries =
            {
                [ThemeVariant.Light] = new ResourceDictionary
                {
                    ["SemanticSuccessBrush"] = Brush("#2E7D32"),
                    ["SemanticErrorBrush"] = Brush("#C62828"),
                    ["SemanticMutedBrush"] = Brush("#757575")
                },
                [ThemeVariant.Dark] = new ResourceDictionary
                {
                    ["SemanticSuccessBrush"] = Brush("#81C784"),
                    ["SemanticErrorBrush"] = Brush("#FF6B6B"),
                    ["SemanticMutedBrush"] = Brush("#9E9E9E")
                }
            }
        };
    }

    private static SolidColorBrush Brush(string value) => new(Color.Parse(value));
}
