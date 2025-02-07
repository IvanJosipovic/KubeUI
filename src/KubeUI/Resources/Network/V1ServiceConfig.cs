using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Resources.Workloads.Pod;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1Service>>(ServiceLifetime.Transient)]
public sealed partial class V1ServiceConfig : ResourceConfigBase<V1Service>
{
    private readonly ILogger<V1DaemonSetConfig> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;
    private readonly IFactory _factory;

    public override string Category => "Network";
    public override int Order => 0;

    public V1ServiceConfig(ILogger<V1DaemonSetConfig> logger, IDialogService dialogService, INotificationManager notificationManager, IFactory factory)
    {
        _logger = logger;
        _dialogService = dialogService;
        _notificationManager = notificationManager;
        _factory = factory;
    }

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Service, string>()
            {
                Name = "Type",
                Display = x => x.Spec.Type,
                Field = x => x.Spec.Type,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Service, string>()
            {
                Name = "Cluster IP",
                Display = x => x.Spec.ClusterIP,
                Field = x => x.Spec.ClusterIP,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Service, int>()
            {
                Name = "Ports",
                Display = x => x.Spec?.Ports?.Select((a) => $"{a.Port}{(string.IsNullOrEmpty(a.Name) ? "" : ":" + a.Name)}/{a.Protocol}").Aggregate((a,b) => a + ", " + b) ?? "",
                Field = x => x.Spec.Ports?.FirstOrDefault()?.Port ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Port Forwarding",
                ItemSourcePath = "SelectedItem.Value.Spec.Ports",
                IconResource = "ic_fluent_cloud_flow_filled",
                ItemTemplate = new()
                {
                    HeaderBinding = new MultiBinding()
                    {
                        Bindings =
                        [
                            new Binding(nameof(V1ServicePort.Name)),
                            new Binding(nameof(V1ServicePort.Port))
                        ],
                        StringFormat = "{0} - {1}"
                    },
                    CommandPath = nameof(PortForwardServiceCommand),
                    CommandParameterPath = ".",
                    CommandParameterAddSelectedItem = true,
                }
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanPortForwardService))]
    private async Task PortForwardService(IList parameters)
    {
        if (parameters[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters[1] is V1ServicePort containerPort)
        {
            var pf = Cluster.AddServicePortForward(pod.Key.Namespace, pod.Key.Name, containerPort.Port);

            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_PortForward_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_PortForward_Content, containerPort.Port, pf.LocalPort),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_PortForward_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_PortForward_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
                await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
            }
        }
    }

    private bool CanPortForwardService(IList? parameters)
    {
        if (parameters?[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters?[1] is V1ServicePort servicePort)
        {
            return servicePort?.Port > 0 &&
                   servicePort.Protocol == "TCP" &&
                   Cluster.CanI<V1Pod>(Verb.Create, pod.Key.Namespace, "portforward") &&
                   Cluster.CanI<V1Endpoints>(Verb.List, pod.Key.Namespace) &&
                   Cluster.CanI<V1Endpoints>(Verb.Watch, pod.Key.Namespace);
        }

        return false;
    }
}
