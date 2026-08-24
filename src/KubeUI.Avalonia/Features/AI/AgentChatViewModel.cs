using System.Collections.ObjectModel;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Services.Settings;
using Avalonia.Threading;
using LiveMarkdown.Avalonia;
using KubeUI.Avalonia.Options;

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
        If KubeUI MCP is unavailable, say so clearly and explain what access is missing and how to enable it.
        Keep responses concise and focused on the user's request.
        """;

    private readonly IAgentRegistry _registry;
    private readonly ISettingsService? _settingsService;
    private readonly IAgentContextService? _contextService;
    private readonly IMcpServerState? _mcpServerState;
    private IAgentSession? _session;
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
    public partial AgentContext? Context { get; set; }

    public AgentChatViewModel(
        IAgentRegistry registry,
        ISettingsService? settingsService = null,
        IAgentContextService? contextService = null,
        IMcpServerState? mcpServerState = null)
    {
        _registry = registry;
        _settingsService = settingsService;
        _contextService = contextService;
        _mcpServerState = mcpServerState;
        Messages.CollectionChanged += MessagesOnCollectionChanged;
        if (_settingsService is not null)
            _settingsService.Settings.PropertyChanged += SettingsOnPropertyChanged;
        if (_contextService is not null)
        {
            Context = _contextService.Context;
            _contextService.ContextChanged += ContextServiceOnContextChanged;
        }
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
        try
        {
            await DisposeSessionAsync();
            _turnCancellation = new CancellationTokenSource();
            var mcpEndpoint = _settingsService is not null && _settingsService.Settings.McpServerEnabled
                ? _mcpServerState?.Endpoint ?? McpServerConfiguration.GetEndpoint(_settingsService.Settings)
                : null;
            _session = await SelectedAgent.CreateSessionAsync(new AgentSessionOptions
            {
                Context = Context,
                McpEndpoint = mcpEndpoint,
                TrustedMcpServers = new HashSet<string>(StringComparer.Ordinal) { "kubeui" }
            }, _turnCancellation.Token);
            var promptTask = _session.PromptAsync(BuildAgentPrompt(text), _turnCancellation.Token);
            await foreach (var item in _session.Events)
            {
                if (item is AgentMessageEvent message)
                    AddAgentMessage(message.Message);
                else if (item is AgentToolStartedEvent toolStarted)
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Tool,
                        string.Format(Assets.Resources.AgentChatView_ToolStartedFormat!, toolStarted.Tool.Name, FormatInput(toolStarted.Tool.Input))));
                else if (item is AgentToolCompletedEvent toolCompleted)
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Tool,
                        string.Format(
                            Assets.Resources.AgentChatView_ToolCompletedFormat!,
                            toolCompleted.Result.Succeeded
                                ? Assets.Resources.AgentChatView_ToolSucceeded
                                : Assets.Resources.AgentChatView_ToolFailed,
                            toolCompleted.Result.Name)));
                else if (item is AgentPlanChangedEvent plan)
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Plan,
                        string.Join(
                            Environment.NewLine,
                            plan.Plan.Steps.Select(step => string.Format(Assets.Resources.AgentChatView_PlanStepFormat!, step)))));
                else if (item is AgentPermissionRequestedEvent permission)
                    Messages.Add(new AgentChatMessage(
                        Assets.Resources.AgentChatView_Permission,
                        string.Format(
                            Assets.Resources.AgentChatView_PermissionFormat!,
                            permission.Request.Action,
                            permission.Request.Resource ?? Assets.Resources.AgentChatView_Unspecified)));
                else if (item is AgentDiagnosticEvent diagnostic)
                    Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_Diagnostic, diagnostic.Text));
                else if (item is AgentTurnCompletedEvent)
                    break;
            }
            await promptTask;
        }
        catch (Exception ex)
        {
            if (_turnCancellation?.IsCancellationRequested == true)
                Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_Status, Assets.Resources.AgentChatView_Canceled));
            else
                Messages.Add(new AgentChatMessage(Assets.Resources.AgentChatView_Error, ex.Message));
        }
        finally
        {
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
        if (_settingsService is not null)
            _settingsService.Settings.PropertyChanged -= SettingsOnPropertyChanged;
        if (_contextService is not null)
            _contextService.ContextChanged -= ContextServiceOnContextChanged;
        await DisposeSessionAsync();
    }

    private void ContextServiceOnContextChanged(object? sender, EventArgs e)
    {
        Context = _contextService?.Context;
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SelectedAgentId))
            SelectedAgent = ResolveConfiguredAgent();
    }

    private IAgent? ResolveConfiguredAgent()
        => Agents.FirstOrDefault(agent => string.Equals(
            agent.Id,
            _settingsService?.Settings.SelectedAgentId,
            StringComparison.Ordinal)) ?? Agents.FirstOrDefault();

    private async Task DisposeSessionAsync()
    {
        if (_session is not null)
        {
            await _session.DisposeAsync().ConfigureAwait(false);
            _session = null;
        }
        _turnCancellation?.Cancel();
    }

    private static string FormatInput(string? input)
        => string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : string.Format(Assets.Resources.AgentChatView_ToolInputFormat!, input);

    private void AddAgentMessage(AgentMessage message)
    {
        if (message.Role == "assistant"
            && Messages.LastOrDefault() is { } previous
            && previous.Role == message.Role)
        {
            Messages[^1] = previous with { Text = previous.Text + message.Text };
            return;
        }

        Messages.Add(new AgentChatMessage(message.Role, message.Text));
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
