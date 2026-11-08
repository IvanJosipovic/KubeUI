using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Controls.Layout;
using Shouldly;

namespace KubeUI.Controls.Tests.Controls.Layout;

public sealed class CompactToolbarTests
{
    [AvaloniaFact]
    public void Slots_keep_compact_spacing_and_right_align_trailing_content()
    {
        var leading = new Button { Width = 24, Height = 24, Content = "+" };
        var count = new TextBlock { Width = 60, Height = 20, Text = "3 items" };
        var trailing = new Button { Width = 80, Height = 24, Content = "Refresh" };
        var toolbar = new CompactToolbar { Leading = leading, Count = count, Trailing = trailing };
        using var host = new ToolbarHost(toolbar, 600);

        toolbar.MinHeight.ShouldBe(32);
        toolbar.Margin.ShouldBe(new Thickness(0, 2));
        leading.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(2);
        count.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(28);
        trailing.TranslatePoint(new Point(trailing.Bounds.Width, 0), toolbar)!.Value.X.ShouldBe(600);

        var screenshotPath = TestScreenshot.CaptureIfEnabled(host.Window, "CompactToolbar_600px");
        if (IsScreenshotCaptureEnabled())
        {
            screenshotPath.ShouldNotBeNull();
            File.Exists(screenshotPath).ShouldBeTrue();
        }
        else
        {
            screenshotPath.ShouldBeNull();
        }

        trailing.Width = 40;
        host.Layout();

        trailing.TranslatePoint(new Point(trailing.Bounds.Width, 0), toolbar)!.Value.X.ShouldBe(600);
    }

