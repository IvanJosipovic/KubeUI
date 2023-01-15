using k8s;
using k8s.KubeConfigModels;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KubeUI.Core.Tests;

public class Kind
{
    public string Version = "0.17.0";

    public string FileName { get; } = "kind" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");

    public async Task DownloadClient()
    {
        var client = new HttpClient();
        var url = string.Empty;

        if (System.IO.File.Exists(FileName)) return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Mac
            url = $"https://kind.sigs.k8s.io/dl/v{Version}/kind-darwin-arm64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                url = $"https://kind.sigs.k8s.io/dl/v{Version}/kind-darwin-amd64";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux
            url = $"https://kind.sigs.k8s.io/dl/v{Version}/kind-linux-amd64";
        }
        else
        {
            // Windows
            url = $"https://kind.sigs.k8s.io/dl/v{Version}/kind-windows-amd64";
        }

        var bytes = await client.GetByteArrayAsync(url);

        File.WriteAllBytes(FileName, bytes);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "chmod",
                Arguments = "+x ./kind",
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            var proc = new Process() { StartInfo = startInfo };
            proc.Start();
        }
    }

    public void CreateCluster(string name, string? image = null)
    {
        var p = new Process();
        p.StartInfo.FileName = FileName;
        p.StartInfo.Arguments = $"create cluster --name {name}" + (string.IsNullOrEmpty(image) ? "" : $" --image {image}");
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error) && error.StartsWith("ERROR:"))
        {
            throw new Exception(error);
        }
    }

    public void DeleteCluster(string name)
    {
        var p = new Process();
        p.StartInfo.FileName = FileName;
        p.StartInfo.Arguments = $"delete cluster --name {name}";
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error) && error.StartsWith("ERROR:"))
        {
            throw new Exception(error);
        }
    }

    public List<string> GetClusters()
    {
        var p = new Process();
        p.StartInfo.FileName = FileName;
        p.StartInfo.Arguments = $"get clusters";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        return new List<string>(result.TrimEnd().Split("\n"));
    }

    public string GetKubeConfig(string name)
    {
        var p = new Process();
        p.StartInfo.FileName = FileName;
        p.StartInfo.Arguments = $"get kubeconfig --name {name}";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        return result;
    }

    public K8SConfiguration GetK8SConfiguration(string name)
    {
        return KubernetesYaml.Deserialize<K8SConfiguration>(GetKubeConfig(name));
    }

    public Kubernetes GetKubernetesClient(string name)
    {
        return new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(GetK8SConfiguration(name)));
    }

    public void DeleteAllClusters()
    {
        foreach (var cluster in GetClusters())
        {
            DeleteCluster(cluster);
        }
    }
}