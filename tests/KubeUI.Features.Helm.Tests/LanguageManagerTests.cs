using System.Collections.Generic;
using KubeUI.Features.Helm.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.Helm.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = LanguageManager.Instance.NavigationView_Helm.Subscribe(seen.Add);

        LanguageManager.Instance.NavigationView_Helm.CurrentValue().ShouldBe("Helm");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Helm");
    }
}
