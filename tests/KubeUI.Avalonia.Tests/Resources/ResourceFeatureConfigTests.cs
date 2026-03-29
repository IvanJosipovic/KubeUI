using Avalonia.Headless.XUnit;
using CronJobPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.CronJob.Views.PropertiesView;
using DeploymentPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Deployment.Views.PropertiesView;
using DaemonSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet.Views.PropertiesView;
using JobPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Job.Views.PropertiesView;
using NodePropertiesView = KubeUI.Avalonia.Resources.Core.v1.Node.Views.PropertiesView;
using ServicePropertiesView = KubeUI.Avalonia.Resources.Network.v1.Service.Views.PropertiesView;
using PodPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views.PropertiesView;
using ReplicaSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet.Views.PropertiesView;
using StatefulSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet.Views.PropertiesView;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret.Views;
using KubeUI.Avalonia.Resources.Core.v1;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Resources.Network;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Resources.Workloads;
using KubeUI.Avalonia.Tests.Infra;
using k8s.Models;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public sealed class ResourceFeatureConfigTests : AvaloniaTestBase
{
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
}
