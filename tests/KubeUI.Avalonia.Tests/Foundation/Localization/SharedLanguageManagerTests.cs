using System.Collections.Generic;
using KubeUI.Foundation.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Foundation.Localization;

public sealed class SharedLanguageManagerTests
{
    [Fact]
    public void shared_language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = LanguageManager.Instance.HomeView_Header.Subscribe(seen.Add);

        LanguageManager.Keys.HomeView_Header.Key.ShouldBe("HomeView_Header");
        LanguageManager.Instance.HomeView_Header.CurrentValue().ShouldBe(seen[^1]);
        seen.ShouldNotBeEmpty();
    }
}
