//using CommunityToolkit.Mvvm.ComponentModel;
//using k8s;
//using k8s.Models;
//using System.Text.Json;
//using System.Text.Json.Nodes;
//using System.Text.Json.Serialization;

//namespace KubeUI.Client;

//[KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
//public partial class GenericObject : ObservableObject, IKubernetesObject<V1ObjectMeta>, ISpec<JsonObject>, IStatus<JsonObject>
//{
//    public const string KubeApiVersion = "v1";
//    public const string KubeKind = "CustomResourceDefinition";
//    public const string KubeGroup = "apiextensions.k8s.io";
//    public const string KubePluralName = "customresourcedefinitions";

//    [ObservableProperty]
//    [property: JsonPropertyName("apiVersion")]
//    private string _apiVersion;

//    [ObservableProperty]
//    [property: JsonPropertyName("kind")]
//    private string _kind;

//    [ObservableProperty]
//    [property: JsonPropertyName("metadata")]
//    private V1ObjectMeta _metadata;

//    [ObservableProperty]
//    [property: JsonPropertyName("spec")]
//    private JsonObject _spec;

//    /// <summary>
//    /// This is a test
//    /// </summary>
//    [ObservableProperty]
//    [property: JsonPropertyName("status")]
//    private JsonObject? _status;

//    [JsonExtensionData]
//    public IDictionary<string,JsonElement>? ExtensionData { get; set; }
//}
