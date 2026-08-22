using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.OpenApi;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public sealed class YamlHoverToolTipBehaviorTests
{
    [Fact]
    public async Task EnsureSchemaLoadAsync_RetriesAfterFailedLoad()
    {
        var behavior = new YamlHoverToolTipBehavior();
        var attempts = 0;

        var firstResult = await behavior.EnsureSchemaLoadAsync(() =>
        {
            attempts++;
            return Task.FromException(new InvalidOperationException("schema load failed"));
        });

        var secondResult = await behavior.EnsureSchemaLoadAsync(() =>
        {
            attempts++;
            return Task.CompletedTask;
        });

        firstResult.ShouldBeFalse();
        secondResult.ShouldBeTrue();
        attempts.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task GetSchemaRoot_RebuildsWhenSchemaCatalogVersionChanges()
    {
        using var cluster = await Application.Current.CreateClusterAsync(connect: false);
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod { Metadata = new V1ObjectMeta { Name = "pod" } });
        var behavior = new YamlHoverToolTipBehavior();

        var firstRoot = behavior.GetSchemaRoot(vm);
        cluster.Runtime.ModelCatalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.Pod"] = new OpenApiSchema(),
                },
            },
        });

        var secondRoot = behavior.GetSchemaRoot(vm);

        secondRoot.ShouldNotBeSameAs(firstRoot);
    }
}
