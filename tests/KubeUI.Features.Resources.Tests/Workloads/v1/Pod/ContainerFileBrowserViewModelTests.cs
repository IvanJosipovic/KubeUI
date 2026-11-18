using Avalonia.Headless.XUnit;
using Avalonia.Platform.Storage;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFileBrowserViewModelTests
{
    private const string PodName = "api";
    private const string PodNamespace = "default";
    private const string ContainerName = "app";

    [AvaloniaFact]
    public async Task Initialize_probes_then_lists_the_root()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc"), File("/etc/hosts", 5)));
        var factory = new FakeContainerFileAccessFactory(access);
        using var vm = CreateViewModel(factory, Mock.Of<IPlatformServices>());

        await vm.InitializeAsync();

        factory.Created.ShouldBe([(PodName, PodNamespace, ContainerName)]);
        access.Probed.ShouldBe(1);
        access.Listed.ShouldBe(["/"]);
        vm.SelectedFolder!.Path.ShouldBe("/");
        vm.Identity.ShouldBe("default/api/app");
        vm.Entries.Select(static entry => entry.Name).ShouldBe(["etc", "hosts"]);
        vm.RootFolders[0].IsLoaded.ShouldBeTrue();
        vm.RootFolders[0].IsExpanded.ShouldBeTrue();
        vm.RootFolders[0].Children.Select(static node => node.Path).ShouldBe(["/etc"]);
        vm.IsLoading.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Selecting_a_loaded_folder_again_reuses_the_cached_listing()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.AddListing("/etc", Listing(File("/etc/hosts", 5)));
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var etc = vm.RootFolders[0].Children[0];

        await vm.LoadFolderAsync(etc);
        await vm.LoadFolderAsync(vm.RootFolders[0]);

        access.Listed.ShouldBe(["/", "/etc"]);
        vm.Entries.Select(static entry => entry.Name).ShouldBe(["etc"]);
    }

    [AvaloniaFact]
    public async Task Selecting_a_cached_folder_expands_its_tree_node()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.AddListing("/etc", Listing(File("/etc/hosts", 5)));
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var etc = vm.RootFolders[0].Children[0];
        await vm.LoadFolderAsync(etc);
        etc.IsExpanded = false;

        await vm.LoadFolderAsync(etc);

        etc.IsExpanded.ShouldBeTrue();
        access.Listed.ShouldBe(["/", "/etc"]);
    }

    [AvaloniaFact]
    public async Task Refresh_lists_the_selected_folder_again()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.AddListing("/etc", Listing(File("/etc/hosts", 5)));
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        await vm.LoadFolderAsync(vm.RootFolders[0].Children[0]);

        await vm.RefreshCommand.ExecuteAsync(null);

        access.Listed.ShouldBe(["/", "/etc", "/etc"]);
    }

    [AvaloniaFact]
    public async Task Refresh_keeps_children_that_already_exist()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.AddListing("/etc", Listing(Directory("/etc/nginx")));
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var etc = vm.RootFolders[0].Children[0];
        await vm.LoadFolderAsync(etc);
        var nginx = etc.Children[0];

        await vm.LoadFolderAsync(vm.RootFolders[0], force: true);

        vm.RootFolders[0].Children.ShouldHaveSingleItem().ShouldBeSameAs(etc);
        etc.IsLoaded.ShouldBeTrue();
        etc.Children.ShouldHaveSingleItem().ShouldBeSameAs(nginx);
    }

    [AvaloniaFact]
    public async Task A_truncated_listing_is_surfaced_and_remembered()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", new ContainerDirectoryListing([Directory("/etc")], Truncated: true));
        using var vm = CreateViewModel(access);

        await vm.InitializeAsync();

        vm.IsTruncated.ShouldBeTrue();
        vm.SelectedFolder!.IsTruncated.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task A_failed_listing_reports_the_error_clears_the_rows_and_stays_retryable()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.ListError = new ContainerFileAccessException("permission denied");
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        vm.Entries.ShouldBeEmpty();
        vm.StatusMessage.ShouldBe("permission denied");
        vm.RootFolders[0].IsLoaded.ShouldBeFalse();

        access.ListError = null;
        access.AddListing("/", Listing(Directory("/etc")));
        await vm.LoadFolderAsync(vm.RootFolders[0]);

        vm.RootFolders[0].IsLoaded.ShouldBeTrue();
        vm.Entries.ShouldHaveSingleItem().Name.ShouldBe("etc");
    }

    [AvaloniaFact]
    public async Task Selecting_a_cached_folder_supersedes_a_slow_pending_listing()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc"), Directory("/opt")));
        access.AddListing("/etc", Listing(File("/etc/hosts", 5)));
        access.AddListing("/opt", Listing(File("/opt/tool", 1)));
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var etc = vm.RootFolders[0].Children[0];
        var opt = vm.RootFolders[0].Children[1];
        await vm.LoadFolderAsync(opt);
        access.Gate("/etc");

        var slow = vm.LoadFolderAsync(etc);
        await vm.LoadFolderAsync(opt);
        access.Release("/etc");
        await slow;

        vm.SelectedFolder.ShouldBeSameAs(opt);
        vm.Entries.ShouldHaveSingleItem().Name.ShouldBe("tool");
        vm.IsLoading.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Previewing_a_file_does_not_cancel_an_in_flight_listing()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(Directory("/etc")));
        access.AddListing("/etc", Listing(Directory("/etc/nginx")));
        access.AddContent("/etc/hosts", Text("hello"));
        access.Gate("/etc");
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();

        var listing = vm.LoadFolderAsync(vm.RootFolders[0].Children[0]);
        await vm.PreviewAsync(File("/etc/hosts", 5));
        access.Release("/etc");
        await listing;

        vm.SelectedFolder!.Path.ShouldBe("/etc");
        vm.Entries.ShouldHaveSingleItem().Name.ShouldBe("nginx");
        vm.PreviewText.ShouldBe("hello");
        vm.IsLoading.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task An_unsupported_container_does_not_list_anything()
    {
        var access = new FakeContainerFileAccess
        {
            Capabilities = new ContainerFileCapabilities(CanList: false, CanRead: true, Problem: null),
        };
        using var vm = CreateViewModel(access);

        await vm.InitializeAsync();

        vm.IsUnsupported.ShouldBeTrue();
        vm.StatusMessage.ShouldContain("find");
        access.Listed.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Downloading_a_truncated_file_is_refused_instead_of_writing_it()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(File("/big.log", 10)));
        access.AddContent("/big.log", new ContainerFileContent([1, 2, 3], Truncated: true, IsText: true));
        var platform = new Mock<IPlatformServices>(MockBehavior.Strict);
        using var vm = CreateViewModel(access, platform.Object);
        await vm.InitializeAsync();

        await vm.DownloadAsync(vm.Entries[0]);

        platform.Verify(picker => picker.SaveFilePickerAsync(It.IsAny<FilePickerSaveOptions>()), Times.Never);
        vm.StatusMessage.ShouldContain("download limit");
    }

    [AvaloniaFact]
    public async Task Downloading_a_file_over_the_limit_does_not_read_it()
    {
        var access = new FakeContainerFileAccess();
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var entry = new ContainerFileEntry(
            "big.log",
            "/big.log",
            IsDirectory: false,
            IsSymbolicLink: false,
            ContainerFileAccess.MaxFileBytes + 1);

        await vm.DownloadAsync(entry);

        access.Read.ShouldBeEmpty();
        vm.StatusMessage.ShouldContain("download limit");
    }

    [AvaloniaFact]
    public async Task Downloading_a_file_writes_its_bytes_to_the_picked_location()
    {
        var access = new FakeContainerFileAccess();
        access.AddListing("/", Listing(File("/notes.txt", 3)));
        access.AddContent("/notes.txt", Text("abc"));
        var destination = new MemoryStream();
        var file = new Mock<IStorageFile>();
        file.Setup(storage => storage.OpenWriteAsync()).ReturnsAsync(destination);
        var platform = new Mock<IPlatformServices>();
        platform
            .Setup(picker => picker.SaveFilePickerAsync(It.IsAny<FilePickerSaveOptions>()))
            .ReturnsAsync(file.Object);
        using var vm = CreateViewModel(access, platform.Object);
        await vm.InitializeAsync();

        await vm.DownloadAsync(vm.Entries[0]);

        destination.ToArray().ShouldBe("abc"u8.ToArray());
        vm.StatusMessage.ShouldContain("Downloaded");
    }

    private static ContainerFileBrowserViewModel CreateViewModel(FakeContainerFileAccess access)
        => CreateViewModel(access, Mock.Of<IPlatformServices>());

    private static ContainerFileBrowserViewModel CreateViewModel(
        FakeContainerFileAccess access,
        IPlatformServices platform)
        => CreateViewModel(new FakeContainerFileAccessFactory(access), platform);

    private static ContainerFileBrowserViewModel CreateViewModel(
        FakeContainerFileAccessFactory factory,
        IPlatformServices platform)
        => new(
            platform,
            factory,
            Mock.Of<ILogger<ContainerFileBrowserViewModel>>())
        {
            Cluster = null!,
            Object = new V1Pod
            {
                Metadata = new V1ObjectMeta { Name = PodName, NamespaceProperty = PodNamespace },
            },
            ContainerName = ContainerName,
        };

    private static ContainerDirectoryListing Listing(params ContainerFileEntry[] entries)
        => new(entries, Truncated: false);

    private static ContainerFileEntry Directory(string path)
        => new(Name(path), path, IsDirectory: true, IsSymbolicLink: false, Size: 4096);

    private static ContainerFileEntry File(string path, long size)
        => new(Name(path), path, IsDirectory: false, IsSymbolicLink: false, Size: size);

    private static ContainerFileContent Text(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return new ContainerFileContent(bytes, Truncated: false, IsText: true);
    }

    private static string Name(string path) => path[(path.LastIndexOf('/') + 1)..];

    private sealed class FakeContainerFileAccessFactory(IContainerFileAccess access) : IContainerFileAccessFactory
    {
        public List<(string Pod, string Namespace, string Container)> Created { get; } = [];

        public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string @namespace, string containerName)
        {
            Created.Add((podName, @namespace, containerName));
            return access;
        }
    }

    private sealed class FakeContainerFileAccess : IContainerFileAccess
    {
        private readonly Dictionary<string, ContainerDirectoryListing> _listings = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ContainerFileContent> _contents = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TaskCompletionSource> _gates = new(StringComparer.Ordinal);

        public List<string> Listed { get; } = [];

        public List<string> Read { get; } = [];

        public int Probed { get; private set; }

        public ContainerFileCapabilities Capabilities { get; set; } = new(CanList: true, CanRead: true, Problem: null);

        public Exception? ListError { get; set; }

        public void AddListing(string path, ContainerDirectoryListing listing) => _listings[path] = listing;

        public void AddContent(string path, ContainerFileContent content) => _contents[path] = content;

        public void Gate(string path)
            => _gates[path] = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release(string path) => _gates[path].TrySetResult();

        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
        {
            Probed++;
            return Task.FromResult(Capabilities);
        }

        public async Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            Listed.Add(path);
            if (_gates.TryGetValue(path, out var gate))
            {
                await gate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (ListError is not null)
            {
                throw ListError;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return _listings.TryGetValue(path, out var listing)
                ? listing
                : new ContainerDirectoryListing([], Truncated: false);
        }

        public Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            Read.Add(path);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_contents[path]);
        }
    }
}
