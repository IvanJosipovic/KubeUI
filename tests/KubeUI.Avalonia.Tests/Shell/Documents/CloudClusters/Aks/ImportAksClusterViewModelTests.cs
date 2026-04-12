using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.ViewModels;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Documents.CloudClusters.Aks;

public sealed class ImportAksClusterViewModelTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task connect_imports_aks_credentials_into_cluster_catalog()
    {
        var catalog = TestApp.CurrentServices!.GetRequiredService<ClusterWorkspaceCatalog>();
        var viewModel = new ImportAksClusterViewModel(
            new FakeAksClusterService(),
            catalog,
            TestApp.CurrentServices!.GetRequiredService<ILogger<ImportAksClusterViewModel>>());

        await WaitForAsync(() => viewModel.Subscriptions.Count == 1 && viewModel.Clusters.Count == 1);

        viewModel.SelectedCluster.ShouldNotBeNull();
        var clusterName = viewModel.SelectedCluster!.Name;

        viewModel.ImportCommand.Execute(null);
        await WaitForAsync(() => catalog.Clusters.Any(x => x.Name == clusterName));

        catalog.Clusters.Any(x => x.Name == clusterName).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task refresh_shows_empty_state_when_no_subscriptions_are_available()
    {
        var catalog = TestApp.CurrentServices!.GetRequiredService<ClusterWorkspaceCatalog>();
        var viewModel = new ImportAksClusterViewModel(
            new EmptyAksClusterService(),
            catalog,
            TestApp.CurrentServices!.GetRequiredService<ILogger<ImportAksClusterViewModel>>());

        await WaitForAsync(() => viewModel.StatusMessage == KubeUI.Avalonia.Assets.Resources.ImportAksClusterView_NoSubscriptions);

        viewModel.Subscriptions.ShouldBeEmpty();
        viewModel.Clusters.ShouldBeEmpty();
        viewModel.SelectedSubscription.ShouldBeNull();
        viewModel.SelectedCluster.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task refresh_is_disabled_while_busy()
    {
        var catalog = TestApp.CurrentServices!.GetRequiredService<ClusterWorkspaceCatalog>();
        var viewModel = new ImportAksClusterViewModel(
            new EmptyAksClusterService(),
            catalog,
            TestApp.CurrentServices!.GetRequiredService<ILogger<ImportAksClusterViewModel>>());

        await WaitForAsync(() => viewModel.StatusMessage == KubeUI.Avalonia.Assets.Resources.ImportAksClusterView_NoSubscriptions);

        viewModel.RefreshCommand.CanExecute(null).ShouldBeTrue();

        viewModel.IsBusy = true;

        viewModel.RefreshCommand.CanExecute(null).ShouldBeFalse();
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private sealed class FakeAksClusterService : IAksClusterService
    {
        public Task<AksAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AksAuthenticationStatus
            {
                AzureCliSignedIn = true,
                AzureCliUsername = "cli@example.com",
                AzureCliTenantId = "tenant-1"
            });
        }

        public Task<IReadOnlyList<AksSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AksSubscriptionInfo> subscriptions =
            [
                new AksSubscriptionInfo
                {
                    SubscriptionId = "sub-1",
                    DisplayName = "Subscription One"
                }
            ];

            return Task.FromResult(subscriptions);
        }

        public Task<IReadOnlyList<AksClusterInfo>> GetClustersAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AksClusterInfo> clusters =
            [
                new AksClusterInfo
                {
                    SubscriptionId = subscriptionId,
                    ResourceGroupName = "rg-1",
                    Name = "aks-test",
                    Location = "westus2",
                    KubernetesVersion = "1.29",
                    Fqdn = "aks-test.example.com"
                }
            ];

            return Task.FromResult(clusters);
        }

        public Task<AksClusterCredentials> GetCredentialsAsync(AksClusterInfo cluster, AksCredentialKind credentialKind = AksCredentialKind.User, CancellationToken cancellationToken = default)
        {
            K8SConfiguration kubeConfig = new()
            {
                FileName = string.Empty,
                Contexts =
                [
                    new Context
                    {
                        Name = cluster.Name,
                        ContextDetails = new ContextDetails
                        {
                            Cluster = cluster.Name,
                            User = "user"
                        }
                    }
                ]
            };

            return Task.FromResult(new AksClusterCredentials
            {
                Cluster = cluster,
                CredentialKind = credentialKind,
                KubeConfig = kubeConfig,
                RequiresKubelogin = false
            });
        }
    }

    private sealed class EmptyAksClusterService : IAksClusterService
    {
        public Task<AksAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AksAuthenticationStatus
            {
                AzureCliSignedIn = false,
                AzureCliUsername = null,
                AzureCliTenantId = null
            });
        }

        public Task<IReadOnlyList<AksSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AksSubscriptionInfo>>([]);
        }

        public Task<IReadOnlyList<AksClusterInfo>> GetClustersAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AksClusterInfo>>([]);
        }

        public Task<AksClusterCredentials> GetCredentialsAsync(AksClusterInfo cluster, AksCredentialKind credentialKind = AksCredentialKind.User, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
