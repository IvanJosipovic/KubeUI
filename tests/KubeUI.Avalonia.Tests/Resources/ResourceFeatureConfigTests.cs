using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Resources.AccessControl;
using KubeUI.Avalonia.Resources.Configuration;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret;
using KubeUI.Avalonia.Resources.Core.v1;
using KubeUI.Avalonia.Resources.Core.v1.Event;
using KubeUI.Avalonia.Resources.Network;
using KubeUI.Avalonia.Resources.Storage;
using KubeUI.Avalonia.Resources.Workloads;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using ClusterRoleBindingPropertiesView = KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding.Views.PropertiesView;
using ClusterRolePropertiesView = KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRole.Views.PropertiesView;
using ConfigMapPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap.Views.PropertiesView;
using CrdPropertiesView = KubeUI.Avalonia.Resources.CustomResourceDefinition.Views.PropertiesView;
using CronJobPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.CronJob.Views.PropertiesView;
using DaemonSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet.Views.PropertiesView;
using DeploymentPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Deployment.Views.PropertiesView;
using EndpointSlicePropertiesView = KubeUI.Avalonia.Resources.Network.v1.EndpointSlice.Views.PropertiesView;
using EventPropertiesView = KubeUI.Avalonia.Resources.Core.v1.Event.Views.PropertiesView;
using HpaPropertiesView = KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler.Views.PropertiesView;
using IngressClassPropertiesView = KubeUI.Avalonia.Resources.Network.v1.IngressClass.Views.PropertiesView;
using IngressPropertiesView = KubeUI.Avalonia.Resources.Network.v1.Ingress.Views.PropertiesView;
using JobPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Job.Views.PropertiesView;
using LeasePropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.Lease.Views.PropertiesView;
using LimitRangePropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.LimitRange.Views.PropertiesView;
using MutatingWebhookConfigurationPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.MutatingWebhookConfiguration.Views.PropertiesView;
using NamespacePropertiesView = KubeUI.Avalonia.Resources.Core.v1.Namespace.Views.PropertiesView;
using NetworkPolicyPropertiesView = KubeUI.Avalonia.Resources.Network.v1.NetworkPolicy.Views.PropertiesView;
using NodePropertiesView = KubeUI.Avalonia.Resources.Core.v1.Node.Views.PropertiesView;
using PersistentVolumeClaimPropertiesView = KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim.Views.PropertiesView;
using PersistentVolumePropertiesView = KubeUI.Avalonia.Resources.Storage.v1.PersistentVolume.Views.PropertiesView;
using PodDisruptionBudgetPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.PodDisruptionBudget.Views.PropertiesView;
using PodPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views.PropertiesView;
using PriorityClassPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.PriorityClass.Views.PropertiesView;
using ReplicaSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet.Views.PropertiesView;
using ResourceQuotaPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.ResourceQuota.Views.PropertiesView;
using RoleBindingPropertiesView = KubeUI.Avalonia.Resources.AccessControl.v1.RoleBinding.Views.PropertiesView;
using RolePropertiesView = KubeUI.Avalonia.Resources.AccessControl.v1.Role.Views.PropertiesView;
using RuntimeClassPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.RuntimeClass.Views.PropertiesView;
using SecretPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.Secret.Views.PropertiesView;
using ServiceAccountPropertiesView = KubeUI.Avalonia.Resources.AccessControl.v1.ServiceAccount.Views.PropertiesView;
using ServicePropertiesView = KubeUI.Avalonia.Resources.Network.v1.Service.Views.PropertiesView;
using StatefulSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet.Views.PropertiesView;
using StorageClassPropertiesView = KubeUI.Avalonia.Resources.Storage.v1.StorageClass.Views.PropertiesView;
using V2HorizontalPodAutoscalerConfig = KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler.V2HorizontalPodAutoscalerConfig;
using ValidatingWebhookConfigurationPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.ValidatingWebhookConfiguration.Views.PropertiesView;

namespace KubeUI.Avalonia.Tests.Resources;

