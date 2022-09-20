using BlazorMonaco;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using KubernetesCRDModelGen.Models.fluxcd.io;
using System.Text;
using System.Text.Json.Nodes;
using YamlDotNet.RepresentationModel;
using YamlDotNet.System.Text.Json;

namespace KubeUI.Core.Components.flux.io;

public partial class HelmReleaseUpgrade : IDisposable
{
    [Parameter]
    public string Namespace { get; set; }

    [Parameter]
    public string Name { get; set; }

    [Parameter]
    public string Container { get; set; }

    [Inject]
    protected ILogger<HelmReleaseUpgrade> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; }

    private bool IsDisposed;

    private KubernetesCRDModelGen.Models.fluxcd.io.HelmRelease? HelmRelease;

    private KubernetesCRDModelGen.Models.fluxcd.io.HelmRepository? HelmRepository;

    private List<string> Versions = new();

    private bool GetFromFlux;

    private MonacoDiffEditor YamlDiffEditor;

    private DiffEditorConstructionOptions DiffEditorConstructionOptions(MonacoDiffEditor editor)
    {
        return new DiffEditorConstructionOptions
        {
            AutomaticLayout = true,
            OriginalEditable = false,
            IgnoreTrimWhitespace = false
        };
    }

    protected override async Task OnInitializedAsync()
    {
        // Get Helm Release
        HelmRelease = ClusterManager.GetActiveCluster().GetObject<KubernetesCRDModelGen.Models.fluxcd.io.HelmRelease>(Namespace, Name);

        // Get Source
        var sourceType = HelmRelease.Spec.Chart.Spec.SourceRef.Kind;

        if (string.IsNullOrEmpty(HelmRelease.Spec.Chart.Spec.SourceRef.Namespace))
        {
            HelmRelease.Spec.Chart.Spec.SourceRef.Namespace = HelmRelease.Namespace();
        }

        if (sourceType == "HelmRepository")
        {
            HelmRepository = ClusterManager.GetActiveCluster().GetObject<KubernetesCRDModelGen.Models.fluxcd.io.HelmRepository>(HelmRelease.Spec.Chart.Spec.SourceRef.Namespace, HelmRelease.Spec.Chart.Spec.SourceRef.Name);

            if (HelmRepository == null)
            {
                throw new Exception($"HelmRepository/{Namespace}/{Name} was not found");
            }

            if (GetFromFlux)
            {
                throw new NotImplementedException();
            }
            else
            {
                Versions = await GetHelmRepositoryChartVersions(HelmRepository.Spec.Url, HelmRelease.Spec.Chart.Spec.Chart);
            }
        }
        else
        {
            throw new NotImplementedException($"Unsupported Source Type: {sourceType}");
        }
    }

    public async Task<List<string>> GetHelmRepositoryChartVersions(string repoUrl, string chartName)
    {
        using var stream = await new HttpClient().GetStreamAsync(repoUrl.TrimEnd('/') + "/index.yaml");
        using var streamReader = new StreamReader(stream);

        var yaml = new YamlStream();
        yaml.Load(streamReader);

        var items = ((yaml.Documents[0].RootNode as YamlMappingNode)
                    .Children[new YamlScalarNode("entries")] as YamlMappingNode)
                    .Children[new YamlScalarNode(chartName)] as YamlSequenceNode;

        var versions = new List<string>();

        foreach (YamlNode item in items)
        {
            var version = (item[new YamlScalarNode("version")] as YamlScalarNode).Value;

            versions.Add(version);
        }

        return versions;
    }

    private async Task<string> GelmHelmRepositoryChartUrl(string repoUrl, string chartName, string version)
    {
        using var stream = await new HttpClient().GetStreamAsync(repoUrl.TrimEnd('/') + "/index.yaml");
        using var streamReader = new StreamReader(stream);
        var yaml = new YamlStream();
        yaml.Load(streamReader);

        var mapping = yaml.Documents[0].RootNode as YamlMappingNode;

        var items = (mapping.Children[new YamlScalarNode("entries")] as YamlMappingNode).Children[new YamlScalarNode(chartName)] as YamlSequenceNode;

        var item = items.First(x => (x[new YamlScalarNode("version")] as YamlScalarNode).Value == version);

        var url = ((item[new YamlScalarNode("urls")] as YamlSequenceNode).Children.First() as YamlScalarNode).Value;

        if (Uri.TryCreate(url, UriKind.Relative, out Uri result))
        {
            url = new Uri(new Uri(repoUrl), url).ToString();
        }

        return url;
    }

    public async Task<string> GetHelmRepositoryChartValues(string repoUrl, string chartName, string version)
    {
        string url = await GelmHelmRepositoryChartUrl(repoUrl, chartName, version);

        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("Cant find Chart Url");
        }

        using var stream = await new HttpClient().GetStreamAsync(url);
        using var gzipStream = new GZipInputStream(stream);
        using (var tarInputStream = new TarInputStream(gzipStream, Encoding.UTF8))
        {
            TarEntry entry;
            while ((entry = tarInputStream.GetNextEntry()) != null)
            {
                if (entry.Name.EndsWith("values.yaml", StringComparison.InvariantCultureIgnoreCase))
                {
                    using var fileContents = new MemoryStream();
                    tarInputStream.CopyEntryContents(fileContents);
                    fileContents.Position = 0;
                    var stringStream = new StreamReader(fileContents);

                    return stringStream.ReadToEnd();
                }
            }
        }

        throw new Exception("values.yaml not found");
    }

    private async Task EditorOnDidInit(MonacoEditorBase editor)
    {
        var clone = Utilities.CloneObject(HelmRelease);
        clone = CleanObject(clone);

        // Get or create the original model
        TextModel original_model = await MonacoEditorBase.CreateModel(clone.ToYaml(), "yaml");

        // Get or create the modified model
        TextModel modified_model = await MonacoEditorBase.CreateModel("Select a version above.", "yaml");

        // Set the editor model
        await YamlDiffEditor.SetModel(new DiffEditorModel
        {
            Original = original_model,
            Modified = modified_model
        });
    }

    private async Task SetCompareVersion(string version)
    {
        var clone = Utilities.CloneObject(HelmRelease);

        clone.Spec.Chart.Spec.Version = version;

        string values;

        // Get Source
        var sourceType = HelmRelease.Spec.Chart.Spec.SourceRef.Kind;

        if (sourceType == "HelmRepository")
        {
            values = await GetHelmRepositoryChartValues(HelmRepository.Spec.Url, HelmRelease.Spec.Chart.Spec.Chart, version);
        }
        else
        {
            values = "";
        }

        clone.Spec.Values = YamlConverter.Deserialize<JsonNode>(values);

        clone = CleanObject(clone);

        await YamlDiffEditor.ModifiedEditor.SetValue(clone.ToYaml());

        var parameters = new DialogParameters()
        {
            { "Left", CleanObject(HelmRelease) },
            { "Right", clone },
        };

        var dialog = Dialog.Show<CompareObject>($"Compare {HelmRelease.Spec.Chart.Spec.Version} and {version}", parameters, new DialogOptions()
        {
            CloseButton = true,
            FullScreen = true
        });
    }

    private KubernetesCRDModelGen.Models.fluxcd.io.HelmRelease CleanObject(KubernetesCRDModelGen.Models.fluxcd.io.HelmRelease model)
    {
        model.Metadata.Generation = null;
        model.Metadata.CreationTimestamp = null;
        model.Metadata.Finalizers = null;
        model.Metadata.ManagedFields = null;
        model.Metadata.ResourceVersion = null;
        model.Metadata.SelfLink = null;
        model.Metadata.Uid = null;

        model.Status = null;

        return model;
    }

    public void Dispose()
    {
        IsDisposed = true;
        YamlDiffEditor.Dispose();
    }
}
