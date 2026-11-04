using Avalonia.Controls.Templates;
using k8s.Models;
using KubeUI.Controls.Converters;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed class PropertiesView : ViewBase<V1Pod>
{
    protected override object Build(V1Pod vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_ControlledBy.CurrentValue()!)
                    .Value(vm.Metadata.OwnerReferences?.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A"),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_Status.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_Node.CurrentValue()!)
                    .Value(vm.Spec?.NodeName ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_PodIp.CurrentValue()!)
                    .Value(vm.Status?.PodIP ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_Service_Account.CurrentValue()!)
                    .Value(vm.Spec?.ServiceAccountName ?? ""),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Metadata.ToBinding())
                    .Content(
                        new StackPanel()
                            .Children(
                                new CollectionItem()
                                    .Key(L10n.Instance.PodPropertiesView_Labels.CurrentValue()!)
                                    .Value(vm.Metadata?.Labels ?? new Dictionary<string, string>())
                                    .ItemTemplate(CreateKeyValueTemplate()),
                                new CollectionItem()
                                    .Key(L10n.Instance.PodPropertiesView_Annotations.CurrentValue()!)
                                    .Value(vm.Metadata?.Annotations ?? new Dictionary<string, string>())
                                    .ItemTemplate(CreateKeyValueTemplate()))),
                new ExpandableSection()
                    .Header(L10n.Instance.PodPropertiesView_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.PodPropertiesView_Pod_IPs.CurrentValue()!)
                                    .Value(vm.Status?.PodIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.PodPropertiesView_QoS_Class.CurrentValue()!)
                                    .Value(vm.Status?.QosClass ?? ""),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Conditions.CurrentValue()!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Tolerations.CurrentValue()!)
                                    .Value(vm.Spec?.Tolerations?.Count ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Secrets.CurrentValue()!)
                                    .Value(vm.Spec?.ImagePullSecrets?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.PodPropertiesView_Pod_Volumes.CurrentValue()!)
                                    .Value(vm.Spec?.Volumes?.Count ?? 0))),
                new ExpandableSection()
                    .Header(L10n.Instance.PodPropertiesView_InitContainers.ToBinding())
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.InitContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
                    .Content(CreateContainers(vm.Spec?.InitContainers ?? [])),
                new ExpandableSection()
                    .Header(L10n.Instance.PodPropertiesView_EphemeralContainers.ToBinding())
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.EphemeralContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
                    .Content(CreateEphemeralContainers(vm.Spec?.EphemeralContainers ?? [])),
                new ExpandableSection()
                    .Header(L10n.Instance.PodPropertiesView_Containers.ToBinding())
                    .IsExpanded(true)
                    .Content(CreateContainers(vm.Spec?.Containers ?? [])));
    }

    private static IDataTemplate CreateKeyValueTemplate()
    {
        return new FuncDataTemplate<KeyValuePair<string, string>>((entry, _) =>
            new SelectableTextBlock()
                .Text($"{entry.Key}={entry.Value}"));
    }

    private static ItemsControl CreateContainers(IEnumerable<V1Container> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1Container>((container, _) => CreateContainerTemplate(container)));
    }

    private static ItemsControl CreateEphemeralContainers(IEnumerable<V1EphemeralContainer> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1EphemeralContainer>((container, _) => CreateEphemeralContainerTemplate(container)));
    }

    private static StackPanel CreateContainerTemplate(V1Container container)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Name.CurrentValue()!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_Image.CurrentValue()!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_PullPolicy.CurrentValue()!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(SharedL10n.Instance.Shared_Ports.CurrentValue()!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Environment.CurrentValue()!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Mounts.CurrentValue()!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Command.CurrentValue()!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Arguments.CurrentValue()!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Requests.CurrentValue()!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(SharedL10n.Instance.Shared_Limits.CurrentValue()!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }

    private static StackPanel CreateEphemeralContainerTemplate(V1EphemeralContainer container)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Name.CurrentValue()!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_Image.CurrentValue()!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.PodPropertiesView_PullPolicy.CurrentValue()!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(SharedL10n.Instance.Shared_Ports.CurrentValue()!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Environment.CurrentValue()!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Mounts.CurrentValue()!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Command.CurrentValue()!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Arguments.CurrentValue()!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(L10n.Instance.PodPropertiesView_Requests.CurrentValue()!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(SharedL10n.Instance.Shared_Limits.CurrentValue()!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }
}
