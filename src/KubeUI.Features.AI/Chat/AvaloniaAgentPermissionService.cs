using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Permissions;

namespace KubeUI.Features.AI.Chat;

public sealed class AvaloniaAgentPermissionService(IDialogService dialogService) : IAgentPermissionService
{
    public async Task<AgentPermissionResult> RequestPermissionAsync(
        AgentPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var owner = new PermissionDialogOwner();
        var resource = request.Resource ?? AiLanguageManager.Instance.AvaloniaAgentPermissionService_UnspecifiedResource.CurrentValue()!;
        var operationImpact = request.IsDestructive
            ? AiLanguageManager.Instance.AvaloniaAgentPermissionService_Destructive.CurrentValue()!
            : AiLanguageManager.Instance.AvaloniaAgentPermissionService_NonDestructive.CurrentValue()!;
        var content = string.IsNullOrWhiteSpace(request.RequestingClient)
            ? string.Format(
                AiLanguageManager.Instance.AvaloniaAgentPermissionService_ContentFormat.CurrentValue()!,
                request.Action,
                resource,
                operationImpact)
            : string.Format(
                AiLanguageManager.Instance.AvaloniaAgentPermissionService_ContentWithRequesterFormat.CurrentValue()!,
                request.RequestingClient,
                request.Action,
                resource,
                operationImpact);
        var settings = new ContentDialogSettings
        {
            Title = AiLanguageManager.Instance.AvaloniaAgentPermissionService_Title.CurrentValue()!,
            Content = content,
            PrimaryButtonText = AiLanguageManager.Instance.AvaloniaAgentPermissionService_Allow.CurrentValue()!,
            SecondaryButtonText = AiLanguageManager.Instance.AvaloniaAgentPermissionService_Deny.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Secondary
        };
        var result = await dialogService.ShowContentDialogAsync(owner, settings).ConfigureAwait(true);
        var allowed = result == FAContentDialogResult.Primary;
        return new AgentPermissionResult(allowed, allowed ? null : AiLanguageManager.Instance.AvaloniaAgentPermissionService_Denied.CurrentValue()!);
    }

    private sealed class PermissionDialogOwner : ViewModelBase
    {
        public PermissionDialogOwner()
        {
            Id = nameof(PermissionDialogOwner);
            Title = AiLanguageManager.Instance.AvaloniaAgentPermissionService_OwnerTitle.CurrentValue()!;
        }
    }
}
