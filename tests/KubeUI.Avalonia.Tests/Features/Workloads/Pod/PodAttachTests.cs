using System.Net.WebSockets;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodAttachTests
{
    [AvaloniaFact]
    public void pod_config_exposes_attach_menu_for_running_containers()
    {
        var config = Application.Current.GetTestServices().GetRequiredService<V1PodConfig>();
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
        MenuItemViewModel attachMenu = items.Single(x => x.Title?.Equals("Attach") == true);

        attachMenu.Items.ShouldNotBeNull();

        List<MenuItemViewModel> attachGroups = attachMenu.Items.Cast<MenuItemViewModel>().ToList();
        attachGroups.Select(x => x.Title).ShouldContain("Init");
        attachGroups.Select(x => x.Title).ShouldContain("Normal");
        attachGroups.Select(x => x.Title).ShouldContain("Ephemeral");
    }

    [AvaloniaFact]
    public void pod_config_exposes_init_ephemeral_groups_for_debug_and_port_forwarding()
    {
        var config = Application.Current.GetTestServices().GetRequiredService<V1PodConfig>();

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

        MenuItemViewModel debugMenu = items.Single(x => x.Title == Assets.Resources.V1PodConfig_DebugContainer);
        List<MenuItemViewModel> debugGroups = debugMenu.Items!.Cast<MenuItemViewModel>().ToList();
        debugGroups.Select(x => x.Title).ShouldContain("Init");
        debugGroups.Select(x => x.Title).ShouldContain("Normal");
        debugGroups.Select(x => x.Title).ShouldContain("Ephemeral");

        MenuItemViewModel portForwardMenu = items.Single(x => x.Title == "Port Forwarding");
        List<MenuItemViewModel> portForwardGroups = portForwardMenu.Items!.Cast<MenuItemViewModel>().ToList();
        portForwardGroups.Select(x => x.Title).ShouldContain("Init");
        portForwardGroups.Select(x => x.Title).ShouldContain("Normal");
        portForwardGroups.Select(x => x.Title).ShouldContain("Ephemeral");

        var initContainer = portForwardGroups.Single(x => x.Title == "Init").Items!.Cast<MenuItemViewModel>().Single(x => x.Title == "init");
        initContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Title).ShouldContain("metrics - 9000");

        var normalContainer = portForwardGroups.Single(x => x.Title == "Normal").Items!.Cast<MenuItemViewModel>().Single(x => x.Title == "app");
        normalContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Title).ShouldContain("http - 8080");

        var ephemeralContainer = portForwardGroups.Single(x => x.Title == "Ephemeral").Items!.Cast<MenuItemViewModel>().Single(x => x.Title == "debug");
        ephemeralContainer.Items!.Cast<MenuItemViewModel>().Select(x => x.Title).ShouldContain("probe - 7777");
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
    public void console_input_stops_after_stream_write_aborts()
    {
        var services = Application.Current.GetTestServices();
        var settings = services.GetRequiredService<ISettingsService>();
        var logger = services.GetRequiredService<ILogger<PodConsoleViewModel>>();
        using var stream = new ThrowingWriteStream(new WebSocketException(WebSocketError.InvalidState));

        using PodConsoleViewModel viewModel = new(logger, settings);
        viewModel.SetStreamsForTesting(stream: stream);

        viewModel.WriteInput("ls"u8.ToArray());
        viewModel.IsDisconnected.ShouldBeTrue();
        stream.WriteCallCount.ShouldBe(1);

        viewModel.WriteInput("pwd"u8.ToArray());
        stream.WriteCallCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public void console_resize_stops_after_stream_write_aborts()
    {
        var services = Application.Current.GetTestServices();
        var settings = services.GetRequiredService<ISettingsService>();
        var logger = services.GetRequiredService<ILogger<PodConsoleViewModel>>();
        using var stream = new ThrowingWriteStream(new IOException());

        using PodConsoleViewModel viewModel = new(logger, settings);
        viewModel.SetStreamsForTesting(refreshStream: stream);

        viewModel.SendResize(80, 24);
        viewModel.IsDisconnected.ShouldBeTrue();
        stream.WriteCallCount.ShouldBe(1);

        viewModel.SendResize(120, 32);
        stream.WriteCallCount.ShouldBe(1);
    }

    private static async Task AssertConnectionModeAsync(bool useAttach, ConnectionMethod expectedMethod)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var runtime = workspace.Runtime;
        runtime.Name = "pod-attach-test";
        runtime.Connected = true;
        runtime.Status = ClusterStatus.Connected;
        var settings = services.GetRequiredService<ISettingsService>();
        var logger = services.GetRequiredService<ILogger<PodConsoleViewModel>>();

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

        using var webSocket = new FakeKubernetesWebSocket();
        var webSocketBuilder = new FakeKubernetesWebSocketBuilder(_ => webSocket);
        using var client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://pod-attach-test" })
        {
            CreateWebSocketBuilder = () => webSocketBuilder,
        };

        runtime.Client = client;

        using PodConsoleViewModel viewModel = new(logger, settings)
        {
            Cluster = workspace,
            Object = pod,
            ContainerName = "app",
            UseAttach = useAttach,
        };

        WebSocket result = await viewModel.OpenConnectionAsync();
        result.ShouldBe(webSocket);

        if (expectedMethod == ConnectionMethod.Attach)
        {
            webSocketBuilder.ConnectedUris.Count.ShouldBe(1);
            webSocketBuilder.ConnectedUris[0].AbsolutePath.ShouldBe("/api/v1/namespaces/default/pods/pod-1/attach");
            webSocketBuilder.ConnectedUris[0].Query.ShouldContain("container=app");
            return;
        }

        webSocketBuilder.ConnectedUris.Count.ShouldBe(1);
        webSocketBuilder.ConnectedUris[0].AbsolutePath.ShouldBe("/api/v1/namespaces/default/pods/pod-1/exec");
        webSocketBuilder.ConnectedUris[0].Query.ShouldContain("command=sh");
        webSocketBuilder.ConnectedUris[0].Query.ShouldContain("command=-c");
        webSocketBuilder.ConnectedUris[0].Query.ShouldContain("command=clear%3B");
    }

    private enum ConnectionMethod
    {
        Exec,
        Attach,
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
