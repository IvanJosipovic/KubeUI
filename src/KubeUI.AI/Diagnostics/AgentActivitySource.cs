using System.Diagnostics;

namespace KubeUI.AI.Diagnostics;

public static class AgentActivitySource
{
    public const string SourceName = "KubeUI.AI";

    public static ActivitySource Source { get; } = new(SourceName, "1.0.0");
}
