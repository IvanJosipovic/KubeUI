using Avalonia.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet.Views;

namespace KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet;

public sealed partial class V1ReplicaSetConfig : ResourceConfigBase<V1ReplicaSet>
{
    public V1ReplicaSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");

    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "desired",
                Name = Assets.Resources.V1ReplicaSetConfig_Desired!,
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "current",
                Name = Assets.Resources.V1ReplicaSetConfig_Current!,
                Field = x => x.Status.AvailableReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "ready",
                Name = Assets.Resources.V1ReplicaSetConfig_Ready!,
                Field = x => x.Status.ReadyReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1ReplicaSet>? selectedItems)
    {
        return [
            new()
            {
                Header = "Restart",
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList(),
            },
        ];
    }

    public override Control[] Properties(V1ReplicaSet resource) => [new PropertiesView()];
}


