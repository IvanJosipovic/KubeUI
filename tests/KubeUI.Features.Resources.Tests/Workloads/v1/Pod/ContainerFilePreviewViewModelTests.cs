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

        vm.Document.Text.ShouldBe("127.0.0.1 localhost");
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

        vm.Document.Text.ShouldBeEmpty();
        vm.Message.ShouldBe(ResourcesLanguageManager.Instance.ContainerFileBrowserView_Binary.CurrentValue());
    }

    [AvaloniaFact]
    public async Task A_truncated_read_is_reported()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/var/log/app.log", new ContainerFileContent("abc"u8.ToArray(), Truncated: true, IsText: true));
        using var vm = CreateViewModel(access, "/var/log/app.log");

        await vm.LoadAsync();

        vm.Document.Text.ShouldBe("abc");
        vm.IsTruncated.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task A_failed_read_reports_the_error()
    {
        var access = new FakeContainerFileAccess();
        access.AddError("/etc/shadow", new ContainerFileAccessException("permission denied"));
        using var vm = CreateViewModel(access, "/etc/shadow");

        await vm.LoadAsync();

        vm.Message.ShouldBe("permission denied");
        vm.Document.Text.ShouldBeEmpty();
        vm.IsLoading.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task A_superseded_failure_does_not_replace_the_current_preview()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/b.txt", Text("b"));
        access.AddError("/a.txt", new ContainerFileAccessException("stale failure"));
        access.Gate("/a.txt");
        using var vm = CreateViewModel(access, "/a.txt");

        var stale = vm.LoadAsync();
        vm.Path = "/b.txt";
        await vm.LoadAsync();
        access.Release("/a.txt");
        await stale;

        vm.Document.Text.ShouldBe("b");
        vm.Message.ShouldBeNull();
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
        private readonly Dictionary<string, Exception> _errors = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TaskCompletionSource> _gates = new(StringComparer.Ordinal);

        public List<string> Read { get; } = [];

        public void AddContent(string path, ContainerFileContent content) => _contents[path] = content;

        public void AddError(string path, Exception error) => _errors[path] = error;

        public void Gate(string path)
            => _gates[path] = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release(string path) => _gates[path].TrySetResult();

        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public async Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            Read.Add(path);
            if (_gates.TryGetValue(path, out var gate))
            {
                // Deliberately not token-aware: a read can fail after the load that started it was superseded.
                await gate.Task;
            }

            if (_errors.TryGetValue(path, out var error))
            {
                throw error;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return _contents[path];
        }
    }
}
