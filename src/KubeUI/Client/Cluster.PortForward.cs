namespace KubeUI.Client
{
    public partial class Cluster
    {
        [ObservableProperty]
        private ObservableCollection<PortForwarder> _portForwarders = [];

        public PortForwarder AddPortForward(string @namespace, string podName, int containerPort)
        {
            var pf = new PortForwarder(this.Client, @namespace, podName, containerPort);

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
}
