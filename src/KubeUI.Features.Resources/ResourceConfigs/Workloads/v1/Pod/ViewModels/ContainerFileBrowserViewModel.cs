using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using DynamicData;
using DynamicData.Binding;
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
}

/// <summary>Browses a running container's files: a folder tree, a table of the selected folder's entries, and a preview.</summary>
public sealed partial class ContainerFileBrowserViewModel : ViewModelBase, IClusterWorkspaceContext, IDisposable
{
    private readonly IPlatformServices _platformServices;
    private readonly ILogger<ContainerFileBrowserViewModel> _logger;
    private IContainerFileAccess? _access;
    private CancellationTokenSource? _operation;
    private bool _disposed;

    /// <summary>Creates the browser.</summary>
    public ContainerFileBrowserViewModel(IPlatformServices platformServices, ILogger<ContainerFileBrowserViewModel> logger)
    {
        _platformServices = platformServices;
        _logger = logger;
        Title = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Title.CurrentValue()!;
        RootFolders.Add(new ContainerFolderNode("/", "/"));
        TableSource = new DynamicTableViewSource<ContainerFileEntry, string>(
            Entries.ToObservableChangeSet<ContainerFileEntry, string>(static entry => entry.Path).AsObservableCache(),
            static entry => entry.Path,
            [
                DynamicTableViewColumn<ContainerFileEntry>.Create("name", "Name", static entry => entry.Name),
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
    public partial V1Pod Object { get; set; }

    /// <summary>The container being browsed.</summary>
    [ObservableProperty]
    public partial string ContainerName { get; set; }

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

    /// <summary>Whether the listing was cut short by the entry limit.</summary>
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
        _access = new ContainerFileAccess(Cluster, Object.Name(), Object.Namespace(), ContainerName);
        IsLoading = true;
        StatusMessage = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Probing.CurrentValue();
        try
        {
            var capabilities = await _access.ProbeAsync(CurrentToken).ConfigureAwait(true);
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
            await LoadFolderAsync(RootFolders[0]).ConfigureAwait(true);
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
            IsLoading = false;
        }
    }

    /// <summary>Loads a folder's entries into the table.</summary>
    public async Task LoadFolderAsync(ContainerFolderNode folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        if (_access is null || IsUnsupported)
        {
            return;
        }

        SelectedFolder = folder;
        IsLoading = true;
        StatusMessage = null;
        try
        {
            var listing = await _access.ListAsync(folder.Path, CurrentToken).ConfigureAwait(true);
            Entries.Clear();
            foreach (var entry in listing.Entries)
            {
                Entries.Add(entry);
            }

            IsTruncated = listing.Truncated;
            folder.IsLoaded = true;
            PopulateChildren(folder, listing.Entries);
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
            IsLoading = false;
        }
    }

    /// <summary>Loads a folder's children when it is expanded.</summary>
    public async Task ExpandAsync(ContainerFolderNode folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        if (folder.IsLoaded)
        {
            return;
        }

        await LoadFolderAsync(folder).ConfigureAwait(true);
    }

    /// <summary>Previews the chosen file.</summary>
    public async Task PreviewAsync(ContainerFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (_access is null || entry.IsDirectory)
        {
            return;
        }

        PreviewPath = entry.Path;
        PreviewText = null;
        PreviewMessage = null;
        PreviewTruncated = false;
        try
        {
            var content = await _access.ReadAsync(entry.Path, CurrentToken).ConfigureAwait(true);
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

        try
        {
            var content = await _access.ReadAsync(entry.Path, CurrentToken).ConfigureAwait(true);
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
            await stream.WriteAsync(content.Content, CurrentToken).ConfigureAwait(true);
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
    private Task Refresh() => SelectedFolder is { } folder ? LoadFolderAsync(folder) : Task.CompletedTask;

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
        : LoadFolderAsync(new ContainerFolderNode(entry.Name, entry.Path));

    private void PopulateChildren(ContainerFolderNode folder, IReadOnlyList<ContainerFileEntry> entries)
    {
        folder.Children.Clear();
        foreach (var entry in entries.Where(static e => e.IsDirectory))
        {
            folder.Children.Add(new ContainerFolderNode(entry.Name, entry.Path));
        }
    }

    private CancellationToken CurrentToken
    {
        get
        {
            _operation?.Cancel();
            _operation?.Dispose();
            _operation = new CancellationTokenSource();
            return _operation.Token;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _operation?.Cancel();
        _operation?.Dispose();
        _operation = null;
        (TableSource as IDisposable)?.Dispose();
    }
}
