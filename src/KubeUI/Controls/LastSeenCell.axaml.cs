using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using k8s.Models;

namespace KubeUI.Controls;

public sealed partial class LastSeenCell : UserControl
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private DateTime _date;
    private bool _subscribed;

    public LastSeenCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Tick += TimerTick;
            s_timer.Start();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is Corev1Event ev && ev.LastTimestamp.HasValue)
        {
            _date = ev.LastTimestamp.Value;
            UpdatePretty();
            _subscribed = true;
        }
        else
        {
            _subscribed = false;
            PrettyString = string.Empty;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _subscribed = false;
        s_timer.Tick -= TimerTick;
    }


    private void TimerTick(object? sender, EventArgs e)
    {
        if (_subscribed)
            UpdatePretty();
    }

    private TimeSpan Delta => DateTime.UtcNow - _date;

    private void UpdatePretty()
    {
        if (_date == default)
        {
            PrettyString = string.Empty;
            return;
        }

        var d = Delta;
        if (d.TotalMilliseconds <= 0) PrettyString = "0ms";
        else if (d.TotalDays >= 365)  PrettyString = $"{(d.TotalDays / 365):N0}y";
        else if (d.TotalDays >= 1)    PrettyString = $"{d.TotalDays:N0}d";
        else if (d.TotalHours >= 1)   PrettyString = $"{d.TotalHours:N0}h";
        else if (d.TotalMinutes >= 1) PrettyString = $"{d.TotalMinutes:N0}m{d.Seconds:N0}s";
        else if (d.TotalSeconds >= 1) PrettyString = $"{d.TotalSeconds:N0}s";
        else                          PrettyString = $"{d.TotalMilliseconds:N0}ms";
    }

    public static readonly DirectProperty<LastSeenCell, string> PrettyStringProperty =
        AvaloniaProperty.RegisterDirect<LastSeenCell, string>(
            nameof(PrettyString),
            o => o.PrettyString,
            (o, v) => o.PrettyString = v);

    private string _pretty = string.Empty;

    public string PrettyString
    {
        get => _pretty;
        private set => SetAndRaise(PrettyStringProperty, ref _pretty, value);
    }
}
