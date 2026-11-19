using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class InteractionExtensionsTests
{
    [AvaloniaFact]
    public void Behaviors_accumulates_and_returns_the_control_for_chaining()
    {
        var control = new Border();
        var first = new TestBehavior();
        var second = new TestBehavior();

        control.Behaviors(first).ShouldBeSameAs(control);
        control.Behaviors(second);

        Interaction.GetBehaviors(control).ShouldBe([first, second]);
    }

    [AvaloniaFact]
    public void Behaviors_accepts_ibehaviors_that_are_not_behaviors()
    {
        var control = new Border();
        var trigger = new EventTriggerBehavior { EventName = nameof(Border.Loaded) };

        control.Behaviors(new TestBehavior(), trigger);

        Interaction.GetBehaviors(control).ShouldContain(trigger);
    }

    private sealed class TestBehavior : Behavior
    {
    }
}
