using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Deployment;

/// <summary>Describes one ReplicaSet-backed Deployment revision for display and rollback.</summary>
/// <param name="Uid">ReplicaSet UID used to verify the selected history entry.</param>
/// <param name="Revision">Numeric Deployment revision, when annotated.</param>
/// <param name="ReplicaSetName">Name of the owning ReplicaSet.</param>
/// <param name="DesiredReplicas">Desired replica count recorded by the ReplicaSet.</param>
/// <param name="ReadyReplicas">Ready replica count recorded by the ReplicaSet.</param>
/// <param name="CreatedAt">ReplicaSet creation time.</param>
/// <param name="ChangeCause">Recorded change-cause annotation, when present.</param>
/// <param name="IsCurrent">Whether this entry matches the Deployment's current revision.</param>
public sealed record DeploymentRevision(
    string Uid,
    int? Revision,
    string ReplicaSetName,
    int DesiredReplicas,
    int ReadyReplicas,
    DateTime? CreatedAt,
    string ChangeCause,
    bool IsCurrent)
{
    /// <summary>Gets the localized, compact row text.</summary>
    public string Display => string.Format(
        CultureInfo.CurrentCulture,
        Assets.Resources.DeploymentRolloutHistoryViewModel_RevisionFormat,
        Revision?.ToString(CultureInfo.CurrentCulture) ?? Assets.Resources.DeploymentRolloutHistoryViewModel_UnknownRevision,
        ReplicaSetName,
        ReadyReplicas,
        DesiredReplicas,
        CreatedAt?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? Assets.Resources.DeploymentRolloutHistoryViewModel_UnknownValue,
        ChangeCause,
        IsCurrent ? Assets.Resources.DeploymentRolloutHistoryViewModel_Current : string.Empty);
}

/// <summary>Loads Deployment revision history and restores a selected revision's Pod template.</summary>
public sealed partial class DeploymentRolloutHistoryViewModel : ViewModelBase, IClusterWorkspaceContext, IDisposable
{
    private const string RevisionAnnotation = "deployment.kubernetes.io/revision";
    private const string ChangeCauseAnnotation = "kubernetes.io/change-cause";
    private readonly IDialogService _dialogService;
    private readonly IKubernetesJsonSerializer _jsonSerializer;
    private readonly ILogger<DeploymentRolloutHistoryViewModel> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private int _disposed;

    public DeploymentRolloutHistoryViewModel(
        IDialogService dialogService,
        IKubernetesJsonSerializer jsonSerializer,
        ILogger<DeploymentRolloutHistoryViewModel> logger,
        INotificationManager notificationManager)
    {
        _dialogService = dialogService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _notificationManager = notificationManager;
        Title = Assets.Resources.DeploymentRolloutHistoryViewModel_Title;
    }

    /// <summary>Gets the workspace that owns the Deployment.</summary>
    public ClusterWorkspace? Cluster { get; private set; }

    /// <summary>Gets the Deployment whose rollout history is displayed.</summary>
    public V1Deployment Deployment { get; private set; } = null!;

    /// <summary>Gets revisions newest first.</summary>
    public ObservableCollection<DeploymentRevision> Revisions { get; } = [];

    /// <summary>Gets or sets the selected revision.</summary>
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    [ObservableProperty]
    public partial DeploymentRevision? SelectedRevision { get; set; }

    /// <summary>Gets or sets the current loading or action message.</summary>
    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    /// <summary>Sets the workspace and Deployment displayed by this document.</summary>
    /// <param name="cluster">Owning cluster workspace.</param>
    /// <param name="deployment">Deployment to inspect.</param>
    public void Initialize(ClusterWorkspace cluster, V1Deployment deployment)
    {
        Cluster = cluster;
        Deployment = deployment;
        Id = $"{nameof(DeploymentRolloutHistoryViewModel)}-{cluster.Runtime.Name}-{deployment.Namespace()}-{deployment.Name()}";
        Title = string.Format(CultureInfo.CurrentCulture, Assets.Resources.DeploymentRolloutHistoryViewModel_TitleFormat, deployment.Name());
    }

