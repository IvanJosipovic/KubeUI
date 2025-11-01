using k8s.Models;

namespace KubeUI.Resources.Workloads.v1.Pod.Controls;

public sealed partial class PodStatusCell : UserControl
{
    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; }

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
}
