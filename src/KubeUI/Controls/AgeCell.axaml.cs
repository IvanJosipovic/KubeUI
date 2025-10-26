using k8s;
using k8s.Models;

namespace KubeUI.Controls;

public sealed partial class AgeCell : UserControl
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private DateTime _date;

    public AgeCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IKubernetesObject<V1ObjectMeta> obj && obj.Metadata.CreationTimestamp.HasValue)
        {
            _date = obj.Metadata.CreationTimestamp.Value;
            UpdatePretty();
        }
        else
        {
            PrettyString = string.Empty;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
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

        if (Delta.TotalMilliseconds <= 0) PrettyString = "0ms";
        else if (Delta.TotalDays >= 365)   PrettyString = $"{(Delta.TotalDays / 365):N0}y";
        else if (Delta.TotalDays >= 1)     PrettyString = $"{Delta.TotalDays:N0}d";
        else if (Delta.TotalHours >= 1)    PrettyString = $"{Delta.TotalHours:N0}h";
        else if (Delta.TotalMinutes >= 1)  PrettyString = $"{Delta.TotalMinutes:N0}m{Delta.Seconds:N0}s";
        else if (Delta.TotalSeconds >= 1)  PrettyString = $"{Delta.TotalSeconds:N0}s";
        else                               PrettyString = $"{Delta.TotalMilliseconds:N0}ms";
    }

    public static readonly DirectProperty<AgeCell, string> PrettyStringProperty =
        AvaloniaProperty.RegisterDirect<AgeCell, string>(
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
