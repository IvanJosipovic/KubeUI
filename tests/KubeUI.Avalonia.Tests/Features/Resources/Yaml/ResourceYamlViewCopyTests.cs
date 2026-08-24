using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class ResourceYamlViewCopyTests
{
    [AvaloniaFact]
    public async Task Generic_resource_yaml_view_can_attach_to_logical_tree()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();
        var kind = new GroupApiVersionKind("tests.kubeui.com", "v1", "Widget", "widgets");
        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {"apiVersion":"tests.kubeui.com/v1","kind":"Widget","metadata":{"name":"widget","namespace":"default"}}
            """);
        viewModel.Initialize(cluster, resource);

        var view = new ResourceYamlView { DataContext = viewModel };
        var window = new Window { Content = view, Width = 800, Height = 600 };
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            view.FindControl<AvaloniaEdit.TextEditor>("Editor").ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Editor_copy_writes_selected_yaml_text_to_the_clipboard()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();

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
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
            editor.ShouldNotBeNull();

            editor.Select(0, editor.Text.Length);
            await TestApplicationExtensions.WaitForUiAsync();

            editor.Copy();
            await TestApplicationExtensions.WaitForUiAsync();

            var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
            clipboard.ShouldNotBeNull();

            string? copiedText = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 2000)
            {
                await TestApplicationExtensions.WaitForUiAsync();
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
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();

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
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
            editor.ShouldNotBeNull();

            editor.Select(0, editor.Text.Length);
            await TestApplicationExtensions.WaitForUiAsync();

            var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
            var copyMenuItem = contextMenu.Items.OfType<MenuItem>()
                .Single(item => string.Equals(item.Header?.ToString(), "Copy", StringComparison.Ordinal));

            copyMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            await TestApplicationExtensions.WaitForUiAsync();

            var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
            clipboard.ShouldNotBeNull();

            string? copiedText = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 2000)
            {
                await TestApplicationExtensions.WaitForUiAsync();
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
