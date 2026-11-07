using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Controls.Editors.Behaviors;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class YamlEditorBehaviorTests
{
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
