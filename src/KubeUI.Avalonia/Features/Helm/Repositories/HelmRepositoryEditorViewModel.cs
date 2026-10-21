using Dock.Model.Core;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm.Repositories;

public sealed partial class HelmRepositoryEditorViewModel : ViewModelBase
{
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly HelmRepositoriesViewModel _parent;
    private HelmConnection? _connection;
    private string? _originalName;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Url { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Error { get; private set; } = string.Empty;

    public HelmRepositoryEditorViewModel(IHelmService helmService, IFactory factory, HelmRepositoriesViewModel parent)
    {
        _helmService = helmService;
        _factory = factory;
        _parent = parent;
    }

    public void Initialize(ClusterWorkspace cluster, HelmConnection connection, HelmRepository? repository)
    {
        _connection = connection;
        _originalName = repository?.Name;
        Name = repository?.Name ?? string.Empty;
        Url = repository?.Url ?? string.Empty;
        Title = repository is null ? Assets.Resources.HelmRepositoryEditorView_NewTitle : Assets.Resources.HelmRepositoryEditorView_EditTitle;
        Id = $"{nameof(HelmRepositoryEditorViewModel)}-{cluster.Runtime.Name}-{_originalName ?? Guid.NewGuid().ToString("N")}";
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _helmService.AddRepositoryAsync(
                _connection,
                Name.Trim(),
                Url.Trim(),
                string.IsNullOrWhiteSpace(Username) ? null : Username,
                string.IsNullOrEmpty(Password) ? null : Password,
                cancellationToken);
            if (_originalName is not null && !string.Equals(_originalName, Name.Trim(), StringComparison.Ordinal))
            {
                await _helmService.RemoveRepositoryAsync(_connection, _originalName, cancellationToken);
            }

            await _parent.RefreshCommand.ExecuteAsync(null);
            _factory.RemoveDockable(this, false);
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => _factory.RemoveDockable(this, false);

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Uri.TryCreate(Url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnUrlChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
