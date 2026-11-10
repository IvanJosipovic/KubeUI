using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using KubeUI.Controls.Editors.Behaviors;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class YamlEditorBehaviorTests
{
    /// <summary>Verifies detaching the behavior releases its folding markers.</summary>
    [AvaloniaFact]
    public void Detaching_folding_behavior_releases_retained_marker_children()
    {
        var editor = new TextEditor();
        var behavior = new YamlFoldingBehavior();
        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(behavior);

        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        var logicalChildren = ((ILogical)margin).LogicalChildren;
        var mutableChildren = (IList)logicalChildren;
        for (var index = 0; index < 100; index++)
        {
            mutableChildren.Add(new Button());
        }

        mutableChildren.Count.ShouldBe(100);
        behaviors.Remove(behavior);
        mutableChildren.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Shared_yaml_behaviors_install_theme_and_fold_support_without_app_then_cleanup()
    {
        var editor = new TextEditor { Text = "root:\n  child: value\n" };
        var folding = new YamlFoldingBehavior();
        var highlighting = new YamlSyntaxHighlightingBehavior();
        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(folding);
        behaviors.Add(highlighting);
        var window = new Window { Content = editor, Width = 500, Height = 300 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            folding.IsInstalled.ShouldBeTrue();
            highlighting.IsInstalled.ShouldBeTrue();

            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            highlighting.IsInstalled.ShouldBeTrue();

            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();
            highlighting.IsInstalled.ShouldBeTrue();

            behaviors.Clear();
            folding.IsInstalled.ShouldBeFalse();
            highlighting.IsInstalled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }
}
