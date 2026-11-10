using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.App.Tests.Infra;
using KubeUI.Controls.Layout;
using Shouldly;

namespace KubeUI.Controls.Tests.Controls.Layout;

public sealed class CompactToolbarTests
{
    [AvaloniaFact]
    [Obsolete]
    public void Slots_keep_compact_spacing_and_right_align_trailing_content()
    {
        var leading = new Button { Width = 24, Content = "+" };
        var middle = new TextBlock { Width = 60, Height = 20, Text = "3 items" };
        var trailing = new Button { Width = 80, Content = "Refresh" };
        var toolbar = new CompactToolbar { Leading = leading, Middle = middle, Trailing = trailing };
        using var host = new ToolbarHost(toolbar, 600);

        toolbar.MinHeight.ShouldBe(32);
        toolbar.Margin.ShouldBe(new Thickness(0, 2));
        leading.MinHeight.ShouldBe(32d);
        trailing.MinHeight.ShouldBe(32d);
        leading.Bounds.Height.ShouldBe(32d);
        trailing.Bounds.Height.ShouldBe(32d);
        leading.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(2);
        middle.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(36);
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

    [AvaloniaFact]
    public void Group_supplies_horizontal_layout_defaults_without_repeating_panel_setup()
    {
        var first = new Button { Content = "First" };
        var second = new TextBox();
        var third = new ComboBox();
        var group = CompactToolbar.Group(first, second, third);
        var toolbar = new CompactToolbar { Leading = group };
        using var host = new ToolbarHost(toolbar, 300);

        group.Orientation.ShouldBe(Orientation.Horizontal);
        group.Spacing.ShouldBe(4d);
        group.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
        group.Children.Count.ShouldBe(3);
        group.Children[0].ShouldBeSameAs(first);
        group.Children[1].ShouldBeSameAs(second);
        group.Children[2].ShouldBeSameAs(third);
        first.MinHeight.ShouldBe(32d);
        second.MinHeight.ShouldBe(32d);
        first.MinWidth.ShouldBe(32d);
        first.Margin.ShouldBe(new Thickness(0));
        first.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
        first.VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
        second.VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
        second.Bounds.Height.ShouldBe(32d);
        third.VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
        third.Bounds.Height.ShouldBe(32d);

        var spacedGroup = CompactToolbar.Group(6, new Button(), new Button());
        spacedGroup.Spacing.ShouldBe(6d);
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
        var oldMiddle = new Border();
        var oldTrailing = new Border();
        var toolbar = new CompactToolbar { Leading = oldLeading, Middle = oldMiddle, Trailing = oldTrailing };
        using var host = new ToolbarHost(toolbar, 400);
        var leading = new Border();
        var middle = new Border();
        var trailing = new Border();

        toolbar.Leading = leading;
        toolbar.Middle = middle;
        toolbar.Trailing = trailing;
        host.Layout();

        oldLeading.Parent.ShouldBeNull();
        oldMiddle.Parent.ShouldBeNull();
        oldTrailing.Parent.ShouldBeNull();
        toolbar.GetVisualDescendants().ShouldContain(leading);
        toolbar.GetVisualDescendants().ShouldContain(middle);
        toolbar.GetVisualDescendants().ShouldContain(trailing);

        toolbar.Leading = null;
        toolbar.Middle = null;
        toolbar.Trailing = null;
        host.Layout();
        leading.Parent.ShouldBeNull();
        middle.Parent.ShouldBeNull();
        trailing.Parent.ShouldBeNull();
    }

    [AvaloniaFact]
    public void Detached_toolbar_accepts_slot_updates_and_lays_out_again_on_reattachment()
    {
        var leading = new Border { Width = 24, Height = 24 };
        var oldMiddle = new Border { Width = 60, Height = 20 };
        var trailing = new Border { Width = 200, Height = 24 };
        var toolbar = new CompactToolbar { Leading = leading, Middle = oldMiddle, Trailing = trailing };
        using var host = new ToolbarHost(toolbar, 180);

        host.Detach();
        TopLevel.GetTopLevel(toolbar).ShouldBeNull();
        var middle = new Border { Width = 40, Height = 20 };
        toolbar.Middle = middle;
        host.Attach(toolbar);

        TopLevel.GetTopLevel(toolbar).ShouldNotBeNull();
        oldMiddle.Parent.ShouldBeNull();
        middle.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(28);
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
        var middle = new Border { Width = 60, Height = 20 };
        var toolbar = new CompactToolbar { Leading = leading, Middle = middle };
        using var host = new ToolbarHost(toolbar, 300);

        leading.IsVisible = false;
        host.Layout();

        middle.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(2);
        leading.IsVisible = true;
        host.Layout();
        middle.TranslatePoint(default, toolbar)!.Value.X.ShouldBe(28);
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
            Middle = new Border { Width = 60, Height = 20 },
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
    public void Very_narrow_toolbar_places_trailing_content_on_another_row_and_wraps_middle_content()
    {
        var middle = new TextBlock { Text = "Many matching items", TextWrapping = TextWrapping.Wrap };
        var trailing = new Border { Width = 200, Height = 24 };
        var toolbar = new CompactToolbar
        {
            Leading = new Border { Width = 24, Height = 24 },
            Middle = middle,
            Trailing = trailing
        };
        using var host = new ToolbarHost(toolbar, 90);
        var scroll = toolbar.GetVisualDescendants().OfType<ScrollViewer>().Single();

        middle.Bounds.Height.ShouldBeGreaterThan(20);
        scroll.Bounds.Y.ShouldBeGreaterThanOrEqualTo(middle.Bounds.Bottom);
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
