namespace KubeUI.Client;

public partial class Cluster
{
    [ObservableProperty]
    public partial ObservableCollection<PortForwarder> PortForwarders { get; set; } = [];

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace);

        pf.SetPod(podName, containerPort);

        if (PortForwarders.Contains(pf))
        {
            return PortForwarders.First(p => p.Equals(pf));
        }

        PortForwarders.Add(pf);

        pf.Start();

        return pf;
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace);

        pf.SetService(serviceName, containerPort);

        if (PortForwarders.Contains(pf))
        {
            return PortForwarders.First(p => p.Equals(pf));
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
}
