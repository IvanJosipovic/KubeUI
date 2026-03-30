using KubeUI.Avalonia.Features.Resources.Common;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet.Views;

namespace KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet;

public sealed partial class V1DaemonSetConfig : ResourceConfigBase<V1DaemonSet>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");

    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1DaemonSet, int>()
            {
                Name = "Pods",
                Field = x => x.Status.NumberReady,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1DaemonSet, string>()
            {
                Name = "Node Selector",
                Field = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1DaemonSet>? selectedItems)
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

    public override Control[] Properties(V1DaemonSet resource) => [new PropertiesView()];
}


