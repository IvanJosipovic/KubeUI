using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using k8s;
using k8s.Models;
using Scrutor;

namespace KubeUI.Client.Metrics;

public enum MetricsServiceType
{
    Auto,

    None,

    [Description("Kubernetes Metrics Server")]
    KubernetesMetricsServer,

    Prometheus,

    [Description("Azure Managed Prometheus")]
    AzureManagedPrometheus,
}

[ServiceDescriptor<IMetricsService>(ServiceLifetime.Transient)]
public partial class MetricsService : ObservableObject, IInitializeCluster, IMetricsService
{
    private readonly ILogger<MetricsService> _logger;

    private readonly ISettingsService _settings;

    private ICluster? _cluster;

    public MetricsServiceType MetricsServiceType => _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType;

    // Used by Prometheus
    private HttpClient? _httpClient;
    private PortForwarder? _prometheusServicePortForward;
    private string? _prometheusServiceNamespace;
    private string? _prometheusServiceName;
    private int? _prometheusServicePort;

    // Used by Kube Metrics

    private DispatcherTimer _metricsRefreshTimer;

    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];

    public MetricsService(ILogger<MetricsService> logger, ISettingsService settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public IEnumerable<PodMetrics> GetPodMetrics(string name, string @namespace)
    {
        return PodMetrics.Where(x => x.Name() == name && x.Namespace() == @namespace);
    }

    public IEnumerable<NodeMetrics> GetNodeMetrics(string name)
    {
        return NodeMetrics.Where(x => x.Name() == name);
    }

    // https://prometheus.io/docs/prometheus/latest/querying/api/#range-queries
    public async Task<PrometheusClientQueryRangeResponse?> GetPrometheusMetrics(string query, DateTimeOffset start, DateTimeOffset end, string step = "1")
    {
        var url = $"{GetPrometheusUrl()}/api/v1/query_range?query={query}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={step}";
        var response = await _httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>();
    }

    private string GetPrometheusUrl()
    {
        return _cluster.MetricsService.MetricsServiceType switch
        {
            MetricsServiceType.Prometheus => $"http://localhost:{_prometheusServicePortForward.LocalPort}",
            MetricsServiceType.AzureManagedPrometheus => throw new NotSupportedException(), //todo
            _ => throw new NotSupportedException(),
        };
    }

    private async Task DetectMetricSource()
    {
        await _cluster.Seed<V1Service>(true);
        var kube = (Kubernetes)_cluster.Client!;

        // Manually set settings, stop detection
        if (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType != MetricsServiceType.Auto)
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
                    _ = Dispatcher.UIThread.InvokeAsync(() => {
                        _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType = MetricsServiceType.Prometheus;
                        _settings.SaveSettings();
                    });

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
                _ = Dispatcher.UIThread.InvokeAsync(() => {
                    _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType = MetricsServiceType.KubernetesMetricsServer;
                    _settings.SaveSettings();
                });
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting Kubernetes Metrics Server");
        }

        _ = Dispatcher.UIThread.InvokeAsync(() => {
            _settings.Settings.GetClusterSettings(_cluster).MetricsServiceType = MetricsServiceType.None;
            _settings.SaveSettings();
        });
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;

        _ = Task.Run(async () =>
        {
            await DetectMetricSource();

            switch (_settings.Settings.GetClusterSettings(_cluster).MetricsServiceType)
            {
                case MetricsServiceType.KubernetesMetricsServer:
                    _metricsRefreshTimer = new(TimeSpan.FromSeconds(30), DispatcherPriority.Background, UpdateKubeMetrics);
                    _metricsRefreshTimer.Start();
                    UpdateKubeMetrics(null, null);
                    break;
                case MetricsServiceType.Prometheus:
                    _httpClient = new HttpClient();
                    _prometheusServicePortForward = _cluster.AddServicePortForward(_prometheusServiceNamespace, _prometheusServiceName, _prometheusServicePort.Value);
                    break;
                case MetricsServiceType.AzureManagedPrometheus:
                    throw new NotSupportedException();
                //break;
                default:
                    throw new NotSupportedException();
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

    public decimal GetPodCpuUsage(string @namespace, string name)
    {
        var cpuUsage = PodMetrics.Where(x => x.Name() == name && x.Namespace() == @namespace).OrderByDescending(x => x.Metadata.CreationTimestamp).FirstOrDefault().Containers.Select(x => x.Containers.Sum(y => y.Usage["cpu"]));
        return cpuUsage;
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
            public IList<ValueTuple<DateTimeOffset, double>> Values { get; set; }

            public class TupleConverter : JsonConverter<IList<ValueTuple<DateTimeOffset, double>>>
            {
                public override IList<ValueTuple<DateTimeOffset, double>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    var list = new List<ValueTuple<DateTimeOffset, double>>();

                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        ValueTuple<DateTimeOffset, double> temp = default;
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
                                    temp.Item2 = double.Parse(reader.GetString());
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

                public override void Write(Utf8JsonWriter writer, IList<ValueTuple<DateTimeOffset, double>> value, JsonSerializerOptions options)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
