using System.Collections.Generic;
using KubeUI.Features.Crossplane.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.Crossplane.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = LanguageManager.Instance.NavigationView_Crossplane.Subscribe(seen.Add);

        LanguageManager.Instance.NavigationView_Crossplane.CurrentValue().ShouldBe("Crossplane");
        LanguageManager.Instance.MRDiffDetectionView_Title.CurrentValue().ShouldBe("MR Diff Detection");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Crossplane");
    }
}
