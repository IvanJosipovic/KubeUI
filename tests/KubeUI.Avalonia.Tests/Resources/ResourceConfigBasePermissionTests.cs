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
    public async Task update_permissions_refreshes_non_list_permissions_sequentially()
    {
        var runtime = new TestCluster();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var config = new TrackingResourceConfig(services);
        config.Initialize(runtime.CreateWorkspace());

        await config.UpdatePermissions();

        config.MaxConcurrency.ShouldBe(1);
        config.RecordedPermissions.ShouldBe(
        [
            (Verb.Create, null),
            (Verb.Delete, null),
            (Verb.List, null),
            (Verb.Patch, null),
            (Verb.Update, null),
            (Verb.Watch, null),
            (Verb.Get, "status")
        ]);
        config.PermissionsLoaded.ShouldBeTrue();
        config.CanListAndWatch.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task update_permissions_prefetches_list_and_watch_before_marking_partial_access_ready()
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

        config.RecordedPermissions.ShouldBe(
        [
            (Verb.List, null),
            (Verb.Watch, null)
        ]);
        config.PermissionsLoaded.ShouldBeTrue();
        config.CanListAndWatch.ShouldBeFalse();
    }

    private sealed class TrackingResourceConfig : ResourceConfigBase<V1Pod>
    {
        private int _activeRefreshes;

        public TrackingResourceConfig(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public List<(Verb Verb, string? SubResource)> RecordedPermissions { get; } = [];

        public int MaxConcurrency { get; private set; }

        public override bool IsNamespaced => true;

        public override IList<(Verb verb, string? subResource)> CustomPermissions() =>
        [
            (Verb.Get, "status")
        ];

        protected override async Task RefreshPermissionAsync(Verb verb, string? subResource)
        {
            RecordedPermissions.Add((verb, subResource));
            var activeRefreshes = Interlocked.Increment(ref _activeRefreshes);
            MaxConcurrency = Math.Max(MaxConcurrency, activeRefreshes);

            try
            {
                await Task.Delay(10);
            }
            finally
            {
                Interlocked.Decrement(ref _activeRefreshes);
            }
        }
    }
}
