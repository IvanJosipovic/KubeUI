using CliWrap;
using k8s;
using k8s.KubeConfigModels;
using System.Runtime.InteropServices;
using System.Text;

namespace KubeUI.Avalonia.Tests.E2E;

/// <summary>
/// Interface for KIND https://github.com/kubernetes-sigs/kind/releases
/// </summary>
public static class Kind
{
    private const string Version = "v0.31.0";

    private const string KubernetesVersion = "kindest/node:v1.35.1";

    public static string FileName { get; } = "kind" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");

    public static async Task DownloadClient()
    {
        if (File.Exists(FileName))
            return;

        using var client = new HttpClient();
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
                .WithArguments("+x ./" + FileName)
                .ExecuteAsync();
        }
    }

    public static async Task CreateCluster(string name, string? image = null, string? config = null)
    {
        var stdErrBuffer = new StringBuilder();

        image ??= KubernetesVersion;

        await Cli.Wrap(FileName)
            .WithArguments($"create cluster --name {name}" + $" --image {image}" + (string.IsNullOrEmpty(config) ? "" : $" --config={config}"))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }
    }

    public static async Task DeleteCluster(string name)
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

    public static async Task<List<string>> GetClusters()
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

        return [.. stdOut.TrimEnd().Split("\n")];
    }

    public static async Task<string> GetKubeConfig(string name)
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

    public static async Task<string> ExportKubeConfig(string name)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments($"export kubeconfig --name {name}")
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

    public static async Task<K8SConfiguration> GetK8SConfiguration(string name)
    {
        return KubernetesYaml.Deserialize<K8SConfiguration>(await GetKubeConfig(name));
    }

    public static async Task<k8s.Kubernetes> GetKubernetesClient(string name)
    {
        return new k8s.Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(await GetK8SConfiguration(name)));
    }

    public static async Task DeleteAllClusters()
    {
        foreach (var cluster in await GetClusters())
        {
            await DeleteCluster(cluster);
        }
    }

    public static async Task LoadDockerImage(string clusterName, string imageName)
    {
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(FileName)
            .WithArguments($"load docker-image {imageName} --name {clusterName}")
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:"))
        {
            throw new Exception(stdErr);
        }
    }
}
