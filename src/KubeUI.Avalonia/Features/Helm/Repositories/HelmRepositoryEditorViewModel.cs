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
    private readonly IHelmListNotifier _listNotifier;
    private HelmConnection? _connection;
    private string? _originalName;
    private string _originalUrl = string.Empty;

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

    public HelmRepositoryEditorViewModel(IHelmService helmService, IFactory factory, IHelmListNotifier listNotifier)
    {
        _helmService = helmService;
        _factory = factory;
        _listNotifier = listNotifier;
    }

    public void Initialize(ClusterWorkspace cluster, HelmConnection connection, HelmRepository? repository)
    {
        _connection = connection;
        _originalName = repository?.Name;
        _originalUrl = repository?.Url ?? string.Empty;
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
            var name = Name.Trim();
            var url = Url.Trim();
            var username = string.IsNullOrWhiteSpace(Username) ? null : Username;
            var password = string.IsNullOrEmpty(Password) ? null : Password;
            var changed = _originalName is null
                || !string.Equals(_originalName, name, StringComparison.Ordinal)
                || !string.Equals(_originalUrl, url, StringComparison.Ordinal)
                || username is not null
                || password is not null;

            // Helm does not return stored credentials, so re-adding an unchanged repository would drop its basic
            // auth without the user asking for any change.
            if (changed)
            {
                await _helmService.AddRepositoryAsync(
                    _connection,
                    name,
                    url,
                    username,
                    password,
                    cancellationToken);
                if (_originalName is not null && !string.Equals(_originalName, name, StringComparison.Ordinal))
                {
                    await _helmService.RemoveRepositoryAsync(_connection, _originalName, cancellationToken);
                }
            }

            _listNotifier.NotifyRepositoriesChanged();
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
