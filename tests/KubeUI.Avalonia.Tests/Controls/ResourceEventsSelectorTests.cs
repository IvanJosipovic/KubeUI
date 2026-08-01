using k8s.Models;
using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Controls;

public sealed class ResourceEventsSelectorTests
{
    [Fact]
    public void FormatPrettyAge_UsesExpectedUnits()
    {
        var now = new DateTime(2026, 3, 29, 0, 34, 52, DateTimeKind.Utc);

        EventTimeFormatter.FormatPrettyAge(now, now).ShouldBe("0ms");
        EventTimeFormatter.FormatPrettyAge(now.AddSeconds(-74), now).ShouldBe("1m14s");
        EventTimeFormatter.FormatPrettyAge(now.AddHours(-2), now).ShouldBe("2h");
    }

    [AvaloniaFact]
    public async Task SelectRecentEvents_sorts_and_limits_to_five()
    {
        IServiceProvider services = Application.Current.GetTestServices();
        TestClusterConfig config = services.GetRequiredService<TestClusterConfig>();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        IClusterRuntime runtime = workspace.Runtime;
        await workspace.Connect();
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.List);
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.Watch);
        await runtime.SeedResource<Corev1Event>(true);

        var resource = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-deployment",
                NamespaceProperty = "default",
                Uid = "resource-uid",
            }
        };

        var now = new DateTime(2026, 3, 29, 0, 40, 0, DateTimeKind.Utc);

        for (var i = 0; i < 6; i++)
        {
            await runtime.AddOrUpdateResource(new Corev1Event
            {
                Metadata = new()
                {
                    Name = $"event-{i}",
                    NamespaceProperty = "default",
                    CreationTimestamp = now.AddMinutes(-i - 1),
                },
                Message = $"message-{i}",
                Type = i == 0 ? "Warning" : "Normal",
                Count = i + 1,
                LastTimestamp = now.AddMinutes(-i - 1),
                Source = new()
                {
                    Component = "kubelet",
                    Host = $"node-{i}",
                },
                InvolvedObject = new()
                {
                    Uid = "resource-uid",
                    FieldPath = $"spec.containers{{container-{i}}}",
                }
            });
        }

        await runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "other-event",
                NamespaceProperty = "default",
                CreationTimestamp = now,
            },
            Message = "other",
            InvolvedObject = new()
            {
                Uid = "other-uid",
            }
        });

        await TestWait.UntilAsync(
            () => runtime.GetResourceSourceCache<Corev1Event>().Items.Count == 7,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var results = ResourceEventsSelector.SelectRecentEvents(
            runtime.GetResourceSourceCache<Corev1Event>().Items,
            resource,
            now);

        results.Length.ShouldBe(5);
        results[0].Message.ShouldBe("message-0");
        results[0].Source.ShouldBe("kubelet node-0");
        results[0].Count.ShouldBe(1);
        results[0].SubObject.ShouldBe("spec.containers{container-0}");
        results[0].LastSeen.ShouldStartWith("1m0s ago");
        results[0].IsWarning.ShouldBeTrue();

        results[4].Message.ShouldBe("message-4");
        results[4].LastSeen.ShouldStartWith("5m0s ago");
    }
}
