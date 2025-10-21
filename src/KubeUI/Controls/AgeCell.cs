using k8s;
using k8s.Models;
using KubeUI.Views;

namespace KubeUI.Controls;

public partial class AgeCell : MyViewBase<IKubernetesObject<V1ObjectMeta>>
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public AgeCell()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(1);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IKubernetesObject<V1ObjectMeta> date && date.Metadata.CreationTimestamp.HasValue)
        {
            _date = date.Metadata.CreationTimestamp.Value;
            Convert();

            if (!s_timer.IsEnabled)
            {
                s_timer.Start();
            }
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Convert();
    }

    private DateTime _date;

    public static readonly DirectProperty<AgeCell, string> PrettyStringProperty =
    AvaloniaProperty.RegisterDirect<AgeCell, string>(
    nameof(PrettyString),
    o => o.PrettyString,
    (o, v) => o.PrettyString = v);

    public string PrettyString
    {
        get { return _prettyString; }
        set { SetAndRaise(PrettyStringProperty, ref _prettyString, value); }
    }

    private string _prettyString = string.Empty;

    private TimeSpan Delta => DateTime.UtcNow - _date;

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

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(IKubernetesObject<V1ObjectMeta>? vm) =>
        new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(PrettyStringProperty)
            .ToolTip(new Binding(nameof(PrettyString)) { Source = this });

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        s_timer.Tick -= Timer_Tick;
    }
}
