using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Controls;

public sealed partial class PodStatusCell : UserControl
{
    public PodStatusCell()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is V1Pod pod)
        {
            if (pod.Metadata?.DeletionTimestamp.HasValue == true)
            {
                PrettyString = "Terminating";
            }
            else
            {
                var ready = pod.Status?.Conditions?.FirstOrDefault(c => c.Type == "Ready");
                PrettyString = ready?.Status == "True"
                    ? "Running"
                    : ready?.Reason ?? "Unknown";
            }
        }
        else
        {
            PrettyString = string.Empty;
        }
    }

    public static readonly DirectProperty<PodStatusCell, string> PrettyStringProperty =
        AvaloniaProperty.RegisterDirect<PodStatusCell, string>(
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
