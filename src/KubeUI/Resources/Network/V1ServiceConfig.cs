using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Network;

public sealed partial class V1ServiceConfig : ResourceConfigBase<V1Service>
{
    public override bool IsNamespaced => true;
    public override string Category => "Network";
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

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Port Forwarding",
                ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Service>>(x => x.SelectedItem.Spec.Ports),
                FluentIcon = Icon.CloudFlow,
                ItemTemplate = new()
                {
                    HeaderBinding = new MultiBinding()
                    {
                        Bindings =
                        [
                            Utilities.FuncBinding<V1ServicePort>(x => x.Name),
                            Utilities.FuncBinding<V1ServicePort>(x => x.Port),
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
}
