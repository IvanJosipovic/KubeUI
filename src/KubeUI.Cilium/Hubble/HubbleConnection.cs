using System.Reactive.Linq;
using System.Net.WebSockets;
using Grpc.Core;
using Grpc.Net.Client;
using Pb = KubeUI.Cilium.Hubble.Models;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;

namespace KubeUI.Cilium.Hubble;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public sealed class HubbleConnection : IAsyncDisposable
{
    private readonly IClusterRuntime _cluster;
    private readonly ILogger<HubbleConnection> _logger;
    private GrpcChannel? _channel;
    private Pb.Observer.ObserverClient? _client;
    private CancellationTokenSource? _cts;
    private PortForwarder? _portForwarder;
    private int _localPort;

    public event Action<ConnectionStatus>? StatusChanged;
    public IObservable<FlowViewModel> Flows { get; private set; } = Observable.Empty<FlowViewModel>();

    public HubbleConnection(IClusterRuntime cluster, ILogger<HubbleConnection> logger)
    {
        _cluster = cluster;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        SetStatus(ConnectionStatus.Connecting);

        try
        {
            _portForwarder = new PortForwarder(_cluster, "kube-system", 0);
            _portForwarder.SetService("hubble-relay", 443);
            _portForwarder.Start();

            // Wait for port forward to be ready
            await WaitForPortForwardReadyAsync(ct);

            _localPort = _portForwarder.LocalPort;

            _channel = GrpcChannel.ForAddress($"http://localhost:{_localPort}", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Insecure,
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
                }
            });

            _client = new Pb.Observer.ObserverClient(_channel);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            var request = new Pb.GetFlowsRequest
            {
                Follow = true,
                Number = 10000
            };

            var call = _client.GetFlows(request, cancellationToken: _cts.Token);

            Flows = Observable.Create<FlowViewModel>(async observer =>
            {
                try
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync(_cts.Token))
                    {
                        if (response.Flow != null)
                        {
                            observer.OnNext(new FlowViewModel(response.Flow));
                        }
                        else if (response.LostEvents != null)
                        {
                            _logger.LogWarning("Lost {Count} events from {Source}", response.LostEvents.NumEventsLost, response.LostEvents.Source);
                        }
                        else if (response.NodeStatus != null)
                        {
                            _logger.LogDebug("Node status: {Node} - {State}", response.NodeName, response.NodeStatus.StateChange);
                        }
                    }
                    observer.OnCompleted();
                }
                catch (OperationCanceledException) when (_cts?.IsCancellationRequested == true)
                {
                    // Expected on disposal
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    // Expected on disposal
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading flow stream");
                    observer.OnError(ex);
                }
            }).Publish().RefCount();

            SetStatus(ConnectionStatus.Connected);
            _logger.LogInformation("Hubble connection established on port {Port}", _localPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Hubble connection");
            SetStatus(ConnectionStatus.Error);
            throw;
        }
    }

    private async Task WaitForPortForwardReadyAsync(CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(10);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            if (_portForwarder?.Status == "Active" && _portForwarder.LocalPort > 0)
            {
                // Test the connection
                try
                {
                    using var tcpClient = new System.Net.Sockets.TcpClient();
                    await tcpClient.ConnectAsync("127.0.0.1", _portForwarder.LocalPort, ct);
                    return;
                }
                catch
                {
                    // Not ready yet
                }
            }

            await Task.Delay(200, ct);
        }

        throw new TimeoutException("Port forward to hubble-relay did not become ready in time");
    }

    private void SetStatus(ConnectionStatus status)
    {
        StatusChanged?.Invoke(status);
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        
        if (_channel != null)
        {
            await _channel.ShutdownAsync().ConfigureAwait(false);
            _channel.Dispose();
        }

        _portForwarder?.Dispose();
        SetStatus(ConnectionStatus.Disconnected);
    }
}