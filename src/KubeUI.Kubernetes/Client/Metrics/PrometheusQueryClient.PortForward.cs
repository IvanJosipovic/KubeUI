using System.Net;
using System.Net.Sockets;

namespace KubeUI.Kubernetes;

public sealed partial class PrometheusQueryClient
{
    private static async Task EnsureServiceAvailableAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Namespace)
            || string.IsNullOrWhiteSpace(endpoint.ServiceName)
            || endpoint.ServicePort is not > 0)
        {
            throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
        }

        if (cluster.GetResource<k8s.Models.V1Service>(endpoint.Namespace, endpoint.ServiceName) is not null)
        {
            return;
        }

        var services = await k8s.CoreV1OperationsExtensions.ListNamespacedServiceAsync(
            cluster.Client.CoreV1,
            endpoint.Namespace,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var service = services.Items.FirstOrDefault(candidate =>
            string.Equals(candidate.Metadata?.Name, endpoint.ServiceName, StringComparison.Ordinal));
        if (service is null)
        {
            throw new InvalidOperationException(
                $"Prometheus Service '{endpoint.Namespace}/{endpoint.ServiceName}' was not found.");
        }

        cluster.GetResourceSourceCache<k8s.Models.V1Service>().Edit(updater => updater.AddOrUpdate(service));
    }

    private void PrepareServicePortForwardTransport(Cluster cluster, ResolvedPrometheusEndpoint endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Namespace)
            || string.IsNullOrWhiteSpace(endpoint.ServiceName)
            || endpoint.ServicePort is not > 0)
        {
            throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
        }

        PortForwarder portForwarder = cluster.AddServicePortForward(
            endpoint.Namespace,
            endpoint.ServiceName,
            endpoint.ServicePort.Value);

        if (!string.Equals(portForwarder.Status, "Active", StringComparison.Ordinal))
        {
            var status = portForwarder.Status;
            throw new InvalidOperationException($"Could not start Prometheus service port-forward: {status}.");
        }

        _httpClient = CreateServicePortForwardHttpClient(endpoint, portForwarder.LocalPort, _directHandlerFactory?.Invoke());
    }

    private static HttpClient CreateServicePortForwardHttpClient(
        ResolvedPrometheusEndpoint endpoint,
        int localPort,
        HttpMessageHandler? handler)
    {
        var scheme = endpoint.UseHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        var host = $"{endpoint.ServiceName}.{endpoint.Namespace}.svc";
        var pathPrefix = endpoint.PathPrefix.TrimEnd('/');
        var baseAddress = new UriBuilder(scheme, host, localPort, pathPrefix + "/").Uri;

#pragma warning disable CA2000 // HttpClient owns the handler and disposes it when the client is disposed.
        HttpClient client = handler is null
            ? new HttpClient(new SocketsHttpHandler
            {
                ConnectCallback = async (_, cancellationToken) =>
                {
                    Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(IPAddress.Loopback, localPort, cancellationToken).ConfigureAwait(false);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                },
            }, disposeHandler: true)
            : new HttpClient(handler);
#pragma warning restore CA2000
        client.BaseAddress = baseAddress;
        return client;
    }
}
