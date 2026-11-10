using Avalonia.Headless.XUnit;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.App.Tests.Infrastructure;

public sealed class SelectionKeyBindingFactoryTests
{
    [AvaloniaFact]
    public void Binding_executes_for_the_current_selection_and_rechecks_command_eligibility()
    {
        var selectedItem = (string?)"first";
        var executedItem = (string?)null;
        var command = new RelayCommand<string?>(item => executedItem = item, item => item == "first");

        var binding = SelectionKeyBindingFactory.Create(Key.Enter, command, () => selectedItem);

        binding.Gesture.ShouldBe(new KeyGesture(Key.Enter));
        binding.Command.ShouldNotBeNull();
        binding.Command.CanExecute(null).ShouldBeTrue();
        binding.Command.Execute(null);
        executedItem.ShouldBe("first");

        selectedItem = "blocked";
        binding.Command.CanExecute(null).ShouldBeFalse();
        binding.Command.Execute(null);
        executedItem.ShouldBe("first");

        selectedItem = null;
        binding.Command.CanExecute(null).ShouldBeFalse();
        binding.Command.Execute(null);
        executedItem.ShouldBe("first");
    }
}
