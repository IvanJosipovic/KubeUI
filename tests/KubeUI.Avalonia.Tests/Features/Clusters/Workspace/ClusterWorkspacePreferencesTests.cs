using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Services.Settings;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Workspace;

public sealed class ClusterWorkspacePreferencesTests
{
    [AvaloniaFact]
    public void attach_applies_persisted_alias_and_enabled_state()
    {
        var services = Application.Current.GetTestServices();
        var runtime = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single().Runtime;
        var settingsService = services.GetRequiredService<ISettingsService>();
        var clusterSettings = settingsService.Settings.GetClusterSettings(runtime);
        clusterSettings.Alias = "production";
        clusterSettings.Enabled = false;
        var preferences = new ClusterWorkspacePreferences(settingsService);
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, runtime);

        preferences.Attach(workspace);

        workspace.Alias.ShouldBe("production");
        workspace.IsEnabled.ShouldBeFalse();
        workspace.DisplayName.ShouldBe("production");
        preferences.Detach(workspace);
    }

    [AvaloniaFact]
    public void workspace_changes_are_persisted_until_detached()
    {
        var services = Application.Current.GetTestServices();
        var runtime = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single().Runtime;
        var settingsService = services.GetRequiredService<ISettingsService>();
        var preferences = new ClusterWorkspacePreferences(settingsService);
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, runtime);
        preferences.Attach(workspace);

        workspace.Alias = "  staging  ";
        workspace.IsEnabled = false;

        var stored = settingsService.Settings.FindClusterSettings(runtime);
        stored.ShouldNotBeNull();
        stored!.Alias.ShouldBe("staging");
        stored.Enabled.ShouldBeFalse();

        workspace.Alias = "   ";
        stored.Alias.ShouldBeNull();
        workspace.DisplayName.ShouldBe(runtime.Name);

        preferences.Detach(workspace);
        workspace.Alias = "ignored";
        stored.Alias.ShouldBeNull();
    }

    [AvaloniaFact]
    public void display_name_and_title_follow_alias_and_runtime_name()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        List<string?> changed = [];
        workspace.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        workspace.Alias = "dev";
        workspace.DisplayName.ShouldBe("dev");
        workspace.Title.ShouldBe("dev");

        workspace.Alias = null;
        workspace.Runtime.Name = "renamed-context";
        workspace.DisplayName.ShouldBe("renamed-context");
        changed.ShouldContain(nameof(ClusterWorkspace.DisplayName));
        changed.ShouldContain(nameof(ClusterWorkspace.Title));
    }

    [AvaloniaFact]
    public async Task disabling_a_connected_workspace_disconnects_it()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();
        await TestWait.UntilAsync(() => workspace.Runtime.Connected, 5000, TestContext.Current.CancellationToken, () => Dispatcher.UIThread.RunJobs());

        workspace.IsEnabled = false;

        await TestWait.UntilAsync(() => !workspace.Runtime.Connected, 5000, TestContext.Current.CancellationToken, () => Dispatcher.UIThread.RunJobs());
    }
}
