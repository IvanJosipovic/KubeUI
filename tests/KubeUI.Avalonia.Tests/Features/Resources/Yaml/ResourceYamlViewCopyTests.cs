using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class ResourceYamlViewCopyTests
{
    [AvaloniaFact]
    public async Task Editor_copy_writes_selected_yaml_text_to_the_clipboard()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        ClusterWorkspace cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();

        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
            viewModel.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "copy-test",
                NamespaceProperty = "default",
            },
        });

        var view = new ResourceYamlView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 800,
            Height = 600,
        };

        try
        {
            window.Show();
            window.Measure(Size.Infinity);
            window.Arrange(new Rect(window.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
            editor.ShouldNotBeNull();

            editor.Select(0, editor.Text.Length);
            Dispatcher.UIThread.RunJobs();

            editor.Copy();
            Dispatcher.UIThread.RunJobs();

            var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
            clipboard.ShouldNotBeNull();

            string? copiedText = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 2000)
            {
                Dispatcher.UIThread.RunJobs();
                copiedText = await clipboard!.TryGetTextAsync();
                if (copiedText == editor.Text)
                {
                    break;
                }

                await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
            }

            copiedText.ShouldBe(editor.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Editor_context_menu_copy_writes_selected_yaml_text_to_the_clipboard()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        ClusterWorkspace cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();

        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
            viewModel.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "copy-test",
                NamespaceProperty = "default",
            },
        });

        var view = new ResourceYamlView
        {
            DataContext = viewModel,
        };
        var window = new Window
        {
            Content = view,
            Width = 800,
            Height = 600,
        };

        try
        {
            window.Show();
            window.Measure(Size.Infinity);
            window.Arrange(new Rect(window.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
            editor.ShouldNotBeNull();

            editor.Select(0, editor.Text.Length);
            Dispatcher.UIThread.RunJobs();

            var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
            var copyMenuItem = contextMenu.Items.OfType<MenuItem>()
                .Single(item => string.Equals(item.Header?.ToString(), "Copy", StringComparison.Ordinal));

            copyMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
            clipboard.ShouldNotBeNull();

            string? copiedText = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 2000)
            {
                Dispatcher.UIThread.RunJobs();
                copiedText = await clipboard!.TryGetTextAsync();
                if (copiedText == editor.Text)
                {
                    break;
                }

                await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
            }

            copiedText.ShouldBe(editor.Text);
        }
        finally
        {
            window.Close();
        }
    }
}
