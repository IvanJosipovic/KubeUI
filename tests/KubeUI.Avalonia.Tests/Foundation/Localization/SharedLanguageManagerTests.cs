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

        using var subscription = LanguageManager.Instance.Shared_Cancel.Subscribe(seen.Add);

        LanguageManager.Keys.Shared_Cancel.Key.ShouldBe("Shared_Cancel");
        LanguageManager.Instance.Shared_Cancel.CurrentValue().ShouldBe(seen[^1]);
        seen.ShouldNotBeEmpty();
    }
}
