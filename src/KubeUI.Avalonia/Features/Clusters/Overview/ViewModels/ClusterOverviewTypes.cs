using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using System.Collections.ObjectModel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed class ClusterScopeOption
{
    public ClusterScopeOption(string label, ClusterScope scope)
    {
        Label = label;
        Scope = scope;
    }

    public string Label { get; }

    public ClusterScope Scope { get; }

    public override string ToString() => Label;
}

public enum ClusterScope
{
    All,
    WorkerNodes,
    MasterNodes,
}

public sealed partial class MetricTabViewModel : ObservableObject, IDisposable
{
    public required string Title { get; init; }

    [ObservableProperty]
    public partial Icon Icon { get; set; }

    [ObservableProperty]
    public partial bool ShowTitle { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ObservableCollection<MetricPanelViewModel> Panels { get; } = [];

    internal void MergePanels(IReadOnlyList<MetricPanelSnapshot> snapshots, bool preserveMissing = false)
    {
        if (!preserveMissing)
        {
            for (var i = Panels.Count - 1; i >= 0; i--)
            {
                if (!snapshots.Any(x => string.Equals(x.Title, Panels[i].Title, StringComparison.Ordinal)))
                {
                    Panels[i].Dispose();
                    Panels.RemoveAt(i);
                }
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Panels.FirstOrDefault(x => string.Equals(x.Title, snapshot.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricPanelViewModel
                {
                    Title = snapshot.Title,
                };
                Panels.Insert(index, existing);
            }

            existing.MergeSeries(snapshot.Series, preserveMissing);

            if (existing != null)
            {
                var currentIndex = Panels.IndexOf(existing);
                if (currentIndex != index)
                {
                    Panels.Move(currentIndex, index);
                }
            }
        }

        ShowTitle = snapshots.Count != 1
            || !string.Equals(Title, snapshots[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        foreach (var panel in Panels)
        {
            panel.Dispose();
        }

        Panels.Clear();
    }
}

public sealed record MetricTimeRangeOption(string Label, int RangeSeconds);
