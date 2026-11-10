using System.Collections;
using System.Net;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.App.Infrastructure.Docking;
using KubeUI.App.Shell.Navigation;
using KubeUI.Features.Resources;
using KubeUI.Features.Resources.Yaml;
using Shouldly;
using ClusterRoleBindingPropertiesView = KubeUI.Features.Resources.AccessControl.v1.ClusterRoleBinding.PropertiesView;
using ClusterRolePropertiesView = KubeUI.Features.Resources.AccessControl.v1.ClusterRole.PropertiesView;
using ConfigMapPropertiesView = KubeUI.Features.Resources.Configuration.v1.ConfigMap.PropertiesView;
using CrdPropertiesView = KubeUI.Features.Resources.CustomResourceDefinition.PropertiesView;
using CronJobPropertiesView = KubeUI.Features.Resources.Workloads.v1.CronJob.PropertiesView;
using DaemonSetPropertiesView = KubeUI.Features.Resources.Workloads.v1.DaemonSet.PropertiesView;
using DeploymentPropertiesView = KubeUI.Features.Resources.Workloads.v1.Deployment.PropertiesView;
using EndpointSlicePropertiesView = KubeUI.Features.Resources.Network.v1.EndpointSlice.PropertiesView;
using EventPropertiesView = KubeUI.Features.Resources.Core.v1.Event.PropertiesView;
using HpaPropertiesView = KubeUI.Features.Resources.Configuration.v2.HorizontalPodAutoscaler.PropertiesView;
using IngressClassPropertiesView = KubeUI.Features.Resources.Network.v1.IngressClass.PropertiesView;
using IngressPropertiesView = KubeUI.Features.Resources.Network.v1.Ingress.PropertiesView;
using JobPropertiesView = KubeUI.Features.Resources.Workloads.v1.Job.PropertiesView;
using LeasePropertiesView = KubeUI.Features.Resources.Configuration.v1.Lease.PropertiesView;
using LimitRangePropertiesView = KubeUI.Features.Resources.Configuration.v1.LimitRange.PropertiesView;
using MutatingWebhookConfigurationPropertiesView = KubeUI.Features.Resources.Configuration.v1.MutatingWebhookConfiguration.PropertiesView;
using NamespacePropertiesView = KubeUI.Features.Resources.Core.v1.Namespace.PropertiesView;
using NetworkPolicyPropertiesView = KubeUI.Features.Resources.Network.v1.NetworkPolicy.PropertiesView;
using NodePropertiesView = KubeUI.Features.Resources.Core.v1.Node.PropertiesView;
using PersistentVolumeClaimPropertiesView = KubeUI.Features.Resources.Storage.v1.PersistentVolumeClaim.PropertiesView;
using PersistentVolumePropertiesView = KubeUI.Features.Resources.Storage.v1.PersistentVolume.PropertiesView;
using PodDisruptionBudgetPropertiesView = KubeUI.Features.Resources.Configuration.v1.PodDisruptionBudget.PropertiesView;
using PodPropertiesView = KubeUI.Features.Resources.Workloads.v1.Pod.PropertiesView;
using PriorityClassPropertiesView = KubeUI.Features.Resources.Configuration.v1.PriorityClass.PropertiesView;
using ReplicaSetPropertiesView = KubeUI.Features.Resources.Workloads.v1.ReplicaSet.PropertiesView;
using ResourceQuotaPropertiesView = KubeUI.Features.Resources.Configuration.v1.ResourceQuota.PropertiesView;
using RoleBindingPropertiesView = KubeUI.Features.Resources.AccessControl.v1.RoleBinding.PropertiesView;
using RolePropertiesView = KubeUI.Features.Resources.AccessControl.v1.Role.PropertiesView;
using RuntimeClassPropertiesView = KubeUI.Features.Resources.Configuration.v1.RuntimeClass.PropertiesView;
using SecretPropertiesView = KubeUI.Features.Resources.Configuration.v1.Secret.PropertiesView;
using ServiceAccountPropertiesView = KubeUI.Features.Resources.AccessControl.v1.ServiceAccount.PropertiesView;
using ServicePropertiesView = KubeUI.Features.Resources.Network.v1.Service.PropertiesView;
using StatefulSetPropertiesView = KubeUI.Features.Resources.Workloads.v1.StatefulSet.PropertiesView;
using StorageClassPropertiesView = KubeUI.Features.Resources.Storage.v1.StorageClass.PropertiesView;
using V2HorizontalPodAutoscalerConfig = KubeUI.Features.Resources.Configuration.v2.HorizontalPodAutoscaler.V2HorizontalPodAutoscalerConfig;
using ValidatingWebhookConfigurationPropertiesView = KubeUI.Features.Resources.Configuration.v1.ValidatingWebhookConfiguration.PropertiesView;

namespace KubeUI.Features.Resources.Tests.ResourceConfigs;

public sealed class ResourceFeatureConfigTests
{
    private static void AssertProperties<TConfig, TResource, TView>(TResource resource)
        where TConfig : ResourceConfigBase<TResource>
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
        where TView : Control
    {
        var config = ResolveConfig<TConfig>();

        var controls = config.Properties(resource);

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<TView>();
    }

    private static void AssertSection<TConfig>(string expectedSectionId, string expectedName)
        where TConfig : IResourceConfig
    {
        IResourceConfig config = ResolveConfig<TConfig>();

        config.Navigation.SectionId.ShouldBe(expectedSectionId);

        var catalog = Application.Current.GetTestServices().GetRequiredService<NavigationSectionCatalog>();
        catalog.Find(expectedSectionId).ShouldNotBeNull().Name.ShouldBe(expectedName);
    }

