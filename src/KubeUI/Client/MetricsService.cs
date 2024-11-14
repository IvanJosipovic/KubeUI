using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using k8s;
using k8s.Models;
using Scrutor;

namespace KubeUI.Client.Metrics;

public enum MetricsServiceType
{
    None,

    [Description("Kubernetes Metrics Server")]
    KubernetesMetricsServer,

    Prometheus,

    [Description("Azure Managed Prometheus")]
    AzureManagedPrometheus
}

[ServiceDescriptor<MetricsService>(ServiceLifetime.Transient)]
public partial class MetricsService : ObservableObject, IInitializeCluster
{
    private readonly ILogger<MetricsService> _logger;

    private readonly ISettingsService _settings;

    private ICluster _cluster;

    // Used by Prometheus

    private string _prometheusServiceNamespace;
    private string _prometheusServiceName;
    private int _prometheusServicePort;
    private PortForwarder _prometheusService;
    private PrometheusClient _prometheusClient;

    // Used by Kube Metrics

    private DispatcherTimer _metricsRefreshTimer;

    [ObservableProperty]
    private ObservableCollection<PodMetrics> _podMetrics = [];

    [ObservableProperty]
    private ObservableCollection<NodeMetrics> _nodeMetrics = [];

    public MetricsService(ILogger<MetricsService> logger, ISettingsService settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public async Task DetectMetricSource()
    {
        await _cluster.Seed<V1Service>(true);
        var kube = (Kubernetes)_cluster.Client!;

        // Manually set sttings, stop detection
        if (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType == MetricsServiceType.AzureManagedPrometheus)
        {
            return;
        }

        //Prometheus Operator
        // Service with label operated-prometheus=true
        try
        {
            var services = await _cluster.GetObjectDictionaryAsync<V1Service>();

            foreach (var service in services)
            {
                if (service.Value.Metadata.Labels?.TryGetValue("operated-prometheus", out var value) == true && value == "true")
                {
                    _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType = MetricsServiceType.Prometheus;
                    _settings.SaveSettings();
                    _prometheusServiceNamespace = service.Value.Namespace();
                    _prometheusServiceName = service.Value.Name();
                    _prometheusServicePort = service.Value.Spec.Ports.First(x => x.Name == "http-web").Port;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting Prometheus Operator");
        }

        //Metrics Server
        // Check for permissions to metrics
        try
        {
            var model = new V1SelfSubjectAccessReview()
            {
                ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
                Kind = V1SelfSubjectAccessReview.KubeKind,
                Spec = new()
                {
                    ResourceAttributes = new()
                    {
                        Group = "metrics.k8s.io",
                        Resource = "pods",
                        Verb = "list"
                    }
                }
            };

            var resp = await kube.CreateSelfSubjectAccessReviewAsync(model);

            var model2 = new V1SelfSubjectAccessReview()
            {
                ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
                Kind = V1SelfSubjectAccessReview.KubeKind,
                Spec = new()
                {
                    ResourceAttributes = new()
                    {
                        Group = "metrics.k8s.io",
                        Resource = "nodes",
                        Verb = "list"
                    }
                }
            };

            var resp2 = await kube.CreateSelfSubjectAccessReviewAsync(model);

            if (resp.Status.Allowed && resp2.Status.Allowed)
            {
                _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType = MetricsServiceType.KubernetesMetricsServer;
                _settings.SaveSettings();
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting Kubernetes Metrics Server");
        }
    }

    public async Task GetData<T>(TimeSpan fromTime, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            if (typeof(T) == typeof(V1Node))
            {

            }
            else if (typeof(T) == typeof(V1Pod))
            {

            }
        }
        else if (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType == MetricsServiceType.Prometheus)
        {
            if (typeof(T) == typeof(V1Node))
            {
               var data1 =  await _prometheusClient.Query("sum(rate(node_cpu_seconds_total{mode=~\"user|system\"}[5m])) by(node)", DateTimeOffset.Now.Add(-fromTime), DateTimeOffset.Now);
            }
            else if (typeof(T) == typeof(V1Pod))
            {

            }
        }
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;

        _ = Task.Run(async () =>
        {
            await DetectMetricSource();

            switch (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType)
            {
                case MetricsServiceType.None:
                    break;
                case MetricsServiceType.KubernetesMetricsServer:
                    _metricsRefreshTimer = new(TimeSpan.FromSeconds(30), DispatcherPriority.Background, UpdateKubeMetrics);
                    _metricsRefreshTimer.Start();
                    UpdateKubeMetrics(null, null);
                    break;
                case MetricsServiceType.Prometheus:
                    _prometheusService = _cluster.AddServicePortForward(_prometheusServiceNamespace, _prometheusServiceName, _prometheusServicePort);

                    _prometheusClient = Application.Current.GetRequiredService<PrometheusClient>();
                    _prometheusClient.Initialize(_prometheusService);
                    await GetData<V1Node>(TimeSpan.FromHours(1), "r720");
                    break;
                case MetricsServiceType.AzureManagedPrometheus:

                    break;
            }
        });
    }

    private async void UpdateKubeMetrics(object? sender, EventArgs e)
    {
        try
        {
            var nodeMetricslist = await _cluster.Client.GetKubernetesNodesMetricsAsync();

            foreach (var item in nodeMetricslist.Items)
            {
                if (!NodeMetrics.Any(x => x.Metadata.Uid == item.Metadata.Uid && x.Metadata.CreationTimestamp == item.Metadata.CreationTimestamp))
                {
                    NodeMetrics.Add(item);
                }
            }

            var podMetricsList = await _cluster.Client.GetKubernetesPodsMetricsAsync();

            foreach (var item in podMetricsList.Items)
            {
                if (!PodMetrics.Any(x => x.Metadata.Uid == item.Metadata.Uid && x.Metadata.CreationTimestamp == item.Metadata.CreationTimestamp))
                {
                    PodMetrics.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Metrics");
        }
    }
}

[ServiceDescriptor<MetricsService>(ServiceLifetime.Transient)]
internal partial class PrometheusClient : ObservableObject
{
    private readonly ILogger<PrometheusClient> _logger;
    private PortForwarder _portForwarder;
    private readonly HttpClient _httpClient;
    public PrometheusClient(ILogger<PrometheusClient> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public void Initialize(PortForwarder portForwarder)
    {
        _portForwarder = portForwarder;
    }

    // https://prometheus.io/docs/prometheus/latest/querying/api/#range-queries
    // http://localhost:60077/api/v1/query_range?query=sum(kube_node_status_capacity{resource="memory"}) by (node)&start=1731378073.038&end=1731378373.038&step=1
    public async Task<PrometheusClientQueryRangeResponse> Query(string query, DateTimeOffset start, DateTimeOffset end, string step = "1")
    {
        var url = $"http://localhost:{_portForwarder.LocalPort}/api/v1/query_range?query={query}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={step}";
        var response = await _httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>();
    }
}

public class PrometheusClientQueryRangeResponse
{
    /// <summary>
    /// "success" | "error"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("data")]
    public DataObject Data { get; set; }

    public class DataObject
    {
        /// <summary>
        /// "matrix" | "vector" | "scalar" | "string"
        /// </summary>
        [JsonPropertyName("resultType")]
        public string ResultType { get; set; }

        [JsonPropertyName("result")]
        public ResultObject[] Result { get; set; }

        public class ResultObject
        {
            [JsonPropertyName("metric")]
            public IDictionary<string, string> Metric { get; set; }

            [JsonPropertyName("values")]
            [JsonConverter(typeof(TupleConverter))]
            public IList<ValueTuple<DateTimeOffset, decimal>> Values { get; set; }

            public class TupleConverter : JsonConverter<IList<ValueTuple<DateTimeOffset, decimal>>>
            {
                public override IList<ValueTuple<DateTimeOffset, decimal>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    var list = new List<ValueTuple<DateTimeOffset, decimal>>();

                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        ValueTuple<DateTimeOffset, decimal> temp = default;
                        bool instance = false;

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonTokenType.StartArray:
                                    instance = true;
                                    break;
                                case JsonTokenType.EndArray:
                                    list.Add(temp);

                                    if (!instance)
                                    {
                                        return list;
                                    }
                                    else
                                    {
                                        instance = false;
                                    }

                                    break;
                                case JsonTokenType.String:
                                    temp.Item2 = decimal.Parse(reader.GetString());
                                    break;
                                case JsonTokenType.Number:
                                    temp.Item1 = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                    }

                    throw new NotSupportedException();
                }

                public override void Write(Utf8JsonWriter writer, IList<ValueTuple<DateTimeOffset, decimal>> value, JsonSerializerOptions options)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
