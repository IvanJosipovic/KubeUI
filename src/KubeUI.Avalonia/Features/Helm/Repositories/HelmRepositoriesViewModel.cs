using System.ComponentModel;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using DynamicData;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;

namespace KubeUI.Avalonia.Features.Helm.Repositories;

public sealed partial class HelmRepositoriesViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IHelmService _helmService;
    private readonly IDialogService _dialogService;
    private readonly IHelmDocumentOpener _documentOpener;
    private readonly IHelmListNotifier _listNotifier;
    private readonly ILogger<HelmRepositoriesViewModel>? _logger;
    private readonly SourceCache<HelmRepository, string> _repositories = new(static repository => repository.Name);
    private readonly DynamicTableViewSource<HelmRepository, string> _tableSource;
    private HelmConnection? _connection;
    private bool _disposed;

    [ObservableProperty]
    public partial int ItemCount { get; private set; }

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial HelmRepository? SelectedRepository { get; set; }

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    public IDynamicTableViewSource TableSource => _tableSource;

    public ISelectionModel SelectionModel => _tableSource.SelectionModel;

    public HelmRepositoriesViewModel(
        IHelmService helmService,
        IDialogService dialogService,
        IHelmDocumentOpener documentOpener,
        IHelmListNotifier listNotifier,
        ILogger<HelmRepositoriesViewModel>? logger = null)
    {
        _helmService = helmService;
        _dialogService = dialogService;
        _documentOpener = documentOpener;
        _listNotifier = listNotifier;
        _logger = logger;
        _listNotifier.RepositoriesChanged += OnRepositoriesChanged;
        _tableSource = new DynamicTableViewSource<HelmRepository, string>(
            _repositories,
            static repository => repository.Name,
            [
                CreateColumn("name", Assets.Resources.HelmRepositoriesView_Name!, static repository => repository.Name, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("url", Assets.Resources.HelmRepositoriesView_Url!, static repository => repository.Url, DynamicTableViewWidthMode.Star, 2)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SetSort([new("name", ListSortDirection.Ascending)]);
        _tableSource.SelectionModel.SelectionChanged += SelectionChanged;
        Title = Assets.Resources.HelmRepositoriesView_Title;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Id = $"{nameof(HelmRepositoriesViewModel)}-{cluster.Runtime.Name}";
        _connection = new HelmConnection(cluster.Runtime.KubeConfigPath, cluster.Runtime.Name, null);
        _ = RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task Refresh(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            var repositories = await _helmService.ListRepositoriesAsync(_connection, cancellationToken);
            _repositories.Edit(updater =>
            {
                updater.Clear();
                updater.AddOrUpdate(repositories);
            });
            ItemCount = repositories.Count;
            Status = string.Empty;
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "Failed to refresh Helm repositories");
            Status = Assets.Resources.HelmRepositoriesView_RefreshFailed!;
        }
    }

    [RelayCommand]
    private async Task UpdateIndexes(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _helmService.UpdateRepositoriesAsync(_connection, cancellationToken);
            Status = Assets.Resources.HelmRepositoriesView_IndexesUpdated!;
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "Failed to update Helm repository indexes");
            Status = Assets.Resources.HelmRepositoriesView_IndexUpdateFailed!;
        }
    }

    [RelayCommand]
    private void NewRepository() => OpenEditor(null);

    [RelayCommand]
    private void EditRepository(HelmRepository? repository)
    {
        if (repository is not null)
        {
            OpenEditor(repository);
        }
    }

    [RelayCommand]
    private async Task DeleteRepository(HelmRepository? repository, CancellationToken cancellationToken = default)
    {
        if (_connection is null || repository is null)
        {
            return;
        }

        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = Assets.Resources.HelmRepositoriesView_DeleteTitle,
            Content = string.Format(Assets.Resources.HelmRepositoriesView_DeleteContent!, repository.Name),
            PrimaryButtonText = Assets.Resources.HelmRepositoriesView_Delete,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary
        });

        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _helmService.RemoveRepositoryAsync(_connection, repository.Name, cancellationToken);
            await Refresh(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "Failed to delete Helm repository {RepositoryName}", repository.Name);
            Status = Assets.Resources.HelmRepositoriesView_DeleteFailed!;
        }
    }

    [RelayCommand]
    private void ViewCharts(HelmRepository? repository)
    {
        if (repository is null || Cluster is null)
        {
            return;
        }

        _documentOpener.OpenCharts(Cluster, repository.Name);
    }

    private void OpenEditor(HelmRepository? repository)
    {
        if (Cluster is null || _connection is null)
        {
            return;
        }

        _documentOpener.OpenRepositoryEditor(Cluster, _connection, repository);
    }

    private void OnRepositoriesChanged(object? sender, EventArgs args)
    {
        if (_disposed)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            _ = RefreshCommand.ExecuteAsync(null);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed)
            {
                _ = RefreshCommand.ExecuteAsync(null);
            }
        });
    }

    private void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs args)
        => SelectedRepository = _tableSource.SelectionModel.SelectedItem as HelmRepository;

    private static DynamicTableViewColumn<HelmRepository> CreateColumn<TValue>(
        string key,
        string header,
        Func<HelmRepository, TValue> selector,
        DynamicTableViewWidthMode widthMode,
        double width)
    {
        var column = DynamicTableViewColumn<HelmRepository>.Create(key, header, selector);
        column.WidthMode = widthMode;
        column.Width = width;
        return column;
    }

    public void Dispose()
    {
        _disposed = true;
        _listNotifier.RepositoriesChanged -= OnRepositoriesChanged;
        _tableSource.SelectionModel.SelectionChanged -= SelectionChanged;
        _tableSource.Dispose();
        _repositories.Dispose();
    }
}
