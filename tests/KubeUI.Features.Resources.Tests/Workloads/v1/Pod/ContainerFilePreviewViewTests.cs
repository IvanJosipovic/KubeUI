using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.Layout;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;


namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFilePreviewViewTests
{
    [AvaloniaFact]
    public async Task Preview_view_shows_the_binary_and_truncated_states()
    {
        var access = new FakeContainerFileAccess();
        access.AddContent("/app/image.bin", new ContainerFileContent([0, 1], Truncated: false, IsText: false));
        access.AddContent("/var/big.log", new ContainerFileContent("abc"u8.ToArray(), Truncated: true, IsText: true));
        using var vm = CreateViewModel(access, "/app/image.bin");
        await vm.LoadAsync();

        var view = new ContainerFilePreviewView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(1200, 320, view);
        window.Show();
        var binary = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Binary.CurrentValue()!;
        await WaitForAsync(() => HasVisibleText(view, binary));

        var editor = view.GetVisualDescendants().OfType<TextEditor>().ShouldHaveSingleItem();
        editor.IsReadOnly.ShouldBeTrue();
        editor.ShowLineNumbers.ShouldBeTrue();
        editor.FontFamily.ShouldNotBeNull();
        editor.FontSize.ShouldBeGreaterThan(0);
        editor.KeyBindings.Count.ShouldBe(4);
        Interaction.GetBehaviors(editor).OfType<ContainerFileSyntaxHighlightingBehavior>().ShouldHaveSingleItem();

        vm.Path = "/var/big.log";
        await vm.LoadAsync();
        var truncated = ResourcesLanguageManager.Instance.ContainerFileBrowserView_PreviewTruncated.CurrentValue()!;
        await WaitForAsync(() => HasVisibleText(view, truncated));
        HasVisibleText(view, binary).ShouldBeFalse();
        vm.Document.Text.ShouldBe("abc");
        editor.Document.Text.ShouldBe("abc");
    }

    private static bool HasVisibleText(Control view, string text)
        => view.GetVisualDescendants()
            .OfType<TextBlock>()
            .Any(block => block.IsVisible && string.Equals(block.Text, text, StringComparison.Ordinal));

    private static ContainerFilePreviewViewModel CreateViewModel(IContainerFileAccess access, string path)
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

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
        => await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

    private sealed class FakeContainerFileAccessFactory(IContainerFileAccess access) : IContainerFileAccessFactory
    {
        public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string podNamespace, string containerName)
            => access;
    }

    private sealed class FakeContainerFileAccess : IContainerFileAccess
    {
        private readonly Dictionary<string, ContainerFileContent> _contents = new(StringComparer.Ordinal);

        public void AddContent(string path, ContainerFileContent content) => _contents[path] = content;

        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(_contents[path]);
    }
}