    private static bool IsScreenshotCaptureEnabled()
    {
        var value = Environment.GetEnvironmentVariable(TestScreenshot.EnableEnvironmentVariable);
        return value is not null && (value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public void Slots_can_be_replaced_and_cleared_without_retaining_old_controls()
    {
        var oldLeading = new Border();
        var oldCount = new Border();
        var oldTrailing = new Border();
        var toolbar = new CompactToolbar { Leading = oldLeading, Count = oldCount, Trailing = oldTrailing };
        using var host = new ToolbarHost(toolbar, 400);
        var leading = new Border();
        var count = new Border();
        var trailing = new Border();

        toolbar.Leading = leading;
        toolbar.Count = count;
        toolbar.Trailing = trailing;
        host.Layout();

        oldLeading.Parent.ShouldBeNull();
        oldCount.Parent.ShouldBeNull();
        oldTrailing.Parent.ShouldBeNull();
        toolbar.GetVisualDescendants().ShouldContain(leading);
        toolbar.GetVisualDescendants().ShouldContain(count);
        toolbar.GetVisualDescendants().ShouldContain(trailing);

        toolbar.Leading = null;
        toolbar.Count = null;
        toolbar.Trailing = null;
        host.Layout();
        leading.Parent.ShouldBeNull();
        count.Parent.ShouldBeNull();
        trailing.Parent.ShouldBeNull();
    }

    [AvaloniaFact]
    public void Detached_toolbar_accepts_slot_updates_and_lays_out_again_on_reattachment()
    {
        var leading = new Border { Width = 24, Height = 24 };
        var oldCount = new Border { Width = 60, Height = 20 };
        var trailing = new Border { Width = 200, Height = 24 };
        var toolbar = new CompactToolbar { Leading = leading, Count = oldCount, Trailing = trailing };
        using var host = new ToolbarHost(toolbar, 180);

        host.Detach();
        TopLevel.GetTopLevel(toolbar).ShouldBeNull();
        var count = new Border { Width = 40, Height = 20 };
        toolbar.Count = count;
        host.Attach(toolbar);

        TopLevel.GetTopLevel(toolbar).ShouldNotBeNull();
        oldCount.Parent.ShouldBeNull();
        count.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(28);
        toolbar.GetVisualDescendants().ShouldContain(leading);
        toolbar.GetVisualDescendants().ShouldContain(trailing);
        var scroll = toolbar.GetVisualDescendants().OfType<ScrollViewer>().Single();
        scroll.Viewport.Width.ShouldBeGreaterThan(0);
        scroll.Extent.Width.ShouldBeGreaterThan(scroll.Viewport.Width);
    }

    [AvaloniaFact]
    public void Hidden_leading_action_releases_its_space()
    {
        var leading = new Border { Width = 24, Height = 24 };
        var count = new Border { Width = 60, Height = 20 };
        var toolbar = new CompactToolbar { Leading = leading, Count = count };
        using var host = new ToolbarHost(toolbar, 300);

        leading.IsVisible = false;
        host.Layout();

        count.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(2);
        leading.IsVisible = true;
        host.Layout();
        count.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(28);
    }

    [AvaloniaFact]
    public void Narrow_toolbar_scrolls_trailing_actions_and_keeps_focus_and_accessible_names()
    {
        var first = new Button { Width = 100, Content = "First" };
        var last = new Button { Width = 80, Content = "Last" };
        AutomationProperties.SetName(last, "Refresh items");
        var toolbar = new CompactToolbar
        {
            Leading = new Border { Width = 24, Height = 24 },
            Count = new Border { Width = 60, Height = 20 },
            Trailing = new StackPanel { Orientation = Orientation.Horizontal, Children = { first, last } }
        };
        using var host = new ToolbarHost(toolbar, 180);
        var scroll = toolbar.GetVisualDescendants().OfType<ScrollViewer>().Single();

        scroll.Viewport.Width.ShouldBeGreaterThan(0);
        scroll.Extent.Width.ShouldBeGreaterThan(scroll.Viewport.Width);
        last.Bounds.Width.ShouldBeLessThanOrEqualTo(scroll.Viewport.Width);
        last.Focus().ShouldBeTrue();
        last.BringIntoView();
        host.Layout();

        last.IsFocused.ShouldBeTrue();
        AutomationProperties.GetName(last).ShouldBe("Refresh items");
        scroll.Offset.X.ShouldBeGreaterThan(0);
        last.TranslatePoint(default, scroll)!.Value.X.ShouldBeGreaterThanOrEqualTo(0);
        last.TranslatePoint(new Point(last.Bounds.Width, 0), scroll)!.Value.X.ShouldBeLessThanOrEqualTo(scroll.Bounds.Width);
    }

    [AvaloniaFact]
    public void Very_narrow_toolbar_places_actions_on_another_row_and_wraps_count_text()
    {
        var count = new TextBlock { Text = "Many matching items", TextWrapping = TextWrapping.Wrap };
        var trailing = new Border { Width = 200, Height = 24 };
        var toolbar = new CompactToolbar
        {
            Leading = new Border { Width = 24, Height = 24 }, Count = count, Trailing = trailing
        };
        using var host = new ToolbarHost(toolbar, 90);
        var scroll = toolbar.GetVisualDescendants().OfType<ScrollViewer>().Single();

        count.Bounds.Height.ShouldBeGreaterThan(20);
        scroll.Bounds.Y.ShouldBeGreaterThanOrEqualTo(count.Bounds.Bottom);
        scroll.Viewport.Width.ShouldBe(90);
        scroll.Extent.Width.ShouldBeGreaterThan(scroll.Viewport.Width);
    }

    private sealed class ToolbarHost : IDisposable
    {
        private readonly Window _window;

        public Window Window => _window;

        public ToolbarHost(CompactToolbar toolbar, double width)
        {
            _window = new Window { Width = width, Height = 300, Content = toolbar };
            toolbar.VerticalAlignment = VerticalAlignment.Top;
            _window.Show();
            Layout();
        }

        public void Layout()
        {
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
        }

        public void Detach()
        {
            _window.Content = null;
            Layout();
        }

        public void Attach(CompactToolbar toolbar)
        {
            _window.Content = toolbar;
            Layout();
        }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }
}
