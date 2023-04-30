namespace KubeUI.Core.Client
{
    public class PodMetricsCache
    {
        private ILogger<PodMetricsCache> Logger { get; }

        private ICluster Cluster { get; }

        public PodMetricsList PodMetricsList { get; private set; }

        private System.Timers.Timer Timer { get; }

        public PodMetricsCache(ICluster cluster, ILogger<PodMetricsCache> logger)
        {
            Logger = logger;
            Cluster = cluster;
            Timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
            Timer.Enabled = true;
            Timer.AutoReset = true;
            Timer.Elapsed += async (sender, eventArgs) => await Timer_Elapsed();

            Timer_Elapsed();
        }

        private async Task Timer_Elapsed()
        {
            try
            {
                PodMetricsList = await ((Cluster)Cluster).Client.GetKubernetesPodsMetricsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error Getting PodMetrics");
            }
        }
    }
}
