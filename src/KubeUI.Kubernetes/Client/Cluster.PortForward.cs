namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private readonly IPortForwardSessionFactory _portForwardSessionFactory;

    [ObservableProperty]
    public partial ObservableCollection<PortForwarder> PortForwarders { get; set; } = [];

#pragma warning disable CA2000 // PortForwarders owns forwarders after Add.
    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        return AddPodPortForward(@namespace, podName, null, containerPort);
    }

    public PortForwarder AddPodPortForward(string @namespace, string podName, string? podUid, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace, localPort: 0, _portForwardSessionFactory);
        pf.SetPod(podName, podUid, containerPort);

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
        return AddServicePortForward(@namespace, serviceName, null, servicePort);
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, string? serviceUid, int servicePort)
    {
        var pf = new PortForwarder(this, @namespace, localPort: 0, _portForwardSessionFactory);
        pf.SetService(serviceName, serviceUid, servicePort);

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

    public void RemovePortForward(PortForwarder pf)
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
