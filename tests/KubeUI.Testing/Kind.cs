using System.Runtime.InteropServices;
using System.Text;
using CliWrap;
using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Testing;

/// <summary>
/// Interface for KIND https://github.com/kubernetes-sigs/kind/releases
/// </summary>
public static class Kind
{
    private const string Version = "v0.32.0";

    private const string KubernetesVersion = "kindest/node:v1.36.1";

    private static readonly SemaphoreSlim ProcessLock = new(1, 1);

    public static string FileName { get; } = "kind" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");

    // Execute local downloaded binary on non-Windows systems
    private static string Executable => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? FileName : $"./{FileName}";

    public static async Task DownloadClient(CancellationToken cancellationToken = default)
    {
        await ProcessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
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
            var bytes = await client.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);

            File.WriteAllBytes(FileName, bytes);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await Cli.Wrap("chmod")
                    .WithArguments($"+x ./{FileName}")
                    .ExecuteAsync(cancellationToken);
            }
        }
        finally
        {
            ProcessLock.Release();
        }
    }

    public static async Task CreateCluster(string name, string? image = null, string? config = null, string? kubeConfigPath = null, CancellationToken cancellationToken = default)
    {
        await ProcessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var stdErrBuffer = new StringBuilder();
            image ??= KubernetesVersion;
            kubeConfigPath ??= KubernetesClientConfiguration.KubeConfigDefaultLocation;

            await Cli.Wrap(Executable)
                .WithArguments($"create cluster --name {name} --image {image} --kubeconfig \"{kubeConfigPath}\"" + (string.IsNullOrEmpty(config) ? "" : $" --config={config}"))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cancellationToken);

            ThrowIfKindError(stdErrBuffer);
        }
        finally
        {
            ProcessLock.Release();
        }
    }

    public static async Task DeleteCluster(string name, CancellationToken cancellationToken = default)
    {
        await ProcessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var stdErrBuffer = new StringBuilder();

            await Cli.Wrap(Executable)
                .WithArguments($"delete cluster --name {name}")
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cancellationToken);

            ThrowIfKindError(stdErrBuffer);
        }
        finally
        {
            ProcessLock.Release();
        }
    }

    public static async Task<string> GetKubeConfig(string name, CancellationToken cancellationToken = default)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        await Cli.Wrap(Executable)
            .WithArguments($"get kubeconfig --name {name}")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync(cancellationToken);

        ThrowIfKindError(stdErrBuffer);
        return stdOutBuffer.ToString();
    }

    public static async Task<K8SConfiguration> GetK8SConfiguration(string name, CancellationToken cancellationToken = default)
    {
        var kubeConfig = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(await GetKubeConfig(name, cancellationToken).ConfigureAwait(false));
        if (kubeConfig is null)
        {
            throw new InvalidOperationException($"kind did not return a valid kubeconfig for cluster '{name}'.");
        }

        return kubeConfig;
    }

    public static async Task<k8s.Kubernetes> GetKubernetesClient(string name, CancellationToken cancellationToken = default)
    {
        return new k8s.Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(await GetK8SConfiguration(name, cancellationToken).ConfigureAwait(false)));
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

