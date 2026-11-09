using Avalonia.Controls.Templates;
using k8s.Models;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties;
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
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_ControlledBy.CurrentValue()!)
                    .Value(vm.Metadata.OwnerReferences?.FirstOrDefault(x => x.Controller == true)?.Name ?? FoundationLanguageManager.Instance.Shared_NotAvailable.CurrentValue()!),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Status.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Node.CurrentValue()!)
                    .Value(vm.Spec?.NodeName ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_PodIp.CurrentValue()!)
                    .Value(vm.Status?.PodIP ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Service_Account.CurrentValue()!)
                    .Value(vm.Spec?.ServiceAccountName ?? ""),
                new MetricsControl { DataContext = vm, Pod = vm },
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Metadata.ToBinding(),
                    new CollectionItem()
                        .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Labels.CurrentValue()!)
                        .Value(vm.Metadata?.Labels ?? new Dictionary<string, string>())
                        .ItemTemplate(CreateKeyValueTemplate()),
                    new CollectionItem()
                        .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Annotations.CurrentValue()!)
                        .Value(vm.Metadata?.Annotations ?? new Dictionary<string, string>())
                        .ItemTemplate(CreateKeyValueTemplate())),
                ResourcePropertiesSection.Create(
                    ResourcesLanguageManager.Instance.PodPropertiesView_Status.ToBinding(),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Pod_IPs.CurrentValue()!)
                        .Value(vm.Status?.PodIPs?.Count ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.PodPropertiesView_QoS_Class.CurrentValue()!)
                        .Value(vm.Status?.QosClass ?? ""),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                        .Value(vm.Status?.Conditions?.Count ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Tolerations.CurrentValue()!)
                        .Value(vm.Spec?.Tolerations?.Count ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Secrets.CurrentValue()!)
                        .Value(vm.Spec?.ImagePullSecrets?.Count ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Pod_Volumes.CurrentValue()!)
                        .Value(vm.Spec?.Volumes?.Count ?? 0)),
                new ExpandableSection()
                    .Header(ResourcesLanguageManager.Instance.PodPropertiesView_InitContainers.ToBinding())
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.InitContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
                    .Content(CreateContainers(vm, vm.Spec?.InitContainers ?? [], includeMetrics: false)),
                new ExpandableSection()
                    .Header(ResourcesLanguageManager.Instance.PodPropertiesView_EphemeralContainers.ToBinding())
                    .IsExpanded(true)
                    .IsVisible(vm, x => x.Spec.EphemeralContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
                    .Content(CreateEphemeralContainers(vm.Spec?.EphemeralContainers ?? [])),
                new ExpandableSection()
                    .Header(ResourcesLanguageManager.Instance.PodPropertiesView_Containers.ToBinding())
                    .IsExpanded(true)
                    .Content(CreateContainers(vm, vm.Spec?.Containers ?? [], includeMetrics: true)));
    }

    private static IDataTemplate CreateKeyValueTemplate()
    {
        return new FuncDataTemplate<KeyValuePair<string, string>>((entry, _) =>
            new SelectableTextBlock()
                .Text($"{entry.Key}={entry.Value}"));
    }

    private static ItemsControl CreateContainers(V1Pod pod, IEnumerable<V1Container> containers, bool includeMetrics)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1Container>((container, _) => CreateContainerTemplate(pod, container, includeMetrics)));
    }

    private static ItemsControl CreateEphemeralContainers(IEnumerable<V1EphemeralContainer> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1EphemeralContainer>((container, _) => CreateEphemeralContainerTemplate(container)));
    }

    private static StackPanel CreateContainerTemplate(V1Pod pod, V1Container container, bool includeMetrics)
    {
        return new StackPanel()
            .Children(
                includeMetrics ? new MetricsControl { DataContext = pod, Pod = pod, Container = container } : new StackPanel(),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Name.CurrentValue()!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Image.CurrentValue()!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_PullPolicy.CurrentValue()!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Ports.CurrentValue()!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Environment.CurrentValue()!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Mounts.CurrentValue()!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Command.CurrentValue()!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Arguments.CurrentValue()!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Requests.CurrentValue()!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Limits.CurrentValue()!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }

    private static StackPanel CreateEphemeralContainerTemplate(V1EphemeralContainer container)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Name.CurrentValue()!)
                    .Value(container.Name ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Image.CurrentValue()!)
                    .Value(container.Image ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_PullPolicy.CurrentValue()!)
                    .Value(container.ImagePullPolicy ?? ""),
                new CollectionItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Ports.CurrentValue()!)
                    .Value(container.Ports ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1ContainerPort>((port, _) =>
                        new SelectableTextBlock()
                            .Text(port.ContainerPort.ToString()))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Environment.CurrentValue()!)
                    .Value(container.Env ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1EnvVar>((entry, _) =>
                        new SelectableTextBlock()
                            .Text($"{entry.Name}={entry.Value}"))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Mounts.CurrentValue()!)
                    .Value(container.VolumeMounts ?? [])
                    .ItemTemplate(new FuncDataTemplate<V1VolumeMount>((mount, _) =>
                        new SelectableTextBlock()
                            .Text(mount.MountPath ?? ""))),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Command.CurrentValue()!)
                    .Value(container.Command ?? []),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Arguments.CurrentValue()!)
                    .Value(container.Args ?? []),
                new CollectionItem()
                    .Key(ResourcesLanguageManager.Instance.PodPropertiesView_Requests.CurrentValue()!)
                    .Value(container.Resources?.Requests ?? new Dictionary<string, ResourceQuantity>()),
                new CollectionItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Limits.CurrentValue()!)
                    .Value(container.Resources?.Limits ?? new Dictionary<string, ResourceQuantity>()));
    }
}
