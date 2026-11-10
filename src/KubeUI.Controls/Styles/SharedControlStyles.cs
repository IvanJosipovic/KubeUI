using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Ursa.Controls;
using AvaloniaStyles = Avalonia.Styling.Styles;

namespace KubeUI.Controls.Styles;

/// <summary>Provides opt-in shared control styling and interaction resources without App dependencies.</summary>
/// <remarks>
/// Hosts that use shared controls should add this style provider after their control-library theme.
/// Application-specific palettes and feature styles remain the host's responsibility.
/// </remarks>
public sealed class SharedControlStyles : AvaloniaStyles
{
    /// <summary>Creates the shared Ursa multi-select styles used by namespace-scope pickers.</summary>
    public SharedControlStyles()
    {
        Add(new Style<MultiComboBoxItem>()
            .Setter(TemplatedControl.PaddingProperty, new Thickness(4, 2, 4, 2))
            .Setter(Layoutable.WidthProperty, 200d)
            .Setter(Layoutable.MinHeightProperty, 22d));

        Add(new Style<MultiComboBoxSelectedItemList>()
            .Setter(Interaction.BehaviorsProperty,
                new BehaviorCollectionTemplate
                {
                    Content = (IServiceProvider? _) =>
                        new TemplateResult<BehaviorCollection>(
                            [new ToggleMultiComboBoxBehavior()],
                            new NameScope())
                }));

        Add(new Style<TextEditor>()
            .Background(new DynamicResourceExtension("SystemAltHighColor")));
    }
}
