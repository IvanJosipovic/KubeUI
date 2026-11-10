using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PrometheusProviderQueryTests
{
    [Fact]
    public void BuildNodeQuery_escapes_address_for_prometheus_string_literal()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["instance"] = "10\\.0\\.0\\.1",
            ["node"] = "node-1",
        };

        var query = provider.BuildQuery(MetricCategory.Nodes, "cpuUsage", options);

        query.ShouldContain("instance=~\"(10\\\\.0\\\\.0\\\\.1)(:[0-9]+)?\"");
        query.ShouldContain(") by (instance)");
        query.ShouldNotContain(") by (node)");
    }

    [Theory]
    [InlineData("fsSize", "node_filesystem_size_bytes")]
    [InlineData("fsUsage", "node_filesystem_size_bytes")]
    public void BuildNodeFilesystemQuery_uses_node_exporter_instance_labels(string queryName, string metricName)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["instance"] = "10\\.0\\.0\\.1",
            ["mountpoints"] = "^/$",
        };

        var query = provider.BuildQuery(MetricCategory.Nodes, queryName, options);

        query.ShouldContain($"{metricName}{{instance=~");
        query.ShouldContain("mountpoint=~\"^/$\"");
        query.ShouldContain(") by (instance)");
        if (queryName == "fsUsage")
        {
            query.ShouldEndWith("})) by (instance)");
        }
        query.ShouldNotContain("kube_pod_info");
        query.ShouldNotContain("on (pod");
    }

    [Fact]
    public void BuildNodeMemoryUsageQuery_matches_node_exporter_instance_port()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["instance"] = "10\\.0\\.0\\.1",
        };

        var query = provider.BuildQuery(MetricCategory.Nodes, "memoryUsage", options);

        query.Split("instance=~\"(10\\\\.0\\\\.0\\\\.1)(:[0-9]+)?\"", StringSplitOptions.None).Length.ShouldBe(5);
    }

    [Fact]
    public void BuildPodQuery_escapes_regex_backslashes_for_prometheus_string_literal()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["pods"] = @"apicurio\.registry",
            ["namespace"] = "default",
            ["selector"] = "pod, namespace",
        };

        var query = provider.BuildQuery(MetricCategory.Pods, "cpuUsage", options);

        query.ShouldContain("pod=~\"apicurio\\\\.registry\"");
    }

    [Fact]
    public void BuildPodQueryByLabel_builds_valid_workload_join_and_rate_expression()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["namespace"] = "default",
            ["labelSelector"] = "label_app=\"web\"",
            ["selector"] = "pod, namespace",
            ["rateAccuracy"] = "5m",
        };

        var query = provider.BuildQuery(MetricCategory.WorkloadPods, "cpuUsage", options);

        query.ShouldContain("rate(container_cpu_usage_seconds_total{image!=\"\",container!=\"\"}[5m]) * on (namespace, pod) group_left() kube_pod_labels{namespace=\"default\",label_app=\"web\"}");
        query.ShouldNotContain("kube_pod_labels{,");
        query.ShouldNotContain("group_right");
    }

    [Theory]
    [InlineData("cpuRequests", "requests", "cpu")]
    [InlineData("cpuLimits", "limits", "cpu")]
    [InlineData("memoryRequests", "requests", "memory")]
    [InlineData("memoryLimits", "limits", "memory")]
    public void BuildPodQueryByLabel_supports_resource_requests_and_limits(string queryName, string resourceMetric, string resource)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["namespace"] = "default",
            ["labelSelector"] = "label_app=\"web\"",
            ["selector"] = "pod, namespace",
        };

        var query = provider.BuildQuery(MetricCategory.WorkloadPods, queryName, options);

        query.ShouldContain($"kube_pod_container_resource_{resourceMetric}{{resource=\"{resource}\"}}");
        query.ShouldContain("* on (namespace, pod) group_left() kube_pod_labels{namespace=\"default\",label_app=\"web\"}");
        query.ShouldNotContain("kube_pod_labels{,");
    }

    [Theory]
    [InlineData("memoryUsage", "container_memory_working_set_bytes", false)]
    [InlineData("fsUsage", "container_fs_usage_bytes", false)]
    [InlineData("fsWrites", "container_fs_writes_bytes_total", true)]
    [InlineData("fsReads", "container_fs_reads_bytes_total", true)]
    [InlineData("networkReceive", "container_network_receive_bytes_total", true)]
    [InlineData("networkTransmit", "container_network_transmit_bytes_total", true)]
    public void BuildPodQueryByLabel_joins_other_workload_metrics(string queryName, string metricName, bool usesRate)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["namespace"] = "default",
            ["labelSelector"] = "label_app=\"web\"",
            ["selector"] = "pod, namespace",
            ["rateAccuracy"] = "5m",
        };

        var query = provider.BuildQuery(MetricCategory.WorkloadPods, queryName, options);

        query.ShouldContain("* on (namespace, pod) group_left() kube_pod_labels{namespace=\"default\",label_app=\"web\"}");
        if (usesRate)
        {
            query.ShouldContain($"rate({metricName}");
            query.ShouldContain("[5m]) * on (namespace, pod)");
        }
        else
        {
            query.ShouldContain($"{metricName}{{");
            query.ShouldNotContain("rate(");
        }
    }

    [Fact]
    public void BuildPodQueryByLabel_rejects_unknown_metrics()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["namespace"] = "default",
            ["labelSelector"] = "label_app=\"web\"",
        };

        Should.Throw<InvalidOperationException>(() => provider.BuildQuery(MetricCategory.WorkloadPods, "unknown", options));
    }

    [Fact]
    public async Task OperatorProvider_propagates_cancellation_while_resolving_services()
    {
        var provider = new OperatorPrometheusProvider();
        using var client = new k8s.Kubernetes(new k8s.KubernetesClientConfiguration { Host = "http://localhost" });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => provider.TryResolveServiceAsync(
            client,
            new ClusterMetricsSettings(),
            cancellation.Token));
    }

    [Fact]
    public async Task ExternalProvider_copies_persisted_authentication_settings_to_resolved_endpoint()
    {
        var provider = new ExternalPrometheusProvider();
        using var client = new k8s.Kubernetes(new k8s.KubernetesClientConfiguration { Host = "http://localhost" });
        var header = new PrometheusHeader { Name = "X-Api-Key", Value = "initial" };
        var settings = new ClusterMetricsSettings
        {
            PrometheusDirectUrl = "https://prometheus.example",
            PrometheusCustomHeaders = [header],
            PrometheusBasicAuthUsername = "metrics-user",
            PrometheusBasicAuthPassword = "secret",
        };

        var endpoint = await provider.TryResolveServiceAsync(client, settings);
        header.Value = "changed-after-resolution";

        endpoint.ShouldNotBeNull();
        endpoint.CustomHeaders.ShouldHaveSingleItem().Value.ShouldBe("initial");
        endpoint.BasicAuthUsername.ShouldBe("metrics-user");
        endpoint.BasicAuthPassword.ShouldBe("secret");
    }

    [Fact]
    public async Task ManualProvider_resolves_direct_url_without_service_settings()
    {
        var provider = new ManualPrometheusProvider();
        var settings = new ClusterMetricsSettings
        {
            PrometheusDirectUrl = "https://prometheus.example",
            PrometheusPathPrefix = "/prometheus",
        };

        var endpoint = await provider.TryResolveServiceAsync(null!, settings);

        endpoint.ShouldNotBeNull();
        endpoint.DirectUrl.ShouldBe("https://prometheus.example");
        endpoint.Namespace.ShouldBeNull();
        endpoint.ServiceName.ShouldBeNull();
        endpoint.ServicePort.ShouldBeNull();
        endpoint.PathPrefix.ShouldBe("/prometheus");
    }

    [Fact]
    public async Task ManualProvider_resolves_service_port_forward_when_direct_url_is_empty()
    {
        var provider = new ManualPrometheusProvider();
        var settings = new ClusterMetricsSettings
        {
            PrometheusServiceName = "prometheus",
            PrometheusServiceNamespace = "monitoring",
            PrometheusServicePort = 9090,
            PrometheusUseHttps = true,
        };

        var endpoint = await provider.TryResolveServiceAsync(null!, settings);

        endpoint.ShouldNotBeNull();
        endpoint.DirectUrl.ShouldBeNull();
        endpoint.Namespace.ShouldBe("monitoring");
        endpoint.ServiceName.ShouldBe("prometheus");
        endpoint.ServicePort.ShouldBe(9090);
        endpoint.UseHttps.ShouldBeTrue();
    }

    [Theory]
    [InlineData("bytesSentSuccess", "^2[0-9]*")]
    [InlineData("bytesSentFailure", "^5[0-9]*")]
    public void BuildIngressQuery_uses_valid_RE2_digit_class(string queryName, string expectedStatusPattern)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["ingress"] = "sample-ingress",
            ["namespace"] = "default",
        };

        var query = provider.BuildQuery(MetricCategory.Ingress, queryName, options);

        query.ShouldContain($"status=~\"{expectedStatusPattern}\"");
        query.ShouldNotContain("\\\\d");
    }

    [Theory]
    [InlineData("requestDurationSeconds", "nginx_ingress_controller_request_duration_seconds")]
    [InlineData("responseDurationSeconds", "nginx_ingress_controller_response_duration_seconds")]
    public void BuildIngressLatencyQuery_calculates_average_duration_per_request(string queryName, string histogramName)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["ingress"] = "sample-ingress",
            ["namespace"] = "default",
        };

        var query = provider.BuildQuery(MetricCategory.Ingress, queryName, options);

        var sumRate = $"sum(rate({histogramName}_sum{{ingress=\"sample-ingress\", namespace=\"default\"}}[1m])) by (ingress, namespace)";
        var countRate = $"sum(rate({histogramName}_count{{ingress=\"sample-ingress\", namespace=\"default\"}}[1m])) by (ingress, namespace)";
        query.ShouldBe($"{sumRate} / {countRate}");
    }
}
