using Avalonia.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet.Views;

namespace KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;

public sealed partial class V1StatefulSetConfig : ResourceConfigBase<V1StatefulSet>
{
    public V1StatefulSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");

    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1StatefulSet, int>()
            {
                Key = "replicas",
                Name = Assets.Resources.V1StatefulSetConfig_Replicas!,
                Field = x => x.Status.Replicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1StatefulSet>? selectedItems)
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

    public override Control[] Properties(V1StatefulSet resource) => [new PropertiesView()];
}


