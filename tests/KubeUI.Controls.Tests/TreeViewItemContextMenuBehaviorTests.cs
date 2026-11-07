using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Controls.Behaviors;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class TreeViewItemContextMenuBehaviorTests
{
    [AvaloniaFact]
    public void Right_click_on_row_without_menu_remains_unselected()
    {
        var tree = new TreeView { ItemsSource = new[] { "row" } };
        tree.DataTemplates.Add(new FuncDataTemplate<string>((row, _) => new TextBlock { Text = row }, true));
        Interaction.GetBehaviors(tree).Add(new TreeViewItemContextMenuBehavior());
        var window = new Window { Content = tree, Width = 300, Height = 180 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var text = tree.GetVisualDescendants().OfType<TextBlock>().Single();
            var point = text.TranslatePoint(new Point(text.Bounds.Width / 2, text.Bounds.Height / 2), window)
                ?? throw new InvalidOperationException("Tree row has no window position.");
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();

            tree.SelectedItem.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }
}
