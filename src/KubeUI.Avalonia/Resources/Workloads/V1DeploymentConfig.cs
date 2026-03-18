using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads;

public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>
{
    public override bool IsNamespaced => true;
    public override string Category => "Workloads";

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

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Restart",
                FluentIcon = Icon.ArrowSync,
                CommandPath = nameof(RestartCommand),
                CommandParameterPath = "SelectedItems"
            },
        ];
    }
}


