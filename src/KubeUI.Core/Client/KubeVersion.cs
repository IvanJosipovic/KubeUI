using System.Text.Json.Serialization;

namespace KubeUI.Core.Client;

public class KubeVersion
{
    [JsonPropertyName("major")]
    public string Major { get; set; }

    [JsonPropertyName("minor")]
    public string Minor { get; set; }
}