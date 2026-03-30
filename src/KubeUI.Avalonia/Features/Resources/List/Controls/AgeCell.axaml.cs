using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.List.Controls;

public sealed partial class AgeCell : UserControl
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public AgeCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Namespace()
            {
                Metadata = new()
                {
                    CreationTimestamp = DateTime.UtcNow,
                }
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        SetPrettyString();
    }

    private void SetPrettyString()
    {
        if (DataContext is IKubernetesObject<V1ObjectMeta> obj)
        {
            if (obj.Metadata.CreationTimestamp.HasValue)
            {
                var _date = obj.Metadata.CreationTimestamp.Value;

                var d = DateTime.UtcNow - _date;
                if (d.TotalMilliseconds <= 0)
                    PrettyString = "0ms";
                else if (d.TotalDays >= 365)
                    PrettyString = $"{(d.TotalDays / 365):N0}y";
                else if (d.TotalDays >= 1)
                    PrettyString = $"{d.TotalDays:N0}d";
                else if (d.TotalHours >= 1)
                    PrettyString = $"{d.TotalHours:N0}h";
                else if (d.TotalMinutes >= 1)
                    PrettyString = $"{d.TotalMinutes:N0}m{d.Seconds:N0}s";
                else if (d.TotalSeconds >= 1)
                    PrettyString = $"{d.TotalSeconds:N0}s";
                else
                    PrettyString = $"{d.TotalMilliseconds:N0}ms";
            }
            else
            {
                PrettyString = string.Empty;
            }
        }
    }
}

