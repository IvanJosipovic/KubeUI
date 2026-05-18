namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private readonly IPortForwardSessionFactory _portForwardSessionFactory;

    [ObservableProperty]
    public partial ObservableCollection<PortForwarder> PortForwarders { get; set; } = [];

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace, localPort: 0, _portForwardSessionFactory);

        pf.SetPod(podName, containerPort);

        var existing = FindPortForwarder(pf);
        if (existing != null)
        {
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

        var existing = FindPortForwarder(pf);
        if (existing != null)
        {
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
}

