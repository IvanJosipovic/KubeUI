using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using KubernetesCRDModelGen.Models.Helm.toolkit.fluxcd.io;
using System.Text;
using System.Text.Json.Nodes;
using YamlDotNet.RepresentationModel;
using YamlDotNet.System.Text.Json;

namespace KubeUI.Core.Components.flux.io;

public partial class HelmReleaseUpgrade
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

    private IKubernetesObject<V1ObjectMeta> Right { get; set; }

    private KubernetesCRDModelGen.Models.Helm.toolkit.fluxcd.io.HelmRelease HelmRelease;

    private KubernetesCRDModelGen.Models.Source.toolkit.fluxcd.io.HelmRepository? HelmRepository;

    private List<string> Versions = new();

    private bool GetFromFlux;

    private string SelectedVersion;

    protected override async Task OnInitializedAsync()
    {
        // Get Helm Release
        HelmRelease = ClusterManager.GetActiveCluster().GetObject<KubernetesCRDModelGen.Models.Helm.toolkit.fluxcd.io.HelmRelease>(Namespace, Name);

        HelmRelease = (KubernetesCRDModelGen.Models.Helm.toolkit.fluxcd.io.HelmRelease?)ObjectCompare.CleanObject(HelmRelease);

        // Get Source
        var sourceType = HelmRelease.Spec.Chart.Spec.SourceRef.Kind;

        if (string.IsNullOrEmpty(HelmRelease.Spec.Chart.Spec.SourceRef.@namespace))
        {
            HelmRelease.Spec.Chart.Spec.SourceRef.@namespace = HelmRelease.Namespace();
        }

        if (sourceType == "HelmRepository")
        {
            HelmRepository = ClusterManager.GetActiveCluster().GetObject<KubernetesCRDModelGen.Models.Source.toolkit.fluxcd.io.HelmRepository>(HelmRelease.Spec.Chart.Spec.SourceRef.@namespace, HelmRelease.Spec.Chart.Spec.SourceRef.Name);

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

            await SetCompareVersion(HelmRelease.Spec.Chart.Spec.Version);
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

    private async Task<string> GetHelmRepositoryChartUrl(string repoUrl, string chartName, string version)
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
        string url = await GetHelmRepositoryChartUrl(repoUrl, chartName, version);

        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("Cant find Chart Url");
        }

        using var stream = await new HttpClient().GetStreamAsync(url);
        using var gzipStream = new GZipInputStream(stream);
        using (var tarInputStream = new TarInputStream(gzipStream, Encoding.UTF8))
        {
            TarEntry entry;
            while ((entry = await tarInputStream.GetNextEntryAsync(new CancellationToken())) != null)
            {
                if (entry.Name.EndsWith("values.yaml", StringComparison.InvariantCultureIgnoreCase))
                {
                    using var fileContents = new MemoryStream();
                    await tarInputStream.CopyEntryContentsAsync(fileContents, new CancellationToken());
                    fileContents.Position = 0;
                    var stringStream = new StreamReader(fileContents);

                    return stringStream.ReadToEnd();
                }
            }
        }

        throw new Exception("values.yaml not found");
    }

    private async Task SetCompareVersion(string version)
    {
        SelectedVersion = version;

        var clone = Utilities.CloneObject(HelmRelease);

        clone.Spec.Chart.Spec.Version = version;

        string values;

        // Get Source
        var sourceType = HelmRelease.Spec.Chart.Spec.SourceRef.Kind;

        if (sourceType == "HelmRepository")
        {
            try
            {
                values = await GetHelmRepositoryChartValues(HelmRepository.Spec.Url, HelmRelease.Spec.Chart.Spec.Chart, version);
                clone.Spec.Values = YamlConverter.Deserialize<JsonNode>(values);
            }
            catch (Exception)
            {
                // Version can't be found
                SelectedVersion = null;
                return;
            }
        }
        else
        {
            values = "";
        }

        Right = ObjectCompare.CleanObject(clone);

        StateHasChanged();
    }
}
