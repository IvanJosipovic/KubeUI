using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFileSyntaxHighlightingBehaviorTests
{
    [AvaloniaFact]
    public void A_known_extension_selects_its_textmate_scope()
    {
        var editor = new TextEditor();
        var behavior = new ContainerFileSyntaxHighlightingBehavior("/app/config.yaml");
        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(behavior);
        var window = new Window { Content = editor, Width = 500, Height = 300 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            behavior.IsInstalled.ShouldBeTrue();
            behavior.Scope.ShouldNotBeNullOrEmpty();
            behavior.Scope.ShouldContain("yaml");

            behaviors.Remove(behavior);
            behavior.IsInstalled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void An_unknown_extension_stays_plain_text()
    {
        var editor = new TextEditor();
        var behavior = new ContainerFileSyntaxHighlightingBehavior("/app/file.unknownextension");
        Interaction.GetBehaviors(editor).Add(behavior);
        var window = new Window { Content = editor, Width = 500, Height = 300 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            behavior.IsInstalled.ShouldBeTrue();
            behavior.Scope.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }
}
