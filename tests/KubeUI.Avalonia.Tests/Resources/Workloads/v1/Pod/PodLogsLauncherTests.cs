using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogsLauncherTests
{
    [AvaloniaFact]
    public async Task Rejected_docking_does_not_throw()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        IFactory factory = services.GetRequiredService<IFactory>();
        PodLogsLauncher launcher = new(services, factory, services.GetRequiredService<ILogger<PodLogsLauncher>>());
        V1Pod pod = CreatePod("rejected");

        await launcher.LaunchAsync(workspace, pod, "Pod");
        await Should.NotThrowAsync(() => launcher.LaunchAsync(workspace, pod, "Pod"));
    }

    [AvaloniaFact]
    public async Task Accepted_docking_initializes_pod_logs_view_model()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        IFactory factory = services.GetRequiredService<IFactory>();
        PodLogsLauncher launcher = new(services, factory, services.GetRequiredService<ILogger<PodLogsLauncher>>());
        V1Pod pod = CreatePod("accepted");

        await launcher.LaunchAsync(workspace, pod, "Pod");

        IDockable dockable = factory.FindDockableById($"PodLogsViewModel-{workspace.Runtime.Name}-Pod-default-accepted-all").ShouldNotBeNull();
        PodLogsViewModel viewModel = dockable.ShouldBeOfType<PodLogsViewModel>();
        viewModel.Cluster.ShouldBe(workspace.Runtime);
        viewModel.Object.ShouldBe(pod);
        viewModel.ContainerName.ShouldBeEmpty();
    }

    private static V1Pod CreatePod(string name)
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
        };
    }
}
