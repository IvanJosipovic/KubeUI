using System.Collections;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public sealed class ResourceConfigBasePermissionTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void permissions_manifest_includes_default_and_custom_permissions()
    {
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = new TrackingResourceConfig(services);

        config.Permissions().ShouldBe(
        [
            (Verb.Create, null),
            (Verb.Delete, null),
            (Verb.List, null),
            (Verb.Patch, null),
            (Verb.Update, null),
            (Verb.Watch, null),
            (Verb.Get, "status")
        ]);
    }

    [AvaloniaFact]
    public async Task update_permissions_does_not_refresh_additional_permissions_when_list_and_watch_are_unavailable()
    {
        var runtime = new TestCluster
        {
            DefaultPermissionAllowed = false,
        };
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = new TrackingResourceConfig(services);
        config.Initialize(runtime.CreateWorkspace());

        await runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "team-a"
            }
        });
        runtime.SetPermission<V1Pod>(Verb.List, true, "team-a");
        runtime.SetPermission<V1Pod>(Verb.Watch, false, "team-a");

        await config.UpdatePermissions();

        config.PermissionsLoaded.ShouldBeTrue();
        config.CanListAndWatch.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task update_permissions_keeps_permissions_unloaded_when_additional_permission_refresh_fails()
    {
        var runtime = new TestCluster
        {
            DefaultPermissionAllowed = true,
        };
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = new ThrowingResourceConfig(services);
        config.Initialize(runtime.CreateWorkspace());

        await config.UpdatePermissions();

        config.CanListAndWatch.ShouldBeTrue();
        config.PermissionsLoaded.ShouldBeFalse();
    }

    private sealed class TrackingResourceConfig : ResourceConfigBase<V1Pod>
    {
        public TrackingResourceConfig(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override bool IsNamespaced => true;

        public override IList<(Verb verb, string? subResource)> CustomPermissions() =>
        [
            (Verb.Get, "status")
        ];
    }

    private sealed class ThrowingResourceConfig : ResourceConfigBase<V1Pod>
    {
        public ThrowingResourceConfig(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override bool IsNamespaced => true;

        protected override Task RefreshPermissionAsync(Verb verb, string? subResource)
        {
            if (verb == Verb.Patch)
            {
                throw new InvalidOperationException("Permission refresh failed.");
            }

            return base.RefreshPermissionAsync(verb, subResource);
        }
    }
}
