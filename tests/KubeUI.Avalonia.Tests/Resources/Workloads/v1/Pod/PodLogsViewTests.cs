using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
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
using Ursa.Controls;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogsViewTests
{
    [AvaloniaFact]
    public async Task scope_switch_updates_pod_name_selector_and_controller_button()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        IServiceProvider services = Application.Current.GetTestServices();
        V1Pod pod = CreatePod();
        V1ReplicaSet replicaSet = new()
        {
            Metadata = new V1ObjectMeta { Name = "app-rs", NamespaceProperty = "default" },
        };
        V1Deployment deployment = new()
        {
            Metadata = new V1ObjectMeta { Name = "app", NamespaceProperty = "default" },
        };
        using PodLogsViewModel viewModel = new(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            new NoOpPodLogStreamClient())
        {
            Cluster = workspace.Runtime,
            Object = pod,
            ContainerName = "app",
            SessionResolution = new PodLogSessionResolution(
                pod,
                "app",
                [pod],
                false,
                false,
                replicaSet),
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
            MultiComboBox[] selectors = view.GetVisualDescendants().OfType<MultiComboBox>().ToArray();
            Control podSelector = selectors[0];
            StackPanel selectionControls = podSelector.Parent.ShouldBeOfType<StackPanel>();
            Grid logControlsBar = selectionControls.Parent.ShouldBeOfType<Grid>();
            Grid topBar = logControlsBar.Parent.ShouldBeOfType<Grid>();
            selectionControls.Children
                .OfType<Label>()
                .ShouldBeEmpty();
            StackPanel actionControls = logControlsBar.Children
                .OfType<StackPanel>()
                .Single(panel => Grid.GetColumn(panel) == 1);
            actionControls.Children
                .OfType<TemplatedControl>()
                .Select(control => ToolTip.GetTip(control))
                .ShouldBe(
                [
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Clear,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Download,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_JumpToPresent,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Controller,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Previous,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Timestamps,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_ShowResourceNames,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_WordWrap,
                ]);
            StackPanel scopeIdentityBar = topBar.Children
                .OfType<StackPanel>()
                .Single(panel => Grid.GetRow(panel) == 0);
            TextBlock scopeHeading = scopeIdentityBar.Children.OfType<TextBlock>().Single();
            Border namespaceChip = scopeIdentityBar.Children.OfType<Border>().Single();
            TextBlock namespaceName = namespaceChip.Child.ShouldBeOfType<TextBlock>();
            Control controllerButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.JumpToControlledByLogsCommand));
            scopeHeading.Text.ShouldBe(pod.Name());
            scopeHeading.FontWeight.ShouldBe(FontWeight.SemiBold);
            namespaceChip.IsVisible.ShouldBeTrue();
            namespaceName.Text.ShouldBe("default");
            Grid.GetRow(scopeIdentityBar).ShouldBe(0);
            Grid.GetRow(logControlsBar).ShouldBe(1);
            podSelector.IsVisible.ShouldBeFalse();
            controllerButton.IsVisible.ShouldBeTrue();
            viewModel.Title.ShouldBe("Pod Logs");

            viewModel.Object = deployment;
            viewModel.SessionResolution = new PodLogSessionResolution(
                pod,
                "app",
                [pod],
                true,
                false,
                null);
            Dispatcher.UIThread.RunJobs();

            scopeHeading.Text.ShouldBe(deployment.Name());
            viewModel.Title.ShouldBe("Deployment Logs");
            namespaceChip.IsVisible.ShouldBeTrue();
            namespaceName.Text.ShouldBe("default");
            podSelector.IsVisible.ShouldBeTrue();
            controllerButton.IsVisible.ShouldBeFalse();
            viewModel.Title.ShouldBe("Deployment Logs");

            deployment.Metadata.NamespaceProperty = null;
            viewModel.Object = null;
            viewModel.Object = deployment;
            Dispatcher.UIThread.RunJobs();

            namespaceChip.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task resource_name_toggle_is_selected_when_multiple_pods_are_viewed()
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

        viewModel.PodSelectionItems = CreatePodSelectionItems(2);
        viewModel.AvailablePods = viewModel.PodSelectionItems.Skip(1).Select(item => item.Pod!).ToArray();
        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>([viewModel.PodSelectionItems[0]]);

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

            ToggleButton toggle = view.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => Equals(ToolTip.GetTip(control), KubeUI.Avalonia.Assets.Resources.PodLogsView_ShowResourceNames));
            viewModel.ShowResourceNames.ShouldBeTrue();
            toggle.IsEnabled.ShouldBeTrue();
            toggle.IsChecked.ShouldBe(true);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task resource_name_toggle_is_selected_when_all_containers_are_viewed()
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
            AvailableContainers =
            [
                new PodLogContainerOption("app", "app", false),
                new PodLogContainerOption("sidecar", "sidecar", false),
                new PodLogContainerOption("metrics", "metrics", false),
            ],
            ContainerSelectionItems =
            [
                new PodLogContainerSelectionItem(string.Empty, "All Containers", false, true),
                new PodLogContainerSelectionItem("app", "app", false, false),
                new PodLogContainerSelectionItem("sidecar", "sidecar", false, false),
                new PodLogContainerSelectionItem("metrics", "metrics", false, false),
            ],
        };

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[0]]);

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

            ToggleButton toggle = view.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => Equals(ToolTip.GetTip(control), KubeUI.Avalonia.Assets.Resources.PodLogsView_ShowResourceNames));
            viewModel.ShowResourceNames.ShouldBeTrue();
            toggle.IsEnabled.ShouldBeTrue();
            toggle.IsChecked.ShouldBe(true);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task jump_to_present_button_is_only_visible_when_logs_are_not_at_the_bottom()
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
            ScrollOffset = new Vector(0, 100000),
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
            Button jumpToPresentButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.JumpToPresentCommand));
            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);
            jumpToPresentButton.IsVisible.ShouldBeFalse();

            await Dispatcher.UIThread.InvokeAsync(() => scrollViewer.Offset = new Vector(scrollViewer.Offset.X, 80));
            await WaitForAsync(() => !viewModel.AutoScrollToBottom && jumpToPresentButton.IsVisible);

            viewModel.JumpToPresent();
            await WaitForAsync(
                () => viewModel.AutoScrollToBottom
                    && !jumpToPresentButton.IsVisible
                    && scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);
        }
        finally
        {
            window.Close();
        }
    }

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
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task view_disposes_textmate_installation_when_unloaded()
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
            PodLogsEditorBehavior behavior = Interaction.GetBehaviors(editor).OfType<PodLogsEditorBehavior>().Single();
            FieldInfo installationField = typeof(PodLogsEditorBehavior).GetField("_textMateInstallation", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Pod log editor installation field was not found.");

            installationField.GetValue(behavior).ShouldNotBeNull();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            installationField.GetValue(behavior).ShouldBeNull();

            window.Content = view;
            Dispatcher.UIThread.RunJobs();

            editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            behavior = Interaction.GetBehaviors(editor).OfType<PodLogsEditorBehavior>().Single();
            installationField.GetValue(behavior).ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void editor_behavior_detaches_cleanly_after_scroll_state_changes()
    {
        TextEditor editor = new();
        PodLogsEditorBehavior behavior = new();
        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(behavior);
        Window window = new()
        {
            Content = editor,
            Width = 800,
            Height = 600,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            behavior.AutoScrollToBottom = false;
            behavior.AutoScrollToBottom = false;
            behavior.ScrollOffset = new Vector(8, 16);
            behavior.JumpToPresentRequested = true;
            Dispatcher.UIThread.RunJobs();

            behaviors.Remove(behavior).ShouldBeTrue();

            behavior.JumpToPresentRequested.ShouldBeFalse();
            behavior.ScrollOffset.ShouldBe(default);
            behavior.AutoScrollToBottom.ShouldBeTrue();
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

            MultiComboBox[] selectors = view.GetVisualDescendants().OfType<MultiComboBox>().ToArray();
            Control podSelector = selectors[0];
            Control containerSelector = selectors[1];
            Control topBar = podSelector.Parent?.Parent?.Parent.ShouldBeOfType<Grid>();

            topBar.Bounds.Height.ShouldBeLessThanOrEqualTo(64);
            podSelector.Width.ShouldBe(containerSelector.Width);
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