    private static TConfig ResolveConfig<TConfig>()
        where TConfig : IResourceConfig
    {
        var services = Application.Current.GetTestServices();
        return services.GetRequiredService<TConfig>();
    }

    [AvaloniaFact]
    public void secret_config_uses_secret_local_properties_view()
    {
        var config = ResolveConfig<V1SecretConfig>();

        var controls = config.Properties(new V1Secret());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<SecretPropertiesView>();
    }

    [AvaloniaFact]
    public void event_config_uses_custom_last_seen_cell()
    {
        var config = ResolveConfig<V1EventConfig>();

        var lastSeenColumn = config.Columns().Single(x => x.Name == ResourcesLanguageManager.Instance.V1EventConfig_Last_Seen.CurrentValue()!);

        lastSeenColumn.CustomControl.ShouldNotBeNull();
        lastSeenColumn.CustomControl!.Name.ShouldBe("EventLastSeenCell");
    }

    [AvaloniaFact]
    public void event_config_uses_event_properties_view()
    {
        AssertProperties<V1EventConfig, Corev1Event, EventPropertiesView>(new Corev1Event());
    }

    [AvaloniaFact]
    public void deployment_config_uses_deployment_properties_view()
    {
        var config = ResolveConfig<V1DeploymentConfig>();

        var controls = config.Properties(new V1Deployment());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<DeploymentPropertiesView>();
    }

