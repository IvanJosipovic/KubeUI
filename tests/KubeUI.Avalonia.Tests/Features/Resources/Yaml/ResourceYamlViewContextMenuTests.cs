using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using k8s.Models;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class ResourceYamlViewContextMenuTests
{
    [AvaloniaFact]
    public async Task Editor_context_menu_copy_item_invoked_with_the_pointer_copies_selected_yaml_text()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

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

        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.Select(0, editor.Text.Length);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
        OpenContextMenuWithPointer(window, editor);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        contextMenu.IsOpen.ShouldBeTrue();
        contextMenu.IsAttachedToVisualTree().ShouldBeTrue();

        var copyMenuItem = contextMenu.Items.OfType<MenuItem>()
            .Single(item => string.Equals(item.Header?.ToString(), "Copy", StringComparison.Ordinal));
        copyMenuItem.IsVisible.ShouldBeTrue();
        copyMenuItem.IsAttachedToVisualTree().ShouldBeTrue();

        ClickMenuItemWithPointer(copyMenuItem);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        contextMenu.IsOpen.ShouldBeFalse();

        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        clipboard.ShouldNotBeNull();

        string? copiedText = null;
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < 5000)
        {
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            copiedText = await clipboard!.TryGetTextAsync();
            if (copiedText == editor.Text)
            {
                break;
            }

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }

        copiedText.ShouldBe(editor.Text);
    }

    [AvaloniaFact]
    public async Task Editor_context_menu_cut_item_invoked_with_the_pointer_cuts_selected_yaml_text()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
        viewModel.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "cut-test",
                NamespaceProperty = "default",
            },
        });
        viewModel.EditMode = true;
        viewModel.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nmetadata:\n  name: cut-test";
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var view = new ResourceYamlView
        {
            DataContext = viewModel,
        };

        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var selectionStart = editor.Text.IndexOf("name", StringComparison.Ordinal);
        var selectionLength = "name".Length;
        editor.Select(selectionStart, selectionLength);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var expectedClipboard = editor.SelectedText;

        var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
        OpenContextMenuWithPointer(window, editor, selectionStart);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        contextMenu.IsOpen.ShouldBeTrue();

        var cutMenuItem = contextMenu.Items.OfType<MenuItem>()
            .Single(item => string.Equals(item.Header?.ToString(), "Cut", StringComparison.Ordinal));
        cutMenuItem.IsVisible.ShouldBeTrue();
        cutMenuItem.IsAttachedToVisualTree().ShouldBeTrue();

        ClickMenuItemWithPointer(cutMenuItem);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        contextMenu.IsOpen.ShouldBeFalse();
        editor.Text.IndexOf("name:", StringComparison.Ordinal).ShouldBeLessThan(0, "the selection was cut from the document");

        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        clipboard.ShouldNotBeNull();

        string? copiedText = null;
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < 5000)
        {
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            copiedText = await clipboard!.TryGetTextAsync();
            if (copiedText == expectedClipboard)
            {
                break;
            }

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }

        copiedText.ShouldBe(expectedClipboard);
    }

    [AvaloniaFact]
    public async Task Editor_read_only_context_menu_shows_copy_but_no_edit_actions()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
        viewModel.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "readonly-test",
                NamespaceProperty = "default",
            },
        });

        var view = new ResourceYamlView
        {
            DataContext = viewModel,
        };

        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
        var items = contextMenu.Items.OfType<MenuItem>().ToList();

        items.Single(item => string.Equals(item.Header?.ToString(), "Copy", StringComparison.Ordinal))
            .IsVisible.ShouldBeTrue();
        items.Where(item => item.Header?.ToString() is "Cut" or "Paste" or "Delete" or "Undo" or "Redo")
            .ShouldAllBe(item => item.IsVisible == false, "edit actions are hidden in read-only mode");
        contextMenu.Items.OfType<Separator>().Single().IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Editor_context_menu_items_are_wired_with_direct_commands_instead_of_reflection_behaviors()
    {
        var services = Application.Current.GetTestServices();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var viewModel = services.GetRequiredService<ResourceYamlViewModel>();
        viewModel.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "command-wiring-test",
                NamespaceProperty = "default",
            },
        });

        var view = new ResourceYamlView
        {
            DataContext = viewModel,
        };

        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var contextMenu = editor.ContextMenu.ShouldBeOfType<ContextMenu>();
        var items = contextMenu.Items.OfType<MenuItem>().ToList();
        items.ShouldNotBeEmpty();

        foreach (var item in items)
        {
            item.Command.ShouldNotBeNull(
                "menu actions must be wired to direct commands; EventTriggerBehavior+CallMethodAction " +
                "invokes the editor methods via reflection, which fails in fully trimmed Release builds " +
                "because the trimmer removes methods that have no static callers");
            Interaction.GetBehaviors(item).ShouldBeEmpty("no reflection-based behaviors should remain on menu items");
        }
    }

    private static void OpenContextMenuWithPointer(Window window, TextEditor editor, int? hoverOffset = null)
    {
        // Right-click over the editor so the ContextRequested notification opens the menu
        // exactly the way a user would open it. The default placement is at the pointer.
        var offset = hoverOffset ?? Math.Max(1, editor.Text.Length / 2);
        var point = GetPointForOffset(editor, offset);
        var windowPoint = editor.TextArea.TextView.TranslatePoint(point, window).ShouldNotBeNull();
        window.MouseMove(windowPoint);
        window.MouseDown(windowPoint, MouseButton.Right);
        window.MouseUp(windowPoint, MouseButton.Right);
    }

    private static void ClickMenuItemWithPointer(MenuItem menuItem)
    {
        var itemTopLevel = TopLevel.GetTopLevel(menuItem).ShouldNotBeNull("the menu item is rendered in a popup");
        var center = menuItem.TranslatePoint(
            new Point(menuItem.Bounds.Width / 2, menuItem.Bounds.Height / 2),
            itemTopLevel).ShouldNotBeNull();

        itemTopLevel.MouseMove(center);
        itemTopLevel.MouseDown(center, MouseButton.Left);
        itemTopLevel.MouseUp(center, MouseButton.Left);
    }

    private static Point GetPointForOffset(TextEditor editor, int offset)
    {
        editor.UpdateLayout();
        editor.TextArea.TextView.UpdateLayout();
        var location = editor.Document!.GetLocation(offset);
        var point = editor.TextArea.TextView.GetVisualPosition(new TextViewPosition(location.Line, location.Column), VisualYPosition.LineMiddle);
        return new Point(point.X + 2, point.Y);
    }
}
