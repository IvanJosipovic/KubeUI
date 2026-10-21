using Avalonia.Controls.Selection;
using Dock.Model.Core;
using DynamicData;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm.Repositories;

public sealed partial class HelmRepositoriesViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly SourceCache<HelmRepository, string> _repositories = new(static repository => repository.Name);
    private readonly DynamicTableViewSource<HelmRepository, string> _tableSource;
    private HelmConnection? _connection;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial HelmRepository? SelectedRepository { get; set; }

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    public IDynamicTableViewSource TableSource => _tableSource;

    public ISelectionModel SelectionModel => _tableSource.SelectionModel;

    public HelmRepositoriesViewModel(IHelmService helmService, IFactory factory, IDialogService dialogService)
    {
        _helmService = helmService;
        _factory = factory;
        _dialogService = dialogService;
        _tableSource = new DynamicTableViewSource<HelmRepository, string>(
            _repositories,
            static repository => repository.Name,
            [
                CreateColumn("name", Assets.Resources.HelmRepositoriesView_Name!, static repository => repository.Name, DynamicTableViewWidthMode.Pixel, 220),
                CreateColumn("url", Assets.Resources.HelmRepositoriesView_Url!, static repository => repository.Url, DynamicTableViewWidthMode.Star, 1)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
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
            Status = string.Empty;
        }
        catch (Exception exception)
        {
            Status = exception.Message;
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
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task UpdateRepository(HelmRepository? repository, CancellationToken cancellationToken = default)
    {
        if (repository is null)
        {
            return;
        }

        await UpdateIndexes(cancellationToken);
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
            SecondaryButtonText = Assets.Resources.HelmRepositoryEditorView_Cancel,
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
            Status = exception.Message;
        }
    }

    private void OpenEditor(HelmRepository? repository)
    {
        if (Cluster is null || _connection is null)
        {
            return;
        }

        var editor = new HelmRepositoryEditorViewModel(_helmService, _factory, this);
        editor.Initialize(Cluster, _connection, repository);
        _factory.AddToDocuments(editor);
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
        _tableSource.SelectionModel.SelectionChanged -= SelectionChanged;
        _tableSource.Dispose();
        _repositories.Dispose();
    }
}
