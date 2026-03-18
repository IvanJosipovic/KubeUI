using System.Text.Json.Serialization;
using k8s.Models;

namespace KubeUI.Kubernetes;

public class V2beta1APIGroupDiscoveryList
{
    [JsonPropertyName("kind")]
    public string kind { get; set; } = string.Empty;

    [JsonPropertyName("apiVersion")]
    public string apiVersion { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public V1ObjectMeta metadata { get; set; } = new();

    [JsonPropertyName("items")]
    public IList<V2beta1APIGroupDiscoveryListItem> items { get; set; } = [];
}

public class V2beta1APIGroupDiscoveryListItem
{
    [JsonPropertyName("metadata")]
    public V2beta1APIGroupDiscoveryListItemMetadata metadata { get; set; } = new();

    [JsonPropertyName("versions")]
    public IList<V2beta1APIGroupDiscoveryListItemVersion> versions { get; set; } = [];
}

public class V2beta1APIGroupDiscoveryListItemMetadata
{
    [JsonPropertyName("creationTimestamp")]
    public object creationTimestamp { get; set; } = new();

    [JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;
}

public class V2beta1APIGroupDiscoveryListItemVersion
{
    [JsonPropertyName("version")]
    public string version { get; set; } = string.Empty;

    [JsonPropertyName("resources")]
    public IList<V2beta1APIGroupDiscoveryListItemVersionResource> resources { get; set; } = [];

    [JsonPropertyName("freshness")]
    public string freshness { get; set; } = string.Empty;
}

public class V2beta1APIGroupDiscoveryListItemVersionResource
{
    [JsonPropertyName("resource")]
    public string resource { get; set; } = string.Empty;

    [JsonPropertyName("responseKind")]
    public V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind responseKind { get; set; } = new();

    [JsonPropertyName("scope")]
    public string scope { get; set; } = string.Empty;

    [JsonPropertyName("singularResource")]
    public string singularResource { get; set; } = string.Empty;

    [JsonPropertyName("verbs")]
    public IList<string> verbs { get; set; } = [];

    [JsonPropertyName("shortNames")]
    public IList<string> shortNames { get; set; } = [];

    [JsonPropertyName("subresources")]
    public IList<V2beta1APIGroupDiscoveryListItemVersionResourceSubresource> subresources { get; set; } = [];

    [JsonPropertyName("categories")]
    public IList<string> categories { get; set; } = [];
}

public class V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
{
    [JsonPropertyName("group")]
    public string group { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string version { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public string kind { get; set; } = string.Empty;
}

public class V2beta1APIGroupDiscoveryListItemVersionResourceSubresource
{
    [JsonPropertyName("subresource")]
    public string subresource { get; set; } = string.Empty;

    [JsonPropertyName("responseKind")]
    public V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind responseKind { get; set; } = new();

    [JsonPropertyName("verbs")]
    public IList<string> verbs { get; set; } = [];
}

