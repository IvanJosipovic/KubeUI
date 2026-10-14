using KubeUI.AI.Agents;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Documents.Settings;

public sealed partial class SettingsViewModel : ViewModelBase, IDisposable, INotifyDataErrorInfo
{
    private const int MaximumExternalMcpClientNameLength = 80;
    private readonly IAgentRegistry? _agentRegistry;
    private readonly IMcpServerState? _mcpServerState;
    private readonly IMcpCredentialService _mcpCredentialService;
    private bool _shouldValidateExternalMcpClientName;
    private string? _externalMcpClientNameError;

    public ISettingsService SettingsService { get; }
    public IReadOnlyList<IAgent> Agents => _agentRegistry?.Agents ?? [];
    public IReadOnlyList<McpExternalClientGrant> ExternalMcpGrants => SettingsService.Settings.McpExternalClientGrants;

    /// <summary>Gets whether the external MCP client list is empty.</summary>
    public bool HasNoExternalMcpGrants => ExternalMcpGrants.Count == 0;

    /// <summary>Gets whether the MCP listener is unavailable while MCP is enabled.</summary>
    public bool IsMcpServerUnavailable =>
        SettingsService.Settings.McpServerEnabled
        && _mcpServerState?.BoundPort is null;

    /// <summary>Gets whether the external MCP client creation form can create a token.</summary>
    public bool CanCreateExternalMcpCredential => _mcpServerState?.BoundPort is not null;

    [ObservableProperty]
    public partial string ExternalMcpClientName { get; set; } = string.Empty;

    /// <summary>Gets cluster choices available for external MCP token scope.</summary>
    public ObservableCollection<McpExternalClusterOption> ExternalMcpClusterOptions { get; } = [];

    /// <summary>Gets whether any settings field currently has validation errors.</summary>
    public bool HasErrors => _externalMcpClientNameError is not null;

    /// <summary>Gets whether a token status message should be shown.</summary>
    public bool HasMcpGrantMessage => !string.IsNullOrEmpty(McpGrantMessage);

    /// <summary>Gets whether a newly created token is available to copy.</summary>
    public bool HasNewlyCreatedMcpToken => !string.IsNullOrEmpty(NewlyCreatedMcpToken);

