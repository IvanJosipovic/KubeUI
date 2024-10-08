using System.Text.Json.Serialization;
using k8s.Models;

namespace KubeUI.Client;

public class V2beta1APIGroupDiscoveryList
{
    [JsonPropertyName("kind")]
    public string kind { get; set; }

    [JsonPropertyName("apiVersion")]
    public string apiVersion { get; set; }

    [JsonPropertyName("metadata")]
    public V1ObjectMeta metadata { get; set; }

    [JsonPropertyName("items")]
    public IList<V2beta1APIGroupDiscoveryListItem> items { get; set; }
}
public class V2beta1APIGroupDiscoveryListItem
{
    [JsonPropertyName("metadata")]
    public V2beta1APIGroupDiscoveryListItemMetadata metadata { get; set; }

    [JsonPropertyName("versions")]
    public IList<V2beta1APIGroupDiscoveryListItemVersion> versions { get; set; }
}

public class V2beta1APIGroupDiscoveryListItemMetadata
{
    [JsonPropertyName("creationTimestamp")]
    public object creationTimestamp { get; set; }

    [JsonPropertyName("name")]
    public string name { get; set; }
}

public class V2beta1APIGroupDiscoveryListItemVersion
{
    [JsonPropertyName("version")]
    public string version { get; set; }

    [JsonPropertyName("resources")]
    public IList<V2beta1APIGroupDiscoveryListItemVersionResource> resources { get; set; }

    [JsonPropertyName("freshness")]
    public string freshness { get; set; }
}

public class V2beta1APIGroupDiscoveryListItemVersionResource
{
    [JsonPropertyName("resource")]
    public string resource { get; set; }

    [JsonPropertyName("responseKind")]
    public V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind responseKind { get; set; }

    [JsonPropertyName("scope")]
    public string scope { get; set; }

    [JsonPropertyName("singularResource")]
    public string singularResource { get; set; }

    [JsonPropertyName("verbs")]
    public IList<string> verbs { get; set; }

    [JsonPropertyName("shortNames")]
    public IList<string> shortNames { get; set; }

    [JsonPropertyName("subresources")]
    public IList<V2beta1APIGroupDiscoveryListItemVersionResourceSubresource> subresources { get; set; }

    [JsonPropertyName("categories")]
    public IList<string> categories { get; set; }
}

public class V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
{
    [JsonPropertyName("group")]
    public string group { get; set; }

    [JsonPropertyName("version")]
    public string version { get; set; }

    [JsonPropertyName("kind")]
    public string kind { get; set; }
}

public class V2beta1APIGroupDiscoveryListItemVersionResourceSubresource
{

    [JsonPropertyName("subresource")]
    public string subresource { get; set; }

    [JsonPropertyName("responseKind")]
    public V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind responseKind { get; set; }

    [JsonPropertyName("verbs")]
    public IList<string> verbs { get; set; }
}
