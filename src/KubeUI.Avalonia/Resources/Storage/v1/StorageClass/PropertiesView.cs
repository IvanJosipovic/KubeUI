using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Storage.v1.StorageClass;

public sealed class PropertiesView : ViewBase<V1StorageClass>
{
    protected override object Build(V1StorageClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.StorageClassPropertiesView_Provisioner!)
                    .Value(vm.Provisioner ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Reclaim_Policy!)
                    .Value(vm.ReclaimPolicy ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.StorageClassPropertiesView_Volume_Binding_Mode!)
                    .Value(vm.VolumeBindingMode ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.StorageClassPropertiesView_Allow_Volume_Expansion!)
                    .Value(vm.AllowVolumeExpansion ?? false),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Mount_Options!)
                    .Value(vm.MountOptions?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.StorageClassPropertiesView_Parameters!)
                    .Value(vm.Parameters?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.StorageClassPropertiesView_Allowed_Topologies!)
                    .Value(vm.AllowedTopologies?.Count ?? 0));
    }
}
