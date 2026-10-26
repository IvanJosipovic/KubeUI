using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

/// <summary>Provides Hubble details for one aggregated service-map node.</summary>
public sealed partial class ServiceMapNodePropertiesViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ServiceMapNode? Node { get; private set; }

    /// <summary>Gets formatted protocol and port details for the selected node.</summary>
    public string AccessPoints
    {
        get
        {
            if (Node is not { } node || node.AccessPoints.Count == 0)
                return Assets.Resources.ServiceMapNodePropertiesView_NoAccessPoints!;

            return string.Join(
                Environment.NewLine,
                node.AccessPoints.Select(accessPoint => string.Format(
                    CultureInfo.CurrentCulture,
                    Assets.Resources.ServiceMapNodePropertiesView_AccessPointFormat!,
                    accessPoint.Protocol,
                    accessPoint.Port,
                    accessPoint.FlowCount,
                    accessPoint.DeniedCount,
                    accessPoint.ApplicationProtocols.Count == 0
                        ? Assets.Resources.ServiceMapNodePropertiesView_NoApplicationProtocols
                        : string.Join(", ", accessPoint.ApplicationProtocols))));
        }
    }

    public ServiceMapNodePropertiesViewModel()
    {
        Id = nameof(ServiceMapNodePropertiesViewModel);
        Title = Assets.Resources.ServiceMapNodePropertiesView_Title!;
        CanFloat = false;
        CanDockAsDocument = false;
    }

    /// <summary>Initializes details from the selected service-map node.</summary>
    /// <param name="node">Aggregated Hubble node to display.</param>
    public void Initialize(ServiceMapNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Node = node;
        Title = string.Format(
            CultureInfo.CurrentCulture,
            Assets.Resources.ServiceMapNodePropertiesView_TitleFormat!,
            node.Name);
        OnPropertyChanged(nameof(AccessPoints));
    }
}
