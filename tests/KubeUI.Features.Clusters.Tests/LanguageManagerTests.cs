using System.Collections.Generic;
using KubeUI.Features.Clusters.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.Clusters.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = LanguageManager.Instance.ClusterErrorView_Title.Subscribe(seen.Add);

        LanguageManager.Instance.ClusterErrorView_Title.CurrentValue().ShouldBe("Cluster Error");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("Cluster Error");
    }
}
