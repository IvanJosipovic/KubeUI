using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Core.v1.Event;

public sealed class PropertiesView : ViewBase<Corev1Event>
{
    protected override object Build(Corev1Event vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.EventPropertiesView_Message!)
                    .Value(vm.Message ?? ""),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Source!)
                    .Value($"{vm.Source?.Component ?? ""}/{vm.Source?.Host ?? ""}"),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Reason!)
                    .Value(vm.Reason ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.EventPropertiesView_First_Seen!)
                    .Value(vm.FirstTimestamp ?? DateTime.MinValue),
                new PropertyItem()
                    .Key(Assets.Resources.EventPropertiesView_Last_Seen!)
                    .Value(vm.LastTimestamp ?? DateTime.MinValue),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Count!)
                    .Value(vm.Count ?? 0),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Type!)
                    .Value(vm.Type ?? ""),
                new ExpandableSection()
                    .Header(Assets.Resources.EventPropertiesView_InvolvedObject!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(SharedAssets.Resources.Shared_Kind!)
                                    .Value($"{vm.InvolvedObject?.ApiVersion ?? ""}/{vm.InvolvedObject?.Kind ?? ""}"),
                                new PropertyItem()
                                    .Key(SharedAssets.Resources.Shared_Name!)
                                    .Value(vm.InvolvedObject?.Name ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.EventPropertiesView_Namespace!)
                                    .Value(vm.InvolvedObject?.NamespaceProperty ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.EventPropertiesView_Field_Path!)
                                    .Value(vm.InvolvedObject?.FieldPath ?? ""))));
    }
}
