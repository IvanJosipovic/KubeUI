using k8s.Models;
using KubeUI.Controls.Styles;

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
            ? ResourcesLanguageManager.Instance.PodStatusCellView_Terminating.CurrentValue()!
            : pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Ready") is { } ready
                ? ready.Status == "True"
                    ? ResourcesLanguageManager.Instance.PodStatusCellView_Running.CurrentValue()!
                    : ready.Reason == "PodCompleted"
                        ? ResourcesLanguageManager.Instance.PodStatusCellView_PodCompleted.CurrentValue()!
                        : ready.Reason ?? ResourcesLanguageManager.Instance.PodStatusCellView_Unknown.CurrentValue()!
                : ResourcesLanguageManager.Instance.PodStatusCellView_Unknown.CurrentValue()!;

        Foreground = status == ResourcesLanguageManager.Instance.PodStatusCellView_PodCompleted.CurrentValue()! || status == ResourcesLanguageManager.Instance.PodStatusCellView_Running.CurrentValue()!
            ? ApplicationBrushResources.GetBrush("PodStatusReadyBrush")
            : ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        return status;
    }

}
