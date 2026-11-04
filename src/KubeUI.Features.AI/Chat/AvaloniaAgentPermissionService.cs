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
        var resource = request.Resource ?? L10n.Instance.AvaloniaAgentPermissionService_UnspecifiedResource.CurrentValue()!;
        var operationImpact = request.IsDestructive
            ? L10n.Instance.AvaloniaAgentPermissionService_Destructive.CurrentValue()!
            : L10n.Instance.AvaloniaAgentPermissionService_NonDestructive.CurrentValue()!;
        var content = string.IsNullOrWhiteSpace(request.RequestingClient)
            ? string.Format(
                L10n.Instance.AvaloniaAgentPermissionService_ContentFormat.CurrentValue()!,
                request.Action,
                resource,
                operationImpact)
            : string.Format(
                L10n.Instance.AvaloniaAgentPermissionService_ContentWithRequesterFormat.CurrentValue()!,
                request.RequestingClient,
                request.Action,
                resource,
                operationImpact);
        var settings = new ContentDialogSettings
        {
            Title = L10n.Instance.AvaloniaAgentPermissionService_Title.CurrentValue()!,
            Content = content,
            PrimaryButtonText = L10n.Instance.AvaloniaAgentPermissionService_Allow.CurrentValue()!,
            SecondaryButtonText = L10n.Instance.AvaloniaAgentPermissionService_Deny.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Secondary
        };
        var result = await dialogService.ShowContentDialogAsync(owner, settings).ConfigureAwait(true);
        var allowed = result == FAContentDialogResult.Primary;
        return new AgentPermissionResult(allowed, allowed ? null : L10n.Instance.AvaloniaAgentPermissionService_Denied.CurrentValue()!);
    }

    private sealed class PermissionDialogOwner : ViewModelBase
    {
        public PermissionDialogOwner()
        {
            Id = nameof(PermissionDialogOwner);
            Title = L10n.Instance.AvaloniaAgentPermissionService_OwnerTitle.CurrentValue()!;
        }
    }
}
