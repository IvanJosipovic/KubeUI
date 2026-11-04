using Avalonia.Collections;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Resources.Common;
using KubeUI.Controls.Platform;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Network.v1.Service;

public sealed partial class V1ServiceConfig : ResourceConfig<V1Service>
{
    private static readonly AuthorizationRequest[] s_portForwardAuthorizationRequests =
    [
        new(GroupApiVersionKind.From<V1Pod>(), Verb.Create, "portforward"),
        new(GroupApiVersionKind.From<V1EndpointSlice>(), Verb.List, null),
        new(GroupApiVersionKind.From<V1EndpointSlice>(), Verb.Watch, null),
    ];

    private readonly IPlatformServices _platformServices;

    public V1ServiceConfig(IServiceProvider serviceProvider, IPlatformServices platformServices)
        : base(serviceProvider)
    {
        _platformServices = platformServices;
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.NetworkId);
    public override int Order => 0;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Service, string>()
            {
                Key = "type",
                Name = L10n.Instance.V1ServiceConfig_Type.CurrentValue()!,
                Field = x => x.Spec.Type,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1Service, string>()
            {
                Key = "cluster-ip",
                Name = L10n.Instance.V1ServiceConfig_Cluster_IP.CurrentValue()!,
                Field = x => x.Spec.ClusterIP,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1Service, int>()
            {
                Key = "ports",
                Name = L10n.Instance.V1ServiceConfig_Ports.CurrentValue()!,
                Display = x => x.Spec?.Ports is { Count: > 0 } ports ? string.Join(", ", ports.Select(x => $"{x.Port}{(string.IsNullOrEmpty(x.Name) ? "" : ":" + x.Name)}/{x.Protocol}")) : "",
                Field = x => x.Spec?.Ports?.FirstOrDefault()?.Port ?? 0,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            AgeColumn(),
        ];
    }

    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Concat(s_portForwardAuthorizationRequests);
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Service>? selectedItems)
    {
        var selectedItem = selectedItems?.FirstOrDefault();

        return [
            new()
            {
                Title = L10n.Instance.V1ServiceConfig_MenuItem_PortForwarding.CurrentValue()!,
                FluentIcon = Icon.CloudFlow,
                Items = selectedItem?.Spec?.Ports == null
                    ? null
                    : new AvaloniaList<MenuItemViewModel>(selectedItem.Spec.Ports.Select(p => new MenuItemViewModel()
                    {
                        Title = string.Format(L10n.Instance.V1ServiceConfig_MenuItem_PortFormat.CurrentValue()!, p.Name, p.Port),
                        Command = PortForwardServiceCommand,
                        CommandParameter = new ArrayList { selectedItem, p },
                    }).ToList()),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanPortForwardService))]
    private async Task PortForwardService(IList parameters)
    {
        if (parameters[0] is V1Service service && parameters[1] is V1ServicePort containerPort)
        {
            var pf = Cluster.Runtime.AddServicePortForward(service.Namespace(), service.Name(), containerPort.Port);

            ContentDialogSettings settings = new()
            {
                Title = L10n.Instance.ResourceListView_PortForward_Title.CurrentValue()!,
                Content = string.Format(L10n.Instance.ResourceListView_PortForward_Content.CurrentValue()!, containerPort.Port, pf.LocalPort),
                PrimaryButtonText = L10n.Instance.ResourceListView_PortForward_Primary.CurrentValue()!,
                SecondaryButtonText = L10n.Instance.ResourceListView_PortForward_Secondary.CurrentValue()!,
                DefaultButton = FAContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == FAContentDialogResult.Primary)
            {
                await _platformServices.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
            }
        }
    }

    private bool CanPortForwardService(IList? parameters)
    {
        if (parameters?[0] is V1Service service && parameters?[1] is V1ServicePort servicePort)
        {
            return servicePort?.Port > 0 &&
                   servicePort.Protocol == "TCP" &&
                   Cluster.Runtime.Permissions.CanI<V1Pod>(Verb.Create, service.Namespace(), "portforward") &&
                   Cluster.Runtime.Permissions.CanI<V1EndpointSlice>(Verb.List, service.Namespace()) &&
                   Cluster.Runtime.Permissions.CanI<V1EndpointSlice>(Verb.Watch, service.Namespace());
        }

        return false;
    }

    public override Control[] Properties(V1Service resource) => [new PropertiesView()];
}
