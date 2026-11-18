using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Converters;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

/// <summary>Browses a container's files: a folder tree, a table of entries, and a preview.</summary>
public sealed class ContainerFileBrowserView : KubeUIView<ContainerFileBrowserViewModel>
{
    private static readonly FuncValueConverter<bool, Icon> FolderIconConverter = new(
        static isExpanded => isExpanded ? Icon.FolderOpen : Icon.Folder);

    protected override object Build(ContainerFileBrowserViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,Auto,*,Auto")
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

    private static CompactToolbar CreateHeader(ContainerFileBrowserViewModel vm)
        => new CompactToolbar
        {
            Leading = CompactToolbar.Group(
                new TextBlock()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(vm, x => x.Identity)
                    .TextTrimming(TextTrimming.CharacterEllipsis)),
            Trailing = CompactToolbar.Group(
                new Button()
                    .Content(new FluentIcon().Icon(Icon.ArrowClockwise))
                    .Command(vm, x => x.RefreshCommand)),
        };

    private static StackPanel CreateStatus(ContainerFileBrowserViewModel vm)
        => new StackPanel()
            .Children(
                new TextBlock()
                    .Text(vm, x => x.StatusMessage)
                    .IsVisible(vm, x => x.StatusMessage, converter: new StringNotNullOrEmptyConverter())
                    .TextWrapping(TextWrapping.Wrap)
                    .Margin(0, 4),
                new TextBlock()
                    .Text(ResourcesLanguageManager.Instance.ContainerFileBrowserView_Truncated.ToBinding())
                    .IsVisible(vm, x => x.IsTruncated)
                    .TextWrapping(TextWrapping.Wrap)
                    .Margin(0, 4));

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
            .Styles(
                new Style<TreeViewItem>()
                    .Setter(
                        TreeViewItem.IsExpandedProperty,
                        CompiledBinding.Create<ContainerFolderNode, bool>(x => x.IsExpanded, mode: BindingMode.TwoWay)))
            .ItemTemplate(new FuncTreeDataTemplate<ContainerFolderNode>(
                (_, _) => new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(6)
                    .Margin(4, 2)
                    .Children(
                        new FluentIcon()
                            .BindValue(
                                FluentIcon.IconProperty,
                                CompiledBinding.Create<ContainerFolderNode, bool>(
                                    static node => node.IsExpanded,
                                    converter: FolderIconConverter))
                            .Width(16)
                            .Height(16)
                            .VerticalAlignment(VerticalAlignment.Center),
                        new TextBlock()
                            .Text(CompiledBinding.Create<ContainerFolderNode, string>(static node => node.Name))
                            .VerticalAlignment(VerticalAlignment.Center)),
                static node => node.Children));

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
