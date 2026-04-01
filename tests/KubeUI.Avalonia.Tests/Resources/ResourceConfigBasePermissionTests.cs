using System.Collections;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public sealed class ResourceConfigBasePermissionTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task update_permissions_refreshes_non_list_permissions_sequentially()
    {
        var runtime = new TestCluster();
        var config = new TrackingResourceConfig();
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

    private sealed class TrackingResourceConfig : ResourceConfigBase<V1Pod>
    {
        private int _activeRefreshes;

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
