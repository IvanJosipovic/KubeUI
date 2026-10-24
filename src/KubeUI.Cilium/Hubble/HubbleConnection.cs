using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using DynamicData.Kernel;
using Grpc.Core;
using Grpc.Net.Client;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Pb = KubeUI.Cilium.Hubble.Models;
using k8s.Models;

namespace KubeUI.Cilium.Hubble;

/// <summary>Reports the current Hubble stream state.</summary>
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

/// <summary>Owns a Hubble Relay port forward and flow stream.</summary>
public sealed class HubbleConnection : IHubbleConnection
{
    private readonly IClusterRuntime _cluster;
    private readonly ILogger<HubbleConnection> _logger;
    private readonly HubbleConnectionOptions _options;
    private readonly CancellationTokenSource _lifetime = new();
    private GrpcChannel? _channel;
    private Pb.Observer.ObserverClient? _client;
    private PortForwarder? _portForwarder;
    private X509Certificate2? _trustedRootCertificate;
    private X509Certificate2? _clientCertificate;
    private int _startState;
    private int _disposeState;
    private ConnectionStatus _status = ConnectionStatus.Disconnected;

    /// <inheritdoc />
    public event Action<ConnectionStatus>? StatusChanged;

    /// <inheritdoc />
    public IObservable<FlowViewModel> Flows { get; private set; } = Observable.Empty<FlowViewModel>();

