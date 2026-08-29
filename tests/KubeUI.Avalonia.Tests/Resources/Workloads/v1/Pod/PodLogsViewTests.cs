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
            MultiComboBox containerSelector = selectors[1];
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
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_FollowLogs,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Controller,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Previous,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_Timestamps,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_ShowResourceNames,
                    KubeUI.Avalonia.Assets.Resources.PodLogsView_WordWrap,
                ]);
            StackPanel scopeIdentityBar = topBar.Children
                .OfType<StackPanel>()
                .Single(panel => Grid.GetRow(panel) == 0);
            StackPanel[] scopeFields = scopeIdentityBar.Children.OfType<StackPanel>().ToArray();
            TextBlock[] nameField = scopeFields[0].Children.OfType<TextBlock>().ToArray();
            TextBlock[] namespaceField = scopeFields[1].Children.OfType<TextBlock>().ToArray();
            TextBlock scopeName = nameField[1];
            TextBlock namespaceName = namespaceField[1];
            Control controllerButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.JumpToControlledByLogsCommand));
            Button followLogsButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.FollowLogsCommand));
            Button clearButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.ClearCommand));
            new[] { nameField[0].Text, namespaceField[0].Text }.ShouldBe(
            [
                $"{KubeUI.Avalonia.Assets.Resources.ResourcePropertiesView_Name}:",
                $"{KubeUI.Avalonia.Assets.Resources.ResourcePropertiesView_Namespace}:",
            ]);
            new[] { nameField[0], namespaceField[0] }
                .ShouldAllBe(label => label.FontWeight == FontWeight.SemiBold);
            scopeFields.ShouldAllBe(field =>
                field.Orientation == global::Avalonia.Layout.Orientation.Horizontal && field.Spacing == 4);
            scopeName.Text.ShouldBe(pod.Name());
            scopeName.FontWeight.ShouldBe(FontWeight.Normal);
            namespaceName.Text.ShouldBe("default");
            scopeIdentityBar.Height.ShouldBe(24);
            scopeIdentityBar.Margin.ShouldBe(new Thickness(2, 0));
            scopeIdentityBar.Spacing.ShouldBe(16);
            scopeFields[0].Margin.ShouldBe(new Thickness(4, 0, 0, 0));
            scopeIdentityBar.Children.OfType<Border>().ShouldBeEmpty();
            logControlsBar.Margin.ShouldBe(new Thickness(2, 0));
            selectors.ShouldAllBe(selector =>
                selector.MaxHeight == 20
                && selector.Margin == new Thickness(0, 0, 2, 0)
                && selector.Classes.Contains("ClearButton"));
            Grid.GetRow(scopeIdentityBar).ShouldBe(0);
            Grid.GetRow(logControlsBar).ShouldBe(1);
            podSelector.IsVisible.ShouldBeFalse();
            containerSelector.IsVisible.ShouldBeTrue();
            controllerButton.IsVisible.ShouldBeTrue();
            followLogsButton.Content
                .ShouldBeOfType<FluentIcons.Avalonia.FluentIcon>()
                .Icon.ShouldBe(FluentIcons.Common.Icon.ArrowDownload);
            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeFalse();
            clearButton.Content
                .ShouldBeOfType<FluentIcons.Avalonia.FluentIcon>()
                .Icon.ShouldBe(FluentIcons.Common.Icon.Broom);
            actionControls.Children.OfType<Border>().ShouldBeEmpty();
            viewModel.Title.ShouldBe("Pod Logs");

            viewModel.AutoScrollToBottom = false;
            Dispatcher.UIThread.RunJobs();

            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeTrue();

            viewModel.Object = deployment;
            viewModel.SessionResolution = new PodLogSessionResolution(
                pod,
                "app",
                [pod],
                true,
                false,
                null);
            Dispatcher.UIThread.RunJobs();

            scopeName.Text.ShouldBe(deployment.Name());
            viewModel.Title.ShouldBe("Deployment Logs");
            scopeFields[1].IsVisible.ShouldBeTrue();
            namespaceName.Text.ShouldBe("default");
            podSelector.IsVisible.ShouldBeTrue();
            controllerButton.IsVisible.ShouldBeFalse();
            viewModel.Title.ShouldBe("Deployment Logs");

            deployment.Metadata.NamespaceProperty = null;
            viewModel.Object = null;
            viewModel.Object = deployment;
            Dispatcher.UIThread.RunJobs();

            scopeFields[1].IsVisible.ShouldBeFalse();
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
            Button followLogsButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.FollowLogsCommand));
            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => scrollViewer.Offset.Y >= scrollViewer.ScrollBarMaximum.Y - 1.0);
            followLogsButton.IsVisible.ShouldBeTrue();
            followLogsButton.IsEnabled.ShouldBeFalse();

            await Dispatcher.UIThread.InvokeAsync(() => scrollViewer.Offset = new Vector(scrollViewer.Offset.X, 80));
            await WaitForAsync(() => !viewModel.AutoScrollToBottom && followLogsButton.IsEnabled);

            viewModel.FollowLogs();
            await WaitForAsync(
                () => viewModel.AutoScrollToBottom
                    && !followLogsButton.IsEnabled
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
            Button followLogsButton = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => ReferenceEquals(button.Command, viewModel.FollowLogsCommand));
            TextEditor editor = view.GetVisualDescendants().OfType<TextEditor>().Single();
            ScrollViewer scrollViewer = await WaitForScrollViewerAsync(editor);
            await WaitForAsync(() => scrollViewer.ScrollBarMaximum.Y == 0);
            followLogsButton.IsEnabled.ShouldBeFalse();

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
