using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads;

/// <summary>Shares scale-subresource UI and permission handling across scalable built-in workloads.</summary>
/// <typeparam name="T">The scalable Kubernetes workload kind.</typeparam>
public abstract partial class ScalableWorkloadConfig<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : ResourceConfig<T>
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected ScalableWorkloadConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected abstract int GetDesiredReplicas(T workload);

    protected abstract Task PatchReplicasAsync(T workload, int replicas);

    protected static V1Patch CreateScalePatch(int replicas)
        => new("{\"spec\":{\"replicas\":" + replicas.ToString(CultureInfo.InvariantCulture) + "}}", V1Patch.PatchType.MergePatch);

    protected MenuItemViewModel CreateScaleMenuItem(IEnumerable<T>? selectedItems)
    {
        var selected = selectedItems?.ToList();
        return new MenuItemViewModel
        {
            Title = ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!,
            FluentIcon = Icon.Resize,
            Command = ScaleCommand,
            CommandParameter = selected,
        };
    }

    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
        => base.AuthorizationRequests().Append(new AuthorizationRequest(Kind, Verb.Patch, "scale"));

    [RelayCommand(CanExecute = nameof(CanScale))]
    private async Task Scale(IList workloads)
    {
        var selected = GetEligibleWorkloads(workloads);
        if (selected.Count == 0)
        {
            return;
        }

        var initialReplicas = selected.Count == 1 ? GetDesiredReplicas(selected[0]) : 0;
        var replicasInput = new NumericUpDown
        {
            Minimum = 0,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "0",
            Value = initialReplicas,
            Width = 180,
        };

        var content = new StackPanel { Spacing = 8 };
        if (selected.Count > 1)
        {
            content.Children.Add(new TextBlock
            {
                Text = string.Format(CultureInfo.CurrentCulture, ResourcesLanguageManager.Instance.ScalableWorkloadConfig_BulkPrompt.CurrentValue()!, selected.Count),
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
            });
        }
        content.Children.Add(new TextBlock { Text = ResourcesLanguageManager.Instance.ScalableWorkloadConfig_ReplicaCount.CurrentValue()! });
        content.Children.Add(replicasInput);

        var settings = new ContentDialogSettings(content)
        {
            Title = ResourcesLanguageManager.Instance.ScalableWorkloadConfig_ScaleTitle.CurrentValue()!,
            PrimaryButtonText = ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Apply.CurrentValue()!,
            SecondaryButtonText = FoundationLanguageManager.Instance.Shared_Cancel.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Secondary,
        };
        var result = await _dialogService.ShowContentDialogAsync(this, settings);
        if (result != FAContentDialogResult.Primary || replicasInput.Value is not decimal value || value < 0 || value > int.MaxValue || decimal.Truncate(value) != value)
        {
            return;
        }

        selected = GetEligibleWorkloads(selected);
        var exceptions = new List<Exception>();
        foreach (var workload in selected)
        {
            try
            {
                await PatchReplicasAsync(workload, decimal.ToInt32(value));
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                ResourceUtilities.HandleException(_logger, _notificationManager, ex, string.Format(CultureInfo.CurrentCulture, ResourcesLanguageManager.Instance.ScalableWorkloadConfig_ScaleError.CurrentValue()!, workload.Namespace(), workload.Name()), sendNotification: true);
            }
        }

        if (exceptions.Count > 0)
        {
            _logger.LogError(new AggregateException(exceptions), "Error scaling {WorkloadKind} resources", Kind.Kind);
        }
    }

    private bool CanScale(IList? workloads)
    {
        return GetEligibleWorkloads(workloads).Count > 0;
    }

    private List<T> GetEligibleWorkloads(IList? workloads)
    {
        if (workloads is not { Count: > 0 } || Cluster is null)
        {
            return [];
        }

        var eligible = new List<T>(workloads.Count);
        foreach (var item in workloads)
        {
            if (item is not T workload)
            {
                continue;
            }

            var name = workload.Name();
            var @namespace = workload.Namespace();
            if (!string.IsNullOrWhiteSpace(name)
                && !string.IsNullOrWhiteSpace(@namespace)
                && Cluster.Runtime.Permissions.CanI(Kind, Verb.Patch, @namespace, "scale"))
            {
                eligible.Add(workload);
            }
        }

        return eligible;
    }
}
