using k8s.Models;

namespace KubeUI.Controls;

public sealed partial class LastSeenCell : UserControl
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private DateTime _date;
    private TimeSpan Delta => DateTime.UtcNow - _date;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; }

    public LastSeenCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

        s_timer.Tick += Timer_Tick;

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new Corev1Event()
            {
                LastTimestamp = DateTime.UtcNow,
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is Corev1Event ev && ev.LastTimestamp.HasValue)
        {
            _date = ev.LastTimestamp.Value;
            UpdatePretty();
        }
        else
        {
            PrettyString = string.Empty;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        s_timer.Tick -= Timer_Tick;
    }


    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdatePretty();
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }
}
