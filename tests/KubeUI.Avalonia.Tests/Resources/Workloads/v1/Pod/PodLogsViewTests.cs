using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Reflection;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogsViewTests
{
    [AvaloniaFact]
    public async Task view_installs_the_avaloniaedit_search_panel_and_behavior()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = CreatePod(),
            ContainerName = "app",
        };

        PodLogsView view = new()
        {
            DataContext = viewModel,
        };

        Window window = new()
        {
            Content = view,
            Width = 800,
            Height = 600,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            editor.SearchPanel.ShouldNotBeNull();
            PodLogsEditorBehavior behavior = Interaction.GetBehaviors(editor).OfType<PodLogsEditorBehavior>().Single();
            FieldInfo? installationField = typeof(PodLogsEditorBehavior).GetField(
                "_textMateInstallation",
                BindingFlags.Instance | BindingFlags.NonPublic);
            installationField.ShouldNotBeNull();
            installationField!.GetValue(behavior).ShouldNotBeNull();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            installationField.GetValue(behavior).ShouldBeNull();

            window.Content = view;
            Dispatcher.UIThread.RunJobs();
            installationField.GetValue(behavior).ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task selected_pods_and_containers_do_not_increase_the_toolbar_height()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = CreatePod(),
            ContainerName = "app",
            PodSelectionItems = CreatePodSelectionItems(12),
            ContainerSelectionItems =
            [
                new PodLogContainerSelectionItem(string.Empty, "All Containers", false, true),
                new PodLogContainerSelectionItem("app", "app", false, false),
                new PodLogContainerSelectionItem("sidecar", "sidecar", false, false),
                new PodLogContainerSelectionItem("metrics", "metrics", false, false),
            ],
            SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
                [
                    new PodLogContainerSelectionItem("app", "app", false, false),
                    new PodLogContainerSelectionItem("sidecar", "sidecar", false, false),
                    new PodLogContainerSelectionItem("metrics", "metrics", false, false),
                ]),
        };

        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>(viewModel.PodSelectionItems.Skip(1));

        PodLogsView view = new()
        {
            DataContext = viewModel,
        };

        Window window = new()
        {
            Content = view,
            Width = 800,
            Height = 300,
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Control topBar = view.FindControl<Control>("TopBar") ?? throw new InvalidOperationException("Top bar was not found.");
            Control podSelector = view.FindControl<Control>("PodSelectionComboBox") ?? throw new InvalidOperationException("Pod selector was not found.");
            Control containerSelector = view.FindControl<Control>("ContainerSelectionComboBox") ?? throw new InvalidOperationException("Container selector was not found.");

            topBar.Bounds.Height.ShouldBeLessThanOrEqualTo(32);
            podSelector.Bounds.Height.ShouldBeLessThanOrEqualTo(32);
            containerSelector.Bounds.Height.ShouldBeLessThanOrEqualTo(32);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task view_sticks_to_bottom_when_logs_append_while_pinned()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = CreatePod(),
            ContainerName = "app",
            AutoScrollToBottom = true,
            ScrollOffset = new Vector(0, 100000),
            Logs = new AvaloniaEdit.Document.TextDocument(CreateManyLines(200)),
        };

        PodLogsView view = new()
        {
            DataContext = viewModel,
        };

        Window window = new()
        {
            Content = view,
            Width = 800,
            Height = 300,
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);

            var previousBottom = scrollViewer.Offset.Y;
            await Dispatcher.UIThread.InvokeAsync(() => viewModel.Logs.Insert(viewModel.Logs.TextLength, Environment.NewLine + "tail line"));

            await WaitForAsync(() => scrollViewer.Offset.Y > previousBottom);
            scrollViewer.Offset.Y.ShouldBeGreaterThan(previousBottom);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task view_does_not_force_scroll_when_reader_has_moved_away_from_the_bottom()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = CreatePod(),
            ContainerName = "app",
            AutoScrollToBottom = true,
            ScrollOffset = new Vector(0, 80),
            Logs = new AvaloniaEdit.Document.TextDocument(CreateManyLines(300)),
        };

        PodLogsView view = new()
        {
            DataContext = viewModel,
        };

        Window window = new()
        {
            Content = view,
            Width = 800,
            Height = 300,
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => Math.Abs(scrollViewer.Offset.Y - 80) < 1.0);

            var beforeAppend = scrollViewer.Offset.Y;
            await Dispatcher.UIThread.InvokeAsync(() => viewModel.Logs.Insert(viewModel.Logs.TextLength, Environment.NewLine + "older line"));

            await WaitForAsync(() => Math.Abs(scrollViewer.Offset.Y - beforeAppend) < 1.0);
            scrollViewer.Offset.Y.ShouldBe(beforeAppend, tolerance: 1.0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task view_jumps_to_present_when_requested()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = CreatePod(),
            ContainerName = "app",
            AutoScrollToBottom = false,
            ScrollOffset = new Vector(0, 80),
            Logs = new AvaloniaEdit.Document.TextDocument(CreateManyLines(300)),
        };

        PodLogsView view = new()
        {
            DataContext = viewModel,
        };

        Window window = new()
        {
            Content = view,
            Width = 800,
            Height = 300,
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => Math.Abs(scrollViewer.Offset.Y - 80) < 1.0);

            viewModel.JumpToPresent();

            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);

            viewModel.JumpToPresentRequested.ShouldBeFalse();
            viewModel.AutoScrollToBottom.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    private static V1Pod CreatePod()
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "app-7c9dd9f4f4-abcde",
                NamespaceProperty = "default",
                Uid = "pod-uid",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                    },
                ],
            },
        };
    }

    private static IReadOnlyList<PodLogPodSelectionItem> CreatePodSelectionItems(int count)
    {
        var items = new List<PodLogPodSelectionItem> { new(null, "All Pods", true) };
        for (var i = 0; i < count; i++)
        {
            var pod = CreatePod();
            pod.Metadata.Name = $"app-{i:00}";
            items.Add(new PodLogPodSelectionItem(pod, pod.Name(), false));
        }

        return items;
    }

    private static async Task<ScrollViewer> WaitForScrollViewerAsync(TextEditor editor)
    {
        ScrollViewer? scrollViewer = null;
        await WaitForAsync(() =>
        {
            scrollViewer = editor.GetVisualDescendants().OfType<ScrollViewer>()
                .FirstOrDefault(candidate => candidate.ScrollBarMaximum.Y > 0);
            return scrollViewer is not null;
        });
        return scrollViewer
            ?? throw new InvalidOperationException("ScrollViewer was not created.");
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    private static string CreateManyLines(int count)
    {
        var builder = new StringBuilder();
        for (var i = 1; i <= count; i++)
        {
            if (i > 1)
            {
                builder.AppendLine();
            }

            builder.Append("line ");
            builder.Append(i);
        }

        return builder.ToString();
    }

    private sealed class NoOpPodLogStreamClient : IPodLogStreamClient
    {
        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class NoOpPodLogExportService : IPodLogExportService
    {
        public Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
