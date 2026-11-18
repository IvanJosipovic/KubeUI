using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;
using DynamicData;
using DynamicData.Binding;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.Platform;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>A folder in the container file tree.</summary>
public sealed partial class ContainerFolderNode : ObservableObject
{
    /// <summary>Creates a folder node.</summary>
    public ContainerFolderNode(string name, string path)
    {
        Name = name;
        Path = path;
    }

    /// <summary>The folder's display name.</summary>
    public string Name { get; }

    /// <summary>The folder's absolute path inside the container.</summary>
    public string Path { get; }

    /// <summary>The folder's children, loaded when it is first expanded.</summary>
    public ObservableCollection<ContainerFolderNode> Children { get; } = [];

    /// <summary>Whether the folder's children have been loaded.</summary>
    [ObservableProperty]
    public partial bool IsLoaded { get; set; }

    /// <summary>Whether the folder is expanded in the tree.</summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    /// <summary>The folder's last listing, reused when the folder is selected again.</summary>
    public IReadOnlyList<ContainerFileEntry> Entries { get; set; } = [];

    /// <summary>Whether the last listing of the folder was truncated.</summary>
    public bool IsTruncated { get; set; }
}

/// <summary>Browses a running container's files: a folder tree, a table of the selected folder's entries, and a preview.</summary>
public sealed partial class ContainerFileBrowserViewModel : ViewModelBase, IClusterWorkspaceContext, IDisposable
{
    private readonly IPlatformServices _platformServices;
    private readonly IContainerFileAccessFactory _accessFactory;
    private readonly ILogger<ContainerFileBrowserViewModel> _logger;
    private IContainerFileAccess? _access;
    private CancellationTokenSource? _navigation;
    private CancellationTokenSource? _preview;
    private CancellationTokenSource? _download;
    private bool _disposed;

    /// <summary>Creates the browser.</summary>
    public ContainerFileBrowserViewModel(
        IPlatformServices platformServices,
        IContainerFileAccessFactory accessFactory,
        ILogger<ContainerFileBrowserViewModel> logger)
    {
        _platformServices = platformServices;
        _accessFactory = accessFactory;
        _logger = logger;
        Title = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Title.CurrentValue()!;
        RootFolders.Add(new ContainerFolderNode("/", "/"));
        TableSource = new DynamicTableViewSource<ContainerFileEntry, string>(
            Entries.ToObservableChangeSet<ContainerFileEntry, string>(static entry => entry.Path).AsObservableCache(),
            static entry => entry.Path,
            [
                DynamicTableViewColumn<ContainerFileEntry>.Create(
                    "name",
                    "Name",
                    static entry => entry.Name,
                    cellTemplate: CreateNameCellTemplate()),
                DynamicTableViewColumn<ContainerFileEntry>.Create(
                    "type",
                    "Type",
                    static entry => entry.IsDirectory ? "Folder" : entry.IsSymbolicLink ? "Link" : "File"),
                DynamicTableViewColumn<ContainerFileEntry>.Create("size", "Size", static entry => entry.Size),
            ]);
    }

    /// <summary>The cluster the browser reads from.</summary>
    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    /// <summary>The pod being browsed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Identity))]
    public partial V1Pod Object { get; set; }

    /// <summary>The container being browsed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Identity))]
    public partial string ContainerName { get; set; }

    /// <summary>The namespace, pod and container identity shown in the header.</summary>
    public string Identity => $"{Object?.Namespace()}/{Object?.Name()}/{ContainerName}";

    /// <summary>The root of the folder tree.</summary>
    public ObservableCollection<ContainerFolderNode> RootFolders { get; } = [];

    /// <summary>The entries of the selected folder.</summary>
    public ObservableCollection<ContainerFileEntry> Entries { get; } = [];

    /// <summary>The table's source over <see cref="Entries"/>.</summary>
    public IDynamicTableViewSource TableSource { get; }

    /// <summary>The folder whose entries the table shows.</summary>
    [ObservableProperty]
    public partial ContainerFolderNode? SelectedFolder { get; set; }

    /// <summary>The entry chosen in the table.</summary>
    [ObservableProperty]
    public partial ContainerFileEntry? SelectedEntry { get; set; }

    /// <summary>What the browser is doing, or why it cannot browse.</summary>
    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    /// <summary>Whether the browser is loading.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>Whether the listing was cut short by the entry or output limit.</summary>
    [ObservableProperty]
    public partial bool IsTruncated { get; set; }

    /// <summary>Whether the container lacks the utilities browsing needs.</summary>
    [ObservableProperty]
    public partial bool IsUnsupported { get; set; }

    /// <summary>The previewed file's path, or null when nothing is previewed.</summary>
    [ObservableProperty]
    public partial string? PreviewPath { get; set; }

    /// <summary>The previewed file's text.</summary>
    [ObservableProperty]
    public partial string? PreviewText { get; set; }

    /// <summary>Why the preview is unavailable, when it is.</summary>
    [ObservableProperty]
    public partial string? PreviewMessage { get; set; }

