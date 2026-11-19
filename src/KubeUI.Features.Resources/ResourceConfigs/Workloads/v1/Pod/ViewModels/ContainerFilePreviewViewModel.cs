using AvaloniaEdit.Document;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>Shows one container file's contents in the bottom tool dock.</summary>
public sealed partial class ContainerFilePreviewViewModel : ViewModelBase, IClusterWorkspaceContext, IDisposable
{
    private readonly IContainerFileAccessFactory _accessFactory;
    private readonly ILogger<ContainerFilePreviewViewModel> _logger;
    private IContainerFileAccess? _access;
    private CancellationTokenSource? _load;
    private bool _disposed;

    /// <summary>Creates the preview.</summary>
    public ContainerFilePreviewViewModel(
        IContainerFileAccessFactory accessFactory,
        ILogger<ContainerFilePreviewViewModel> logger)
    {
        _accessFactory = accessFactory;
        _logger = logger;
    }

    /// <summary>The cluster the previewed file is read from.</summary>
    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    /// <summary>The pod the previewed file is read from.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Identity))]
    public partial V1Pod Object { get; set; }

    /// <summary>The container the previewed file is read from.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Identity))]
    public partial string ContainerName { get; set; }

    /// <summary>The absolute path of the previewed file inside the container.</summary>
    [ObservableProperty]
    public partial string Path { get; set; }

    /// <summary>The namespace, pod and container identity shown in the header.</summary>
    public string Identity => $"{Object?.Namespace()}/{Object?.Name()}/{ContainerName}";

    /// <summary>The document holding the previewed file's text.</summary>
    public TextDocument Document { get; } = new();

    /// <summary>Why the file cannot be previewed, when it cannot.</summary>
    [ObservableProperty]
    public partial string? Message { get; set; }

    /// <summary>Whether the previewed file was truncated.</summary>
    [ObservableProperty]
    public partial bool IsTruncated { get; set; }

    /// <summary>Whether the preview is loading.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>Reads the file into the preview.</summary>
    public async Task LoadAsync()
    {
        var token = Renew();
        IsLoading = true;
        Document.Text = string.Empty;
        Message = null;
        IsTruncated = false;
        try
        {
            _access ??= _accessFactory.Create(Cluster, Object.Name(), Object.Namespace(), ContainerName);
            var content = await _access.ReadAsync(Path, token).ConfigureAwait(true);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!content.IsText)
            {
                Message = ResourcesLanguageManager.Instance.ContainerFileBrowserView_Binary.CurrentValue();
                return;
            }

            Document.Text = System.Text.Encoding.UTF8.GetString(content.Content);
            IsTruncated = content.Truncated;
        }
        catch (OperationCanceledException)
        {
            // Superseded or closed.
        }
        catch (Exception ex)
        {
            // A superseded load can still fail after its token was cancelled; that failure must not
            // replace the message of the load that replaced it.
            if (token.IsCancellationRequested)
            {
                return;
            }

            _logger.LogWarning(ex, "Container file preview could not read {Path}.", Path);
            Message = ex.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>Reads the file again.</summary>
    [RelayCommand]
    private Task Refresh() => LoadAsync();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _load?.Cancel();
        _load?.Dispose();
        _load = null;
    }

    private CancellationToken Renew()
    {
        // The previous source is cancelled but not disposed: the operation it belongs to may still
        // observe its token, and disposing it from this thread can race that operation's registrations.
        _load?.Cancel();
        _load = new CancellationTokenSource();
        return _load.Token;
    }
}
