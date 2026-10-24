using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.Avalonia.Infrastructure.Presentation;
using FlowDirection = KubeUI.Cilium.Hubble.FlowDirection;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class FlowFilterViewModel : ViewModelBase
{
    [ObservableProperty] public partial string? SourceNamespace { get; set; }
    [ObservableProperty] public partial string? DestinationNamespace { get; set; }
    [ObservableProperty] public partial string? SourcePod { get; set; }
    [ObservableProperty] public partial string? DestinationPod { get; set; }
    [ObservableProperty] public partial FlowVerdict? Verdict { get; set; }
    [ObservableProperty] public partial FlowDirection? Direction { get; set; }
    [ObservableProperty] public partial int? DestinationPort { get; set; }
    [ObservableProperty] public partial string? Protocol { get; set; }
    [ObservableProperty] public partial bool ShowOnlyErrors { get; set; }

    public bool Matches(FlowViewModel flow)
        => Capture().Matches(flow);

    internal FlowFilterCriteria Capture()
        => new(
            SourceNamespace,
            DestinationNamespace,
            SourcePod,
            DestinationPod,
            Verdict,
            Direction,
            DestinationPort,
            Protocol,
            ShowOnlyErrors);

    public void Reset()
    {
        SourceNamespace = DestinationNamespace = SourcePod = DestinationPod = null;
        Verdict = null;
        Direction = null;
        DestinationPort = null;
        Protocol = null;
        ShowOnlyErrors = false;
    }
}

internal readonly record struct FlowFilterCriteria(
    string? SourceNamespace,
    string? DestinationNamespace,
    string? SourcePod,
    string? DestinationPod,
    FlowVerdict? Verdict,
    FlowDirection? Direction,
    int? DestinationPort,
    string? Protocol,
    bool ShowOnlyErrors)
{
    public bool Matches(FlowViewModel flow)
    {
        if (ShowOnlyErrors && flow.Verdict is not (FlowVerdict.Dropped or FlowVerdict.Error)) return false;
        if (!string.IsNullOrEmpty(SourceNamespace) && flow.SourceNamespace != SourceNamespace) return false;
        if (!string.IsNullOrEmpty(DestinationNamespace) && flow.DestinationNamespace != DestinationNamespace) return false;
        if (!string.IsNullOrEmpty(SourcePod) && !flow.SourcePod.Contains(SourcePod, StringComparison.OrdinalIgnoreCase)) return false;
        if (!string.IsNullOrEmpty(DestinationPod) && !flow.DestinationPod.Contains(DestinationPod, StringComparison.OrdinalIgnoreCase)) return false;
        if (Verdict.HasValue && flow.Verdict != Verdict) return false;
        if (Direction.HasValue && flow.Direction != Direction) return false;
        if (DestinationPort.HasValue && flow.DestinationPort != DestinationPort) return false;
        if (!string.IsNullOrEmpty(Protocol) && !flow.Protocol.Contains(Protocol, StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}
