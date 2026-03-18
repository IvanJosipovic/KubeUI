using CliWrap;
using k8s;
using k8s.KubeConfigModels;
using System.Runtime.InteropServices;
using System.Text;

namespace KubeUI.Kubernetes.Tests.Infra;

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
        {
            return;
        }

        using var client = new HttpClient();
        var arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "amd64";
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? "darwin"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";

        var url = $"https://kind.sigs.k8s.io/dl/{Version}/kind-{os}-{arch}";
        var bytes = await client.GetByteArrayAsync(url);

        File.WriteAllBytes(FileName, bytes);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await Cli.Wrap("chmod")
                .WithArguments($"+x ./{FileName}")
                .ExecuteAsync();
        }
    }

    public static async Task CreateCluster(string name, string? image = null, string? config = null)
    {
        var stdErrBuffer = new StringBuilder();
        image ??= KubernetesVersion;
        var kubeConfigPath = KubernetesClientConfiguration.KubeConfigDefaultLocation;

        await Cli.Wrap(FileName)
            .WithArguments($"create cluster --name {name} --image {image} --kubeconfig \"{kubeConfigPath}\"" + (string.IsNullOrEmpty(config) ? "" : $" --config={config}"))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        ThrowIfKindError(stdErrBuffer);
    }

    public static async Task DeleteCluster(string name)
    {
        var stdErrBuffer = new StringBuilder();
        var kubeConfigPath = KubernetesClientConfiguration.KubeConfigDefaultLocation;

        await Cli.Wrap(FileName)
            .WithArguments($"delete cluster --name {name} --kubeconfig \"{kubeConfigPath}\"")
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        ThrowIfKindError(stdErrBuffer);
    }

    public static async Task<string> GetKubeConfig(string name)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var kubeConfigPath = KubernetesClientConfiguration.KubeConfigDefaultLocation;

        await Cli.Wrap(FileName)
            .WithArguments($"get kubeconfig --name {name} --kubeconfig \"{kubeConfigPath}\"")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        ThrowIfKindError(stdErrBuffer);
        return stdOutBuffer.ToString();
    }

    public static async Task<K8SConfiguration> GetK8SConfiguration(string name)
    {
        return KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(await GetKubeConfig(name));
    }

    public static async Task<k8s.Kubernetes> GetKubernetesClient(string name)
    {
        return new k8s.Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(await GetK8SConfiguration(name)));
    }

    private static void ThrowIfKindError(StringBuilder stdErrBuffer)
    {
        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(stdErr);
        }
    }
}

