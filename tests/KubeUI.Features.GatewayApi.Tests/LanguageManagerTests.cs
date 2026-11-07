using System.Collections.Generic;
using KubeUI.Features.GatewayApi.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.GatewayApi.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_GatewayGroup.Subscribe(seen.Add);

        GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_GatewayGroup.CurrentValue().ShouldBe("Gateway");
        GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_ExperimentalGroup.CurrentValue().ShouldBe("Experimental");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Gateway");
    }
}
