using System.Runtime.InteropServices;
using System.Text;
using CliWrap;
using CliWrap.Exceptions;
using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Testing.Kubernetes.Scenarios;

/// <summary>
/// Interface for KIND https://github.com/kubernetes-sigs/kind/releases
/// </summary>
public static class Kind
{
    private const string Version = "v0.32.0";

    private const string KubernetesVersion = "kindest/node:v1.36.1";

    private static readonly SemaphoreSlim DownloadLock = new(1, 1);

    public static string FileName { get; } = "kind" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");

    private static string Executable => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? FileName : $"./{FileName}";

    public static async Task DownloadClient(CancellationToken cancellationToken = default)
    {
        await DownloadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var client = new HttpClient();
            var arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "amd64";
            var os = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "darwin"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";

            if (!File.Exists(FileName))
            {
                await DownloadBinaryAsync(client, os, arch, FileName, cancellationToken).ConfigureAwait(false);
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await Cli.Wrap("chmod")
                    .WithArguments(["+x", $"./{FileName}"])
                    .ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            DownloadLock.Release();
        }
    }

    public static async Task CreateCluster(string name, string? image = null, string? config = null, string? kubeConfigPath = null, CancellationToken cancellationToken = default)
    {
        var stdErrBuffer = new StringBuilder();
        image ??= KubernetesVersion;
        kubeConfigPath ??= KubernetesClientConfiguration.KubeConfigDefaultLocation;

        await ExecuteKindAsync(
            BuildCreateArguments(name, image, config, kubeConfigPath),
            standardOutput: null,
            standardError: stdErrBuffer,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        ThrowIfKindError(stdErrBuffer);
    }

    public static async Task DeleteCluster(
        string name,
        string? kubeConfigPath = null,
        CancellationToken cancellationToken = default)
    {
        var stdErrBuffer = new StringBuilder();
        var arguments = new List<string>
        {
            "delete",
            "cluster",
            "--name",
            name,
        };

        if (!string.IsNullOrWhiteSpace(kubeConfigPath))
        {
            arguments.Add("--kubeconfig");
            arguments.Add(kubeConfigPath);
        }

        await ExecuteKindAsync(
            arguments,
            standardOutput: null,
            standardError: stdErrBuffer,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        ThrowIfKindError(stdErrBuffer);
    }

    public static async Task<string> GetKubeConfig(string name, CancellationToken cancellationToken = default)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        await ExecuteKindAsync(
            ["get", "kubeconfig", "--name", name],
            stdOutBuffer,
            stdErrBuffer,
            cancellationToken).ConfigureAwait(false);

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

    private static void ThrowIfKindError(StringBuilder stdErrBuffer)
    {
        var stdErr = stdErrBuffer.ToString();

        if (!string.IsNullOrEmpty(stdErr) && stdErr.StartsWith("ERROR:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(stdErr);
        }
    }

    private static async Task DownloadBinaryAsync(HttpClient client, string os, string arch, string fileName, CancellationToken cancellationToken)
    {
        var url = new Uri($"https://kind.sigs.k8s.io/dl/{Version}/kind-{os}-{arch}");
        var bytes = await client.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(fileName, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static List<string> BuildCreateArguments(string name, string image, string? config, string kubeConfigPath)
    {
        var arguments = new List<string>
        {
            "create",
            "cluster",
            "--name",
            name,
            "--image",
            image,
            "--kubeconfig",
            kubeConfigPath,
            "--wait",
            "2m"
        };

        if (!string.IsNullOrWhiteSpace(config))
        {
            arguments.Add("--config");
            arguments.Add(config);
        }

        return arguments;
    }

    private static async Task ExecuteKindAsync(
        IReadOnlyList<string> arguments,
        StringBuilder? standardOutput = null,
        StringBuilder? standardError = null,
        CancellationToken cancellationToken = default)
    {
        var command = Cli.Wrap(Executable).WithArguments(arguments);
        if (standardOutput is not null)
        {
            command = command.WithStandardOutputPipe(PipeTarget.ToStringBuilder(standardOutput));
        }

        if (standardError is not null)
        {
            command = command.WithStandardErrorPipe(PipeTarget.ToStringBuilder(standardError));
        }

        try
        {
            await command.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (CommandExecutionException exception)
        {
            var output = standardOutput?.ToString();
            var error = standardError?.ToString();
            var details = string.Join(
                Environment.NewLine,
                new[] { output, error }.Where(static value => !string.IsNullOrWhiteSpace(value)));

            throw new InvalidOperationException(
                $"Kind command failed: {string.Join(' ', arguments)}{Environment.NewLine}{details}",
                exception);
        }
    }

}
