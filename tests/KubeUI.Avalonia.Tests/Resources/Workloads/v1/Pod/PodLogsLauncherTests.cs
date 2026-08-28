using System.Net;
using System.Text;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogsLauncherTests
{
    [AvaloniaFact]
    public async Task Deployment_launch_seeds_related_resources_and_streams_pod_logs()
    {
        V1Deployment deployment = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "api",
                NamespaceProperty = "default",
                Uid = "deployment-uid",
            },
        };
        V1ReplicaSet replicaSet = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "api-rs",
                NamespaceProperty = "default",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new V1OwnerReference
                    {
                        ApiVersion = "apps/v1",
                        Kind = V1Deployment.KubeKind,
                        Name = deployment.Name(),
                        Uid = deployment.Uid(),
                        Controller = true,
                    },
                ],
            },
        };
        V1Pod pod = CreatePod("api-pod");
        pod.Metadata!.Uid = "pod-uid";
        pod.Metadata.OwnerReferences =
        [
            new V1OwnerReference
            {
                ApiVersion = "apps/v1",
                Kind = V1ReplicaSet.KubeKind,
                Name = replicaSet.Name(),
                Uid = replicaSet.Uid(),
                Controller = true,
            },
        ];
        pod.Status = new V1PodStatus
        {
            Phase = "Running",
            ContainerStatuses =
            [
                new V1ContainerStatus
                {
                    Name = "app",
                    State = new V1ContainerState { Running = new V1ContainerStateRunning() },
                },
            ],
        };

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.HttpHandlerFactory = () => [new DeploymentPodLogHandler(replicaSet, pod)];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        workspace.Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1ReplicaSet>()).ShouldBeFalse();
        workspace.Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1Pod>()).ShouldBeFalse();

        IServiceProvider services = Application.Current.GetTestServices();
        IFactory factory = services.GetRequiredService<IFactory>();
        PodLogsLauncher launcher = new(services, factory, services.GetRequiredService<ILogger<PodLogsLauncher>>());

        await launcher.LaunchAsync(workspace, deployment, V1Deployment.KubeKind);

        using PodLogsViewModel viewModel = factory
            .FindDockableById($"PodLogsViewModel-{workspace.Runtime.Name}-Deployment-default-api-all")
            .ShouldBeOfType<PodLogsViewModel>();
        await TestWait.UntilAsync(
            () => viewModel.Logs.Text.Contains("deployment pod line", StringComparison.Ordinal),
            3000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
        workspace.Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1ReplicaSet>()).ShouldBeTrue();
        workspace.Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1Pod>()).ShouldBeTrue();
        viewModel.AvailablePods.Select(item => item.Name()).ShouldContain("api-pod");
        viewModel.Title.ShouldBe("Deployment Logs");
        viewModel.IsControllerScope.ShouldBeTrue();
        viewModel.SelectedPodItems.Single().IsAll.ShouldBeTrue();
        viewModel.SelectedContainerItems.Single().IsAll.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Rejected_docking_does_not_throw()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        IFactory factory = services.GetRequiredService<IFactory>();
        PodLogsLauncher launcher = new(services, factory, services.GetRequiredService<ILogger<PodLogsLauncher>>());
        V1Pod pod = CreatePod("rejected");

        await launcher.LaunchAsync(workspace, pod, "Pod");
        using PodLogsViewModel viewModel = factory.FindDockableById($"PodLogsViewModel-{workspace.Runtime.Name}-Pod-default-rejected-all")
            .ShouldBeOfType<PodLogsViewModel>();
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
        using PodLogsViewModel viewModel = dockable.ShouldBeOfType<PodLogsViewModel>();
        viewModel.Cluster.ShouldBe(workspace.Runtime);
        viewModel.Object.ShouldBe(pod);
        viewModel.ContainerName.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Launch_uses_the_runtime_kind_for_known_resources()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        IFactory factory = services.GetRequiredService<IFactory>();
        PodLogsLauncher launcher = new(services, factory, services.GetRequiredService<ILogger<PodLogsLauncher>>());
        V1Deployment resource = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "alertmanager",
                NamespaceProperty = "monitoring",
            },
        };

        await launcher.LaunchAsync(workspace, resource, resource.Name());

        using PodLogsViewModel viewModel = factory
            .FindDockableById($"PodLogsViewModel-{workspace.Runtime.Name}-Deployment-monitoring-alertmanager-all")
            .ShouldBeOfType<PodLogsViewModel>();
        viewModel.Title.ShouldBe("Deployment Logs");
    }

    private static V1Pod CreatePod(string name)
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
        };
    }

    private sealed class DeploymentPodLogHandler(V1ReplicaSet replicaSet, V1Pod pod) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isDeploymentWrite = path.Contains("/deployments", StringComparison.Ordinal)
                && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put);
            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent("deployment pod line\n", Encoding.UTF8, "text/plain"),
                };
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (isDeploymentWrite)
            {
                await SendResourceAsync(replicaSet, "apis/apps/v1", "replicasets", cancellationToken).ConfigureAwait(false);
                await SendResourceAsync(pod, "api/v1", "pods", cancellationToken).ConfigureAwait(false);
            }

            return response;
        }

        private async Task SendResourceAsync<T>(
            T resource,
            string apiPath,
            string resourcePath,
            CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            using HttpRequestMessage request = new(
                HttpMethod.Post,
                $"http://fake-kubernetes/{apiPath}/namespaces/default/{resourcePath}")
            {
                Content = new StringContent(KubernetesJson.Serialize(resource), Encoding.UTF8, "application/json"),
            };
            using HttpMessageInvoker invoker = new(InnerHandler!, disposeHandler: false);
            await invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
