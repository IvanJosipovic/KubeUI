using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Cilium.Hubble;

namespace KubeUI.Features.Cilium.Hubble;

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
                return CiliumLanguageManager.Instance.ServiceMapNodePropertiesView_NoAccessPoints.CurrentValue()!;

            return string.Join(
                Environment.NewLine,
                node.AccessPoints.Select(accessPoint => string.Format(
                    CultureInfo.CurrentCulture,
                    CiliumLanguageManager.Instance.ServiceMapNodePropertiesView_AccessPointFormat.CurrentValue()!,
                    accessPoint.Protocol,
                    accessPoint.Port,
                    accessPoint.FlowCount,
                    accessPoint.DeniedCount,
                    accessPoint.ApplicationProtocols.Count == 0
                        ? CiliumLanguageManager.Instance.ServiceMapNodePropertiesView_NoApplicationProtocols.CurrentValue()!
                        : string.Join(", ", accessPoint.ApplicationProtocols))));
        }
    }

    public ServiceMapNodePropertiesViewModel()
    {
        Id = nameof(ServiceMapNodePropertiesViewModel);
        Title = CiliumLanguageManager.Instance.ServiceMapNodePropertiesView_Title.CurrentValue()!;
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
            CiliumLanguageManager.Instance.ServiceMapNodePropertiesView_TitleFormat.CurrentValue()!,
            node.Name);
        OnPropertyChanged(nameof(AccessPoints));
    }
}
