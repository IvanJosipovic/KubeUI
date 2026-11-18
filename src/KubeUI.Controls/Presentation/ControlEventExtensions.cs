using Avalonia.Controls;

namespace KubeUI.Controls.Presentation;

/// <summary>
/// Fluent event subscriptions that the markup-extension generator does not already provide. Prefer the generated
/// <c>On*</c> extensions where they exist; add a method here only for an event or control type the generator misses.
/// </summary>
public static class ControlEventExtensions
{
    /// <summary>
    /// Subscribes to <see cref="TreeView.SelectionChanged"/>. The generator emits selection extensions only for specific
    /// control types, and <see cref="TreeView"/> derives from <see cref="ItemsControl"/> rather than
    /// <c>SelectingItemsControl</c>, so it needs this overload.
    /// </summary>
    /// <typeparam name="T">The control type, returned for chaining.</typeparam>
    /// <param name="control">The control to subscribe.</param>
    /// <param name="handler">The handler to run when the selection changes.</param>
    /// <returns>The same control, for chaining.</returns>
    public static T OnSelectionChanged<T>(this T control, EventHandler<SelectionChangedEventArgs> handler)
        where T : TreeView
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(handler);
        control.SelectionChanged += handler;
        return control;
    }
}
