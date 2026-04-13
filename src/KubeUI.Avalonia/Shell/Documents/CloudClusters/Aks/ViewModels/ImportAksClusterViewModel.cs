using Azure.Identity;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.ViewModels;

public sealed partial class ImportAksClusterViewModel : ViewModelBase
{
    private readonly IAksClusterService _aksClusterService;
    private readonly ClusterWorkspaceCatalog _clusterCatalog;
    private readonly ILogger<ImportAksClusterViewModel> _logger;
    private bool _suppressSelectedSubscriptionLoad;

    public ImportAksClusterViewModel(
        IAksClusterService aksClusterService,
        ClusterWorkspaceCatalog clusterCatalog,
        ILogger<ImportAksClusterViewModel> logger)
    {
        _aksClusterService = aksClusterService;
        _clusterCatalog = clusterCatalog;
        _logger = logger;

        Title = Assets.Resources.ImportAksClusterViewModel_Title;
        Id = nameof(ImportAksClusterViewModel);

        _ = RefreshAuthenticationAsync();
        _ = RefreshSubscriptionsAsync();
    }

    [ObservableProperty]
    public partial ObservableCollection<AksSubscriptionInfo> Subscriptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<AksClusterInfo> Clusters { get; set; } = [];

    [ObservableProperty]
    public partial AksSubscriptionInfo? SelectedSubscription { get; set; }

    [ObservableProperty]
    public partial AksClusterInfo? SelectedCluster { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? AuthenticationStatusMessage { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        await RefreshAuthenticationAsync().ConfigureAwait(false);
        await RefreshSubscriptionsAsync().ConfigureAwait(false);
    }

    private bool CanRefresh()
    {
        return !IsBusy;
    }

    private async Task RefreshAuthenticationAsync()
    {
        try
        {
            var authenticationStatus = await _aksClusterService.GetAuthenticationStatusAsync().ConfigureAwait(false);

            string statusText = authenticationStatus.AzureCliSignedIn
                ? Assets.Resources.ImportAksClusterView_AzureCliSignedIn
                : Assets.Resources.ImportAksClusterView_AzureCliNotSignedIn;

            if (authenticationStatus.AzureCliSignedIn)
            {
                if (!string.IsNullOrWhiteSpace(authenticationStatus.AzureCliUsername) || !string.IsNullOrWhiteSpace(authenticationStatus.AzureCliTenantId))
                {
                    statusText = string.Concat(
                        statusText,
                        Environment.NewLine,
                        string.Format(
                            Assets.Resources.ImportAksClusterView_AccountFormat,
                            authenticationStatus.AzureCliUsername ?? "(unknown)",
                            authenticationStatus.AzureCliTenantId ?? "(unknown)"));
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() => AuthenticationStatusMessage = statusText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load AKS authentication state.");
            await Dispatcher.UIThread.InvokeAsync(() => AuthenticationStatusMessage = ex.Message);
        }
    }

    private async Task RefreshSubscriptionsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var subscriptions = await _aksClusterService.GetSubscriptionsAsync().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _suppressSelectedSubscriptionLoad = true;
                Subscriptions = new ObservableCollection<AksSubscriptionInfo>(subscriptions);
                SelectedSubscription = Subscriptions.FirstOrDefault();
                Clusters = [];
                SelectedCluster = null;
                _suppressSelectedSubscriptionLoad = false;
            });

            if (SelectedSubscription is not null)
            {
                IsBusy = false;
                await LoadClustersAsync(SelectedSubscription).ConfigureAwait(false);
            }
            else
            {
                StatusMessage = Assets.Resources.ImportAksClusterView_NoSubscriptions;
            }
        }
        catch (CredentialUnavailableException ex)
        {
            _logger.LogInformation(ex, "AKS subscription discovery is unavailable because Azure CLI is not signed in.");
            StatusMessage = Assets.Resources.ImportAksClusterView_NoSubscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load AKS subscriptions.");
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            NotifyImportCanExecuteChanged();
        }
    }

    private async Task LoadClustersAsync(AksSubscriptionInfo? subscription)
    {
        if (subscription == null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var clusters = await _aksClusterService.GetClustersAsync(subscription.SubscriptionId).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Clusters = new ObservableCollection<AksClusterInfo>(clusters.OrderBy(cluster => cluster.Name, StringComparer.Ordinal));
                SelectedCluster = Clusters.FirstOrDefault();
            });
        }
        catch (CredentialUnavailableException ex)
        {
            _logger.LogInformation(ex, "AKS cluster discovery is unavailable because Azure CLI is not signed in.");
            StatusMessage = Assets.Resources.ImportAksClusterView_NoSubscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load AKS clusters for {SubscriptionId}", subscription.SubscriptionId);
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            NotifyImportCanExecuteChanged();
        }
    }

    private bool CanImport()
    {
        return SelectedCluster != null && !IsBusy;
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async Task ImportAsync()
    {
        if (SelectedCluster == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await Dispatcher.UIThread.InvokeAsync(() => StatusMessage = Assets.Resources.ImportAksClusterView_Importing);

            var credentials = await _aksClusterService.GetCredentialsAsync(
                SelectedCluster,
                AksCredentialKind.User).ConfigureAwait(false);

            _clusterCatalog.ImportIntoKubeConfig(credentials.KubeConfig);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = string.Format(Assets.Resources.ImportAksClusterView_Imported, SelectedCluster.Name);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to import AKS cluster {Cluster}", SelectedCluster.Name);
            await Dispatcher.UIThread.InvokeAsync(() => StatusMessage = ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyImportCanExecuteChanged();
        }
    }

    partial void OnSelectedSubscriptionChanged(AksSubscriptionInfo? value)
    {
        if (_suppressSelectedSubscriptionLoad)
        {
            return;
        }

        _ = LoadClustersAsync(value);
    }

    partial void OnSelectedClusterChanged(AksClusterInfo? value)
    {
        NotifyImportCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        NotifyRefreshCanExecuteChanged();
        NotifyImportCanExecuteChanged();
    }

    private void NotifyRefreshCanExecuteChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            RefreshCommand.NotifyCanExecuteChanged();
            return;
        }

        Dispatcher.UIThread.Post(() => RefreshCommand.NotifyCanExecuteChanged());
    }

    private void NotifyImportCanExecuteChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ImportCommand.NotifyCanExecuteChanged();
            return;
        }

        Dispatcher.UIThread.Post(() => ImportCommand.NotifyCanExecuteChanged());
    }
}
