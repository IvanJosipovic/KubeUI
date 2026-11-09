using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Threading;
using Shouldly;

namespace KubeUI.App.Tests.Controls.Presentation;

public sealed class RefreshingCellTextBlockTests
{
    [AvaloniaFact]
    public void Data_context_changes_update_text_and_tooltip_without_a_clock()
    {
        var cell = new TestCell(null);
        var window = new Window { Content = cell };
        try
        {
            window.Show();
            cell.Name.ShouldBe("CellTextBlock");
            cell.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
            cell.MaxLines.ShouldBe(1);

            cell.DataContext = new CellValue { Text = "first" };
            AssertText(cell, "first");

            cell.DataContext = new CellValue { Text = "second" };
            AssertText(cell, "second");

            cell.DataContext = null;
            AssertText(cell, string.Empty);
        }
        finally
        {
            window.Content = null;
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Clock_tick_refreshes_text_and_tooltip_from_current_data_context()
    {
        using var clock = new TestRefreshClock();
        var value = new CellValue { Text = "before" };
        var cell = new TestCell(clock) { DataContext = value };
        var window = new Window { Content = cell };
        try
        {
            clock.ActiveSubscriptions.ShouldBe(0);
            window.Show();
            clock.ActiveSubscriptions.ShouldBe(1);
            var callsBeforeTick = cell.ResolveCalls;

            value.Text = "after";
            clock.Tick();

            cell.ResolveCalls.ShouldBe(callsBeforeTick + 1);
            AssertText(cell, "after");

            cell.DataContext = new CellValue { Text = "replacement" };
            clock.Tick();
            AssertText(cell, "replacement");
        }
        finally
        {
            window.Content = null;
            window.Close();
        }

        clock.ActiveSubscriptions.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Detach_unsubscribes_and_reattach_has_only_one_subscription()
    {
        using var clock = new TestRefreshClock();
        var value = new CellValue { Text = "initial" };
        var cell = new TestCell(clock) { DataContext = value };
        var window = new Window { Content = cell };
        try
        {
            window.Show();
            clock.TotalSubscriptions.ShouldBe(1);

            for (var attachment = 1; attachment <= 3; attachment++)
            {
                clock.ActiveSubscriptions.ShouldBe(1);
                window.Content = null;
                clock.ActiveSubscriptions.ShouldBe(0);
                var callsWhileDetached = cell.ResolveCalls;
                var textWhileDetached = cell.Text;
                value.Text = $"attachment {attachment}";

                clock.Tick();

                cell.ResolveCalls.ShouldBe(callsWhileDetached);
                cell.Text.ShouldBe(textWhileDetached);
                ToolTip.GetTip(cell).ShouldBe(textWhileDetached);

                window.Content = cell;
                clock.ActiveSubscriptions.ShouldBe(1);
                clock.TotalSubscriptions.ShouldBe(attachment + 1);
                var callsBeforeTick = cell.ResolveCalls;

                clock.Tick();

                cell.ResolveCalls.ShouldBe(callsBeforeTick + 1);
                AssertText(cell, value.Text);
            }
        }
        finally
        {
            window.Content = null;
            window.Close();
        }

        clock.ActiveSubscriptions.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Subclass_can_explicitly_refresh_without_a_clock_or_attachment()
    {
        var value = new CellValue { Text = "before" };
        var cell = new TestCell(null) { DataContext = value };
        try
        {
            value.Text = "after";

            cell.Refresh();

            AssertText(cell, "after");
        }
        finally
        {
            cell.DataContext = null;
        }
    }

    private static void AssertText(TestCell cell, string expected)
    {
        cell.Text.ShouldBe(expected);
        ToolTip.GetTip(cell).ShouldBe(expected);
    }

    private sealed class CellValue
    {
        public string Text { get; set; } = string.Empty;
    }

    private sealed class TestCell(IUiRefreshClock? refreshClock) : RefreshingCellTextBlock(refreshClock)
    {
        public int ResolveCalls { get; private set; }

        public void Refresh() => RefreshText();

        protected override string ResolveText(object? dataContext)
        {
            ResolveCalls++;
            return dataContext is CellValue value ? value.Text : string.Empty;
        }
    }

    private sealed class TestRefreshClock : IUiRefreshClock, IDisposable
    {
        private readonly List<Action> _callbacks = [];

        public int ActiveSubscriptions => _callbacks.Count;

        public int TotalSubscriptions { get; private set; }

        public IDisposable Subscribe(Action callback)
        {
            _callbacks.Add(callback);
            TotalSubscriptions++;
            return new Subscription(this, callback);
        }

        public void Tick()
        {
            foreach (var callback in _callbacks.ToArray())
            {
                callback();
            }
        }

        public void Dispose() => _callbacks.Clear();

        private sealed class Subscription(TestRefreshClock clock, Action callback) : IDisposable
        {
            private TestRefreshClock? _clock = clock;

            public void Dispose()
            {
                _clock?._callbacks.Remove(callback);
                _clock = null;
            }
        }
    }
}
