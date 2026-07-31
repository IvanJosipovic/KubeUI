using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public sealed class ResourceConfigBasePermissionTests
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
}
