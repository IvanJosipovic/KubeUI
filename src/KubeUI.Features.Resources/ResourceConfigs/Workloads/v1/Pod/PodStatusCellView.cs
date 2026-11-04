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
            ? L10n.Instance.PodStatusCell_Terminating.CurrentValue()!
            : pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Ready") is { } ready
                ? ready.Status == "True"
                    ? L10n.Instance.PodStatusCell_Running.CurrentValue()!
                    : ready.Reason == "PodCompleted"
                        ? L10n.Instance.PodStatusCell_PodCompleted.CurrentValue()!
                        : ready.Reason ?? L10n.Instance.PodStatusCell_Unknown.CurrentValue()!
                : L10n.Instance.PodStatusCell_Unknown.CurrentValue()!;

        Foreground = status == L10n.Instance.PodStatusCell_PodCompleted.CurrentValue()! || status == L10n.Instance.PodStatusCell_Running.CurrentValue()!
            ? ApplicationBrushResources.GetBrush("PodStatusReadyBrush")
            : ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        return status;
    }

}
