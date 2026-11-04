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

        using var subscription = LanguageManager.Instance.CrossplaneNavigationSectionProvider_SectionTitle.Subscribe(seen.Add);

        LanguageManager.Instance.CrossplaneNavigationSectionProvider_SectionTitle.CurrentValue().ShouldBe("Crossplane");
        LanguageManager.Instance.MRDiffDetectionView_Title.CurrentValue().ShouldBe("MR Diff Detection");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Crossplane");
    }
}
