using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret.Views;
using KubeUI.Avalonia.Resources.Core.v1;
using KubeUI.Avalonia.Tests.Infra;
using k8s.Models;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public sealed class ResourceFeatureConfigTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void secret_config_uses_secret_local_properties_view()
    {
        var config = new V1SecretConfig();

        var controls = config.Properties(new V1Secret());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<SecretPropertiesView>();
    }

    [AvaloniaFact]
    public void event_config_uses_custom_last_seen_cell()
    {
        var config = new V1EventConfig();

        var lastSeenColumn = config.Columns().Single(x => x.Name == "Last Seen");

        lastSeenColumn.CustomControl.Name.ShouldBe("EventLastSeenCell");
    }
}
