using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using AppConverters = KubeUI.Avalonia.Converters.Converters;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PropertiesView : ViewBase<V1Pod>
{
    protected override object Build(V1Pod vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_ControlledBy!)
                    .Value(vm, x => x, BindingMode.OneWay, AppConverters.ObjectOwnerName),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Status!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Node!)
                    .Value(vm.Spec?.NodeName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_PodIp!)
                    .Value(vm.Status?.PodIP ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Service_Account!)
                    .Value(vm.Spec?.ServiceAccountName ?? ""),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Metadata!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new CollectionItem()
                                    .Key(Assets.Resources.PodPropertiesView_Labels!)
                                    .Value(vm.Metadata?.Labels ?? new Dictionary<string, string>())
                                    .ItemTemplate(CreateKeyValueTemplate()),
                                new CollectionItem()
                                    .Key(Assets.Resources.PodPropertiesView_Annotations!)
                                    .Value(vm.Metadata?.Annotations ?? new Dictionary<string, string>())
                                    .ItemTemplate(CreateKeyValueTemplate()))),
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_Pod_IPs!)
                                    .Value(vm.Status?.PodIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_QoS_Class!)
                                    .Value(vm.Status?.QosClass ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Tolerations!)
                                    .Value(vm.Spec?.Tolerations?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Secrets!)
                                    .Value(vm.Spec?.ImagePullSecrets?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_Pod_Volumes!)
                                    .Value(vm.Spec?.Volumes?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_InitContainers!)
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.InitContainers, BindingMode.OneWay, KubeUI.Avalonia.Converters.NotEmptyCollectionConverter.Instance)
                    .Content(CreateContainers(vm.Spec?.InitContainers ?? [])),
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_EphemeralContainers!)
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.EphemeralContainers, BindingMode.OneWay, KubeUI.Avalonia.Converters.NotEmptyCollectionConverter.Instance)
                    .Content(CreateEphemeralContainers(vm.Spec?.EphemeralContainers ?? [])),
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_Containers!)
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
                    .Key(Assets.Resources.Shared_Name!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Image!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_PullPolicy!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(Assets.Resources.Shared_Ports!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Environment!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Mounts!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Command!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Arguments!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Requests!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(Assets.Resources.Shared_Limits!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }

    private static StackPanel CreateEphemeralContainerTemplate(V1EphemeralContainer container)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Name!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Image!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_PullPolicy!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(Assets.Resources.Shared_Ports!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Environment!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Mounts!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Command!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Arguments!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(Assets.Resources.PodPropertiesView_Requests!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(Assets.Resources.Shared_Limits!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }
}
