using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Styles;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

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
            ? AppResources.PodStatusCell_Terminating!
            : pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Ready") is { } ready
                ? ready.Status == "True"
                    ? AppResources.PodStatusCell_Running!
                    : ready.Reason == "PodCompleted"
                        ? AppResources.PodStatusCell_PodCompleted!
                        : ready.Reason ?? AppResources.PodStatusCell_Unknown!
                : AppResources.PodStatusCell_Unknown!;

        Foreground = status == AppResources.PodStatusCell_PodCompleted || status == AppResources.PodStatusCell_Running
            ? ApplicationBrushResources.GetBrush("PodStatusReadyBrush")
            : ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        return status;
    }

}