    [AvaloniaFact]
    public void scalable_workload_configs_expose_scale_actions()
    {
        var deployment = ResolveConfig<V1DeploymentConfig>();
        var statefulSet = ResolveConfig<V1StatefulSetConfig>();
        var replicaSet = ResolveConfig<V1ReplicaSetConfig>();

        deployment.GetCustomMenuItems(new[] { new V1Deployment() }).ShouldContain(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
        statefulSet.GetCustomMenuItems(new[] { new V1StatefulSet() }).ShouldContain(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
        replicaSet.GetCustomMenuItems(new[] { new V1ReplicaSet() }).ShouldContain(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
    }

    [AvaloniaFact]
    public void replicaset_context_menu_does_not_expose_restart()
    {
        var config = ResolveConfig<V1ReplicaSetConfig>();
        var actions = config.GetCustomMenuItems(new[] { new V1ReplicaSet() }).ToArray();

        actions.ShouldContain(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
        actions.ShouldNotContain(item => item.Title == ResourcesLanguageManager.Instance.V1DeploymentConfig_MenuItem_Restart.CurrentValue()!);
    }

    [AvaloniaFact]
    public void scale_and_restart_actions_use_distinct_icons()
    {
        var config = ResolveConfig<V1DeploymentConfig>();
        var actions = config.GetCustomMenuItems(new[] { new V1Deployment() });
        var scale = actions.Single(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
        var restart = actions.Single(item => item.Title == ResourcesLanguageManager.Instance.V1DeploymentConfig_MenuItem_Restart.CurrentValue()!);

        scale.FluentIcon.ShouldNotBe(restart.FluentIcon);
    }

    [AvaloniaFact]
    public async Task statefulset_on_delete_restart_action_is_disabled()
    {
        var statefulSet = new V1StatefulSet
        {
            Metadata = new V1ObjectMeta { Name = "on-delete-statefulset", NamespaceProperty = "default" },
            Spec = new V1StatefulSetSpec
            {
                UpdateStrategy = new V1StatefulSetUpdateStrategy { Type = "OnDelete" },
            },
        };

        await AssertOnDeleteRestartDisabledAsync<V1StatefulSetConfig, V1StatefulSet>(
            statefulSet,
            ResourcesLanguageManager.Instance.V1StatefulSetConfig_MenuItem_Restart.CurrentValue()!);
    }

    [AvaloniaFact]
    public async Task daemonset_on_delete_restart_action_is_disabled()
    {
        var daemonSet = new V1DaemonSet
        {
            Metadata = new V1ObjectMeta { Name = "on-delete-daemonset", NamespaceProperty = "default" },
            Spec = new V1DaemonSetSpec
            {
                UpdateStrategy = new V1DaemonSetUpdateStrategy { Type = "OnDelete" },
            },
        };

        await AssertOnDeleteRestartDisabledAsync<V1DaemonSetConfig, V1DaemonSet>(
            daemonSet,
            ResourcesLanguageManager.Instance.V1DaemonSetConfig_MenuItem_Restart.CurrentValue()!);
    }

    [AvaloniaFact]
    public async Task statefulset_restart_skips_on_delete_items_in_mixed_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var manual = CreateStatefulSet("restart-manual", "OnDelete");
        var rolling = CreateStatefulSet("restart-rolling", "RollingUpdate");
        await workspace.Runtime.Client!.AppsV1.CreateNamespacedStatefulSetAsync(
            manual, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Client.AppsV1.CreateNamespacedStatefulSetAsync(
            rolling, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1StatefulSet>(Verb.Patch, "default");
        var config = (V1StatefulSetConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1StatefulSet>());
        var action = config.GetCustomMenuItems(new[] { manual, rolling })
            .Single(item => item.Title == ResourcesLanguageManager.Instance.V1StatefulSetConfig_MenuItem_Restart.CurrentValue()!);
        var command = action.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        command.CanExecute(action.CommandParameter).ShouldBeTrue();

        await command.ExecuteAsync(action.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);

        var updatedManual = await workspace.Runtime.Client.AppsV1.ReadNamespacedStatefulSetAsync(
            manual.Name(), manual.Namespace(), cancellationToken: TestContext.Current.CancellationToken);
        var updatedRolling = await workspace.Runtime.Client.AppsV1.ReadNamespacedStatefulSetAsync(
            rolling.Name(), rolling.Namespace(), cancellationToken: TestContext.Current.CancellationToken);
        updatedManual.Spec.Template.Metadata.Annotations.ShouldBeNull();
        updatedRolling.Spec.Template.Metadata.Annotations.ShouldContainKey("kubectl.kubernetes.io/restartedAt");
        ((TestApp)Application.Current!).Notification.ShouldBeNull();
    }

    private static V1StatefulSet CreateStatefulSet(string name, string updateStrategyType)
    {
        var labels = new Dictionary<string, string> { ["app"] = name };
        return new V1StatefulSet
        {
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1StatefulSetSpec
            {
                Selector = new V1LabelSelector { MatchLabels = labels },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = labels },
                    Spec = new V1PodSpec
                    {
                        Containers = [new V1Container { Name = "app", Image = "example:latest" }],
                    },
                },
                UpdateStrategy = new V1StatefulSetUpdateStrategy { Type = updateStrategyType },
            },
        };
    }

    private static async Task AssertOnDeleteRestartDisabledAsync<TConfig, TResource>(TResource resource, string? restartTitle)
        where TConfig : ResourceConfigBase<TResource>
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdateCanI<TResource>(Verb.Patch, "default");
        var config = (TConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<TResource>());
        var action = config.GetCustomMenuItems(new[] { resource }).Single(item => item.Title == restartTitle);
        var command = action.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var parameters = action.CommandParameter.ShouldBeAssignableTo<IList>();
        string? dialogContent = null;
        ((TestApp)Application.Current!).ConfigureContentDialog = settings => dialogContent = settings.Content?.ToString();

        try
        {
            command.CanExecute(parameters).ShouldBeFalse();
            await command.ExecuteAsync(parameters).WaitAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            ((TestApp)Application.Current!).ConfigureContentDialog = null;
        }

        dialogContent.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task resource_menu_edit_yaml_opens_resource_in_edit_mode()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Update, "default");
        var config = (V1PodConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        var pod = new V1Pod { Metadata = new V1ObjectMeta { Name = "edit-test", NamespaceProperty = "default" } };
        var action = config.GetDefaultMenuItems(new[] { pod })
            .Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_EditYaml.CurrentValue()!);
        var command = action.Command.ShouldBeAssignableTo<IRelayCommand>();
        command.CanExecute(action.CommandParameter).ShouldBeTrue();
        command.CanExecute(new List<V1Pod> { pod, pod }).ShouldBeFalse();

        command.Execute(action.CommandParameter);

        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var bottomDock = factory.GetDockable<IToolDock>("BottomDock").ShouldNotBeNull();
        var yamlViewModel = bottomDock.VisibleDockables!.OfType<ResourceYamlViewModel>()
            .Single(viewModel => viewModel.Object?.Metadata?.Name == pod.Metadata.Name);
        try
        {
            yamlViewModel.EditMode.ShouldBeTrue();
        }
        finally
        {
            factory.CloseDockable(yamlViewModel);
        }
    }

    [AvaloniaFact]
    public async Task resource_menu_edit_yaml_promotes_existing_yaml_document()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Update, "default");
        var config = (V1PodConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        var pod = new V1Pod { Metadata = new V1ObjectMeta { Name = "edit-existing", NamespaceProperty = "default" } };
        var defaultActions = config.GetDefaultMenuItems(new[] { pod }).ToArray();
        defaultActions.Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_ViewYaml.CurrentValue()!)
            .Command.ShouldBeAssignableTo<IRelayCommand>().Execute(defaultActions.Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_ViewYaml.CurrentValue()!).CommandParameter);

        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var bottomDock = factory.GetDockable<IToolDock>("BottomDock").ShouldNotBeNull();
        var yamlViewModel = bottomDock.VisibleDockables!.OfType<ResourceYamlViewModel>()
            .Single(viewModel => viewModel.Object?.Metadata?.Name == pod.Metadata.Name);
        try
        {
            defaultActions.Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_EditYaml.CurrentValue()!)
                .Command.ShouldBeAssignableTo<IRelayCommand>().Execute(
                    defaultActions.Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_EditYaml.CurrentValue()!).CommandParameter);

            bottomDock.VisibleDockables!.OfType<ResourceYamlViewModel>()
                .Count(viewModel => viewModel.Object?.Metadata?.Name == pod.Metadata.Name).ShouldBe(1);
            yamlViewModel.EditMode.ShouldBeTrue();
        }
        finally
        {
            factory.CloseDockable(yamlViewModel);
        }
    }

    [AvaloniaFact]
    public async Task resource_menu_edit_yaml_requires_update_permission()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var config = Application.Current.GetRequiredTestService<V1PodConfig>();
        config.Initialize(workspace);
        var pod = new V1Pod { Metadata = new V1ObjectMeta { Name = "edit-denied", NamespaceProperty = "default" } };
        var action = config.GetDefaultMenuItems(new[] { pod })
            .Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_EditYaml.CurrentValue()!);

        action.Command.ShouldBeAssignableTo<IRelayCommand>().CanExecute(action.CommandParameter).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void workload_action_menus_expose_cronjob_suspend_resume_and_pod_eviction()
    {
        var cronJob = ResolveConfig<V1CronJobConfig>();
        var pod = ResolveConfig<V1PodConfig>();

        var cronJobActions = cronJob.GetCustomMenuItems(new[] { new V1CronJob() }).Select(item => item.Title).ToArray();
        cronJobActions.ShouldContain(ResourcesLanguageManager.Instance.V1CronJobConfig_Suspend.CurrentValue()!);
        cronJobActions.ShouldContain(ResourcesLanguageManager.Instance.V1CronJobConfig_Resume.CurrentValue()!);

        var podActions = pod.GetCustomMenuItems(new[] { new V1Pod() }).Select(item => item.Title).ToArray();
        podActions.ShouldContain(ResourcesLanguageManager.Instance.V1PodConfig_Evict.CurrentValue()!);
    }

    [AvaloniaFact]
    public void scalable_and_evict_actions_request_subresource_permissions()
    {
        ResolveConfig<V1DeploymentConfig>().AuthorizationRequests()
            .ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1Deployment>(), Verb.Patch, "scale"));
        ResolveConfig<V1StatefulSetConfig>().AuthorizationRequests()
            .ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1StatefulSet>(), Verb.Patch, "scale"));
        ResolveConfig<V1ReplicaSetConfig>().AuthorizationRequests()
            .ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1ReplicaSet>(), Verb.Patch, "scale"));
        ResolveConfig<V1PodConfig>().AuthorizationRequests()
            .ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Create, "eviction"));
    }

    [AvaloniaFact]
    public async Task deployment_scale_action_patches_scale_subresource()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta { Name = "scale-test", NamespaceProperty = "default" },
            Spec = new V1DeploymentSpec { Replicas = 2 },
        };
        await workspace.Runtime.Client!.AppsV1.CreateNamespacedDeploymentAsync(deployment, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1Deployment>(Verb.Patch, "default", "scale");
        var config = (V1DeploymentConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Deployment>());
        var action = config.GetCustomMenuItems(new[]
        {
            deployment,
            new V1Deployment { Metadata = new V1ObjectMeta { Name = "missing-namespace" } },
        }).Single(item => item.Title == ResourcesLanguageManager.Instance.ScalableWorkloadConfig_Scale.CurrentValue()!);
        var command = action.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var parameters = action.CommandParameter.ShouldBeAssignableTo<IList>();
        command.CanExecute(parameters).ShouldBeTrue();
        var requestedReplicas = 2.5m;
        ((TestApp)Application.Current!).ConfigureContentDialog = settings =>
        {
            var content = settings.Content.ShouldBeOfType<StackPanel>();
            content.Children.OfType<NumericUpDown>().Single().Value = requestedReplicas;
        };

        try
        {
            await command.ExecuteAsync(parameters).WaitAsync(TestContext.Current.CancellationToken);
            (await workspace.Runtime.Client.AppsV1.ReadNamespacedDeploymentAsync(
                "scale-test", "default", cancellationToken: TestContext.Current.CancellationToken)).Spec.Replicas.ShouldBe(2);

            requestedReplicas = 5;
            await command.ExecuteAsync(parameters).WaitAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            ((TestApp)Application.Current!).ConfigureContentDialog = null;
        }

        var scaled = await workspace.Runtime.Client.AppsV1.ReadNamespacedDeploymentScaleAsync(
            deployment.Name(), deployment.Namespace(), cancellationToken: TestContext.Current.CancellationToken);
        scaled.Spec.Replicas.ShouldBe(5);
        ((TestApp)Application.Current!).Notification.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task deployment_pause_and_resume_patch_only_paused_state()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var deployment = await workspace.Runtime.Client!.AppsV1.CreateNamespacedDeploymentAsync(new V1Deployment
        {
            Metadata = new V1ObjectMeta { Name = "pause-test", NamespaceProperty = "default" },
            Spec = new V1DeploymentSpec { Replicas = 2 },
        }, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1Deployment>(Verb.Patch, "default");
        var config = (V1DeploymentConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Deployment>());
        var pause = config.GetCustomMenuItems(new[]
        {
            deployment,
            new V1Deployment { Metadata = new V1ObjectMeta { Name = "missing-namespace" } },
        }).Single(item => item.Title == ResourcesLanguageManager.Instance.V1DeploymentConfig_Pause.CurrentValue()!);
        var pauseCommand = pause.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        pauseCommand.CanExecute(pause.CommandParameter).ShouldBeTrue();

        await pauseCommand.ExecuteAsync(pause.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);
        ((TestApp)Application.Current!).Notification.ShouldBeNull();

        var paused = await workspace.Runtime.Client.AppsV1.ReadNamespacedDeploymentAsync("pause-test", "default", cancellationToken: TestContext.Current.CancellationToken);
        paused.Spec.Paused.ShouldBe(true);
        paused.Spec.Replicas.ShouldBe(2);

        var resume = config.GetCustomMenuItems(new[] { paused }).Single(item => item.Title == ResourcesLanguageManager.Instance.V1DeploymentConfig_Resume.CurrentValue()!);
        var resumeCommand = resume.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        resumeCommand.CanExecute(resume.CommandParameter).ShouldBeTrue();
        await resumeCommand.ExecuteAsync(resume.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);

        var resumed = await workspace.Runtime.Client.AppsV1.ReadNamespacedDeploymentAsync("pause-test", "default", cancellationToken: TestContext.Current.CancellationToken);
        resumed.Spec.Paused.ShouldBe(false);
        resumed.Spec.Replicas.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task cronjob_suspend_action_patches_suspend_field()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var cronJob = CreateCronJob("default", "suspend-test");
        await workspace.Runtime.Client!.BatchV1.CreateNamespacedCronJobAsync(cronJob, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1CronJob>(Verb.Patch, "default");
        var config = (V1CronJobConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1CronJob>());
        var action = config.GetCustomMenuItems(new[]
        {
            cronJob,
            new V1CronJob { Metadata = new V1ObjectMeta { Name = "missing-namespace" } },
        }).Single(item => item.Title == ResourcesLanguageManager.Instance.V1CronJobConfig_Suspend.CurrentValue()!);
        var command = action.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var parameters = action.CommandParameter.ShouldBeAssignableTo<IList>();
        command.CanExecute(parameters).ShouldBeTrue();

        await command.ExecuteAsync(parameters).WaitAsync(TestContext.Current.CancellationToken);
        ((TestApp)Application.Current!).Notification.ShouldBeNull();

        var updated = await workspace.Runtime.Client.BatchV1.ReadNamespacedCronJobAsync(
            cronJob.Name(), cronJob.Namespace(), cancellationToken: TestContext.Current.CancellationToken);
        updated.Spec.Suspend.ShouldBe(true);
    }

    [AvaloniaFact]
    public async Task pod_evict_action_uses_eviction_subresource()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "evict-test", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "busybox" }] },
        };
        await workspace.Runtime.Client!.CoreV1.CreateNamespacedPodAsync(pod, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1Pod>(Verb.Create, "default", "eviction");
        var config = (V1PodConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        var action = config.GetCustomMenuItems(new[]
        {
            pod,
            new V1Pod { Metadata = new V1ObjectMeta { Name = "missing-namespace" } },
        }).Single(item => item.Title == ResourcesLanguageManager.Instance.V1PodConfig_Evict.CurrentValue()!);
        var command = action.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var parameters = action.CommandParameter.ShouldBeAssignableTo<IList>();
        command.CanExecute(parameters).ShouldBeTrue();

        await command.ExecuteAsync(parameters).WaitAsync(TestContext.Current.CancellationToken);
        ((TestApp)Application.Current!).Notification.ShouldBeNull();

        var missingPod = await Should.ThrowAsync<HttpOperationException>(() => workspace.Runtime.Client.CoreV1.ReadNamespacedPodAsync(
            pod.Name(), pod.Namespace(), cancellationToken: TestContext.Current.CancellationToken));
        missingPod.Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [AvaloniaFact]
    public async Task deployment_rollout_history_lists_owned_revisions_and_undoes_selected_template()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var deployment = await workspace.Runtime.Client!.AppsV1.CreateNamespacedDeploymentAsync(new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Name = "rollout-test",
                NamespaceProperty = "default",
                Annotations = new Dictionary<string, string> { ["deployment.kubernetes.io/revision"] = "2" },
            },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Template = CreatePodTemplate("new-image"),
            },
        }, "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Client.AppsV1.CreateNamespacedReplicaSetAsync(CreateRevisionReplicaSet(deployment, "rev-old", "1", "old-image"), "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Client.AppsV1.CreateNamespacedReplicaSetAsync(CreateRevisionReplicaSet(deployment, "rev-current", "2", "new-image"), "default", cancellationToken: TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI<V1Deployment>(Verb.Patch, "default");
        await workspace.Runtime.Permissions.UpdateCanI<V1Deployment>(Verb.Get, "default");
        await workspace.Runtime.Permissions.UpdateCanI<V1ReplicaSet>(Verb.List, "default");

        using var viewModel = Application.Current.GetTestServices().GetRequiredService<DeploymentRolloutHistoryViewModel>();
        viewModel.Initialize(workspace, deployment);
        await viewModel.LoadAsync();

        viewModel.Revisions.Select(revision => revision.Revision).ShouldBe(new int?[] { 2, 1 });
        viewModel.Revisions[0].IsCurrent.ShouldBeTrue();
        viewModel.Revisions[1].IsCurrent.ShouldBeFalse();
        var undoCommand = viewModel.UndoCommand;
        undoCommand.CanExecute(null).ShouldBeFalse();
        var undoCanExecuteChanged = false;
        undoCommand.CanExecuteChanged += (_, _) => undoCanExecuteChanged = true;
        viewModel.SelectedRevision = viewModel.Revisions[1];
        undoCanExecuteChanged.ShouldBeTrue();
        undoCommand.CanExecute(null).ShouldBeTrue();

        await undoCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        var updated = await workspace.Runtime.Client.AppsV1.ReadNamespacedDeploymentAsync("rollout-test", "default", cancellationToken: TestContext.Current.CancellationToken);
        updated.Spec.Template.Spec.Containers.Single().Image.ShouldBe("old-image");
    }

    private static V1ReplicaSet CreateRevisionReplicaSet(V1Deployment deployment, string name, string revision, string image)
        => new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = deployment.Namespace(),
                Annotations = new Dictionary<string, string> { ["deployment.kubernetes.io/revision"] = revision },
                OwnerReferences =
                [
                    new V1OwnerReference
                    {
                        ApiVersion = "apps/v1",
                        Kind = "Deployment",
                        Name = deployment.Name(),
                        Uid = deployment.Uid(),
                        Controller = true,
                    },
                ],
            },
            Spec = new V1ReplicaSetSpec { Replicas = 1, Template = CreatePodTemplate(image) },
        };

    private static V1PodTemplateSpec CreatePodTemplate(string image)
        => new()
        {
            Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = "rollout-test" } },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = image }] },
        };

    [AvaloniaFact]
    public void deployment_config_exposes_visualize_action_for_single_selection()
    {
        var config = ResolveConfig<V1DeploymentConfig>();
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" },
        };

        var action = config.GetDefaultMenuItems(new[] { deployment })
            .Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_Visualize.CurrentValue()!);

        action.Command.ShouldNotBeNull();
        action.CommandParameter.ShouldBeAssignableTo<IList>();
        action.Command!.CanExecute(action.CommandParameter).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void deployment_config_disables_visualize_for_multiple_selection()
    {
        var config = ResolveConfig<V1DeploymentConfig>();
        var deployments = new[]
        {
            new V1Deployment { Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" } },
            new V1Deployment { Metadata = new V1ObjectMeta { Name = "worker", NamespaceProperty = "default" } },
        };

        var action = config.GetDefaultMenuItems(deployments)
            .Single(item => item.Title == ResourcesLanguageManager.Instance.ResourceConfigBase_MenuItem_Visualize.CurrentValue()!);

        action.Command!.CanExecute(action.CommandParameter).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task pod_logs_menu_only_shows_explicit_session_destinations()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var config = ResolveConfig<V1PodConfig>();
        config.Initialize(workspace);
        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container { Name = "app" },
                    new V1Container { Name = "sidecar" },
                ],
            },
        };

        var action = config.GetCustomMenuItems(new[] { pod })
            .Single(item => item.Title == FoundationLanguageManager.Instance.Shared_ViewLogs.CurrentValue()!);

        action.Command.ShouldBeNull();
        action.Items.ShouldNotBeNull();
        action.Items[0].Title.ShouldBe(FoundationLanguageManager.Instance.Shared_OpenNewLogsView.CurrentValue()!);
        action.Items.Select(static item => item.Title).ShouldAllBe(title =>
            title == FoundationLanguageManager.Instance.Shared_OpenNewLogsView.CurrentValue()!
            || title == FoundationLanguageManager.Instance.Shared_AddToCurrentLogsView.CurrentValue()!);
    }

    [AvaloniaFact]
    public void service_config_uses_service_properties_view()
    {
        var config = ResolveConfig<V1ServiceConfig>();

        var controls = config.Properties(new V1Service());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<ServicePropertiesView>();
    }

    [AvaloniaFact]
    public void service_config_authorization_requests_include_port_forward_dependencies()
    {
        var config = ResolveConfig<V1ServiceConfig>();

        config.AuthorizationRequests().ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Create, "portforward"));
        config.AuthorizationRequests().ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1EndpointSlice>(), Verb.List, null));
        config.AuthorizationRequests().ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1EndpointSlice>(), Verb.Watch, null));
    }

    [AvaloniaFact]
    public void cronjob_config_authorization_requests_include_start_dependency()
    {
        var config = ResolveConfig<V1CronJobConfig>();

        config.AuthorizationRequests().ShouldContain(new AuthorizationRequest(GroupApiVersionKind.From<V1Job>(), Verb.Create, null));
    }

    [AvaloniaFact]
    public void node_config_uses_node_properties_view()
    {
        var config = ResolveConfig<V1NodeConfig>();

        var controls = config.Properties(new V1Node());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<NodePropertiesView>();
    }

    [AvaloniaFact]
    public void pod_config_uses_pod_properties_view()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<V1PodConfig>();

        var controls = config.Properties(new V1Pod());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<PodPropertiesView>();
    }

    [AvaloniaFact]
    public void daemonset_config_uses_daemonset_properties_view()
    {
        var config = ResolveConfig<V1DaemonSetConfig>();

        var controls = config.Properties(new V1DaemonSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<DaemonSetPropertiesView>();
    }

    [AvaloniaFact]
    public void statefulset_config_uses_statefulset_properties_view()
    {
        var config = ResolveConfig<V1StatefulSetConfig>();

        var controls = config.Properties(new V1StatefulSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<StatefulSetPropertiesView>();
    }

    [AvaloniaFact]
    public void replicaset_config_uses_replicaset_properties_view()
    {
        var config = ResolveConfig<V1ReplicaSetConfig>();

        var controls = config.Properties(new V1ReplicaSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<ReplicaSetPropertiesView>();
    }

    [AvaloniaFact]
    public void cronjob_config_uses_cronjob_properties_view()
    {
        var config = ResolveConfig<V1CronJobConfig>();

        var controls = config.Properties(new V1CronJob());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<CronJobPropertiesView>();
    }

    [AvaloniaFact]
    public void cronjob_config_start_menu_creates_job_from_cronjob_template()
    {
        var cronJob = CreateCronJob("volsync", "immich-rclone-backup");
        var job = V1CronJobConfig.CreateJobFromCronJob(cronJob, new DateTimeOffset(2026, 5, 15, 13, 14, 15, TimeSpan.Zero));

        job.ApiVersion.ShouldBe("batch/v1");
        job.Kind.ShouldBe("Job");
        job.Name().ShouldBe("immich-rclone-backup-manual-202605151314150000000");
        job.Namespace().ShouldBe("volsync");
        job.Spec.ShouldBeSameAs(cronJob.Spec.JobTemplate.Spec);
        job.Metadata.Labels.ShouldContainKeyAndValue("app", "immich");
        job.Metadata.Annotations.ShouldContainKeyAndValue("template", "backup");
        job.Metadata.Annotations.ShouldContainKeyAndValue("cronjob.kubernetes.io/instantiate", "manual");

        var ownerReference = job.Metadata.OwnerReferences.Single();
        ownerReference.ApiVersion.ShouldBe("batch/v1");
        ownerReference.Kind.ShouldBe("CronJob");
        ownerReference.Name.ShouldBe("immich-rclone-backup");
        ownerReference.Uid.ShouldBe("cronjob-uid");
        ownerReference.Controller.ShouldBe(true);
        ownerReference.BlockOwnerDeletion.ShouldBeNull();
    }

    [AvaloniaFact]
    public void cronjob_config_start_menu_matches_kubectl_annotation_precedence()
    {
        var cronJob = CreateCronJob("volsync", "immich-rclone-backup");
        cronJob.Spec.JobTemplate.Metadata.Annotations["cronjob.kubernetes.io/instantiate"] = "template";

        var job = V1CronJobConfig.CreateJobFromCronJob(cronJob, new DateTimeOffset(2026, 5, 15, 13, 14, 15, TimeSpan.Zero));

        job.Metadata.Annotations.ShouldContainKeyAndValue("cronjob.kubernetes.io/instantiate", "template");
    }

    [AvaloniaFact]
    public async Task cronjob_start_context_menu_creates_job_resource()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        using var _ = workspace;
        await workspace.Runtime.SeedResource<V1Namespace>(true);
        await workspace.Runtime.SeedResource<V1CronJob>(true);
        await workspace.Runtime.SeedResource<V1Job>(true);
        await workspace.Runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "volsync" },
        });

        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create, "volsync");
        var config = (V1CronJobConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1CronJob>());
        var cronJob = CreateCronJob("volsync", "immich-rclone-backup");

        var startItem = config.GetCustomMenuItems(new[] { cronJob }).Single(item => item.Title == "Start");
        var command = startItem.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var commandParameter = startItem.CommandParameter.ShouldBeAssignableTo<IList>();

        command.CanExecute(commandParameter).ShouldBeTrue();

        await command.ExecuteAsync(commandParameter).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResourceList<V1Job>().Count == 1,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var job = workspace.Runtime.GetResourceList<V1Job>().Single();
        job.Namespace().ShouldBe("volsync");
        job.Name().ShouldStartWith("immich-rclone-backup-manual-");
        job.Spec.Template.Spec.Containers.Single().Image.ShouldBe(cronJob.Spec.JobTemplate.Spec.Template.Spec.Containers.Single().Image);
        job.Spec.Template.Spec.RestartPolicy.ShouldBe(cronJob.Spec.JobTemplate.Spec.Template.Spec.RestartPolicy);
        job.Metadata.Annotations.ShouldContainKeyAndValue("cronjob.kubernetes.io/instantiate", "manual");
    }

    [AvaloniaFact]
    public async Task cronjob_start_context_menu_requires_job_create_permission()
    {
        var services = Application.Current.GetTestServices();
        var clusterConfig = services.GetRequiredService<TestClusterConfig>();
        clusterConfig.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
        clusterConfig.InitialResources = new[]
        {
            (IKubernetesObject<V1ObjectMeta>)new V1Namespace { Metadata = new V1ObjectMeta { Name = "volsync" } },
        }
            .Concat(KubernetesRbac.ClusterWide(
                new RbacRule("namespaces", "list"),
                new RbacRule("namespaces", "watch")))
            .ToArray();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1Namespace>(true);
        await workspace.Runtime.SeedResource<V1CronJob>(true);
        await workspace.Runtime.SeedResource<V1Job>(true);
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create);
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create, "volsync");
        var config = (V1CronJobConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1CronJob>());
        var cronJob = CreateCronJob("volsync", "immich-rclone-backup");

        var startItem = config.GetCustomMenuItems(new[] { cronJob }).Single(item => item.Title == "Start");
        var command = startItem.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var commandParameter = startItem.CommandParameter.ShouldBeAssignableTo<IList>();

        command.CanExecute(commandParameter).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task cronjob_start_context_menu_allows_namespace_scoped_job_create_permission()
    {
        var services = Application.Current.GetTestServices();
        var clusterConfig = services.GetRequiredService<TestClusterConfig>();
        clusterConfig.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
        clusterConfig.InitialResources = new[]
        {
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "volsync" } },
        }
            .Concat(KubernetesRbac.ClusterWide(
                new RbacRule("namespaces", "list"),
                new RbacRule("namespaces", "watch")))
            .Concat(KubernetesRbac.InNamespace("volsync", new RbacRule("jobs", "create", "batch")))
            .ToArray();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create);
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create, "volsync");
        await workspace.Runtime.SeedResource<V1Namespace>(true);
        await workspace.Runtime.SeedResource<V1CronJob>(true);
        await workspace.Runtime.SeedResource<V1Job>(true);
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create);
        await ((Cluster)workspace.Runtime).UpdateCanI<V1Job>(Verb.Create, "volsync");
        var config = (V1CronJobConfig)workspace.GetResourceConfig(GroupApiVersionKind.From<V1CronJob>());
        var cronJob = CreateCronJob("volsync", "immich-rclone-backup");

        var startItem = config.GetCustomMenuItems(new[] { cronJob }).Single(item => item.Title == "Start");
        var command = startItem.Command.ShouldBeAssignableTo<IAsyncRelayCommand>();
        var commandParameter = startItem.CommandParameter.ShouldBeAssignableTo<IList>();

        command.CanExecute(commandParameter).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void job_config_uses_job_properties_view()
    {
        var config = ResolveConfig<V1JobConfig>();

        var controls = config.Properties(new V1Job());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<JobPropertiesView>();
    }

    private static V1CronJob CreateCronJob(string @namespace, string name)
    {
        return new V1CronJob
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = @namespace,
                Uid = "cronjob-uid",
            },
            Spec = new V1CronJobSpec
            {
                Schedule = "0 1 * * *",
                JobTemplate = new V1JobTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            ["app"] = "immich",
                        },
                        Annotations = new Dictionary<string, string>
                        {
                            ["template"] = "backup",
                        },
                    },
                    Spec = new V1JobSpec
                    {
                        Template = new V1PodTemplateSpec
                        {
                            Spec = new V1PodSpec
                            {
                                RestartPolicy = "Never",
                                Containers =
                                [
                                    new V1Container
                                    {
                                        Name = "backup",
                                        Image = "busybox",
                                    },
                                ],
                            },
                        },
                    },
                },
            },
        };
    }

    [AvaloniaFact] public void storage_class_config_uses_storage_class_properties_view() => AssertProperties<V1StorageClassConfig, V1StorageClass, StorageClassPropertiesView>(new V1StorageClass());
    [AvaloniaFact] public void persistent_volume_claim_config_uses_pvc_properties_view() => AssertProperties<V1PersistentVolumeClaimConfig, V1PersistentVolumeClaim, PersistentVolumeClaimPropertiesView>(new V1PersistentVolumeClaim());
    [AvaloniaFact] public void persistent_volume_config_uses_pv_properties_view() => AssertProperties<V1PersistentVolumeConfig, V1PersistentVolume, PersistentVolumePropertiesView>(new V1PersistentVolume());
    [AvaloniaFact] public void service_account_config_uses_service_account_properties_view() => AssertProperties<V1ServiceAccountConfig, V1ServiceAccount, ServiceAccountPropertiesView>(new V1ServiceAccount());
    [AvaloniaFact] public void role_binding_config_uses_role_binding_properties_view() => AssertProperties<V1RoleBindingConfig, V1RoleBinding, RoleBindingPropertiesView>(new V1RoleBinding());
    [AvaloniaFact] public void role_config_uses_role_properties_view() => AssertProperties<V1RoleConfig, V1Role, RolePropertiesView>(new V1Role());
    [AvaloniaFact] public void cluster_role_config_uses_cluster_role_properties_view() => AssertProperties<V1ClusterRoleConfig, V1ClusterRole, ClusterRolePropertiesView>(new V1ClusterRole());
    [AvaloniaFact] public void cluster_role_binding_config_uses_cluster_role_binding_properties_view() => AssertProperties<V1ClusterRoleBindingConfig, V1ClusterRoleBinding, ClusterRoleBindingPropertiesView>(new V1ClusterRoleBinding());
    [AvaloniaFact] public void ingress_config_uses_ingress_properties_view() => AssertProperties<V1IngressConfig, V1Ingress, IngressPropertiesView>(new V1Ingress());
    [AvaloniaFact] public void ingress_class_config_uses_ingress_class_properties_view() => AssertProperties<V1IngressClassConfig, V1IngressClass, IngressClassPropertiesView>(new V1IngressClass());
    [AvaloniaFact] public void endpoint_slice_config_uses_endpoint_slice_properties_view() => AssertProperties<V1EndpointSliceConfig, V1EndpointSlice, EndpointSlicePropertiesView>(new V1EndpointSlice());
    [AvaloniaFact] public void network_policy_config_uses_network_policy_properties_view() => AssertProperties<V1NetworkPolicyConfig, V1NetworkPolicy, NetworkPolicyPropertiesView>(new V1NetworkPolicy());
    [AvaloniaFact] public void hpa_config_uses_hpa_properties_view() => AssertProperties<V2HorizontalPodAutoscalerConfig, V2HorizontalPodAutoscaler, HpaPropertiesView>(new V2HorizontalPodAutoscaler());
    [AvaloniaFact] public void validating_webhook_config_uses_properties_view() => AssertProperties<V1ValidatingWebhookConfigurationConfig, V1ValidatingWebhookConfiguration, ValidatingWebhookConfigurationPropertiesView>(new V1ValidatingWebhookConfiguration());
    [AvaloniaFact] public void mutating_webhook_config_uses_properties_view() => AssertProperties<V1MutatingWebhookConfig, V1MutatingWebhookConfiguration, MutatingWebhookConfigurationPropertiesView>(new V1MutatingWebhookConfiguration());
    [AvaloniaFact] public void runtime_class_config_uses_properties_view() => AssertProperties<V1RuntimeClassConfig, V1RuntimeClass, RuntimeClassPropertiesView>(new V1RuntimeClass());
    [AvaloniaFact] public void lease_config_uses_properties_view() => AssertProperties<V1LeaseConfig, V1Lease, LeasePropertiesView>(new V1Lease());
    [AvaloniaFact] public void priority_class_config_uses_properties_view() => AssertProperties<V1PriorityClassConfig, V1PriorityClass, PriorityClassPropertiesView>(new V1PriorityClass());
    [AvaloniaFact] public void resource_quota_config_uses_properties_view() => AssertProperties<V1ResourceQuotaConfig, V1ResourceQuota, ResourceQuotaPropertiesView>(new V1ResourceQuota());
    [AvaloniaFact] public void pod_disruption_budget_config_uses_properties_view() => AssertProperties<V1PodDisruptionBudgetConfig, V1PodDisruptionBudget, PodDisruptionBudgetPropertiesView>(new V1PodDisruptionBudget());
    [AvaloniaFact] public void config_map_config_uses_properties_view() => AssertProperties<V1ConfigMapConfig, V1ConfigMap, ConfigMapPropertiesView>(new V1ConfigMap());
    [AvaloniaFact] public void limit_range_config_uses_properties_view() => AssertProperties<V1LimitRangeConfig, V1LimitRange, LimitRangePropertiesView>(new V1LimitRange());
    [AvaloniaFact] public void namespace_config_uses_properties_view() => AssertProperties<V1NamespaceConfig, V1Namespace, NamespacePropertiesView>(new V1Namespace());
    [AvaloniaFact]
    public void crd_config_uses_properties_view()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<V1CustomResourceDefinitionConfig>();

        var controls = config.Properties(new V1CustomResourceDefinition());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<CrdPropertiesView>();
    }

    [AvaloniaFact] public void workloads_section_is_localized() => AssertSection<V1DeploymentConfig>(NavigationSections.WorkloadsId, "Workloads");
    [AvaloniaFact] public void network_section_is_localized() => AssertSection<V1ServiceConfig>(NavigationSections.NetworkId, "Network");
    [AvaloniaFact] public void storage_section_is_localized() => AssertSection<V1StorageClassConfig>(NavigationSections.StorageId, "Storage");
    [AvaloniaFact] public void configuration_section_is_localized() => AssertSection<V1ConfigMapConfig>(NavigationSections.ConfigurationId, "Configuration");
    [AvaloniaFact] public void access_control_section_is_localized() => AssertSection<V1RoleConfig>(NavigationSections.AccessControlId, "Access Control");
}
