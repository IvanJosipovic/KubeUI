using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.Core.v1.Event;

public sealed class PropertiesView : ViewBase<Corev1Event>
{
    protected override object Build(Corev1Event vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.EventPropertiesView_Message.CurrentValue()!)
                    .Value(vm.Message ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Source.CurrentValue()!)
                    .Value($"{vm.Source?.Component ?? ""}/{vm.Source?.Host ?? ""}"),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Reason.CurrentValue()!)
                    .Value(vm.Reason ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.EventPropertiesView_First_Seen.CurrentValue()!)
                    .Value(vm.FirstTimestamp ?? DateTime.MinValue),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.EventPropertiesView_Last_Seen.CurrentValue()!)
                    .Value(vm.LastTimestamp ?? DateTime.MinValue),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Count.CurrentValue()!)
                    .Value(vm.Count ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Type.CurrentValue()!)
                    .Value(vm.Type ?? ""),
                ResourcePropertiesSection.Create(
                    ResourcesLanguageManager.Instance.EventPropertiesView_InvolvedObject,
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Kind.CurrentValue()!)
                        .Value($"{vm.InvolvedObject?.ApiVersion ?? ""}/{vm.InvolvedObject?.Kind ?? ""}"),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Name.CurrentValue()!)
                        .Value(vm.InvolvedObject?.Name ?? ""),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.EventPropertiesView_Namespace.CurrentValue()!)
                        .Value(vm.InvolvedObject?.NamespaceProperty ?? ""),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.EventPropertiesView_Field_Path.CurrentValue()!)
                        .Value(vm.InvolvedObject?.FieldPath ?? "")));
    }
}
