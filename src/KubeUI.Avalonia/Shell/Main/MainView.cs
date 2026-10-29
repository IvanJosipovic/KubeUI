using System.Windows.Input;
using Avalonia.Input;
using Dock.Avalonia.Controls;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Presentation;


namespace KubeUI.Avalonia.Shell.Main;

public sealed class MainView : KubeUIView<MainViewModel>
{
    public MainView()
    {
        if (DesignTimePreview.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<MainViewModel>();
        }
    }

    protected override object Build(MainViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Background(Brushes.Transparent)
            .KeyBindings(
            CreateFontSizeBinding(Key.OemPlus, KeyModifiers.Control, vm.IncreaseFontSizeCommand),
            CreateFontSizeBinding(Key.Add, KeyModifiers.Control, vm.IncreaseFontSizeCommand),
            CreateFontSizeBinding(Key.OemMinus, KeyModifiers.Control, vm.DecreaseFontSizeCommand),
            CreateFontSizeBinding(Key.Subtract, KeyModifiers.Control, vm.DecreaseFontSizeCommand))
            .Children(
                new DockControl()
                    .Col(0)
                    .Margin(4)
                    .Factory(vm, vm => vm.Factory)
                    .Layout(vm, vm => vm.Layout));
    }

    private static KeyBinding CreateFontSizeBinding(Key key, KeyModifiers modifiers, ICommand command)
    {
        return new KeyBinding
        {
            Gesture = new KeyGesture(key, modifiers),
            Command = command
        };
    }
}
