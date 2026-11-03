using System.Collections;
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
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads;

/// <summary>Shares scale-subresource UI and permission handling across scalable built-in workloads.</summary>
/// <typeparam name="T">The scalable Kubernetes workload kind.</typeparam>
public abstract partial class ScalableWorkloadConfig<T> : ResourceConfig<T>
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
            Title = Assets.Resources.ScalableWorkloadConfig_Scale,
            FluentIcon = Icon.ArrowSync,
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
                Text = string.Format(CultureInfo.CurrentCulture, Assets.Resources.ScalableWorkloadConfig_BulkPrompt!, selected.Count),
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
            });
        }
        content.Children.Add(new TextBlock { Text = Assets.Resources.ScalableWorkloadConfig_ReplicaCount });
        content.Children.Add(replicasInput);

        var settings = new ContentDialogSettings(content)
        {
            Title = Assets.Resources.ScalableWorkloadConfig_ScaleTitle,
            PrimaryButtonText = Assets.Resources.ScalableWorkloadConfig_Apply,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
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
                Utilities.HandleException(_logger, _notificationManager, ex, string.Format(CultureInfo.CurrentCulture, Assets.Resources.ScalableWorkloadConfig_ScaleError!, workload.Namespace(), workload.Name()), sendNotification: true);
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
