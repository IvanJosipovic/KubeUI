using System.ComponentModel;

namespace KubeUI.Features.AI.Chat;

/// <summary>Exposes the settings that the AI chat feature needs from its host application.</summary>
public interface IAgentChatSettings : INotifyPropertyChanged
{
    /// <summary>Gets the configured agent identifier.</summary>
    string? SelectedAgentId { get; }

    /// <summary>Gets whether the host MCP endpoint is enabled.</summary>
    bool McpServerEnabled { get; }
}