    /// <summary>Creates a connection using the configured relay settings.</summary>
    public HubbleConnection(
        IClusterRuntime cluster,
        ILogger<HubbleConnection> logger,
        HubbleConnectionOptions? options = null)
    {
        _cluster = cluster;
        _logger = logger;
        _options = options ?? new HubbleConnectionOptions();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _startState, 1, 0) != 0)
        {
            throw new InvalidOperationException("Hubble connection can only be started once.");
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetime.Token);

        SetStatus(ConnectionStatus.Connecting);
        try
        {
            ValidateOptions();
            await _cluster.SeedResource<V1Service>(true, linkedCancellation.Token).ConfigureAwait(false);
            var relayPort = ResolveRelayPort();
            var useTls = _options.UseTls ?? relayPort == 443;

            _portForwarder = new PortForwarder(_cluster, _options.Namespace, 0);
            _portForwarder.SetService(_options.ServiceName, relayPort);
            _portForwarder.Start();

            if (_portForwarder.Status != "Active" || _portForwarder.LocalPort <= 0)
            {
                throw new InvalidOperationException($"Unable to start port forward to {_options.Namespace}/{_options.ServiceName}.");
            }

            _channel = GrpcChannel.ForAddress(
                $"{(useTls ? "https" : "http")}://localhost:{_portForwarder.LocalPort}",
                new GrpcChannelOptions
                {
                    HttpHandler = CreateHttpHandler(useTls)
                });
            _client = new Pb.Observer.ObserverClient(_channel);
            Flows = Observable.Create<FlowViewModel>(ReadFlowsAsync).Publish().RefCount();
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            SetStatus(ConnectionStatus.Disconnected);
            await DisposeTransportAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Hubble connection");
            SetStatus(ConnectionStatus.Error);
            await DisposeTransportAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task ReadFlowsAsync(IObserver<FlowViewModel> observer, CancellationToken subscriptionToken)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            subscriptionToken,
            _lifetime.Token);
        try
        {
            var request = new Pb.GetFlowsRequest { Follow = true, Number = 10000 };
            using var call = _client!.GetFlows(
                request,
                cancellationToken: linkedCancellation.Token);

            await foreach (var response in call.ResponseStream.ReadAllAsync(linkedCancellation.Token))
            {
                SetStatus(ConnectionStatus.Connected);
                if (response.Flow != null)
                {
                    observer.OnNext(new FlowViewModel(response.Flow));
                }
                else if (response.LostEvents != null)
                {
                    _logger.LogWarning(
                        "Lost {Count} events from {Source}",
                        response.LostEvents.NumEventsLost,
                        response.LostEvents.Source);
                }
                else if (response.NodeStatus != null)
                {
                    _logger.LogDebug(
                        "Node status: {Node} - {State}",
                        response.NodeName,
                        response.NodeStatus.StateChange);
                }
            }

            if (!_lifetime.IsCancellationRequested)
            {
                SetStatus(ConnectionStatus.Disconnected);
                observer.OnCompleted();
            }
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            if (_lifetime.IsCancellationRequested)
            {
                SetStatus(ConnectionStatus.Disconnected);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled && linkedCancellation.IsCancellationRequested)
        {
            if (_lifetime.IsCancellationRequested)
            {
                SetStatus(ConnectionStatus.Disconnected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading flow stream");
            SetStatus(ConnectionStatus.Error);
            observer.OnError(ex);
        }
    }

    private int ResolveRelayPort()
    {
        var serviceLookup = _cluster.GetResourceSourceCache<V1Service>()
            .Lookup(new ResourceCacheKey(_options.Namespace, _options.ServiceName));
        var service = serviceLookup.HasValue ? serviceLookup.Value : null;
        if (service?.Spec?.Ports is not { Count: > 0 } ports)
        {
            throw new InvalidOperationException($"Hubble Relay service '{_options.Namespace}/{_options.ServiceName}' was not found or has no ports.");
        }

        if (_options.ServicePort is { } configuredPort)
        {
            if (ports.Any(port => port.Port == configuredPort))
            {
                return configuredPort;
            }

            throw new InvalidOperationException($"Hubble Relay service does not expose configured port {configuredPort}.");
        }

        var grpcPort = ports.FirstOrDefault(port => port.Port is 80 or 443)
            ?? ports.FirstOrDefault();
        if (grpcPort?.Port is not > 0)
        {
            throw new InvalidOperationException("Hubble Relay service has no usable gRPC port.");
        }

        return grpcPort.Port;
    }

    private SocketsHttpHandler CreateHttpHandler(bool useTls)
    {
        var handler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
        };

        if (!useTls)
        {
            return handler;
        }

        if (!string.IsNullOrWhiteSpace(_options.TlsServerName))
        {
            handler.SslOptions.TargetHost = _options.TlsServerName;
        }

        if (!string.IsNullOrWhiteSpace(_options.TrustedRootCertificatePem))
        {
            _trustedRootCertificate = X509Certificate2.CreateFromPem(_options.TrustedRootCertificatePem);
            handler.SslOptions.RemoteCertificateValidationCallback = ValidateRelayCertificate;
        }

        if (!string.IsNullOrWhiteSpace(_options.ClientCertificatePem)
            && !string.IsNullOrWhiteSpace(_options.ClientPrivateKeyPem))
        {
            _clientCertificate = X509Certificate2.CreateFromPem(
                _options.ClientCertificatePem,
                _options.ClientPrivateKeyPem);
            handler.SslOptions.ClientCertificates = new X509CertificateCollection { _clientCertificate };
        }

        return handler;
    }

    private bool ValidateRelayCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (certificate is null
            || (sslPolicyErrors & (SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable)) != 0
            || _trustedRootCertificate is null)
        {
            return false;
        }

        using X509Certificate2 serverCertificate = new(certificate);
        using X509Chain validationChain = new();
        validationChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        validationChain.ChainPolicy.CustomTrustStore.Add(_trustedRootCertificate);
        return validationChain.Build(serverCertificate);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Namespace))
        {
            throw new InvalidOperationException("Hubble Relay namespace cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(_options.ServiceName))
        {
            throw new InvalidOperationException("Hubble Relay service name cannot be empty.");
        }

        if (_options.ServicePort is <= 0)
        {
            throw new InvalidOperationException("Hubble Relay service port must be positive.");
        }

        var hasClientCertificate = !string.IsNullOrWhiteSpace(_options.ClientCertificatePem);
        var hasClientPrivateKey = !string.IsNullOrWhiteSpace(_options.ClientPrivateKeyPem);
        if (hasClientCertificate != hasClientPrivateKey)
        {
            throw new InvalidOperationException("Both the Hubble Relay client certificate and private key must be configured.");
        }
    }

    private void SetStatus(ConnectionStatus status)
    {
        if (_status == status)
        {
            return;
        }

        _status = status;
        StatusChanged?.Invoke(status);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        _lifetime.Cancel();
        await DisposeTransportAsync().ConfigureAwait(false);
        _lifetime.Dispose();
        SetStatus(ConnectionStatus.Disconnected);
    }

    private async ValueTask DisposeTransportAsync()
    {
        if (_channel is not null)
        {
            await _channel.ShutdownAsync().ConfigureAwait(false);
            _channel.Dispose();
            _channel = null;
        }

        _portForwarder?.Dispose();
        _portForwarder = null;
        _client = null;
        _trustedRootCertificate?.Dispose();
        _trustedRootCertificate = null;
        _clientCertificate?.Dispose();
        _clientCertificate = null;
    }
}
