using CliWrap;
using k8s;
using k8s.KubeConfigModels;
using System.Runtime.InteropServices;
using System.Text;

namespace KubeUI.Core.Tests;

/// <summary>
/// Interface for KIND https://github.com/kubernetes-sigs/kind/releases
/// </summary>
public class Kind
{
    public string Version = "latest";

    public string FileName { get; } = "kind" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");

    public async Task DownloadClient()
    {
        if (File.Exists(FileName)) return;

        var client = new HttpClient();
        var arch = "amd64";

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            arch = "arm64";
        }

        var os = "linux";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            os = "darwin";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            os = "windows";
        }

        var url = $"https://kind.sigs.k8s.io/dl/{Version}/kind-{os}-{arch}";

        var bytes = await client.GetByteArrayAsync(url);

        File.WriteAllBytes(FileName, bytes);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await Cli.Wrap("chmod")
                .WithArguments("+x ./kind")
                .ExecuteAsync();
        }
    }

    public async Task CreateCluster(string name, string? image = null)
    {
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments($"create cluster --name {name}" + (string.IsNullOrEmpty(image) ? "" : $" --image {image}"))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }
    }

    public async Task DeleteCluster(string name)
    {
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments($"delete cluster --name {name}")
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }
    }

    public async Task<List<string>> GetClusters()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments("get clusters")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdOut = stdOutBuffer.ToString();
        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }

        return new List<string>(stdOut.TrimEnd().Split("\n"));
    }

    public async Task<string> GetKubeConfig(string name)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments($"get kubeconfig --name {name}")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdOut = stdOutBuffer.ToString();
        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }

        return stdOut;
    }

    public async Task<K8SConfiguration> GetK8SConfiguration(string name)
    {
        return KubernetesYaml.Deserialize<K8SConfiguration>(await GetKubeConfig(name));
    }

    public async Task<Kubernetes> GetKubernetesClient(string name)
    {
        return new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(await GetK8SConfiguration(name)));
    }

    public async Task DeleteAllClusters()
    {
        foreach (var cluster in await GetClusters())
        {
            await DeleteCluster(cluster);
        }
    }
}
