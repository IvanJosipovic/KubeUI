using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Ursa.Controls;

namespace KubeUI.Avalonia.Styles;

internal sealed class ToggleMultiComboBoxBehavior : Behavior<MultiComboBoxSelectedItemList>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PointerReleased += OnPointerReleased;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerReleased -= OnPointerReleased;
        }

        base.OnDetaching();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var multiComboBox = AssociatedObject?.FindAncestorOfType<MultiComboBox>();
        if (multiComboBox == null)
        {
            return;
        }

        multiComboBox.IsDropDownOpen = !multiComboBox.IsDropDownOpen;
    }
}
