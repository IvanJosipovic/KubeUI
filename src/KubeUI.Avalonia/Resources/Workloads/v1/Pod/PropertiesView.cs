using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Pod>>, IResourcePropertiesRefreshable<V1Pod>
{
    private V1Pod? _currentPod;
    private ExpandableSection _initContainersSection = null!;
    private ItemsControl _initContainers = null!;
    private ExpandableSection _ephemeralContainersSection = null!;
    private ItemsControl _ephemeralContainers = null!;
    private ItemsControl _containers = null!;

    protected override object Build(ResourcePropertiesViewModel<V1Pod> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var pod = vm.Object ?? throw new InvalidOperationException("Pod properties require a resource.");

        _currentPod = pod;
        _initContainers = CreateInitContainers(pod.Spec?.InitContainers ?? []);
        _initContainersSection = new ExpandableSection()
            .Header(Assets.Resources.PodPropertiesView_InitContainers!)
            .IsExpanded(true)
            .IsVisible(vm, viewModel => viewModel.Object!.Spec!.InitContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
            .Content(_initContainers);
        _ephemeralContainers = CreateEphemeralContainers(pod.Spec?.EphemeralContainers ?? []);
        _ephemeralContainersSection = new ExpandableSection()
            .Header(Assets.Resources.PodPropertiesView_EphemeralContainers!)
            .IsExpanded(true)
            .IsVisible(vm, viewModel => viewModel.Object!.Spec!.EphemeralContainers, BindingMode.OneWay, NotEmptyCollectionConverter.Instance)
            .Content(_ephemeralContainers);
        _containers = CreateContainers(pod, pod.Spec?.Containers ?? []);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_ControlledBy!)
                    .BindValue(vm, static resource => resource?.Metadata?.OwnerReferences?.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A"),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Status!)
                    .BindValue(vm, static resource => resource?.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Node!)
                    .BindValue(vm, static resource => resource?.Spec?.NodeName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_PodIp!)
                    .BindValue(vm, static resource => resource?.Status?.PodIP ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PodPropertiesView_Service_Account!)
                    .BindValue(vm, static resource => resource?.Spec?.ServiceAccountName ?? ""),
                new MetricsControl { DataContext = pod },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Metadata!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new CollectionItem()
                                    .Key(Assets.Resources.PodPropertiesView_Labels!)
                                    .BindValue(vm, static resource => resource?.Metadata?.Labels)
                                    .ItemTemplate(CreateKeyValueTemplate()),
                                new CollectionItem()
                                    .Key(Assets.Resources.PodPropertiesView_Annotations!)
                                    .BindValue(vm, static resource => resource?.Metadata?.Annotations)
                                    .ItemTemplate(CreateKeyValueTemplate()))),
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_Pod_IPs!)
                                    .BindValue(vm, static resource => resource?.Status?.PodIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_QoS_Class!)
                                    .BindValue(vm, static resource => resource?.Status?.QosClass ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Tolerations!)
                                    .BindValue(vm, static resource => resource?.Spec?.Tolerations?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Secrets!)
                                    .BindValue(vm, static resource => resource?.Spec?.ImagePullSecrets?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.PodPropertiesView_Pod_Volumes!)
                                    .BindValue(vm, static resource => resource?.Spec?.Volumes?.Count ?? 0))),
                _initContainersSection,
                _ephemeralContainersSection,
                new ExpandableSection()
                    .Header(Assets.Resources.PodPropertiesView_Containers!)
                    .IsExpanded(true)
                    .Content(_containers));
    }

    public void Refresh(V1Pod resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var previousContainers = _currentPod?.Spec?.Containers ?? [];
        var currentContainers = resource.Spec?.Containers ?? [];
        var preserveContainerRows = previousContainers.Select(static container => container.Name)
            .SequenceEqual(currentContainers.Select(static container => container.Name), StringComparer.Ordinal);

        var initContainers = resource.Spec?.InitContainers ?? [];
        _initContainers.ItemsSource = initContainers;
        _initContainersSection.IsVisible = initContainers.Count > 0;

        var ephemeralContainers = resource.Spec?.EphemeralContainers ?? [];
        _ephemeralContainers.ItemsSource = ephemeralContainers;
        _ephemeralContainersSection.IsVisible = ephemeralContainers.Count > 0;

        _currentPod = resource;
        if (!preserveContainerRows)
        {
            _containers.ItemsSource = currentContainers;
        }

        RefreshContainerRows(resource, currentContainers);
    }

    private void RefreshContainerRows(V1Pod resource, IEnumerable<V1Container> containers)
    {
        var updatedContainers = new Dictionary<string, V1Container>(StringComparer.Ordinal);
        foreach (var container in containers)
        {
            if (!string.IsNullOrWhiteSpace(container.Name))
            {
                updatedContainers[container.Name] = container;
            }
        }

        var metricsControls = ResourcePropertiesViewRefresher.EnumerateControls(this)
            .OfType<MetricsControl>()
            .Where(static metricsControl => !string.IsNullOrWhiteSpace(metricsControl.Container?.Name))
            .ToArray();

        foreach (var metricsControl in metricsControls)
        {
            if (updatedContainers.TryGetValue(metricsControl.Container!.Name!, out var container))
            {
                metricsControl.UpdateResourceSnapshot(resource, resource, container);
                if ((metricsControl.GetVisualParent<Control>() ?? GetLogicalParentControl(metricsControl)) is Grid row
                    && row.Children.Count > 1)
                {
                    var properties = CreateContainerPropertiesTemplate(container);
                    Grid.SetRow(properties, 1);
                    row.Children[1] = properties;
                }
            }
        }
    }

    private static Control? GetLogicalParentControl(Control control)
    {
        var current = (control as ILogical)?.LogicalParent;
        while (current is not null)
        {
            if (current is Control parent)
            {
                return parent;
            }

            current = current.LogicalParent;
        }

        return null;
    }

    private static IDataTemplate CreateKeyValueTemplate()
    {
        return new FuncDataTemplate<KeyValuePair<string, string>>((entry, _) =>
            new SelectableTextBlock()
                .Text($"{entry.Key}={entry.Value}"));
    }

    private static ItemsControl CreateInitContainers(IEnumerable<V1Container> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1Container>((container, _) => CreateContainerPropertiesTemplate(container)));
    }

    private ItemsControl CreateContainers(V1Pod pod, IEnumerable<V1Container> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1Container>((container, _) => CreateContainerTemplate(_currentPod ?? pod, container)));
    }

    private static ItemsControl CreateEphemeralContainers(IEnumerable<V1EphemeralContainer> containers)
    {
        return new ItemsControl()
            .ItemsSource(containers)
            .ItemTemplate(new FuncDataTemplate<V1EphemeralContainer>((container, _) => CreateEphemeralContainerTemplate(container)));
    }

    private static Grid CreateContainerTemplate(V1Pod pod, V1Container container)
    {
        var metricsControl = new MetricsControl
        {
            DataContext = pod,
            Pod = pod,
            Container = container,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Grid.SetRow(metricsControl, 0);

        var properties = CreateContainerPropertiesTemplate(container);
        Grid.SetRow(properties, 1);

        return new Grid()
            .Rows("Auto,*")
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Children(metricsControl, properties);
    }

    private static StackPanel CreateContainerPropertiesTemplate(V1Container container)
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
