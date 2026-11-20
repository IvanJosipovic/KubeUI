using Avalonia.Controls;
using AvaloniaEdit;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFilePreviewLauncherTests
{
    [AvaloniaFact]
    public async Task Showing_a_file_docks_a_preview_in_the_bottom_dock()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var access = new FakeContainerFileAccess();
        const string yaml = "apiVersion: v1\nkind: Pod\nmetadata:\n  name: api\nspec:\n  replicas: 2\n";
        access.AddContent("/app/config.yaml", Text(yaml));
        var launcher = CreateLauncher(access);
        var pod = CreatePod("launcher-docks");

        await launcher.ShowAsync(workspace, pod, "app", File("/app/config.yaml", 5));

        var preview = BottomDock().VisibleDockables!
            .OfType<ContainerFilePreviewViewModel>()
            .Where(preview => ReferenceEquals(preview.Object, pod))
            .ShouldHaveSingleItem();
        preview.Title.ShouldBe("config.yaml");
        preview.Path.ShouldBe("/app/config.yaml");
        preview.Cluster.ShouldBe(workspace);
        preview.Object.ShouldBe(pod);
        preview.ContainerName.ShouldBe("app");
        preview.Document.Text.ShouldBe(yaml);

        // Documentation capture; no-op unless KUBEUI_TEST_SCREENSHOTS is set.
        var dockControl = new DockControl { Layout = Application.Current.GetRequiredTestService<IFactory>().GetDockable<IRootDock>("Root") };
        using var window = Application.Current.CreateTestWindow(content: dockControl);
        window.Show();
        await TestWait.UntilAsync(
            () => FindEditor(dockControl, yaml) is not null,
            3000,
            TestContext.Current.CancellationToken,
            () =>
            {
                Dispatcher.UIThread.RunJobs();
                using var frame = window.CaptureRenderedFrame();
            });
        TestScreenshot.CaptureIfEnabled(window, "ContainerFilePreviewDock");
    }

    [AvaloniaFact]
    public async Task Reopening_the_same_file_focuses_one_preview_and_reads_it_again()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var access = new FakeContainerFileAccess();
        access.AddContent("/etc/notes.txt", Text("hello"));
        var launcher = CreateLauncher(access);
        var pod = CreatePod("launcher-reuses");

        await launcher.ShowAsync(workspace, pod, "app", File("/etc/notes.txt", 5));
        var preview = BottomDock().VisibleDockables!
            .OfType<ContainerFilePreviewViewModel>()
            .Where(preview => ReferenceEquals(preview.Object, pod))
            .ShouldHaveSingleItem();
        await launcher.ShowAsync(workspace, pod, "app", File("/etc/notes.txt", 5));

        BottomDock().VisibleDockables!
            .OfType<ContainerFilePreviewViewModel>()
            .Where(preview => ReferenceEquals(preview.Object, pod))
            .ShouldHaveSingleItem()
            .ShouldBeSameAs(preview);
        BottomDock().ActiveDockable.ShouldBeSameAs(preview);
        access.Read.ShouldBe(["/etc/notes.txt", "/etc/notes.txt"]);
    }

    [AvaloniaFact]
    public async Task Different_files_get_their_own_previews()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var access = new FakeContainerFileAccess();
        access.AddContent("/etc/notes.txt", Text("notes"));
        access.AddContent("/etc/hosts", Text("hosts"));
        var launcher = CreateLauncher(access);
        var pod = CreatePod("launcher-separate");

        await launcher.ShowAsync(workspace, pod, "app", File("/etc/notes.txt", 5));
        await launcher.ShowAsync(workspace, pod, "app", File("/etc/hosts", 5));

        BottomDock().VisibleDockables!
            .OfType<ContainerFilePreviewViewModel>()
            .Where(preview => ReferenceEquals(preview.Object, pod))
            .Select(static preview => preview.Path)
            .ShouldBe(["/etc/notes.txt", "/etc/hosts"]);
    }

    [AvaloniaFact]
    public async Task Namespace_and_pod_boundaries_do_not_share_an_id()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var access = new FakeContainerFileAccess();
        access.AddContent("/etc/notes.txt", Text("notes"));
        var launcher = CreateLauncher(access);

        var first = CreatePod("c");
        first.Metadata!.NamespaceProperty = "a-b";
        var second = CreatePod("b-c");
        second.Metadata!.NamespaceProperty = "a";

        await launcher.ShowAsync(workspace, first, "app", File("/etc/notes.txt", 5));
        await launcher.ShowAsync(workspace, second, "app", File("/etc/notes.txt", 5));

        var previews = BottomDock().VisibleDockables!
            .OfType<ContainerFilePreviewViewModel>()
            .Where(preview => ReferenceEquals(preview.Object, first) || ReferenceEquals(preview.Object, second))
            .ToList();
        previews.Count.ShouldBe(2);
        previews[0].Id.ShouldNotBe(previews[1].Id);
    }

    private static ContainerFilePreviewLauncher CreateLauncher(IContainerFileAccess access)
        => new(
            () => new ContainerFilePreviewViewModel(
                new FakeContainerFileAccessFactory(access),
                Mock.Of<ILogger<ContainerFilePreviewViewModel>>()),
            Application.Current.GetRequiredTestService<IFactory>());

    private static IToolDock BottomDock()
        => Application.Current.GetRequiredTestService<IFactory>()
            .GetDockable<IToolDock>("BottomDock")
            .ShouldNotBeNull();

    private static TextEditor? FindEditor(Control view, string text)
        => view.GetVisualDescendants()
            .OfType<TextEditor>()
            .FirstOrDefault(editor => string.Equals(editor.Document.Text, text, StringComparison.Ordinal));

    private static V1Pod CreatePod(string name) => new()
    {
        ApiVersion = V1Pod.KubeApiVersion,
        Kind = V1Pod.KubeKind,
        Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
        Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "busybox:1.36" }] },
    };

    private static ContainerFileEntry File(string path, long size)
        => new(path[(path.LastIndexOf('/') + 1)..], path, IsDirectory: false, IsSymbolicLink: false, Size: size);

    private static ContainerFileContent Text(string text)
        => new(System.Text.Encoding.UTF8.GetBytes(text), Truncated: false, IsText: true);

    private sealed class FakeContainerFileAccessFactory(IContainerFileAccess access) : IContainerFileAccessFactory
    {
        public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string podNamespace, string containerName)
            => access;
    }

    private sealed class FakeContainerFileAccess : IContainerFileAccess
    {
        private readonly Dictionary<string, ContainerFileContent> _contents = new(StringComparer.Ordinal);

        public List<string> Read { get; } = [];

        public void AddContent(string path, ContainerFileContent content) => _contents[path] = content;

        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            Read.Add(path);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_contents[path]);
        }
    }
}
