using System.Net.WebSockets;
using System.Reflection;
using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Assets;
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
        attachGroups.Select(x => x.Header).ShouldContain("Init");
        attachGroups.Select(x => x.Header).ShouldContain("Normal");
        attachGroups.Select(x => x.Header).ShouldContain("Ephemeral");
    }

    [AvaloniaFact]
    public void pod_config_exposes_init_ephemeral_groups_for_debug_and_port_forwarding()
    {
        var config = TestApp.CurrentServices!.GetRequiredService<V1PodConfig>();

        var pod = new V1Pod
        {
            Spec = new V1PodSpec
            {
                InitContainers =
                [
                    new V1Container
                    {
                        Name = "init",
                        Ports =
                        [
                            new V1ContainerPort
                            {
                                Name = "metrics",
                                ContainerPort = 9000,
                                Protocol = "TCP",
                            }
                        ],
                    }
                ],
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Ports =
                        [
                            new V1ContainerPort
                            {
                                Name = "http",
                                ContainerPort = 8080,
                                Protocol = "TCP",
                            }
                        ],
                    }
                ],
                EphemeralContainers =
                [
                    new V1EphemeralContainer
                    {
                        Name = "debug",
                        Ports =
                        [
                            new V1ContainerPort
                            {
                                Name = "probe",
                                ContainerPort = 7777,
                                Protocol = "TCP",
                            }
                        ],
                    }
                ]
            }
        };

        List<MenuItemViewModel> items = config.GetCustomMenuItems(new[] { pod }).ToList();

        MenuItemViewModel debugMenu = items.Single(x => x.Header == Assets.Resources.V1PodConfig_DebugContainer);
        List<MenuItemViewModel> debugGroups = debugMenu.Items!.Cast<MenuItemViewModel>().ToList();
        debugGroups.Select(x => x.Header).ShouldContain("Init");
        debugGroups.Select(x => x.Header).ShouldContain("Normal");
        debugGroups.Select(x => x.Header).ShouldContain("Ephemeral");

        MenuItemViewModel portForwardMenu = items.Single(x => x.Header == "Port Forwarding");
        List<MenuItemViewModel> portForwardGroups = portForwardMenu.Items!.Cast<MenuItemViewModel>().ToList();
        portForwardGroups.Select(x => x.Header).ShouldContain("Init");
        portForwardGroups.Select(x => x.Header).ShouldContain("Normal");
        portForwardGroups.Select(x => x.Header).ShouldContain("Ephemeral");

        var initContainer = portForwardGroups.Single(x => x.Header == "Init").Items!.Cast<MenuItemViewModel>().Single(x => x.Header == "init");
        initContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Header).ShouldContain("metrics - 9000");

        var normalContainer = portForwardGroups.Single(x => x.Header == "Normal").Items!.Cast<MenuItemViewModel>().Single(x => x.Header == "app");
        normalContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Header).ShouldContain("http - 8080");

        var ephemeralContainer = portForwardGroups.Single(x => x.Header == "Ephemeral").Items!.Cast<MenuItemViewModel>().Single(x => x.Header == "debug");
        ephemeralContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Header).ShouldContain("probe - 7777");
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

    [AvaloniaFact]
    public void console_input_disconnects_when_stream_write_aborts()
    {
        var settings = TestApp.CurrentServices!.GetRequiredService<KubeUI.Avalonia.Services.Settings.ISettingsService>();
        var logger = TestApp.CurrentServices.GetRequiredService<ILogger<PodConsoleViewModel>>();
        var stream = new ThrowingWriteStream(new WebSocketException(WebSocketError.InvalidState));

        PodConsoleViewModel viewModel = new(logger, settings);
        SetPrivateField(viewModel, "_stream", stream);

        viewModel.WriteInput("ls"u8.ToArray());
        viewModel.IsDisconnected.ShouldBeTrue();
        stream.WriteCallCount.ShouldBe(1);

        viewModel.WriteInput("pwd"u8.ToArray());
        stream.WriteCallCount.ShouldBe(1);
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

    private static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        FieldInfo? field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        field!.SetValue(target, value);
    }

    private sealed class ThrowingWriteStream(Exception exception)
        : Stream
    {
        private readonly Exception _exception = exception;

        public int WriteCallCount { get; private set; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteCallCount++;
            throw _exception;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            WriteCallCount++;
            throw _exception;
        }
    }
}
