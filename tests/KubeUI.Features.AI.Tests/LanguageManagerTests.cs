using System.Collections.Generic;
using KubeUI.Features.AI.Assets;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Features.AI.Tests;

public sealed class LanguageManagerTests
{
    [Fact]
    public void language_manager_serves_json_values_and_notifies_subscribers()
    {
        var seen = new List<string?>();

        using var subscription = AiLanguageManager.Instance.AgentChatView_Title.Subscribe(seen.Add);

        AiLanguageManager.Instance.AgentChatView_Title.CurrentValue().ShouldBe("AI Assistant");
        seen.ShouldNotBeEmpty();
        seen[^1].ShouldBe("AI Assistant");
    }
}
