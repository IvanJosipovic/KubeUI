using k8s;
using k8s.Models;
using KubeUI.Core.Client;
using KubeUI.Core.Client.Informer;

namespace KubeUI.Console
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ClusterManager clusterManager;
        private readonly ILoggerFactory loggerFactory;

        public Worker(ILogger<Worker> logger, ClusterManager clusterManager, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            this.clusterManager = clusterManager;
            this.loggerFactory = loggerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var informer = new ResourceInformer<Corev1Event>(loggerFactory.CreateLogger<ResourceInformer<Corev1Event>>(), new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));

            informer.Register(new ResourceInformerCallback<Corev1Event>((x, y) =>
            {
                _logger.LogInformation("{type} - {type} - {name}", typeof(Corev1Event), x, y.Metadata.Name);
            }));

            await informer.RunAsync(stoppingToken);
        }
    }
}