using Avalonia.Collections;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Avalonia.Resources.Network.v1.Service.Views;

namespace KubeUI.Avalonia.Resources.Network;

public sealed partial class V1ServiceConfig : ResourceConfigBase<V1Service>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Network", "Network");
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Service, string>()
            {
                Name = "Type",
                Field = x => x.Spec.Type,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Service, string>()
            {
                Name = "Cluster IP",
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

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Service>? selectedItems)
    {
        var selectedItem = selectedItems?.FirstOrDefault();

        return [
            new()
            {
                Header = "Port Forwarding",
                FluentIcon = Icon.CloudFlow,
                Items = selectedItem?.Spec?.Ports == null
                    ? null
                    : new AvaloniaList<MenuItemViewModel>(selectedItem.Spec.Ports.Select(p => new MenuItemViewModel()
                    {
                        Header = $"{p.Name} - {p.Port}",
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
            var pf = Cluster.AddServicePortForward(service.Namespace(), service.Name(), containerPort.Port);

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
        if (parameters?[0] is V1Service service && parameters?[1] is V1ServicePort servicePort)
        {
            return servicePort?.Port > 0 &&
                   servicePort.Protocol == "TCP" &&
                   Cluster.CanI<V1Pod>(Verb.Create, service.Namespace(), "portforward") &&
                   Cluster.CanI<V1EndpointSlice>(Verb.List, service.Namespace()) &&
                   Cluster.CanI<V1EndpointSlice>(Verb.Watch, service.Namespace());
        }

        return false;
    }

    public override Control[] Properties(V1Service resource) => [new PropertiesView()];
}


