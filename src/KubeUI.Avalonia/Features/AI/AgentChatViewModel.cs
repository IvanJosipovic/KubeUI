using System.Text.Json;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using LiveMarkdown.Avalonia;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.Avalonia.Features.AI;

public sealed partial class AgentChatViewModel : ViewModelBase, IAsyncDisposable
{
    private const string KubeUiOperatingGuidance = """
        You are KubeUI's Kubernetes assistant.
        Prefer the KubeUI MCP server for cluster, resource, event, log, graph, and KubeUI operations.
        Use KubeUI MCP before filesystem, terminal, external tools, or skills.
        Call kubeui_list_supported_resources before listing a resource type and use its FullApiVersion value exactly.
        For non-core resources never reduce a group/version such as pkg.crossplane.io/v1 to v1.
        Ask for approval before using external tools, skills, or any operation that can change resources.
        Do not use another MCP server unless the user has explicitly approved that server in this chat.
        Never invent cluster state or claim an operation succeeded without tool confirmation.
        Finally, If KubeUI MCP is unavailable, say so clearly and explain what access is missing and how to enable it.
        Keep responses concise and focused on the user's request.
        """;

    private readonly IAgentRegistry _registry;
    private readonly ISettingsService _settingsService;
    private readonly IAgentContextService _contextService;
    private readonly IMcpServerState _mcpServerState;
    private readonly IMcpCredentialService? _mcpCredentialService;
    private readonly ILogger<AgentChatViewModel>? _logger;
    private IAgentSession? _session;
    private IDisposable? _mcpCredentialLease;
    private CancellationTokenSource? _turnCancellation;

    public ObservableCollection<AgentChatMessage> Messages { get; } = [];
    public ObservableStringBuilder MarkdownBuilder { get; } = new();
    public IReadOnlyList<IAgent> Agents => _registry.Agents;

    [ObservableProperty]
    public partial IAgent? SelectedAgent { get; set; }

    [ObservableProperty]
    public partial string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public new partial AgentContext? Context { get; set; }

