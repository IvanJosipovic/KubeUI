using k8s;
using k8s.Models;
using Microsoft.CodeAnalysis;

namespace KubeUI.Controls;

public sealed partial class AgeCell : UserControl
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public AgeCell()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Namespace()
            {
                Metadata = new()
                {
                    CreationTimestamp = DateTime.Now,
                }
            };
        }
#endif
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

        s_timer.Tick += Timer_Tick;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        s_timer.Tick -= Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }
}
