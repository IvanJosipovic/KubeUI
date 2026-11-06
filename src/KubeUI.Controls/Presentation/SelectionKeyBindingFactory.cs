using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace KubeUI.Controls.Presentation;

/// <summary>Creates key bindings that execute commands for the current selection.</summary>
public static class SelectionKeyBindingFactory
{
    /// <summary>Creates a key binding that reads its command parameter when executed.</summary>
    /// <param name="key">Key that activates the binding.</param>
    /// <param name="command">Command to execute when it accepts the selected item.</param>
    /// <param name="getCommandParameter">Gets the current command parameter.</param>
    /// <returns>A key binding for the selected item.</returns>
    public static KeyBinding Create(Key key, ICommand command, Func<object?> getCommandParameter)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(getCommandParameter);

        return new KeyBinding
        {
            Gesture = new KeyGesture(key),
            Command = new RelayCommand(
                () =>
                {
                    var parameter = getCommandParameter();
                    if (parameter is not null && command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                    }
                },
                () =>
                {
                    var parameter = getCommandParameter();
                    return parameter is not null && command.CanExecute(parameter);
                })
        };
    }
}
