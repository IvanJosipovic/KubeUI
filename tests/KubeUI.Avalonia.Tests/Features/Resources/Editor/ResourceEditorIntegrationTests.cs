using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Editor;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes.Resources.Editor;
using KubeUI.Testing.Kubernetes.Bootstrap;
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
        spec.Properties["nodeSelector"].ValueKind.ShouldBe(ResourceEditorValueKind.MapKind);
        spec.Properties["nodeSelector"].Items.ShouldNotBeNull();
        spec.Properties["containers"].ValueKind.ShouldBe(ResourceEditorValueKind.ArrayKind);
        spec.Properties["containers"].Items!.ValueKind.ShouldBe(ResourceEditorValueKind.ObjectKind);
        spec.Properties["restartPolicy"].ValueKind.ShouldBe(ResourceEditorValueKind.EnumKind);
    }

    [AvaloniaFact]
    public void OneOf_schema_branches_do_not_require_mutually_exclusive_fields()
    {
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
                                OneOf = new List<IOpenApiSchema>
                                {
                                    new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        Required = new HashSet<string>(["alpha"], StringComparer.Ordinal),
                                        Properties = new Dictionary<string, IOpenApiSchema>
                                        {
                                            ["alpha"] = new OpenApiSchema { Type = JsonSchemaType.String },
                                        },
                                    },
                                    new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        Required = new HashSet<string>(["beta"], StringComparer.Ordinal),
                                        Properties = new Dictionary<string, IOpenApiSchema>
                                        {
                                            ["beta"] = new OpenApiSchema { Type = JsonSchemaType.String },
                                        },
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
        var spec = root.Properties["spec"];

        spec.ValueKind.ShouldBe(ResourceEditorValueKind.ObjectKind);
        spec.Properties.Keys.ShouldBe(["alpha", "beta"], ignoreOrder: true);
        spec.Required.ShouldBeEmpty("oneOf branches are alternatives, so only one branch may be required at a time");
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

        var view = new ResourceEditorView { DataContext = vm };
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
            Equals(ToolTip.GetTip(button), Assets.Resources.ResourceEditorView_Save));
        view.GetVisualDescendants().OfType<Button>().ShouldContain(button =>
            Equals(ToolTip.GetTip(button), Assets.Resources.ResourceEditorView_Validate));

        vm.ResetCommand.Execute(null);

        vm.IsDirty.ShouldBeFalse();
        vm.Document!.Root["metadata"]!["name"]!.GetValue<string>().ShouldBe("pod-1");

        await vm.ValidateNowCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        vm.ValidationMessage.ShouldBe(Assets.Resources.ResourceEditorView_ValidationSucceeded);
        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(Assets.Resources.ResourceEditorView_ValidationSucceeded);
        var actionBar = view.GetVisualDescendants().OfType<FAInfoBar>().Single();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Title.ShouldBe(Assets.Resources.ResourceEditorView_ValidationSucceeded);
    }

    [AvaloniaFact]
    public async Task Saving_rebuilds_the_editor_tree_so_later_edits_are_kept()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        };

        vm.Initialize(cluster, pod);
        var name = vm.EditorRoot!.Children.Single(node => node.Name == "metadata").Children.Single(node => node.Name == "name");
        name.StringValue = "pod-2";

        await vm.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.IsDirty.ShouldBeFalse();
        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(Assets.Resources.ResourceEditorView_SaveSucceeded);

        var nameAfterSave = vm.EditorRoot!.Children.Single(node => node.Name == "metadata").Children.Single(node => node.Name == "name");
        nameAfterSave.StringValue = "pod-3";

        vm.IsDirty.ShouldBeTrue("an edit made after a save must mark the document as dirty");
        vm.Document!.Root["metadata"]!["name"]!.GetValue<string>().ShouldBe("pod-3");
        vm.CanSave.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Clean_editor_refreshes_its_document_when_the_cluster_resource_changes()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        await cluster.Runtime.SeedResource<V1Pod>(true).WaitAsync(TestContext.Current.CancellationToken);
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        };
        await cluster.Runtime.AddOrUpdateResource(pod).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => cluster.Runtime.GetResource<V1Pod>("default", "pod-1")?.Metadata?.Uid is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.Initialize(cluster, pod);
        var previousEditorRoot = vm.EditorRoot;
        var updatedPod = Utilities.CloneObject(pod);
        updatedPod.Metadata.Labels = new Dictionary<string, string> { ["source"] = "watch" };
        await cluster.Runtime.AddOrUpdateResource(updatedPod).WaitAsync(TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => vm.Object?.Metadata?.Labels?.TryGetValue("source", out var value) == true && value == "watch",
            timeout: TimeSpan.FromSeconds(5),
            pollInterval: TimeSpan.FromMilliseconds(25),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        vm.Document!.Root["metadata"]!["labels"]!["source"]!.GetValue<string>().ShouldBe("watch");
        vm.IsDirty.ShouldBeFalse();
        vm.EditorRoot.ShouldNotBeSameAs(previousEditorRoot);
    }

    [AvaloniaFact]
    public async Task Saving_a_created_resource_records_the_server_assigned_identity()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeForCreate(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "temp", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        });

        await vm.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.IsCreateMode.ShouldBeFalse();
        vm.HasActionFailureResult.ShouldBeFalse();
        vm.ErrorMessage.ShouldBeNull();
        vm.Document!.Root["metadata"]!["uid"]!.GetValue<string>().ShouldNotBeNullOrEmpty();
    }

    [AvaloniaFact]
    public async Task A_failed_save_reports_the_server_error_to_the_user()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        var previousHandlerFactory = config.HttpHandlerFactory;
        var writeGate = new WriteRejectionGate();
        config.HttpHandlerFactory = () => [new RejectWriteHandler(writeGate)];

        using var cluster = await Application.Current.CreateClusterAsync();
        config.HttpHandlerFactory = previousHandlerFactory;
        await cluster.Runtime.EnsureOpenApiSchemasAsync().WaitAsync(TestContext.Current.CancellationToken);

        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        });

        var name = vm.EditorRoot!.Children.Single(node => node.Name == "metadata").Children.Single(node => node.Name == "name");
        name.StringValue = "pod-2";

        // The write is rejected by the transport, so the editor must report the failure.
        writeGate.RejectWrites = true;
        await vm.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(Assets.Resources.ResourceEditorView_SaveConflict);
        vm.ActionResultMessage.ShouldNotBeNullOrWhiteSpace();
        vm.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [AvaloniaFact]
    public async Task A_save_rejected_for_another_reason_reports_a_save_failure()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        var previousHandlerFactory = config.HttpHandlerFactory;
        var writeGate = new WriteRejectionGate();
        writeGate.RejectWrites = true;
        writeGate.RejectionStatusCode = HttpStatusCode.UnprocessableEntity;
        config.HttpHandlerFactory = () => [new RejectWriteHandler(writeGate)];

        using var cluster = await Application.Current.CreateClusterAsync();
        config.HttpHandlerFactory = previousHandlerFactory;
        await cluster.Runtime.EnsureOpenApiSchemasAsync().WaitAsync(TestContext.Current.CancellationToken);

        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        });

        var name = vm.EditorRoot!.Children.Single(node => node.Name == "metadata").Children.Single(node => node.Name == "name");
        name.StringValue = "pod-2";

        await vm.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(Assets.Resources.ResourceEditorView_SaveFailed);
        vm.ActionResultMessage.ShouldNotBeNullOrWhiteSpace();
        vm.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [AvaloniaFact]
    public async Task Root_level_required_errors_are_attached_to_the_affected_node()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        cluster.Runtime.ModelCatalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.Pod"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Required = new HashSet<string>(["spec"], StringComparer.Ordinal),
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["spec"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["replicas"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
                                },
                            },
                        },
                    },
                },
            },
        });

        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "nginx" }] },
        });

        // Removing the required "spec" property from the document must surface a validation
        // error on the node whose path is "spec".
        var specNode = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        specNode.ClearSectionCommand.Execute(null);

        vm.ValidationErrors.ShouldNotBeEmpty();
        vm.ValidationErrors.ShouldAllBe(error => error.Path == "spec");

        var spec = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        spec.HasErrors.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task UI_editor_shows_local_validation_error_immediately_after_edit()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeForCreate(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
                CreationTimestamp = DateTime.UtcNow,
            },
            Spec = new V1PodSpec
            {
                ActiveDeadlineSeconds = 30,
                TerminationGracePeriodSeconds = 30,
                Containers = [new V1Container { Name = "app", Image = "nginx" }],
            },
        });

        var spec = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        var deadline = spec.Children.Single(node => node.Name == "activeDeadlineSeconds");
        var terminationGracePeriod = spec.Children.Single(node => node.Name == "terminationGracePeriodSeconds");
        deadline.NumberValue = "not-a-number";
        terminationGracePeriod.NumberValue = "also-not-a-number";

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultMessage.ShouldContain("activeDeadlineSeconds");
        vm.ActionResultMessage.ShouldContain("terminationGracePeriodSeconds");
        var view = new ResourceEditorView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(900, 700, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var deadlineControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, deadline));
        var deadlineEditor = deadlineControl.GetVisualDescendants().OfType<TextBox>().Single();
        DataValidationErrors.GetHasErrors(deadlineEditor).ShouldBeTrue();
        DataValidationErrors.GetErrors(deadlineEditor).ShouldContain("Enter a valid number.");
    }

    [AvaloniaFact]
    public async Task UI_editor_clears_validation_error_when_optional_value_is_emptied()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeForCreate(cluster, new V1Pod
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
        var view = new ResourceEditorView { DataContext = vm };
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
        textBox.IsFocused.ShouldBeTrue();
        textBox.CaretIndex = textBox.Text.Length;

        for (var index = 0; index < 4; index++)
        {
            window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        }

        creationTimestamp.StringValue.ShouldBe(string.Empty);
        vm.ValidationErrors.ShouldBeEmpty();
        vm.HasActionFailureResult.ShouldBeFalse();
        vm.ErrorMessage.ShouldBeNull();
        DataValidationErrors.GetHasErrors(textBox).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task UI_editor_validate_reports_server_validation_errors_for_an_invalid_new_pod()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var vm = Application.Current.GetRequiredTestService<ResourceEditorViewModel>();
        vm.InitializeForCreate(cluster, new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "temp", NamespaceProperty = "default" },
        });

        await vm.ValidateNowCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        vm.ValidationMessage.ShouldBeNull();
        vm.ErrorMessage.ShouldContain("containers");
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe(Assets.Resources.ResourceEditorView_ValidationFailed);
        vm.ActionResultMessage.ShouldContain("Pod spec.containers must contain at least one container.");
        vm.ActionResultMessage.ShouldNotContain("Operation returned an invalid status code");

        var spec = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        var containers = spec.Children.Single(node => node.Name == "containers");
        containers.HasErrors.ShouldBeTrue();

        var view = new ResourceEditorView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(900, 700, view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var containersControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, containers));
        var section = containersControl.GetVisualDescendants().OfType<ExpandableSection>().Single();
        DataValidationErrors.GetHasErrors(section).ShouldBeTrue();
        DataValidationErrors.GetErrors(section)
            .ShouldContain("Pod spec.containers must contain at least one container.");

        var specNode = vm.EditorRoot!.Children.Single(node => node.Name == "spec");
        var rootControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, vm.EditorRoot));
        var specControl = view.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, specNode));
        var rootOutline = rootControl.Content.ShouldBeOfType<Border>();
        var specOutline = specControl.Content.ShouldBeOfType<Border>();
        var rootSection = rootOutline.Child.ShouldBeOfType<ExpandableSection>();
        var specSection = specOutline.Child.ShouldBeOfType<ExpandableSection>();
        DataValidationErrors.GetHasErrors(rootSection).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(specSection).ShouldBeTrue();
        DataValidationErrors.GetErrors(rootSection)
            .ShouldContain("spec.containers: Pod spec.containers must contain at least one container.");
        DataValidationErrors.GetErrors(specSection)
            .ShouldContain("spec.containers: Pod spec.containers must contain at least one container.");
        rootOutline.BorderThickness.ShouldBe(new Thickness(1));
        specOutline.BorderThickness.ShouldBe(new Thickness(1));
        vm.EditorRoot.HasErrors.ShouldBeTrue();
        specNode.HasErrors.ShouldBeTrue();
        containersControl.GetVisualDescendants().OfType<TextBlock>()
            .Any(text => text.Text is not null && text.Text.Contains("spec.containers: ", StringComparison.Ordinal))
            .ShouldBeFalse();
        view.GetVisualDescendants().OfType<FAInfoBar>().Single().Message
            .ShouldContain("spec.containers");
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

        await launcher.Open(cluster, pod);
        var editor = documents.VisibleDockables!.OfType<ResourceEditorViewModel>().Single();
        try
        {
            await launcher.Open(cluster, pod);

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

        await launcher.OpenForCreate(cluster, pod);

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

    [AvaloniaFact]
    public async Task New_pod_editor_renders_fields_while_openapi_schema_loading_is_in_progress()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        var previousHandlerFactory = config.HttpHandlerFactory;
        var schemaGate = new OpenApiSchemaRequestGate();
        config.HttpHandlerFactory = () => [new OpenApiSchemaRequestGateHandler(schemaGate)];

        ClusterWorkspace? cluster = null;
        ResourceEditorViewModel? editor = null;
        Task? openTask = null;
        var factory = services.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        try
        {
            cluster = await Application.Current.CreateClusterAsync();
            config.HttpHandlerFactory = previousHandlerFactory;
            await schemaGate.RequestStarted.WaitAsync(TestContext.Current.CancellationToken);
            cluster.Runtime.ModelCatalog.OpenApiSchemas.Count.ShouldBe(0);

            var launcher = services.GetRequiredService<IResourceEditorLauncher>();
            openTask = launcher.OpenForCreate(cluster, new V1Pod
            {
                ApiVersion = "v1",
                Kind = "Pod",
                Metadata = new V1ObjectMeta { Name = "temp", NamespaceProperty = "default" },
            });
            openTask.IsCompleted.ShouldBeFalse();
            documents.VisibleDockables!.OfType<ResourceEditorViewModel>().ShouldBeEmpty();

            schemaGate.Release();
            await openTask.WaitAsync(TestContext.Current.CancellationToken);
            editor = documents.VisibleDockables!.OfType<ResourceEditorViewModel>().Single();

            var viewLocator = services.GetRequiredService<ViewLocator>();
            var host = new ContentControl
            {
                Content = editor,
                ContentTemplate = viewLocator,
            };
            using var window = Application.Current.CreateTestWindow(900, 700, host);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var view = host.GetVisualDescendants().OfType<ResourceEditorView>().Single();
            view.DataContext.ShouldBeSameAs(editor);
            view.GetVisualDescendants().OfType<TextBox>().ShouldContain(textBox =>
                AutomationProperties.GetAutomationId(textBox) == "ResourceEditor_metadata.name");
            view.GetVisualDescendants().OfType<ResourceEditorNodeControl>().ShouldContain(control =>
                control.Node.Path == "spec");
        }
        finally
        {
            config.HttpHandlerFactory = previousHandlerFactory;
            schemaGate.Release();
            if (openTask is not null)
                await openTask;
            else if (cluster is not null)
                await cluster.Runtime.EnsureOpenApiSchemasAsync();
            if (editor is not null)
            {
                factory.RemoveDockable(editor, collapse: false);
                editor.Dispose();
            }
            cluster?.Dispose();
        }
    }

    private sealed class OpenApiSchemaRequestGate
    {
        private readonly TaskCompletionSource _requestStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task RequestStarted => _requestStarted.Task;
        public Task WaitForRelease => _release.Task;

        public void MarkRequestStarted() => _requestStarted.TrySetResult();
        public void Release() => _release.TrySetResult();
    }

    private sealed class OpenApiSchemaRequestGateHandler(OpenApiSchemaRequestGate gate) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath == "/openapi/v3")
            {
                gate.MarkRequestStarted();
                await gate.WaitForRelease.WaitAsync(cancellationToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>
    /// Controls whether resource writes are rejected by the transport.
    /// </summary>
    private sealed class WriteRejectionGate
    {
        public bool RejectWrites { get; set; }

        public HttpStatusCode RejectionStatusCode { get; set; } = HttpStatusCode.Conflict;
    }

    /// <summary>
    /// Rejects resource writes so the editor can be observed reporting a failed save.
    /// </summary>
    private sealed class RejectWriteHandler(WriteRejectionGate gate) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (gate.RejectWrites
                && IsResourceWrite(request))
            {
                var response = new HttpResponseMessage(gate.RejectionStatusCode)
                {
                    Content = new StringContent(
                        """{"kind":"Status","status":"Failure","message":"The resource was rejected.","reason":"Invalid","code":422}""",
                        Encoding.UTF8,
                        "application/json"),
                };
                return Task.FromResult(response);
            }

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Determines whether the request writes a resource. Authorization review requests also
        /// use POST, so they must not be treated as resource writes.
        /// </summary>
        private static bool IsResourceWrite(HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Post
                && request.Method != HttpMethod.Put
                && request.Method != HttpMethod.Patch)
            {
                return false;
            }

            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            return !path.StartsWith("/apis/authorization.k8s.io/", StringComparison.Ordinal)
                && !path.StartsWith("/apis/authentication.k8s.io/", StringComparison.Ordinal);
        }
    }
}
