namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private readonly IPortForwardSessionFactory _portForwardSessionFactory;

    [ObservableProperty]
    public partial ObservableCollection<PortForwarder> PortForwarders { get; set; } = [];

#pragma warning disable CA2000 // PortForwarders owns forwarders after Add.
    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace, localPort: 0, _portForwardSessionFactory);
        pf.SetPod(podName, containerPort);

        var existing = FindPortForwarder(pf);
        if (existing != null)
        {
            pf.Dispose();
            return existing;
        }

        PortForwarders.Add(pf);
        pf.Start();
        return pf;
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        var pf = new PortForwarder(this, @namespace, localPort: 0, _portForwardSessionFactory);
        pf.SetService(serviceName, servicePort);

        return AddOrReusePortForwarderOnDispatcher(pf);
    }

    private PortForwarder AddOrReusePortForwarderOnDispatcher(PortForwarder candidate)
    {
        PortForwarder? result = null;
        _dispatcher.Invoke(() => result = AddOrReusePortForwarder(candidate));
        return result!;
    }

    private PortForwarder AddOrReusePortForwarder(PortForwarder candidate)
    {
        var existing = FindPortForwarder(candidate);
        if (existing != null)
        {
            if (string.Equals(existing.Status, "Active", StringComparison.Ordinal))
            {
                candidate.Dispose();
                return existing;
            }

            RemovePortForwardCore(existing);
        }

        PortForwarders.Add(candidate);
        candidate.Start();
        return candidate;
    }

    public void RemovePortForward(PortForwarder pf)
    {
        _dispatcher.Invoke(() => RemovePortForwardCore(pf));
    }

    private void RemovePortForwardCore(PortForwarder pf)
    {
        pf.Stop();
        PortForwarders.Remove(pf);
    }

    private PortForwarder? FindPortForwarder(PortForwarder candidate)
    {
        foreach (var portForwarder in PortForwarders)
        {
            if (portForwarder.Equals(candidate))
            {
                return portForwarder;
            }
        }

        return null;
    }
#pragma warning restore CA2000
}
