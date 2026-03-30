using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KubeUI.Kubernetes;

public enum MetricsServiceType
{
    Auto,
    None,

    [Description("Kubernetes Metrics Server")]
    KubernetesMetricsServer,

    [Description("Prometheus (Port Forward in Cluster)")]
    Prometheus,

    [Description("Prometheus (External)")]
    PrometheusExternal,

    [Description("Azure Managed Prometheus")]
    AzureManagedPrometheus,
}

public partial class ClusterMetricsSettings : ObservableObject
{
    [ObservableProperty]
    public partial MetricsServiceType MetricsServiceType { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServerUrl { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServiceName { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServiceNamespace { get; set; }

    [ObservableProperty]
    public partial int? PrometheusServicePort { get; set; }

    [ObservableProperty]
    public partial string? PrometheusClientId { get; set; }

    [ObservableProperty]
    public partial string? PrometheusClientSecret { get; set; }
}

public sealed class PrometheusClientQueryRangeResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("data")]
    public DataObject Data { get; set; } = new();

    public sealed class DataObject
    {
        [JsonPropertyName("resultType")]
        public string ResultType { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        public ResultObject[] Result { get; set; } = [];
    }

    public sealed class ResultObject
    {
        [JsonPropertyName("metric")]
        public IDictionary<string, string> Metric { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

        [JsonPropertyName("values")]
        [JsonConverter(typeof(TupleConverter))]
        public IList<(DateTimeOffset Timestamp, double Value)> Values { get; set; } = [];
    }

    private sealed class TupleConverter : JsonConverter<IList<(DateTimeOffset Timestamp, double Value)>>
    {
        public override IList<(DateTimeOffset Timestamp, double Value)> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var values = new List<(DateTimeOffset Timestamp, double Value)>();

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Prometheus values payload must be an array.");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return values;
                }

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException("Prometheus value entry must be an array.");
                }

                reader.Read();
                var timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());

                reader.Read();
                var rawValue = reader.GetString();

                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray)
                {
                    throw new JsonException("Prometheus value entry must contain exactly two items.");
                }

                values.Add((timestamp, double.Parse(rawValue ?? "0", CultureInfo.InvariantCulture)));
            }

            throw new JsonException("Unexpected end of Prometheus values payload.");
        }

        public override void Write(Utf8JsonWriter writer, IList<(DateTimeOffset Timestamp, double Value)> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
