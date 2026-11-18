using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

/// <summary>Browses a container's files: a folder tree, a table of entries, and a preview.</summary>
public sealed class ContainerFileBrowserView : KubeUIView<ContainerFileBrowserViewModel>
{
    protected override object Build(ContainerFileBrowserViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,Auto,*,Auto")
            .Margin(8)
            .Children(
                CreateHeader(vm).Row(0),
                CreateStatus(vm).Row(1),
                new Grid()
                    .Cols("240,*")
                    .Row(2)
                    .Children(
                        new Border().Col(0).Child(CreateTree(vm)),
                        CreateTable(vm).Col(1)),
                CreatePreview(vm).Row(3));
    }

    private static StackPanel CreateHeader(ContainerFileBrowserViewModel vm)
        => new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(8)
            .Children(
                new TextBlock()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(vm, x => x.ContainerName),
                new Button()
                    .Content(new FluentIcon().Icon(Icon.ArrowClockwise))
                    .Command(vm, x => x.RefreshCommand));

    private static TextBlock CreateStatus(ContainerFileBrowserViewModel vm)
        => new TextBlock()
            .Text(vm, x => x.StatusMessage)
            .IsVisible(vm, x => x.StatusMessage, converter: new StringNotNullOrEmptyConverter())
            .TextWrapping(TextWrapping.Wrap)
            .Margin(0, 4);

    private static Border CreatePreview(ContainerFileBrowserViewModel vm)
        => new Border()
            .IsVisible(vm, x => x.PreviewPath, converter: new StringNotNullOrEmptyConverter())
            .Child(
                new ScrollViewer()
                    .Content(
                        new TextBox()
                            .Text(vm, x => x.PreviewText)
                            .IsReadOnly(true)
                            .AcceptsReturn(true)
                            .TextWrapping(TextWrapping.NoWrap)));

    private static TreeView CreateTree(ContainerFileBrowserViewModel vm)
    {
        var tree = new TreeView()
            .ItemsSource(vm, x => x.RootFolders)
            .ItemTemplate(new FuncTreeDataTemplate<ContainerFolderNode>(
                (node, _) => new TextBlock().Text(node.Name).Margin(4, 2),
                node => node.Children));

        tree.OnSelectionChanged(new EventHandler<SelectionChangedEventArgs>((_, _) =>
        {
            if (tree.SelectedItem is ContainerFolderNode folder)
            {
                _ = vm.LoadFolderAsync(folder);
            }
        }));

        return tree;
    }

    private static DynamicTableView CreateTable(ContainerFileBrowserViewModel vm)
        => new DynamicTableView
        {
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets),
        }
            .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
            .Source(vm.TableSource)
            .ContextMenu(new ContextMenu())
            .OnDoubleTapped(e =>
            {
                if (FindRow(e.Source)?.DataContext is not ContainerFileEntry entry)
                {
                    return;
                }

                _ = entry.IsDirectory
                    ? vm.OpenFolderCommand.ExecuteAsync(entry)
                    : vm.PreviewCommand.ExecuteAsync(entry);
            });

    private static IEnumerable<MenuItem> CreateContextMenuItems(ContainerFileBrowserViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var entry in targets.OfType<ContainerFileEntry>())
        {
            if (entry.IsDirectory)
            {
                yield return new MenuItem()
                    .Header(ResourcesLanguageManager.Instance.ContainerFileBrowserView_Open.ToBinding())
                    .Command(vm, x => x.OpenFolderCommand)
                    .CommandParameter(entry)
                    .Icon(new FluentIcon().Icon(Icon.Folder));
                continue;
            }

            yield return new MenuItem()
                .Header(ResourcesLanguageManager.Instance.ContainerFileBrowserView_ViewContents.ToBinding())
                .Command(vm, x => x.PreviewCommand)
                .CommandParameter(entry)
                .Icon(new FluentIcon().Icon(Icon.Document));

            yield return new MenuItem()
                .Header(ResourcesLanguageManager.Instance.ContainerFileBrowserView_Download.ToBinding())
                .Command(vm, x => x.DownloadCommand)
                .CommandParameter(entry)
                .Icon(new FluentIcon().Icon(Icon.Save));
        }
    }

    private static TableViewRow? FindRow(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TableViewRow>().FirstOrDefault() : null;
}
