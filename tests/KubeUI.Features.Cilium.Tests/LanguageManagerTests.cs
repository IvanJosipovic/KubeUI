using System.Collections.Generic;
using KubeUI.Features.Cilium.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.Cilium.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = LanguageManager.Instance.NavigationView_Cilium.Subscribe(seen.Add);

        LanguageManager.Instance.NavigationView_Cilium.CurrentValue().ShouldBe("Cilium");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Cilium");
    }
}
