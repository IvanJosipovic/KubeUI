using Avalonia.Xaml.Interactivity;

namespace KubeUI.Controls.Presentation;

/// <summary>
/// Fluent helpers for attaching Xaml.Behaviors to a control. The markup-extension generator emits setters for
/// ordinary styled properties but not for attached properties such as
/// <see cref="Interaction.BehaviorsProperty"/>, so attaching a behavior needs an explicit extension.
/// </summary>
public static class InteractionExtensions
{
    /// <summary>Attaches one or more behaviors to a control.</summary>
    /// <typeparam name="T">The control type, returned for chaining.</typeparam>
    /// <param name="control">The control to attach the behaviors to.</param>
    /// <param name="behaviors">The behaviors to attach, in order.</param>
    /// <returns>The same control, for chaining.</returns>
    /// <remarks>
    /// The existing collection is preserved and extended, so repeated calls accumulate behaviors rather than
    /// replacing them. Prefer this over <c>Interaction.GetBehaviors(control).Add(...)</c>.
    /// </remarks>
    public static T Behaviors<T>(this T control, params IBehavior[] behaviors)
        where T : AvaloniaObject
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(behaviors);

        var collection = Interaction.GetBehaviors(control);
        foreach (var behavior in behaviors)
        {
            ArgumentNullException.ThrowIfNull(behavior);

            // BehaviorCollection stores AvaloniaObjects; Xaml.Behaviors' behaviors are controls, but
            // the interface itself does not promise that, so fail clearly for an alien implementation.
            if (behavior is not AvaloniaObject element)
            {
                throw new ArgumentException(
                    $"Behavior '{behavior.GetType().Name}' is not an AvaloniaObject.",
                    nameof(behaviors));
            }

            collection.Add(element);
        }

        return control;
    }
}
