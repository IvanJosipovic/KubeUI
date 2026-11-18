using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.App.Tests.Infra;
using KubeUI.Controls.Layout;
using Shouldly;

namespace KubeUI.App.Tests.Controls.Layout;

public sealed class CompactToolbarTests
{
    [AvaloniaTheory]
    [InlineData(32)]
    [InlineData(64)]
    public void Labels_in_all_slots_center_their_text_vertically(double rowHeight)
    {
        var leading = new Label { Content = "Leading" };
        var middle = new Label { Content = "Middle" };
        var trailing = new Label { Content = "Trailing" };
        var toolbar = new CompactToolbar
        {
            VerticalAlignment = VerticalAlignment.Top,
            Leading = CompactToolbar.Group(leading, new Button { Content = "Action", Height = rowHeight }),
            Middle = middle,
            Trailing = CompactToolbar.Group(trailing)
        };
        using var window = Application.Current.CreateTestWindow(600, 300, toolbar);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        foreach (var label in new[] { leading, middle, trailing })
        {
            var text = label.GetVisualDescendants().OfType<TextBlock>().Single();
            text.Bounds.Height.ShouldBe(text.DesiredSize.Height, 1d);
            var textTop = text.TranslatePoint(default, toolbar)!.Value.Y;
            var textCenter = textTop + text.Bounds.Height / 2;
            Math.Abs(toolbar.Bounds.Height / 2 - textCenter).ShouldBeLessThanOrEqualTo(1d);
        }
    }
}
