using System.Collections.Generic;
using KubeUI.Features.Resources.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.Resources.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = ResourcesLanguageManager.Instance.ResourcePropertiesView_Name.Subscribe(seen.Add);

        ResourcesLanguageManager.Instance.ResourcePropertiesView_Name.CurrentValue().ShouldBe("Name");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Name");
    }
}
