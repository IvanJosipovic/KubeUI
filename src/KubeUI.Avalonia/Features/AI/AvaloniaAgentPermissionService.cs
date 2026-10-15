using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.AI;

public sealed class AvaloniaAgentPermissionService(IDialogService dialogService) : IAgentPermissionService
{
    public async Task<AgentPermissionResult> RequestPermissionAsync(
        AgentPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var owner = new PermissionDialogOwner();
        var resource = request.Resource ?? Assets.Resources.AgentPermissionDialog_UnspecifiedResource;
        var operationImpact = request.IsDestructive
            ? Assets.Resources.AgentPermissionDialog_Destructive
            : Assets.Resources.AgentPermissionDialog_NonDestructive;
        var content = string.IsNullOrWhiteSpace(request.RequestingClient)
            ? string.Format(
                Assets.Resources.AgentPermissionDialog_ContentFormat!,
                request.Action,
                resource,
                operationImpact)
            : string.Format(
                Assets.Resources.AgentPermissionDialog_ContentWithRequesterFormat!,
                request.RequestingClient,
                request.Action,
                resource,
                operationImpact);
        var settings = new ContentDialogSettings
        {
            Title = Assets.Resources.AgentPermissionDialog_Title,
            Content = content,
            PrimaryButtonText = Assets.Resources.AgentPermissionDialog_Allow,
            SecondaryButtonText = Assets.Resources.AgentPermissionDialog_Deny,
            DefaultButton = FAContentDialogButton.Secondary
        };
        var result = await dialogService.ShowContentDialogAsync(owner, settings).ConfigureAwait(true);
        var allowed = result == FAContentDialogResult.Primary;
        return new AgentPermissionResult(allowed, allowed ? null : Assets.Resources.AgentPermissionDialog_Denied);
    }

    private sealed class PermissionDialogOwner : ViewModelBase
    {
        public PermissionDialogOwner()
        {
            Id = nameof(PermissionDialogOwner);
            Title = Assets.Resources.AgentPermissionDialog_OwnerTitle;
        }
    }
}
