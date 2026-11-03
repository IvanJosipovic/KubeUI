using k8s.Models;
using KubeUI.Controls.Styles;
using KubeUI.Features.Resources.List.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed class PodStatusCellView : RefreshingCellTextBlock
{
    public PodStatusCellView()
        : base(null)
    {
    }

    protected override string ResolveText(object? dataContext)
    {
        if (dataContext is not V1Pod pod)
        {
            Foreground = ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
            return string.Empty;
        }

        var status = pod.Metadata?.DeletionTimestamp.HasValue == true
            ? Assets.Resources.PodStatusCell_Terminating!
            : pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Ready") is { } ready
                ? ready.Status == "True"
                    ? Assets.Resources.PodStatusCell_Running!
                    : ready.Reason == "PodCompleted"
                        ? Assets.Resources.PodStatusCell_PodCompleted!
                        : ready.Reason ?? Assets.Resources.PodStatusCell_Unknown!
                : Assets.Resources.PodStatusCell_Unknown!;

        Foreground = status == Assets.Resources.PodStatusCell_PodCompleted || status == Assets.Resources.PodStatusCell_Running
            ? ApplicationBrushResources.GetBrush("PodStatusReadyBrush")
            : ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        return status;
    }

}