    public AgentChatViewModel(
        IAgentRegistry registry,
        ISettingsService settingsService,
        IAgentContextService contextService,
        IMcpServerState mcpServerState,
        IMcpCredentialService? mcpCredentialService = null,
        ILogger<AgentChatViewModel>? logger = null)
    {
        _registry = registry;
        _settingsService = settingsService;
        _contextService = contextService;
        _mcpServerState = mcpServerState;
        _mcpCredentialService = mcpCredentialService;
        _logger = logger;
        Messages.CollectionChanged += MessagesOnCollectionChanged;
        _settingsService.Settings.PropertyChanged += SettingsOnPropertyChanged;
        Context = _contextService.Context;
        _contextService.ContextChanged += ContextServiceOnContextChanged;
        SelectedAgent = ResolveConfiguredAgent();
        Id = nameof(AgentChatViewModel);
        Title = Assets.Resources.AgentChatView_Title;
        CanClose = true;
        CanPin = true;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        var text = Prompt.Trim();
        if (text.Length == 0 || SelectedAgent is null || IsBusy)
            return;
        Prompt = string.Empty;
        Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_User, text));
        IsBusy = true;
        Task? promptTask = null;
        try
        {
            await DisposeSessionAsync();
            _turnCancellation = new CancellationTokenSource();
            var mcpEndpoint = _settingsService.Settings.McpServerEnabled
                && _mcpServerState.BoundPort is not null
                ? McpServerConfiguration.GetEndpoint(_mcpServerState.BoundPort.Value)
                : null;
            var mcpCredential = mcpEndpoint is null || _mcpCredentialService is null
                ? null
                : _mcpCredentialService.CreateSessionCredential();
            _mcpCredentialLease = mcpCredential;
            try
            {
                _session = await SelectedAgent.CreateSessionAsync(new AgentSessionOptions
                {
                    Context = Context,
                    McpEndpoint = mcpEndpoint,
                    McpHeaders = mcpCredential is null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Authorization"] = $"Bearer {mcpCredential.Token}"
                        },
                    McpCredentialLease = mcpCredential,
                    TrustedMcpServers = new HashSet<string>(StringComparer.Ordinal) { "kubeui" }
                }, _turnCancellation.Token);
            }
            catch (Exception exception)
            {
                mcpCredential?.Dispose();
                _mcpCredentialLease = null;
                throw;
            }
            promptTask = _session.PromptAsync(BuildAgentPrompt(text), _turnCancellation.Token);
            await foreach (var item in _session.Events)
            {
                if (item is SessionUpdateEvent { Update.Value: SessionUpdateVariant0UserMessageChunk userChunk }
                    && userChunk.Content.Value is TextContent userText)
                    Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_User, userText.Text));
                else if (item is SessionUpdateEvent { Update.Value: SessionUpdateVariant1AgentMessageChunk assistantChunk }
                    && assistantChunk.Content.Value is TextContent assistantText)
                    AddAgentMessage(assistantText.Text);
                else if (item is SessionUpdateEvent { Update.Value: ToolCall toolStarted })
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Tool,
                        string.Format(
                            Assets.Resources.AgentChatView_ToolStartedFormat!,
                            toolStarted.Title ?? toolStarted.Name ?? toolStarted.ToolCallId,
                            FormatInput(toolStarted.RawInput))));
                else if (item is SessionUpdateEvent { Update.Value: ToolCallUpdate toolCompleted }
                    && toolCompleted.Status is "completed" or "failed")
                {
                    var succeeded = toolCompleted.Status == "completed";
                    var toolName = toolCompleted.Title ?? toolCompleted.Name ?? toolCompleted.ToolCallId;
                    if (!succeeded)
                    {
                        _logger?.LogError(
                            "AI agent {AgentId} tool {ToolName} failed.",
                            SelectedAgent?.Id,
                            toolName);
                    }
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Tool,
                        string.Format(
                            Assets.Resources.AgentChatView_ToolCompletedFormat!,
                            succeeded
                                ? Assets.Resources.AgentChatView_ToolSucceeded
                                : Assets.Resources.AgentChatView_ToolFailed,
                            toolName)));
                }
                else if (item is SessionUpdateEvent { Update.Value: Plan plan })
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Plan,
                        string.Join(
                            Environment.NewLine,
                            plan.Entries.Select(entry => string.Format(Assets.Resources.AgentChatView_PlanStepFormat!, entry.Content)))));
                else if (item is Completed)
                    break;
            }
            await promptTask;
        }
        catch (Exception ex)
        {
            if (_turnCancellation?.IsCancellationRequested == true)
                Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_Status, Assets.Resources.AgentChatView_Canceled));
            else
            {
                _logger?.LogError(ex, "AI chat turn failed for agent {AgentId}", SelectedAgent?.Id);
                Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_Error, ex.Message));
            }
        }
        finally
        {
            try
            {
                await promptTask.ConfigureAwait(false);
            }
            catch
            {
            }
            IsBusy = false;
            _turnCancellation?.Dispose();
            _turnCancellation = null;
        }
    }

    private static string BuildAgentPrompt(string userPrompt)
        => $"{KubeUiOperatingGuidance}\n\nUser request:\n{userPrompt}";

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (!IsBusy || _session is null)
            return;

        _turnCancellation?.Cancel();
        await _session.CancelAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        Messages.CollectionChanged -= MessagesOnCollectionChanged;
        _settingsService.Settings.PropertyChanged -= SettingsOnPropertyChanged;
        _contextService.ContextChanged -= ContextServiceOnContextChanged;
        await DisposeSessionAsync();
    }

    private void ContextServiceOnContextChanged(object? sender, EventArgs e)
    {
        Context = _contextService.Context;
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SelectedAgentId))
            SelectedAgent = ResolveConfiguredAgent();
    }

    private IAgent? ResolveConfiguredAgent()
        => Agents.FirstOrDefault(agent => string.Equals(
            agent.Id,
            _settingsService.Settings.SelectedAgentId,
            StringComparison.Ordinal)) ?? Agents.FirstOrDefault();

    private async Task DisposeSessionAsync()
    {
        var session = _session;
        _session = null;
        var credentialLease = _mcpCredentialLease;
        _mcpCredentialLease = null;
        try
        {
            if (session is not null)
                await session.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            credentialLease?.Dispose();
            _turnCancellation?.Cancel();
            _turnCancellation?.Dispose();
            _turnCancellation = null;
        }
    }

    private void AddAgentMessage(string text)
    {
        if (Messages.LastOrDefault() is { } previous
            && previous.Role == "assistant")
        {
            Messages[^1] = previous with { Text = previous.Text + text };
            return;
        }

        Messages.Add(new AgentChatMessage("assistant", text));
    }

    private static string FormatInput(JsonElement? input)
    {
        if (input is not { } element || element.ValueKind == JsonValueKind.Undefined)
            return string.Empty;

        var raw = element.GetRawText();
        return string.IsNullOrWhiteSpace(raw)
            ? string.Empty
            : string.Format(Assets.Resources.AgentChatView_ToolInputFormat!, raw);
    }

    private void MessagesOnCollectionChanged(object? _, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ApplyMessagesChange(e));
            return;
        }

        ApplyMessagesChange(e);
    }

    private void ApplyMessagesChange(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add when e.NewItems is not null:
                foreach (AgentChatMessage message in e.NewItems)
                    AppendMarkdownMessage(message);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace
                when e.OldItems?.Count == 1
                && e.NewItems?.Count == 1
                && e.NewStartingIndex == Messages.Count - 1:
                var oldMessage = (AgentChatMessage)e.OldItems[0]!;
                var newMessage = (AgentChatMessage)e.NewItems[0]!;
                if (newMessage.Text.StartsWith(oldMessage.Text, StringComparison.Ordinal))
                {
                    MarkdownBuilder.Append(newMessage.Text[oldMessage.Text.Length..]);
                    break;
                }
                RebuildMarkdown();
                break;
            default:
                RebuildMarkdown();
                break;
        }
    }

    private void RebuildMarkdown()
    {
        MarkdownBuilder.Clear();
        foreach (var message in Messages)
            AppendMarkdownMessage(message);
    }

    private void AppendMarkdownMessage(AgentChatMessage message)
    {
        if (MarkdownBuilder.Length > 0)
            MarkdownBuilder.Append("\n\n");

        MarkdownBuilder.Append($"**{message.Role}**\n\n");
        MarkdownBuilder.Append(message.Text);
    }
}
