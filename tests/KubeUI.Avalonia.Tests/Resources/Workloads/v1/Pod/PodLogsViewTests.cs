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
using k8s;
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
            Ursa.Controls.TreeComboBox sourcesSelector = view.GetVisualDescendants()
                .OfType<Ursa.Controls.TreeComboBox>()
                .Single();
            StackPanel selectionControls = sourcesSelector.Parent.ShouldBeOfType<StackPanel>();
            Grid logControlsBar = selectionControls.Parent.ShouldBeOfType<Grid>();
            Grid topBar = logControlsBar.Parent.ShouldBeOfType<Grid>();
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
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_FollowLogs,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Controller,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Previous,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Timestamps,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_ShowResourceNames,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_WordWrap,
                ]);
            Control controllerButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.JumpToControlledByLogsCommand));
            ToggleButton followLogsButton = view.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(button => Equals(ToolTip.GetTip(button), KubeUI.Avalonia.Assets.Resources.PodLogsView_FollowLogs));
            Button clearButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.ClearCommand));
            viewModel.SelectedScopeItems.Single().DisplayName.ShouldBe("Pod/default/app-7c9dd9f4f4-abcde");
            logControlsBar.Margin.ShouldBe(new Thickness(2, 0));
            topBar.Children.Count.ShouldBe(1);
            Grid.GetRow(logControlsBar).ShouldBe(0);
            view.GetVisualDescendants().OfType<Ursa.Controls.MultiComboBox>().ShouldBeEmpty();
            sourcesSelector.Width.ShouldBe(300);
            sourcesSelector.PlaceholderText.ShouldBe(KubeUI.Avalonia.Assets.Resources.PodLogsView_Sources);
            controllerButton.IsVisible.ShouldBeTrue();
            followLogsButton.Content
                .ShouldBeOfType<FluentIcons.Avalonia.FluentIcon>()
                .Icon.ShouldBe(FluentIcons.Common.Icon.ArrowDownload);
            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeTrue();
            followLogsButton.IsChecked.ShouldBe(true);
            clearButton.Content
                .ShouldBeOfType<FluentIcons.Avalonia.FluentIcon>()
                .Icon.ShouldBe(FluentIcons.Common.Icon.Broom);
            actionControls.Children.OfType<Border>().ShouldBeEmpty();
            viewModel.Title.ShouldBe("Pod Logs");

            viewModel.AutoScrollToBottom = false;
            Dispatcher.UIThread.RunJobs();

            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeTrue();
            followLogsButton.IsChecked.ShouldBe(false);

            V1Pod secondPod = CreatePod();
            secondPod.Metadata.Name = "app-second";
            secondPod.Metadata.Uid = "pod-second-uid";
            viewModel.SetScopes([pod, secondPod], V1Pod.KubeKind);
            Dispatcher.UIThread.RunJobs();

            sourcesSelector.IsVisible.ShouldBeTrue();

            viewModel.Object = deployment;
            viewModel.SessionResolution = new PodLogSessionResolution(
                pod,
                "app",
                [pod],
                true,
                false,
                null);
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedScopeItems.Single().DisplayName.ShouldBe("Deployment/default/app");
            viewModel.Title.ShouldBe("Deployment Logs");
            sourcesSelector.IsVisible.ShouldBeTrue();
            controllerButton.IsVisible.ShouldBeFalse();
            viewModel.Title.ShouldBe("Deployment Logs");

            deployment.Metadata.NamespaceProperty = null;
            viewModel.Object = null;
            viewModel.Object = deployment;
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedScopeItems.Single().DisplayName.ShouldBe("Deployment/app");
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

        V1Pod secondPod = CreatePod();
        secondPod.Metadata.Name = "app-second";
        secondPod.Metadata.Uid = "pod-second-uid";
        SetResolvedSources(viewModel, [viewModel.Object.ShouldBeOfType<V1Pod>(), secondPod]);

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
            ContainerName = string.Empty,
        };
        V1Pod sourcePod = viewModel.Object.ShouldBeOfType<V1Pod>();
        sourcePod.Spec!.Containers =
        [
            new V1Container { Name = "app" },
            new V1Container { Name = "sidecar" },
            new V1Container { Name = "metrics" },
        ];
        SetResolvedSources(viewModel, [sourcePod]);

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
    public async Task follow_logs_button_is_enabled_only_when_logs_are_not_at_the_bottom()
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
            ToggleButton followLogsButton = view.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(button => Equals(ToolTip.GetTip(button), KubeUI.Avalonia.Assets.Resources.PodLogsView_FollowLogs));
            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            viewModel.FollowLogs();
            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);
            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeTrue();
            followLogsButton.IsChecked.ShouldBe(true);

            await Dispatcher.UIThread.InvokeAsync(() => scrollViewer.Offset = new Vector(scrollViewer.Offset.X, 80));
            await WaitForAsync(() => !viewModel.AutoScrollToBottom && followLogsButton.IsEnabled);

            followLogsButton.IsChecked = true;
            await WaitForAsync(
                () => viewModel.AutoScrollToBottom
                    && followLogsButton.IsEnabled
                    && followLogsButton.IsChecked == true
                    && scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task follow_logs_button_is_enabled_when_resizing_creates_vertical_overflow()
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
            Logs = new AvaloniaEdit.Document.TextDocument(CreateManyLines(20)),
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

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            ToggleButton followLogsButton = view.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(button => Equals(ToolTip.GetTip(button), KubeUI.Avalonia.Assets.Resources.PodLogsView_FollowLogs));
            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => scrollViewer.ScrollBarMaximum.Y == 0);
            followLogsButton.IsEnabled.ShouldBeTrue();
            followLogsButton.IsChecked.ShouldBe(true);

            window.Height = 180;

            await WaitForAsync(() => scrollViewer.ScrollBarMaximum.Y > 0);
            await WaitForAsync(() => followLogsButton.IsEnabled);
            viewModel.AutoScrollToBottom.ShouldBeFalse();
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
            behavior.FollowLogsRequested = true;
            Dispatcher.UIThread.RunJobs();

            behaviors.Remove(behavior).ShouldBeTrue();

            behavior.FollowLogsRequested.ShouldBeFalse();
            behavior.ScrollOffset.ShouldBe(default);
            behavior.AutoScrollToBottom.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task sources_tree_combo_does_not_increase_the_toolbar_height()
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
            Height = 300,
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Ursa.Controls.TreeComboBox sourcesSelector = view.GetVisualDescendants()
                .OfType<Ursa.Controls.TreeComboBox>()
                .Single();
            Control topBar = view.GetVisualDescendants()
                .OfType<Grid>()
                .Single(grid => grid.Height == 32);

            topBar.Bounds.Height.ShouldBeLessThanOrEqualTo(32);
            sourcesSelector.Bounds.Height.ShouldBeLessThanOrEqualTo(32);
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
            viewModel.FollowLogs();
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

            viewModel.FollowLogs();

            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);

            viewModel.FollowLogsRequested.ShouldBeFalse();
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

    private static void SetResolvedSources(PodLogsViewModel viewModel, IReadOnlyList<V1Pod> pods)
    {
        viewModel.MultiSessionResolution = new PodLogMultiSessionResolution(
            pods.Select(static pod => new PodLogScopeResolution(
                new PodLogScopeIdentity(pod.Namespace(), pod.Name(), pod.Metadata?.Uid, V1Pod.KubeKind),
                [pod],
                pod,
                null,
                null)).ToArray(),
            pods,
            pods[0],
            "app",
            false);
        viewModel.SetScopes(pods.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray(), V1Pod.KubeKind);
    }

    private static async Task<ScrollViewer> WaitForScrollViewerAsync(TextEditor editor)
    {
        ScrollViewer? scrollViewer = null;
        await WaitForAsync(() =>
        {
            scrollViewer = editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
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