    /// <summary>Raised when an external MCP client field's validation errors change.</summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>Gets validation errors for the requested property.</summary>
    /// <param name="propertyName">Property name, or null to get all validation errors.</param>
    /// <returns>Validation errors for the requested property.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (!string.IsNullOrEmpty(propertyName)
            && propertyName != nameof(ExternalMcpClientName))
            return Array.Empty<string>();

        return _externalMcpClientNameError is { } error ? [error] : Array.Empty<string>();
    }

    [ObservableProperty]
    public partial string? NewlyCreatedMcpToken { get; set; }

    [ObservableProperty]
    public partial string? McpGrantMessage { get; set; }

    [ObservableProperty]
    public partial McpExternalClientGrant? SelectedExternalMcpGrant { get; set; }

    public decimal McpServerPort
    {
        get => SettingsService.Settings.McpServerPort;
        set
        {
            var port = decimal.ToInt32(decimal.Clamp(value, 1024, 65535));
            if (SettingsService.Settings.McpServerPort == port)
                return;

            SettingsService.Settings.McpServerPort = port;
            OnPropertyChanged();
        }
    }

    public IAgent? SelectedAgent
    {
        get => Agents.FirstOrDefault(agent => string.Equals(
            agent.Id,
            SettingsService.Settings.SelectedAgentId,
            StringComparison.Ordinal)) ?? Agents.FirstOrDefault();
        set
        {
            var id = value?.Id;
            if (string.Equals(SettingsService.Settings.SelectedAgentId, id, StringComparison.Ordinal))
                return;

            SettingsService.Settings.SelectedAgentId = id;
            OnPropertyChanged();
        }
    }

    /// <summary>Creates the settings document view model.</summary>
    /// <param name="settingsService">Application settings service.</param>
    /// <param name="agentRegistry">Optional AI agent registry.</param>
    /// <param name="mcpCredentialService">Optional MCP credential service.</param>
    /// <param name="clusterCatalog">Optional catalog used to populate cluster scope choices.</param>
    /// <param name="mcpServerState">Optional runtime state of the embedded MCP listener.</param>
    public SettingsViewModel(
        ISettingsService settingsService,
        IAgentRegistry? agentRegistry = null,
        IMcpCredentialService? mcpCredentialService = null,
        IClusterRuntimeCatalog? clusterCatalog = null,
        IMcpServerState? mcpServerState = null)
    {
        Title = Assets.Resources.SettingsView_Title;
        Id = nameof(SettingsViewModel);

        SettingsService = settingsService;
        _agentRegistry = agentRegistry;
        _mcpServerState = mcpServerState;
        _mcpCredentialService = mcpCredentialService ?? new McpCredentialService(settingsService);

        if (clusterCatalog is not null)
        {
            foreach (var clusterName in clusterCatalog.Clusters
                .Select(cluster => cluster.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal))
            {
                ExternalMcpClusterOptions.Add(new McpExternalClusterOption(clusterName));
            }
        }

        SettingsService.Settings.PropertyChanged += Settings_PropertyChanged;
        if (_mcpServerState is not null)
            _mcpServerState.PropertyChanged += McpServerState_PropertyChanged;
    }

    [RelayCommand]
    private void CreateExternalMcpCredential()
    {
        _shouldValidateExternalMcpClientName = true;
        NewlyCreatedMcpToken = null;
        McpGrantMessage = null;
        if (!ValidateExternalMcpClientName())
            return;

        if (_mcpServerState?.BoundPort is null)
        {
            McpGrantMessage = Assets.Resources.SettingsView_McpServerUnavailable;
            return;
        }

        try
        {
            var clusters = ExternalMcpClusterOptions
                .Where(option => option.IsSelected)
                .Select(option => option.Name)
                .ToArray();
            var created = _mcpCredentialService.CreateExternalCredential(ExternalMcpClientName, clusters);
            NewlyCreatedMcpToken = created.Token;
            SelectedExternalMcpGrant = created.Grant;
            McpGrantMessage = Assets.Resources.SettingsView_McpTokenCreated;
            OnPropertyChanged(nameof(ExternalMcpGrants));
            OnPropertyChanged(nameof(HasNoExternalMcpGrants));
            ResetExternalMcpCredentialForm();
        }
        catch (ArgumentException)
        {
            NewlyCreatedMcpToken = null;
            McpGrantMessage = Assets.Resources.SettingsView_McpTokenInvalid;
        }
        catch (InvalidOperationException)
        {
            NewlyCreatedMcpToken = null;
            McpGrantMessage = Assets.Resources.SettingsView_McpTokenCreateFailed;
        }
    }

    [RelayCommand]
    private void RevokeExternalMcpCredential(McpExternalClientGrant? grant)
    {
        var selected = grant ?? SelectedExternalMcpGrant;
        if (selected is null)
            return;
        try
        {
            if (_mcpCredentialService.RevokeExternalCredential(selected.Id))
            {
                SelectedExternalMcpGrant = null;
                NewlyCreatedMcpToken = null;
                McpGrantMessage = Assets.Resources.SettingsView_McpTokenRevoked;
                OnPropertyChanged(nameof(ExternalMcpGrants));
                OnPropertyChanged(nameof(HasNoExternalMcpGrants));
                ValidateExternalMcpClientName();
            }
        }
        catch (InvalidOperationException)
        {
            McpGrantMessage = Assets.Resources.SettingsView_McpTokenRevokeFailed;
        }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();
        if (e.PropertyName == nameof(Options.Settings.SelectedAgentId))
            OnPropertyChanged(nameof(SelectedAgent));
        else if (e.PropertyName == nameof(Options.Settings.McpServerPort))
            OnPropertyChanged(nameof(McpServerPort));
        else if (e.PropertyName == nameof(Options.Settings.McpServerEnabled))
            NotifyMcpServerAvailabilityChanged();
    }

    private void McpServerState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IMcpServerState.BoundPort))
            NotifyMcpServerAvailabilityChanged();
    }

    private void NotifyMcpServerAvailabilityChanged()
    {
        OnPropertyChanged(nameof(IsMcpServerUnavailable));
        OnPropertyChanged(nameof(CanCreateExternalMcpCredential));
    }

    partial void OnExternalMcpClientNameChanged(string value)
    {
        if (_shouldValidateExternalMcpClientName)
            ValidateExternalMcpClientName();
    }

    partial void OnMcpGrantMessageChanged(string? value)
        => OnPropertyChanged(nameof(HasMcpGrantMessage));

    partial void OnNewlyCreatedMcpTokenChanged(string? value)
        => OnPropertyChanged(nameof(HasNewlyCreatedMcpToken));

    private void ResetExternalMcpCredentialForm()
    {
        _shouldValidateExternalMcpClientName = false;
        ExternalMcpClientName = string.Empty;
        foreach (var option in ExternalMcpClusterOptions)
            option.IsSelected = false;

        _externalMcpClientNameError = null;
        OnPropertyChanged(nameof(HasErrors));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(ExternalMcpClientName)));
    }

    private bool ValidateExternalMcpClientName()
    {
        var normalizedName = ExternalMcpClientName.Trim();
        _externalMcpClientNameError = string.IsNullOrWhiteSpace(ExternalMcpClientName)
            ? Assets.Resources.SettingsView_McpClientName_Required
            : normalizedName.Length > MaximumExternalMcpClientNameLength
                ? Assets.Resources.SettingsView_McpClientName_TooLong
                : SettingsService.Settings.McpExternalClientGrants.Any(grant =>
                    string.Equals(grant.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase))
                    ? Assets.Resources.SettingsView_McpClientName_Duplicate
                    : null;
        OnPropertyChanged(nameof(HasErrors));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(ExternalMcpClientName)));
        return _externalMcpClientNameError is null;
    }

    public void Dispose()
    {
        SettingsService.Settings.PropertyChanged -= Settings_PropertyChanged;
        if (_mcpServerState is not null)
            _mcpServerState.PropertyChanged -= McpServerState_PropertyChanged;
    }
}

/// <summary>A cluster option shown in external MCP token access settings.</summary>
public sealed partial class McpExternalClusterOption : ObservableObject
{
    /// <summary>Creates an unselected cluster access option.</summary>
    /// <param name="name">KubeUI cluster name.</param>
    public McpExternalClusterOption(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>Gets the KubeUI cluster name.</summary>
    public string Name { get; }

    /// <summary>Gets or sets whether the external client may access this cluster.</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
