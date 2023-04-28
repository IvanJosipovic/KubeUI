namespace KubeUI.Core.Client
{
    public class PodMetricsCache
    {
        private ICluster Cluster { get; set; }

        public PodMetricsList PodMetricsList { get; private set; }

        private System.Timers.Timer Timer { get; set; }

        public PodMetricsCache(ICluster cluster)
        {
            this.Cluster = cluster;
            Timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
            Timer.Enabled = true;
            Timer.AutoReset = true;
            Timer.Elapsed += async (sender, eventArgs) => await Timer_Elapsed();

            Timer_Elapsed();
        }

        private async Task Timer_Elapsed()
        {
            PodMetricsList = await ((Cluster)Cluster).Client.GetKubernetesPodsMetricsAsync();
        }
    }
}
