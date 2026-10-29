using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Avalonia.Features.Clusters.Catalog;
using KubeUI.Avalonia.Services.Settings;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Catalog;

public sealed class ClusterListViewModelTests
{
    [AvaloniaFact]
    public async Task added_hidden_kubeconfig_is_listed_and_can_be_removed_without_deleting_file()
    {
        using var directory = new TemporaryDirectory();
        var path = WriteKubeConfig(directory.Path, ".custom.yaml", "custom-context");
        var services = Application.Current.GetTestServices();
        var catalog = services.GetRequiredService<ClusterWorkspaceCatalog>();
        using var vm = services.GetRequiredService<ClusterListViewModel>();

        vm.AddKubeConfigPaths([path]);
        await WaitAsync(() => vm.RemovableKubeConfigs.Contains(path) && catalog.Clusters.Any(x => x.Runtime.Name == "custom-context"));
        vm.RemovableKubeConfigs.ShouldNotContain(services.GetRequiredService<IKubeConfigPathProvider>().DefaultPath);

        await vm.RemoveKubeConfigCommand.ExecuteAsync(path).WaitAsync(TestContext.Current.CancellationToken);
        await WaitAsync(() => !vm.RemovableKubeConfigs.Contains(path) && catalog.Clusters.All(x => x.Runtime.Name != "custom-context"));

        File.Exists(path).ShouldBeTrue();
        services.GetRequiredService<ISettingsService>().Settings.KubeConfigs.ShouldNotContain(path);
    }

    [AvaloniaFact]
    public async Task invalid_kubeconfig_shows_warning_and_is_not_added()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "notes.txt");
        File.WriteAllText(path, "not a kubeconfig");
        var services = Application.Current.GetTestServices();
        using var vm = services.GetRequiredService<ClusterListViewModel>();

        vm.AddKubeConfigPaths([path]);
        await vm.RemoveKubeConfigCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        ((TestApp)Application.Current!).Notification.ShouldNotBeNull();
        services.GetRequiredService<ISettingsService>().Settings.KubeConfigs.ShouldNotContain(path);
    }

    [AvaloniaFact]
    public void auto_discovery_toggle_updates_catalog_and_settings()
    {
        var services = Application.Current.GetTestServices();
        var runtimeCatalog = services.GetRequiredService<IClusterRuntimeCatalog>();
        using var vm = services.GetRequiredService<ClusterListViewModel>();
        vm.KubeConfigAutoDiscoveryEnabled.ShouldBe(runtimeCatalog.KubeConfigAutoDiscoveryEnabled);

        vm.KubeConfigAutoDiscoveryEnabled = false;
        runtimeCatalog.KubeConfigAutoDiscoveryEnabled.ShouldBeFalse();
        services.GetRequiredService<ISettingsService>().Settings.KubeConfigAutoDiscoveryEnabled.ShouldBeFalse();

        vm.KubeConfigAutoDiscoveryEnabled = true;
        runtimeCatalog.KubeConfigAutoDiscoveryEnabled.ShouldBeTrue();
    }

    private static Task WaitAsync(Func<bool> predicate) =>
        TestWait.UntilAsync(predicate, 5000, TestContext.Current.CancellationToken, () => Dispatcher.UIThread.RunJobs());

    private static string WriteKubeConfig(string directory, string fileName, string contextName)
    {
        var path = Path.Combine(directory, fileName);
        K8SConfiguration config = new()
        {
            ApiVersion = "v1",
            Kind = "Config",
            Clusters = [new k8s.KubeConfigModels.Cluster { Name = contextName, ClusterEndpoint = new ClusterEndpoint { Server = "https://example.com" } }],
            Users = [new User { Name = contextName, UserCredentials = new UserCredentials() }],
            Contexts = [new Context { Name = contextName, ContextDetails = new ContextDetails { Cluster = contextName, User = contextName } }],
        };
        File.WriteAllText(path, KubernetesYaml.Serialize(config));
        return path;
    }
}