    /// <summary>Whether the previewed file was truncated.</summary>
    [ObservableProperty]
    public partial bool PreviewTruncated { get; set; }

    /// <summary>Prepares the browser for a pod and container, probing capabilities.</summary>
    public async Task InitializeAsync()
    {
        var token = BeginNavigation();
        IsLoading = true;
        StatusMessage = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Probing.CurrentValue();
        try
        {
            _access = _accessFactory.Create(Cluster, Object.Name(), Object.Namespace(), ContainerName);
            var capabilities = await _access.ProbeAsync(token).ConfigureAwait(true);
            if (!capabilities.CanBrowse)
            {
                IsUnsupported = true;
                StatusMessage = capabilities.Problem
                    ?? string.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        ResourcesLanguageManager.Instance.ContainerFileBrowserView_Unsupported.CurrentValue()!,
                        string.Join(", ", capabilities.Missing));
                return;
            }

            StatusMessage = null;
            await LoadCoreAsync(RootFolders[0], token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Superseded or closed.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Container file browser could not initialize.");
            StatusMessage = ex.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>Shows a folder's entries, listing them again only when <paramref name="force"/> is set.</summary>
    /// <param name="folder">The folder to show.</param>
    /// <param name="force">Whether to list the folder again even when it is already loaded.</param>
    public async Task LoadFolderAsync(ContainerFolderNode folder, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(folder);
        if (_access is null || IsUnsupported)
        {
            return;
        }

        if (!force && folder.IsLoaded)
        {
            // Cancel any listing still in flight so it cannot replace the cached folder's rows.
            BeginNavigation();
            IsLoading = false;
            ShowFolder(folder);
            folder.IsExpanded = true;
            return;
        }

        await LoadCoreAsync(folder, BeginNavigation()).ConfigureAwait(true);
    }

    /// <summary>Previews the chosen file.</summary>
    public async Task PreviewAsync(ContainerFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (_access is null || entry.IsDirectory)
        {
            return;
        }

        var token = Renew(ref _preview);
        PreviewPath = entry.Path;
        PreviewText = null;
        PreviewMessage = null;
        PreviewTruncated = false;
        try
        {
            var content = await _access.ReadAsync(entry.Path, token).ConfigureAwait(true);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!content.IsText)
            {
                PreviewMessage = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Binary.CurrentValue();
                return;
            }

            PreviewText = System.Text.Encoding.UTF8.GetString(content.Content);
            PreviewTruncated = content.Truncated;
        }
        catch (OperationCanceledException)
        {
            // Superseded or closed.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Container file browser could not read {Path}.", entry.Path);
            PreviewMessage = ex.Message;
        }
    }

    /// <summary>Downloads the chosen file to a location the user picks.</summary>
    public async Task DownloadAsync(ContainerFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (_access is null || entry.IsDirectory)
        {
            return;
        }

        if (entry.Size > ContainerFileAccess.MaxFileBytes)
        {
            StatusMessage = FileTooLarge();
            return;
        }

        var token = Renew(ref _download);
        try
        {
            var content = await _access.ReadAsync(entry.Path, token).ConfigureAwait(true);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (content.Truncated)
            {
                // Writing a silently truncated file would lose data without saying so.
                StatusMessage = FileTooLarge();
                return;
            }

            var file = await _platformServices.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Download.CurrentValue(),
                SuggestedFileName = entry.Name,
            }).ConfigureAwait(true);

            if (file is null)
            {
                return;
            }

            await using var stream = await file.OpenWriteAsync().ConfigureAwait(true);
            await stream.WriteAsync(content.Content, token).ConfigureAwait(true);
            StatusMessage = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                ResourcesLanguageManager.Instance.ContainerFileBrowserView_Downloaded.CurrentValue()!,
                entry.Name);
        }
        catch (OperationCanceledException)
        {
            // Superseded or closed.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Container file browser could not download {Path}.", entry.Path);
            StatusMessage = ex.Message;
        }
    }

    /// <summary>Reloads the selected folder.</summary>
    [RelayCommand]
    private Task Refresh() => SelectedFolder is { } folder
        ? LoadFolderAsync(folder, force: true)
        : Task.CompletedTask;

    /// <summary>Previews an entry chosen from the context menu.</summary>
    [RelayCommand]
    private Task Preview(ContainerFileEntry? entry) => entry is null ? Task.CompletedTask : PreviewAsync(entry);

    /// <summary>Downloads an entry chosen from the context menu.</summary>
    [RelayCommand]
    private Task Download(ContainerFileEntry? entry) => entry is null ? Task.CompletedTask : DownloadAsync(entry);

    /// <summary>Opens a folder chosen from the context menu.</summary>
    [RelayCommand]
    private Task OpenFolder(ContainerFileEntry? entry) => entry is null
        ? Task.CompletedTask
        : LoadFolderAsync(FindOrCreateChild(entry));

    private async Task LoadCoreAsync(ContainerFolderNode folder, CancellationToken token)
    {
        // Clear before the round trip so a superseded load never leaves the previous folder's rows
        // visible under the newly selected folder.
        SelectedFolder = folder;
        IsLoading = true;
        IsTruncated = false;
        StatusMessage = null;
        ReplaceEntries([]);
        try
        {
            var listing = await _access!.ListAsync(folder.Path, token).ConfigureAwait(true);
            if (token.IsCancellationRequested)
            {
                return;
            }

            ApplyListing(folder, listing);
        }
        catch (OperationCanceledException)
        {
            // Superseded or closed.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Container file browser could not list {Path}.", folder.Path);
            StatusMessage = ex.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    private void ShowFolder(ContainerFolderNode folder)
    {
        SelectedFolder = folder;
        IsTruncated = folder.IsTruncated;
        ReplaceEntries(folder.Entries);
    }

    private void ApplyListing(ContainerFolderNode folder, ContainerDirectoryListing listing)
    {
        folder.Entries = listing.Entries;
        folder.IsTruncated = listing.Truncated;
        folder.IsLoaded = true;
        ShowFolder(folder);
        PopulateChildren(folder, listing.Entries);
        // Expand after the children exist so the tree reveals the loaded folder in one step,
        // including the initial root listing.
        folder.IsExpanded = true;
    }

    private ContainerFolderNode FindOrCreateChild(ContainerFileEntry entry)
    {
        var sibling = SelectedFolder?.Children.FirstOrDefault(
            node => string.Equals(node.Path, entry.Path, StringComparison.Ordinal));
        if (sibling is not null)
        {
            return sibling;
        }

        var created = new ContainerFolderNode(entry.Name, entry.Path);
        SelectedFolder?.Children.Add(created);
        return created;
    }

    private void ReplaceEntries(IReadOnlyList<ContainerFileEntry> entries)
    {
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }

    private static void PopulateChildren(ContainerFolderNode folder, IReadOnlyList<ContainerFileEntry> entries)
    {
        // Reuse existing nodes so a refresh keeps the expanded subtrees and their cached listings.
        var existing = folder.Children.ToDictionary(static node => node.Path, StringComparer.Ordinal);
        var directories = entries.Where(static entry => entry.IsDirectory).ToList();
        var paths = directories.Select(static entry => entry.Path).ToHashSet(StringComparer.Ordinal);

        for (var index = folder.Children.Count - 1; index >= 0; index--)
        {
            if (!paths.Contains(folder.Children[index].Path))
            {
                folder.Children.RemoveAt(index);
            }
        }

        for (var index = 0; index < directories.Count; index++)
        {
            var entry = directories[index];
            if (!existing.TryGetValue(entry.Path, out var node))
            {
                folder.Children.Insert(index, new ContainerFolderNode(entry.Name, entry.Path));
                continue;
            }

            var current = folder.Children.IndexOf(node);
            if (current != index)
            {
                folder.Children.Move(current, index);
            }
        }
    }

    private static FuncDataTemplate<ContainerFileEntry> CreateNameCellTemplate()
        => new(
            (_, _) => new StackPanel()
                .Orientation(Orientation.Horizontal)
                .Spacing(6)
                .Children(
                    EntryIcon(
                        Icon.Folder,
                        CompiledBinding.Create<ContainerFileEntry, bool>(static entry => entry.IsDirectory)),
                    EntryIcon(
                        Icon.Link,
                        CompiledBinding.Create<ContainerFileEntry, bool>(static entry => entry.IsSymbolicLink)),
                    EntryIcon(
                        Icon.Document,
                        CompiledBinding.Create<ContainerFileEntry, bool>(static entry => entry.IsFile)),
                    new TextBlock()
                        .Text(CompiledBinding.Create<ContainerFileEntry, string>(static entry => entry.Name))
                        .VerticalAlignment(VerticalAlignment.Center)),
            supportsRecycling: true);

    private static FluentIcon EntryIcon(Icon icon, BindingBase isVisible)
        => new FluentIcon()
            .Icon(icon)
            .Width(16)
            .Height(16)
            .VerticalAlignment(VerticalAlignment.Center)
            .IsVisible(isVisible);

    private string FileTooLarge() => string.Format(
        System.Globalization.CultureInfo.CurrentCulture,
        ResourcesLanguageManager.Instance.ContainerFileBrowserView_TooLarge.CurrentValue()!,
        ContainerFileAccess.MaxFileBytes / (1024 * 1024));

    private static CancellationToken Renew(ref CancellationTokenSource? source)
    {
        // The previous source is cancelled but not disposed: the operation it belongs to may still
        // observe its token, and disposing it from this thread can race that operation's registrations.
        source?.Cancel();
        source = new CancellationTokenSource();
        return source.Token;
    }

    private CancellationToken BeginNavigation() => Renew(ref _navigation);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Reset(ref _navigation);
        Reset(ref _preview);
        Reset(ref _download);
        (TableSource as IDisposable)?.Dispose();

        static void Reset(ref CancellationTokenSource? source)
        {
            source?.Cancel();
            source?.Dispose();
            source = null;
        }
    }
}
