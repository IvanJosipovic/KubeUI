using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

internal static class SelectionKeyBindingFactory
{
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