    /// <summary>Loads ReplicaSets owned by the Deployment and orders them by revision.</summary>
    public async Task LoadAsync()
    {
        if (Cluster is null)
        {
            return;
        }

        Revisions.Clear();
        StatusMessage = null;
        try
        {
            var deployment = await Cluster.Runtime.Client!.AppsV1.ReadNamespacedDeploymentAsync(
                Deployment.Name(), Deployment.Namespace(), cancellationToken: _lifetimeCts.Token);
            var replicaSets = await Cluster.Runtime.Client.AppsV1.ListNamespacedReplicaSetAsync(
                Deployment.Namespace(), cancellationToken: _lifetimeCts.Token);
            var currentRevision = GetRevision(deployment.Metadata?.Annotations);

            foreach (var replicaSet in replicaSets.Items
                .Where(replicaSet => IsOwnedBy(replicaSet, deployment))
                .Select(replicaSet => ToRevision(replicaSet, currentRevision))
                .OrderByDescending(revision => revision.Revision ?? int.MinValue)
                .ThenByDescending(revision => revision.CreatedAt))
            {
                Revisions.Add(replicaSet);
            }

            StatusMessage = Revisions.Count == 0 ? Assets.Resources.DeploymentRolloutHistoryViewModel_NoHistory : null;
            SelectedRevision = Revisions.FirstOrDefault();
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException && _lifetimeCts.IsCancellationRequested)
            {
                return;
            }
            StatusMessage = Assets.Resources.DeploymentRolloutHistoryViewModel_LoadError;
            Utilities.HandleException(_logger, _notificationManager, ex, Assets.Resources.DeploymentRolloutHistoryViewModel_LoadError, sendNotification: true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private async Task Undo()
    {
        if (Cluster is null)
        {
            return;
        }

        var target = SelectedRevision;
        if (target is null)
        {
            return;
        }

        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = Assets.Resources.DeploymentRolloutHistoryViewModel_UndoTitle,
            Content = string.Format(Assets.Resources.DeploymentRolloutHistoryViewModel_UndoContent, Deployment.Name(), target.Revision, target.ReplicaSetName),
            PrimaryButtonText = Assets.Resources.DeploymentRolloutHistoryViewModel_Undo,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary,
        });
        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            var deployment = await Cluster.Runtime.Client!.AppsV1.ReadNamespacedDeploymentAsync(
                Deployment.Name(), Deployment.Namespace(), cancellationToken: _lifetimeCts.Token);
            var replicaSets = await Cluster.Runtime.Client.AppsV1.ListNamespacedReplicaSetAsync(
                Deployment.Namespace(), cancellationToken: _lifetimeCts.Token);
            var source = replicaSets.Items.FirstOrDefault(replicaSet =>
                string.Equals(replicaSet.Metadata?.Uid, target.Uid, StringComparison.Ordinal)
                && IsOwnedBy(replicaSet, deployment)
                && GetRevision(replicaSet.Metadata?.Annotations) == target.Revision);
            if (source?.Spec?.Template is null || string.IsNullOrWhiteSpace(deployment.Metadata?.ResourceVersion))
            {
                StatusMessage = Assets.Resources.DeploymentRolloutHistoryViewModel_StaleRevision;
                return;
            }

            var templateJson = _jsonSerializer.Serialize(source.Spec.Template);
            var revisionJson = JsonSerializer.Serialize(deployment.Metadata.ResourceVersion);
            var patchJson = "[{\"op\":\"test\",\"path\":\"/metadata/resourceVersion\",\"value\":" + revisionJson
                + "},{\"op\":\"replace\",\"path\":\"/spec/template\",\"value\":" + templateJson + "}]";
            await Cluster.Runtime.Client.AppsV1.PatchNamespacedDeploymentAsync(
                new V1Patch(patchJson, V1Patch.PatchType.JsonPatch), Deployment.Name(), Deployment.Namespace(), cancellationToken: _lifetimeCts.Token);
            await LoadAsync();
            StatusMessage = Assets.Resources.DeploymentRolloutHistoryViewModel_UndoComplete;
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException && _lifetimeCts.IsCancellationRequested)
            {
                return;
            }
            StatusMessage = Assets.Resources.DeploymentRolloutHistoryViewModel_UndoError;
            Utilities.HandleException(_logger, _notificationManager, ex, Assets.Resources.DeploymentRolloutHistoryViewModel_UndoError, sendNotification: true);
        }
    }

    private bool CanUndo()
        => SelectedRevision is { IsCurrent: false }
            && Cluster is not null
            && Cluster.Runtime.Permissions.CanI<V1Deployment>(Verb.Patch, Deployment.Namespace())
            && Cluster.Runtime.Permissions.CanI<V1Deployment>(Verb.Get, Deployment.Namespace())
            && Cluster.Runtime.Permissions.CanI<V1ReplicaSet>(Verb.List, Deployment.Namespace());

    private static bool IsOwnedBy(V1ReplicaSet replicaSet, V1Deployment deployment)
        => !string.IsNullOrWhiteSpace(deployment.Uid())
            && replicaSet.Metadata?.OwnerReferences?.Any(owner =>
                owner.Controller == true
                && owner.Kind == V1Deployment.KubeKind
                && owner.ApiVersion == V1Deployment.KubeGroup + "/" + V1Deployment.KubeApiVersion
                && string.Equals(owner.Uid, deployment.Uid(), StringComparison.Ordinal)) == true;

    private static DeploymentRevision ToRevision(V1ReplicaSet replicaSet, int? currentRevision)
    {
        var revision = GetRevision(replicaSet.Metadata?.Annotations);
        var annotations = replicaSet.Metadata?.Annotations;
        var changeCause = annotations is not null && annotations.TryGetValue(ChangeCauseAnnotation, out var value)
            ? value
            : Assets.Resources.DeploymentRolloutHistoryViewModel_UnknownValue;
        return new DeploymentRevision(
            replicaSet.Uid() ?? string.Empty,
            revision,
            replicaSet.Name() ?? string.Empty,
            replicaSet.Spec?.Replicas ?? 0,
            replicaSet.Status?.ReadyReplicas ?? 0,
            replicaSet.Metadata?.CreationTimestamp,
            changeCause,
            revision.HasValue && revision == currentRevision);
    }

    private static int? GetRevision(IDictionary<string, string>? annotations)
        => annotations is not null && annotations.TryGetValue(RevisionAnnotation, out var value)
            && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var revision)
                ? revision
                : null;

    /// <summary>Cancels outstanding requests and releases the document lifetime token.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
    }
}
