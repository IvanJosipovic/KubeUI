using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFilePreviewViewModelTests
{
    [AvaloniaFact]
    public async Task Load_reads_the_file_text()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/etc/hosts", Text("127.0.0.1 localhost"));
        using var vm = CreateViewModel(access, "/etc/hosts");

        await vm.LoadAsync();

        vm.Text.ShouldBe("127.0.0.1 localhost");
        vm.Message.ShouldBeNull();
        vm.IsTruncated.ShouldBeFalse();
        vm.IsLoading.ShouldBeFalse();
        access.Read.ShouldBe(["/etc/hosts"]);
    }

    [AvaloniaFact]
    public async Task Binary_content_reports_the_binary_message()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/app/image.bin", new ContainerFileContent([0, 1, 2], Truncated: false, IsText: false));
        using var vm = CreateViewModel(access, "/app/image.bin");

        await vm.LoadAsync();

        vm.Text.ShouldBeNull();
        vm.Message.ShouldBe(ResourcesLanguageManager.Instance.ContainerFileBrowserView_Binary.CurrentValue());
    }

    [AvaloniaFact]
    public async Task A_truncated_read_is_reported()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/var/log/app.log", new ContainerFileContent("abc"u8.ToArray(), Truncated: true, IsText: true));
        using var vm = CreateViewModel(access, "/var/log/app.log");

        await vm.LoadAsync();

        vm.Text.ShouldBe("abc");
        vm.IsTruncated.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task A_failed_read_reports_the_error()
    {
        var access = new FakeContainerFileAccess { ReadError = new ContainerFileAccessException("permission denied") };
        using var vm = CreateViewModel(access, "/etc/shadow");

        await vm.LoadAsync();

        vm.Message.ShouldBe("permission denied");
        vm.Text.ShouldBeNull();
        vm.IsLoading.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Refresh_reads_the_file_again()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/etc/hosts", Text("127.0.0.1 localhost"));
        using var vm = CreateViewModel(access, "/etc/hosts");
        await vm.LoadAsync();

        await vm.RefreshCommand.ExecuteAsync(null);

        access.Read.ShouldBe(["/etc/hosts", "/etc/hosts"]);
    }

    [AvaloniaFact]
    public void The_identity_names_the_pod_and_container()
    {
        using var vm = CreateViewModel(new FakeContainerFileAccess(), "/etc/hosts");

        vm.Identity.ShouldBe("default/api/app");
    }

    private static ContainerFilePreviewViewModel CreateViewModel(FakeContainerFileAccess access, string path)
        => new(
            new FakeContainerFileAccessFactory(access),
            Mock.Of<ILogger<ContainerFilePreviewViewModel>>())
        {
            Cluster = null!,
            Object = new V1Pod
            {
                Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" },
            },
            ContainerName = "app",
            Path = path,
            Title = "hosts",
        };

    private static ContainerFileContent Text(string text)
        => new(System.Text.Encoding.UTF8.GetBytes(text), Truncated: false, IsText: true);

    private sealed class FakeContainerFileAccessFactory(IContainerFileAccess access) : IContainerFileAccessFactory
    {
        public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string @namespace, string containerName)
            => access;
    }

    private sealed class FakeContainerFileAccess : IContainerFileAccess
    {
        private readonly Dictionary<string, ContainerFileContent> _contents = new(StringComparer.Ordinal);

        public List<string> Read { get; } = [];

        public Exception? ReadError { get; set; }

        public void AddContent(string path, ContainerFileContent content) => _contents[path] = content;

        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            Read.Add(path);
            cancellationToken.ThrowIfCancellationRequested();
            if (ReadError is not null)
            {
                throw ReadError;
            }

            return Task.FromResult(_contents[path]);
        }
    }
}