public sealed class ResourceFeatureConfigTests : AvaloniaTestBase
{
    private static void AssertProperties<TConfig, TResource, TView>(TResource resource)
        where TConfig : ResourceConfigBase<TResource>, new()
        where TResource : class, k8s.IKubernetesObject<k8s.Models.V1ObjectMeta>, new()
        where TView : Control
    {
        var config = new TConfig();

        var controls = config.Properties(resource);

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<TView>();
    }

    private static void AssertCategory<TConfig>(string expected)
        where TConfig : IResourceConfig, new()
    {
        IResourceConfig config = new TConfig();

        config.Category.ShouldBe(expected);
    }

    [AvaloniaFact]
    public void secret_config_uses_secret_local_properties_view()
    {
        var config = new V1SecretConfig();

        var controls = config.Properties(new V1Secret());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<SecretPropertiesView>();
    }

    [AvaloniaFact]
    public void event_config_uses_custom_last_seen_cell()
    {
        var config = new V1EventConfig();

        var lastSeenColumn = config.Columns().Single(x => x.Name == "Last Seen");

        lastSeenColumn.CustomControl.Name.ShouldBe("EventLastSeenCell");
    }

    [AvaloniaFact]
    public void event_config_uses_event_properties_view()
    {
        AssertProperties<V1EventConfig, Corev1Event, EventPropertiesView>(new Corev1Event());
    }

    [AvaloniaFact]
    public void deployment_config_uses_deployment_properties_view()
    {
        var config = new V1DeploymentConfig();

        var controls = config.Properties(new V1Deployment());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<DeploymentPropertiesView>();
    }

    [AvaloniaFact]
    public void service_config_uses_service_properties_view()
    {
        var config = new V1ServiceConfig();

        var controls = config.Properties(new V1Service());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<ServicePropertiesView>();
    }

    [AvaloniaFact]
    public void node_config_uses_node_properties_view()
    {
        var config = new V1NodeConfig();

        var controls = config.Properties(new V1Node());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<NodePropertiesView>();
    }

    [AvaloniaFact]
    public void pod_config_uses_pod_properties_view()
    {
        var config = new V1PodConfig();

        var controls = config.Properties(new V1Pod());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<PodPropertiesView>();
    }

    [AvaloniaFact]
    public void daemonset_config_uses_daemonset_properties_view()
    {
        var config = new V1DaemonSetConfig();

        var controls = config.Properties(new V1DaemonSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<DaemonSetPropertiesView>();
    }

    [AvaloniaFact]
    public void statefulset_config_uses_statefulset_properties_view()
    {
        var config = new V1StatefulSetConfig();

        var controls = config.Properties(new V1StatefulSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<StatefulSetPropertiesView>();
    }

    [AvaloniaFact]
    public void replicaset_config_uses_replicaset_properties_view()
    {
        var config = new V1ReplicaSetConfig();

        var controls = config.Properties(new V1ReplicaSet());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<ReplicaSetPropertiesView>();
    }

    [AvaloniaFact]
    public void cronjob_config_uses_cronjob_properties_view()
    {
        var config = new V1CronJobConfig();

        var controls = config.Properties(new V1CronJob());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<CronJobPropertiesView>();
    }

    [AvaloniaFact]
    public void job_config_uses_job_properties_view()
    {
        var config = new V1JobConfig();

        var controls = config.Properties(new V1Job());

        controls.Length.ShouldBe(1);
        controls[0].ShouldBeOfType<JobPropertiesView>();
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
    [AvaloniaFact] public void crd_config_uses_properties_view() => AssertProperties<V1CustomResourceDefinitionConfig, V1CustomResourceDefinition, CrdPropertiesView>(new V1CustomResourceDefinition());

    [AvaloniaFact] public void workloads_category_is_localized() => AssertCategory<V1DeploymentConfig>("Workloads");
    [AvaloniaFact] public void network_category_is_localized() => AssertCategory<V1ServiceConfig>("Network");
    [AvaloniaFact] public void storage_category_is_localized() => AssertCategory<V1StorageClassConfig>("Storage");
    [AvaloniaFact] public void configuration_category_is_localized() => AssertCategory<V1ConfigMapConfig>("Configuration");
    [AvaloniaFact] public void access_control_category_is_localized() => AssertCategory<V1RoleConfig>("Access Control");
}
