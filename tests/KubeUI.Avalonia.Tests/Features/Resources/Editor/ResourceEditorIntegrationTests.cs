using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentIcons.Avalonia;
using FluentAvalonia.UI.Controls;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Editor;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Kubernetes;
using Microsoft.OpenApi;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Editor;

public sealed class ResourceEditorIntegrationTests
{
    [AvaloniaFact]
    public void Kubernetes_openapi_schema_builds_nested_arrays_maps_and_enums()
    {
        var container = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
            },
        };
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.Pod"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["spec"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["containers"] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = container },
                                    ["nodeSelector"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        AdditionalPropertiesAllowed = true,
                                        AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.String },
                                    },
                                    ["restartPolicy"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.String,
                                        Enum = [JsonValue.Create("Always"), JsonValue.Create("Never")],
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };
        var catalog = new KubernetesOpenApiSchemaCatalog();
        catalog.Register(document);

        var root = ResourceEditorSchemaNode.CreateRoot(
            GroupApiVersionKind.From<V1Pod>(),
            catalog);
        ResourceEditorSchemaNode.CreateRoot(GroupApiVersionKind.From<V1Pod>(), catalog)
            .ShouldBeSameAs(root);

        var spec = root.Properties["spec"];
        spec.Properties["nodeSelector"].ValueKind.ShouldBe(ResourceEditorValueKind.Map);
        spec.Properties["nodeSelector"].Items.ShouldNotBeNull();
        spec.Properties["containers"].ValueKind.ShouldBe(ResourceEditorValueKind.Array);
        spec.Properties["containers"].Items!.ValueKind.ShouldBe(ResourceEditorValueKind.Object);
        spec.Properties["restartPolicy"].ValueKind.ShouldBe(ResourceEditorValueKind.Enum);
    }

    [AvaloniaFact]
    public async Task Resource_editor_view_model_and_view_render_a_resource_document_and_reset_edits()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default", Labels = new Dictionary<string, string> { ["team"] = "platform" } },
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app", Image = "nginx" }],
            }
        };

        vm.Initialize(cluster, pod);
        var name = vm.EditorRoot!.Children.Single(node => node.Name == "metadata").Children.Single(node => node.Name == "name");
        name.StringValue = "pod-2";
        vm.IsDirty.ShouldBeTrue();

        var view = new ResourceEditorView { ViewModel = vm };
        using var window = Application.Current.CreateTestWindow(900, 700, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<ExpandableSection>().ShouldNotBeEmpty();
        var toolbar = view.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Children.OfType<Button>().Count() == 3);
        toolbar.Spacing.ShouldBe(0);
        foreach (var button in toolbar.Children.OfType<Button>())
            button.Content.ShouldBeOfType<FluentIcon>();
        view.GetVisualDescendants().OfType<Button>().ShouldContain(button =>
            Equals(ToolTip.GetTip(button), KubeUI.Avalonia.Assets.Resources.ResourceEditorView_Save));
        view.GetVisualDescendants().OfType<Button>().ShouldContain(button =>
            Equals(ToolTip.GetTip(button), KubeUI.Avalonia.Assets.Resources.ResourceEditorView_Validate));

        vm.ResetCommand.Execute(null);

        vm.IsDirty.ShouldBeFalse();
        vm.Document!.Root["metadata"]!["name"]!.GetValue<string>().ShouldBe("pod-1");

        await vm.ValidateNowCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        vm.ValidationMessage.ShouldBe(KubeUI.Avalonia.Assets.Resources.ResourceEditorView_ValidationSucceeded);
        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(KubeUI.Avalonia.Assets.Resources.ResourceEditorView_ValidationSucceeded);
        var actionBar = view.GetVisualDescendants().OfType<FAInfoBar>().Single();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Title.ShouldBe(KubeUI.Avalonia.Assets.Resources.ResourceEditorView_ValidationSucceeded);
    }

    [AvaloniaFact]
    public async Task UI_editor_shows_local_validation_error_immediately_after_edit()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeNew(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec
            {
                ActiveDeadlineSeconds = 30,
                Containers = [new V1Container { Name = "app", Image = "nginx" }],
            },
        });

        var spec = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        var deadline = spec.Children.Single(node => node.Name == "activeDeadlineSeconds");
        deadline.NumberValue = "not-a-number";

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultMessage.ShouldContain("activeDeadlineSeconds");
        deadline.HasValidationError.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task UI_editor_clears_validation_error_when_optional_value_is_emptied()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeNew(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
                CreationTimestamp = DateTime.UtcNow,
            },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        });

        var metadata = vm.EditorRoot!.Children.Single(node => node.Name == "metadata");
        var creationTimestamp = metadata.Children.Single(node => node.Name == "creationTimestamp");
        var view = new ResourceEditorView { ViewModel = vm };
        using var window = Application.Current.CreateTestWindow(900, 700, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var creationControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, creationTimestamp));
        var textBox = creationControl.GetVisualDescendants().OfType<TextBox>().Single();
        textBox.Text = "abcd";
        textBox.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var clickPoint = textBox.TranslatePoint(
            new Point(textBox.Bounds.Width / 2, textBox.Bounds.Height / 2),
            window);
        clickPoint.ShouldNotBeNull();
        window.MouseDown(clickPoint.Value, MouseButton.Left);
        window.MouseUp(clickPoint.Value, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        textBox.IsFocused.ShouldBeTrue();
        textBox.Text.ShouldBe("abcd");
        await vm.ValidateNowCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        vm.HasActionFailureResult.ShouldBeTrue();

        for (var index = 0; index < 4; index++)
            window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);

        creationTimestamp.StringValue.ShouldBe(string.Empty);
        vm.ValidationErrors.ShouldBeEmpty();
        vm.HasActionFailureResult.ShouldBeFalse();
        vm.ErrorMessage.ShouldBeNull();
        creationTimestamp.HasValidationError.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task UI_editor_validate_reports_server_validation_errors_for_an_invalid_new_pod()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeNew(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "temp", NamespaceProperty = "default" },
        });

        await vm.ValidateNowCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.ValidationMessage.ShouldBeNull();
        vm.ErrorMessage.ShouldContain("containers");
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(KubeUI.Avalonia.Assets.Resources.ResourceEditorView_ValidationFailed);
        vm.ActionResultMessage.ShouldContain("Pod spec.containers must contain at least one container.");
        vm.ActionResultMessage.ShouldNotContain("Operation returned an invalid status code");

        var spec = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        var containers = spec.Children.Single(node => node.Name == "containers");
        containers.HasValidationError.ShouldBeTrue();

        var view = new ResourceEditorView { ViewModel = vm };
        using var window = Application.Current.CreateTestWindow(900, 700, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var containersControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, containers));
        containersControl.GetVisualDescendants().OfType<Border>()
            .Any(border => border.BorderBrush is not null && border.BorderBrush != Brushes.Transparent)
            .ShouldBeTrue();
        view.GetVisualDescendants().OfType<TextBlock>()
            .Any(text => text.Text is not null && text.Text.Contains("spec.containers: ", StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Launcher_opens_one_editor_document_and_reuses_it()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var launcher = Application.Current.GetRequiredTestService<IResourceEditorLauncher>();
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        };

        launcher.Open(cluster, pod);
        var editor = documents.VisibleDockables!.OfType<ResourceEditorViewModel>().Single();
        try
        {
            launcher.Open(cluster, pod);

            documents.VisibleDockables.OfType<ResourceEditorViewModel>().ShouldBe([editor]);
            documents.ActiveDockable.ShouldBeSameAs(editor);
        }
        finally
        {
            factory.RemoveDockable(editor, collapse: false);
            editor.Dispose();
        }
    }

    [AvaloniaFact]
    public async Task Launcher_opens_a_new_resource_editor_document()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var launcher = Application.Current.GetRequiredTestService<IResourceEditorLauncher>();
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "temp", NamespaceProperty = "default" },
        };

        launcher.OpenNew(cluster, pod);

        var editor = documents.VisibleDockables!.OfType<ResourceEditorViewModel>().Single();
        try
        {
            editor.IsCreateMode.ShouldBeTrue();
            editor.Title.ShouldBe("Create Pod");
            editor.Object.ShouldBeSameAs(pod);
        }
        finally
        {
            factory.RemoveDockable(editor, collapse: false);
            editor.Dispose();
        }
    }
}
