using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using k8s;
using k8s.Models;

namespace KubeUI.Controls;

public partial class StatusCell : UserControl
{
    public StatusCell()
    {
        InitializeComponent();
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Default, Timer_Tick);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IKubernetesObject<V1ObjectMeta> date && date.Metadata.CreationTimestamp.HasValue)
        {
            Date = date.Metadata.CreationTimestamp.Value;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Convert();
    }

    public static readonly DirectProperty<StatusCell, DateTime> DateProperty =
    AvaloniaProperty.RegisterDirect<StatusCell, DateTime>(
    nameof(Date),
    o => o.Date,
    (o, v) => o.Date = v);

    private DateTime _date;

    public DateTime Date
    {
        get { return _date; }
        set { SetAndRaise(DateProperty, ref _date, value); Convert(); }
    }

    public static readonly DirectProperty<StatusCell, string> PrettyStringProperty =
    AvaloniaProperty.RegisterDirect<StatusCell, string>(
    nameof(PrettyString),
    o => o.PrettyString,
    (o, v) => o.PrettyString = v);

    public string PrettyString
    {
        get { return _prettyString; }
        set { SetAndRaise(PrettyStringProperty, ref _prettyString, value); }
    }

    private string _prettyString = string.Empty;

    private TimeSpan Delta => DateTime.UtcNow - Date;

    private void Convert()
    {
        if (Delta.TotalMilliseconds <= 0)
        {
            PrettyString = "0ms";
        }
        else if (Delta.TotalDays >= 365)
        {
            PrettyString = ((Delta.TotalDays / 365).ToString("N0")) + "y";
        }
        else if (Delta.TotalDays >= 1)
        {
            PrettyString = (Delta.TotalDays.ToString("N0")) + "d";
        }
        else if (Delta.TotalHours >= 1)
        {
            PrettyString = (Delta.TotalHours.ToString("N0")) + "h";
        }
        else if (Delta.TotalMinutes >= 1)
        {
            PrettyString = $"{Delta.TotalMinutes:N0}m{Delta.Seconds:N0}s";
        }
        else if (Delta.TotalSeconds >= 1)
        {
            PrettyString = (Delta.TotalSeconds.ToString("N0")) + "s";
        }
        else
        {
            PrettyString = (Delta.TotalMilliseconds.ToString("N0")) + "ms";
        }
    }

    private DispatcherTimer _timer;

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _timer.Stop();
        _timer.Tick -= Timer_Tick;
    }
}
