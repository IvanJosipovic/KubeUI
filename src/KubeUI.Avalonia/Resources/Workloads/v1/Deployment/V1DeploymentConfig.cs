using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Resources.Workloads.v1.Deployment.Views;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Deployment;

public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>
{
    public V1DeploymentConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");

    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Deployment, int>()
            {
                Name = "Pods",
                Display = x => $"{x.Status?.AvailableReplicas ?? 0}/{x.Spec?.Replicas ?? 0}",
                Field = x => x.Status?.AvailableReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, int>()
            {
                Name = "Replicas",
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, string>()
            {
                Name = "Available",
                Field = x => x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Deployment>? selectedItems)
    {
        return [
            new()
            {
                Header = "Restart",
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList()
            },
        ];
    }

    public override Control[] Properties(V1Deployment resource) => [new PropertiesView()];
}


