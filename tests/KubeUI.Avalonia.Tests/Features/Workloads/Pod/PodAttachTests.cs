using System.Net.WebSockets;
using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodAttachTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void pod_config_exposes_attach_menu_for_running_containers()
    {
        var config = TestApp.CurrentServices!.GetRequiredService<V1PodConfig>();
        config.CustomPermissions().ShouldContain((Verb.Create, "attach"));

        var pod = new V1Pod
        {
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app"
                    }
                ],
                EphemeralContainers =
                [
                    new V1EphemeralContainer
                    {
                        Name = "debug"
                    }
                ]
            }
        };

        List<MenuItemViewModel> items = config.GetCustomMenuItems(new[] { pod }).ToList();
        MenuItemViewModel attachMenu = items.Single(x => x.Header?.Equals("Attach") == true);

        attachMenu.Items.ShouldNotBeNull();

        List<MenuItemViewModel> attachGroups = attachMenu.Items.Cast<MenuItemViewModel>().ToList();
        attachGroups.Select(x => x.Header).ShouldContain("Normal");
        attachGroups.Select(x => x.Header).ShouldContain("Ephemeral");
    }

    [AvaloniaFact]
    public async Task exec_mode_uses_pod_exec_websocket()
    {
        await AssertConnectionModeAsync(useAttach: false, expectedMethod: ConnectionMethod.Exec);
    }

    [AvaloniaFact]
    public async Task attach_mode_uses_pod_attach_websocket()
    {
        await AssertConnectionModeAsync(useAttach: true, expectedMethod: ConnectionMethod.Attach);
    }

    private static async Task AssertConnectionModeAsync(bool useAttach, ConnectionMethod expectedMethod)
    {
        var runtime = new TestCluster();
        var workspace = runtime.CreateWorkspace();
        var settings = TestApp.CurrentServices!.GetRequiredService<KubeUI.Avalonia.Services.Settings.ISettingsService>();
        var logger = TestApp.CurrentServices.GetRequiredService<ILogger<PodConsoleViewModel>>();

        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                    },
                ],
            },
        };

        Mock<IKubernetes> client = new();
        Mock<WebSocket> webSocket = new();

        client
            .Setup(x => x.WebSocketNamespacedPodExecAsync(
                pod.Name(),
                pod.Namespace(),
                It.Is<IEnumerable<string>>(command => command.SequenceEqual(new[]
                {
                    "sh",
                    "-c",
                    "clear; (bash || ash || sh || echo 'No Shell Found!')",
                })),
                "app",
                true,
                true,
                true,
                true,
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(webSocket.Object);

        client
            .Setup(x => x.WebSocketNamespacedPodAttachAsync(
                pod.Name(),
                pod.Namespace(),
                "app",
                true,
                true,
                true,
                true,
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(webSocket.Object);

        workspace.Client = client.Object;

        PodConsoleViewModel viewModel = new(logger, settings)
        {
            Cluster = workspace,
            Object = pod,
            ContainerName = "app",
            UseAttach = useAttach,
        };

        WebSocket result = await viewModel.OpenConnectionAsync();
        result.ShouldBe(webSocket.Object);

        if (expectedMethod == ConnectionMethod.Attach)
        {
            client.Verify(x => x.WebSocketNamespacedPodAttachAsync(
                pod.Name(),
                pod.Namespace(),
                "app",
                true,
                true,
                true,
                true,
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(x => x.WebSocketNamespacedPodExecAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()), Times.Never);
            return;
        }

        client.Verify(x => x.WebSocketNamespacedPodExecAsync(
            pod.Name(),
            pod.Namespace(),
            It.Is<IEnumerable<string>>(command => command.SequenceEqual(new[]
            {
                "sh",
                "-c",
                "clear; (bash || ash || sh || echo 'No Shell Found!')",
            })),
            "app",
            true,
            true,
            true,
            true,
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, List<string>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        client.Verify(x => x.WebSocketNamespacedPodAttachAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, List<string>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private enum ConnectionMethod
    {
        Exec,
        Attach,
    }
}
