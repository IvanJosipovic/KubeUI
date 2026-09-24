using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Testing.Kubernetes.Bootstrap;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;
using ConfigMapPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap.PropertiesView;
using SecretPropertiesView = KubeUI.Avalonia.Resources.Configuration.v1.Secret.PropertiesView;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class DataDisplayTests
{
    [AvaloniaFact]
    public async Task config_map_properties_render_data_values()
    {
        var resource = CreateConfigMap();
        var view = new ConfigMapPropertiesView
        {
            DataContext = resource,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        GetDisplayedTexts(view).ShouldContain("config-value");
        GetDisplayedTexts(view).ShouldContain("remove-value");
        view.GetVisualDescendants()
            .OfType<ExpandableSection>()
            .ShouldContain(section => Equals(section.Header, AppResources.Shared_Data));
    }

    [AvaloniaFact]
    public async Task secret_properties_render_data_values_and_certificates()
    {
        var resource = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "secret", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]>
            {
                ["config"] = Encoding.UTF8.GetBytes("secret-value"),
            },
        };
        var view = new SecretPropertiesView
        {
            DataContext = resource,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        GetDisplayedTexts(view).ShouldContain("secret-value");
        view.GetVisualDescendants()
            .OfType<ExpandableSection>()
            .ShouldContain(section => Equals(section.Header, AppResources.SecretPropertiesView_Certificates));
    }

    [AvaloniaFact]
    public async Task secret_properties_render_reserved_text_without_prefix()
    {
        var resource = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "secret", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]>
            {
                ["config"] = Encoding.UTF8.GetBytes("base64:secret"),
            },
        };
        var view = new SecretPropertiesView
        {
            DataContext = resource,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        GetDisplayedTexts(view).ShouldContain("base64:secret");
        GetDisplayedTexts(view).ShouldNotContain("text:base64:secret");
    }

    [AvaloniaFact]
    public async Task secret_properties_show_valid_certificate_details()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=test",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(5));
        var resource = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "secret", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]>
            {
                ["tls.crt"] = certificate.Export(X509ContentType.Cert),
            },
        };
        var view = new SecretPropertiesView
        {
            DataContext = resource,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var certificateItem = view.GetVisualDescendants().OfType<CertificateItemView>().Single();
        certificateItem.Certificates.ShouldHaveSingleItem();
        certificateItem.GetVisualDescendants()
            .OfType<ExpandableSection>()
            .Single()
            .IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task edit_is_hidden_without_patch_permission()
    {
        var resource = CreateConfigMap();
        using var workspace = await CreateDeniedWorkspaceAsync(resource);
        var view = CreateResourcePropertiesView(workspace, resource);
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await WaitForUiAsync();

        var display = FindDisplay<V1ConfigMap, string>(view);

        display.ViewModel.CanEdit.ShouldBeFalse();
        display.ViewModel.BeginEditCommand.CanExecute(null).ShouldBeFalse();
        display.GetVisualDescendants()
            .OfType<Button>()
            .ShouldNotContain(button => button.IsVisible && Equals(ToolTip.GetTip(button), AppResources.DataDisplay_Edit));
    }

    [AvaloniaFact]
    public async Task add_remove_cancel_keeps_live_object_unchanged()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);
        await WaitForUiAsync();

        display.ViewModel.BeginEditCommand.Execute(null);
        var originalRow = display.ViewModel.Rows.Single(row => row.Key == "config");
        originalRow.Value = "draft-value";
        display.ViewModel.AddCommand.Execute(null);
        var addedRow = display.ViewModel.Rows.Last();
        addedRow.Key = "added";
        addedRow.Value = "added-value";
        display.ViewModel.RemoveCommand.Execute(display.ViewModel.Rows.Single(row => row.Key == "remove"));

        display.ViewModel.CancelCommand.Execute(null);

        display.ViewModel.EditMode.ShouldBeFalse();
        resource.Data.ShouldContainKeyAndValue("config", "config-value");
        resource.Data.ShouldContainKeyAndValue("remove", "remove-value");
        resource.Data.ShouldNotContainKey("added");
        display.ViewModel.Rows.Select(row => (row.Key, row.Value)).ShouldBe([
            ("config", "config-value"),
            ("remove", "remove-value"),
        ]);
        harness.Recorder.Requests.ShouldNotContain(request => request.Method == HttpMethod.Patch);
    }

    [AvaloniaFact]
    public async Task edit_mode_uses_editable_text_boxes_and_existing_keys_are_editable()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);
        using var window = Application.Current.CreateTestWindow(content: display);

        window.Show();
        await WaitForUiAsync();

        display.GetVisualDescendants()
            .OfType<Button>()
            .ShouldContain(button => button.IsVisible && Equals(ToolTip.GetTip(button), AppResources.DataDisplay_Edit));

        display.ViewModel.BeginEditCommand.Execute(null);
        await WaitForUiAsync();

        display.GetVisualDescendants().OfType<TextBox>().ShouldNotBeEmpty();
        display.GetVisualDescendants().OfType<TextBox>().ShouldAllBe(editor => !editor.IsReadOnly);
        var visibleEditors = display.GetVisualDescendants().OfType<TextBox>().Where(editor => editor.IsVisible).ToArray();
        visibleEditors.Length.ShouldBe(display.ViewModel.Rows.Count * 2);
        visibleEditors.ShouldAllBe(editor => editor.Background == Brushes.Transparent);
        visibleEditors.ShouldAllBe(editor => editor.BorderBrush == Brushes.Transparent);
        visibleEditors.ShouldAllBe(editor => editor.BorderThickness == new Thickness(0));
        visibleEditors.ShouldAllBe(editor => editor.FocusAdorner == null);
        visibleEditors.ShouldAllBe(editor => editor.TextWrapping == TextWrapping.NoWrap);
        visibleEditors.ShouldAllBe(editor => ScrollViewer.GetHorizontalScrollBarVisibility(editor) == ScrollBarVisibility.Auto);
        visibleEditors.ShouldAllBe(editor => ScrollViewer.GetVerticalScrollBarVisibility(editor) == ScrollBarVisibility.Auto);
        var keyEditor = display.GetVisualDescendants().OfType<TextBox>().Single(editor => editor.Text == "config");
        keyEditor.Text = "renamed";
        await WaitForUiAsync();
        display.ViewModel.Rows.Single(row => row.Key == "renamed").Key.ShouldBe("renamed");
        var valueEditor = display.GetVisualDescendants().OfType<TextBox>().Single(editor => editor.Text == "config-value");
        valueEditor.Text = "typed-value";
        await WaitForUiAsync();
        display.ViewModel.Rows.Single(row => row.Key == "renamed").Value.ShouldBe("typed-value");

        display.ViewModel.AddCommand.Execute(null);
        await WaitForUiAsync();

        display.GetVisualDescendants().OfType<TextBox>().Count(editor => editor.IsVisible).ShouldBe(display.ViewModel.Rows.Count * 2);
    }

    [AvaloniaFact]
    public async Task view_mode_keeps_selectable_text_and_edit_mode_hides_it()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);
        using var window = Application.Current.CreateTestWindow(content: display);

        window.Show();
        await WaitForUiAsync();

        var viewEditors = display.GetVisualDescendants()
            .OfType<TextBox>()
            .Where(editor => editor.IsVisible)
            .ToArray();
        var viewTextBlocks = display.GetVisualDescendants()
            .OfType<SelectableTextBlock>()
            .Where(textBlock => textBlock.IsVisible)
            .ToArray();

        viewEditors.ShouldBeEmpty();
        viewTextBlocks.Length.ShouldBe(display.ViewModel.Rows.Count * 2);

        display.ViewModel.BeginEditCommand.Execute(null);
        await WaitForUiAsync();

        var editEditors = display.GetVisualDescendants()
            .OfType<TextBox>()
            .Where(editor => editor.IsVisible)
            .ToArray();
        var editTextBlocks = display.GetVisualDescendants()
            .OfType<SelectableTextBlock>()
            .Where(textBlock => textBlock.IsVisible)
            .ToArray();

        editEditors.Length.ShouldBe(display.ViewModel.Rows.Count * 2);
        editEditors.ShouldAllBe(editor => !editor.IsReadOnly);
        editTextBlocks.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task focused_editors_keep_their_normal_appearance()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);
        using var window = Application.Current.CreateTestWindow(content: display);

        window.Show();
        await WaitForUiAsync();

        display.ViewModel.BeginEditCommand.Execute(null);
        await WaitForUiAsync();

        var editor = display.GetVisualDescendants().OfType<TextBox>().Single(textBox => textBox.Text == "config");
        editor.Focus().ShouldBeTrue();
        await WaitForUiAsync();

        var border = editor.GetVisualDescendants()
            .OfType<Border>()
            .Single(element => element.Name == "PART_BorderElement");

        border.Background.ShouldBe(Brushes.Transparent);
        border.BorderBrush.ShouldBe(Brushes.Transparent);
        border.BorderThickness.ShouldBe(new Thickness(0));
    }

    [AvaloniaFact]
    public async Task empty_or_duplicate_new_key_disables_save()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);

        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.AddCommand.Execute(null);
        var addedRow = display.ViewModel.Rows.Last();

        display.ViewModel.CanSave.ShouldBeFalse();
        addedRow.Key = "config";
        display.ViewModel.CanSave.ShouldBeFalse();
        addedRow.Key = "added";
        addedRow.Value = "value";
        display.ViewModel.CanSave.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task config_map_save_patches_only_data_and_removes_keys()
    {
        var resource = CreateConfigMap();
        resource.BinaryData = new Dictionary<string, byte[]>
        {
            ["binary"] = [1, 2, 3],
        };
        resource.Immutable = true;
        resource.Metadata.Labels = new Dictionary<string, string>
        {
            ["keep"] = "metadata",
        };
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);

        display.ViewModel.BeginEditCommand.Execute(null);
        var configRow = display.ViewModel.Rows.Single(row => row.Key == "config");
        configRow.Key = "renamed";
        configRow.Value = "updated-value";
        display.ViewModel.AddCommand.Execute(null);
        var addedRow = display.ViewModel.Rows.Last();
        addedRow.Key = "added";
        addedRow.Value = "added-value";
        display.ViewModel.RemoveCommand.Execute(display.ViewModel.Rows.Single(row => row.Key == "remove"));

        await display.ViewModel.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        display.ViewModel.EditMode.ShouldBeTrue();
        display.ViewModel.HasActionSuccessResult.ShouldBeTrue();
        var patchRequest = harness.Recorder.Requests.Single(request => request.Method == HttpMethod.Patch);
        var patch = JsonNode.Parse(patchRequest.Body!).AsObject();
        patch.Count.ShouldBe(1);
        var data = patch["data"].AsObject();
        data["config"].ShouldBeNull();
        data["renamed"]!.GetValue<string>().ShouldBe("updated-value");
        data["added"]!.GetValue<string>().ShouldBe("added-value");
        data["remove"].ShouldBeNull();

        using var client = workspace.Runtime.Client!.GetGenericClient<V1ConfigMap>();
        var saved = await client.ReadNamespacedAsync<V1ConfigMap>("default", "config", TestContext.Current.CancellationToken);
        saved.Data.ShouldContainKeyAndValue("renamed", "updated-value");
        saved.Data.ShouldNotContainKey("config");
        saved.Data.ShouldContainKeyAndValue("added", "added-value");
        saved.Data.ShouldNotContainKey("remove");
        saved.BinaryData.ShouldNotBeNull();
        saved.BinaryData["binary"].ShouldBe([1, 2, 3]);
        saved.Immutable.ShouldBe(true);
        saved.Metadata.Labels.ShouldContainKeyAndValue("keep", "metadata");
        saved.Metadata.Name.ShouldBe(resource.Metadata.Name);
        saved.Metadata.NamespaceProperty.ShouldBe(resource.Metadata.NamespaceProperty);
    }

    [AvaloniaFact]
    public async Task secret_save_encodes_utf8_data_as_base64()
    {
        var resource = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "secret", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]>
            {
                ["greeting"] = Encoding.UTF8.GetBytes("hello"),
            },
        };
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);

        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.Rows.Single(row => row.Key == "greeting").Value = "snowman ☃";

        await display.ViewModel.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        var patchRequest = harness.Recorder.Requests.Single(request => request.Method == HttpMethod.Patch);
        var data = JsonNode.Parse(patchRequest.Body!)!["data"]!.AsObject();
        data["greeting"]!.GetValue<string>().ShouldBe(
            Convert.ToBase64String(Encoding.UTF8.GetBytes("snowman ☃")));

        using var client = workspace.Runtime.Client!.GetGenericClient<V1Secret>();
        var saved = await client.ReadNamespacedAsync<V1Secret>("default", "secret", TestContext.Current.CancellationToken);
        Encoding.UTF8.GetString(saved.Data!["greeting"]).ShouldBe("snowman ☃");
    }

    [AvaloniaFact]
    public async Task secret_renaming_invalid_utf8_value_preserves_original_bytes()
    {
        byte[] originalBytes = [0xFF];
        var resource = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "secret", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]>
            {
                ["binary"] = originalBytes,
            },
        };
        var harness = await CreateAdminWorkspaceAsync(resource);
        using var workspace = harness.Workspace;
        var view = new SecretPropertiesView
        {
            DataContext = resource,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await WaitForUiAsync();
        var display = FindDisplay<V1Secret, byte[]>(view);
        display.Initialize(workspace);
        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.Rows.Single(row => row.Key == "binary").Key = "renamed";

        await display.ViewModel.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        var patchRequest = harness.Recorder.Requests.Single(request => request.Method == HttpMethod.Patch);
        var patchValue = JsonNode.Parse(patchRequest.Body!)!["data"]!["renamed"]!.GetValue<string>();
        Convert.FromBase64String(patchValue).ShouldBe(originalBytes);

        using var client = workspace.Runtime.Client!.GetGenericClient<V1Secret>();
        var saved = await client.ReadNamespacedAsync<V1Secret>("default", "secret", TestContext.Current.CancellationToken);
        saved.Data!["renamed"].ShouldBe(originalBytes);
        saved.Data.ShouldNotContainKey("binary");
    }

    [AvaloniaFact]
    public async Task save_response_for_previous_resource_does_not_replace_refreshed_resource()
    {
        var resource = CreateConfigMap();
        var refreshedResource = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = "other", NamespaceProperty = "default" },
            Data = new Dictionary<string, string>
            {
                ["current"] = "current-value",
            },
        };
        var patchGate = new BlockingPatchGate();
        var harness = await CreateAdminWorkspaceAsync(resource, () => new BlockingPatchHandler(patchGate));
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);
        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.Rows.Single(row => row.Key == "config").Value = "saved-value";

        var saveTask = display.ViewModel.SaveCommand.ExecuteAsync(null);
        await patchGate.RequestReceived.WaitAsync(TestContext.Current.CancellationToken);

        display.Refresh(refreshedResource);
        display.ViewModel.Resource.ShouldBeSameAs(refreshedResource);
        display.ViewModel.Rows.Select(row => (row.Key, row.Value)).ShouldBe([
            ("current", "current-value"),
        ]);

        patchGate.Release();
        await saveTask.WaitAsync(TestContext.Current.CancellationToken);

        display.ViewModel.Resource.ShouldBeSameAs(refreshedResource);
        display.ViewModel.EditMode.ShouldBeFalse();
        display.ViewModel.Rows.Select(row => (row.Key, row.Value)).ShouldBe([
            ("current", "current-value"),
        ]);
    }

    [AvaloniaFact]
    public async Task merge_patch_initializes_missing_object_targets_before_removing_nested_values()
    {
        var resource = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = "config", NamespaceProperty = "default" },
        };
        using var workspace = (await CreateAdminWorkspaceAsync(resource)).Workspace;
        using var client = workspace.Runtime.Client!.GetGenericClient<V1ConfigMap>();

        var saved = await client.PatchNamespacedAsync<V1ConfigMap>(
            new V1Patch("{\"data\":{\"obsolete\":null}}", V1Patch.PatchType.MergePatch),
            "default",
            "config",
            TestContext.Current.CancellationToken);

        saved.Data.ShouldNotBeNull();
        saved.Data.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task failed_save_keeps_edit_mode_and_draft()
    {
        var resource = CreateConfigMap();
        var harness = await CreateAdminWorkspaceAsync(resource, static () => new PatchFailureHandler());
        using var workspace = harness.Workspace;
        var display = CreateDisplay(resource);
        display.Initialize(workspace);

        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.Rows.Single(row => row.Key == "config").Value = "draft-value";

        await display.ViewModel.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        display.ViewModel.EditMode.ShouldBeTrue();
        display.ViewModel.Rows.Single(row => row.Key == "config").Value.ShouldBe("draft-value");
        display.ViewModel.HasActionFailureResult.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task same_resource_refresh_preserves_draft_until_cancel()
    {
        var resource = CreateConfigMap();
        using var workspace = (await CreateAdminWorkspaceAsync(resource)).Workspace;
        var services = Application.Current.GetTestServices();
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1ConfigMap>>();
        viewModel.Initialize(workspace, resource);
        var view = new ResourcePropertiesView<V1ConfigMap>
        {
            DataContext = viewModel,
        };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await WaitForUiAsync();
        var display = FindDisplay<V1ConfigMap, string>(view);
        display.ViewModel.BeginEditCommand.Execute(null);
        display.ViewModel.Rows.Single(row => row.Key == "config").Value = "local-draft";

        viewModel.Object = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = "config", NamespaceProperty = "default" },
            Data = new Dictionary<string, string>
            {
                ["config"] = "server-value",
                ["new-server-key"] = "server-addition",
            },
        };
        await WaitForUiAsync();
        await WaitForUiAsync();

        var refreshedDisplay = FindDisplay<V1ConfigMap, string>(view);
        refreshedDisplay.ShouldBeSameAs(display);
        refreshedDisplay.ViewModel.EditMode.ShouldBeTrue();
        refreshedDisplay.ViewModel.Rows.Single(row => row.Key == "config").Value.ShouldBe("local-draft");

        refreshedDisplay.ViewModel.CancelCommand.Execute(null);
        refreshedDisplay.ViewModel.Rows.Select(row => (row.Key, row.Value)).ShouldBe([
            ("config", "server-value"),
            ("new-server-key", "server-addition"),
        ]);
    }

    private static V1ConfigMap CreateConfigMap()
    {
        return new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = "config", NamespaceProperty = "default" },
            Data = new Dictionary<string, string>
            {
                ["config"] = "config-value",
                ["remove"] = "remove-value",
            },
        };
    }

    private static ResourcePropertiesView<V1ConfigMap> CreateResourcePropertiesView(ClusterWorkspace workspace, V1ConfigMap resource)
    {
        var services = Application.Current.GetTestServices();
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1ConfigMap>>();
        viewModel.Initialize(workspace, resource);
        return new ResourcePropertiesView<V1ConfigMap>
        {
            DataContext = viewModel,
        };
    }

    private static DataDisplay<V1ConfigMap, string> CreateDisplay(V1ConfigMap resource)
    {
        return new DataDisplay<V1ConfigMap, string>(
            resource,
            GroupApiVersionKind.From<V1ConfigMap>(),
            static item => item.Data,
            static value => value,
            static value => value);
    }

    private static DataDisplay<V1Secret, byte[]> CreateDisplay(V1Secret resource)
    {
        return new DataDisplay<V1Secret, byte[]>(
            resource,
            GroupApiVersionKind.From<V1Secret>(),
            static item => item.Data,
            static value => Encoding.UTF8.GetString(value),
            static value => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
    }

    private static DataDisplay<TResource, TValue> FindDisplay<TResource, TValue>(Control root)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return root.GetVisualDescendants()
            .OfType<DataDisplay<TResource, TValue>>()
            .Single();
    }

    private static string[] GetDisplayedTexts(Control root)
    {
        return root.GetVisualDescendants()
            .OfType<SelectableTextBlock>()
            .Where(block => block.IsVisible)
            .Select(block => block.Text)
            .Where(text => text is not null)
            .Select(text => text!)
            .Concat(root.GetVisualDescendants()
                .OfType<TextBox>()
                .Where(textBox => textBox.IsVisible)
                .Select(textBox => textBox.Text)
                .Where(text => text is not null)
                .Select(text => text!))
            .ToArray();
    }

    private static async Task<ClusterWorkspace> CreateDeniedWorkspaceAsync(V1ConfigMap resource)
    {
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.HttpHandlerFactory = null;
            config.HttpHandlers = [];
            config.InitialResources = new[]
                {
                    (IKubernetesObject<V1ObjectMeta>)new V1Namespace { Metadata = new V1ObjectMeta { Name = "default" } },
                    resource,
                }
                .Concat(KubernetesRbac.ClusterWide(
                    new RbacRule("namespaces", "list"),
                    new RbacRule("namespaces", "watch")))
                .Concat(KubernetesRbac.InNamespace(
                    "default",
                    new RbacRule("configmaps", "get"),
                    new RbacRule("configmaps", "list"),
                    new RbacRule("configmaps", "watch")))
                .ToArray();
        });

        await ((Cluster)workspace.Runtime).UpdateCanI<V1ConfigMap>(Verb.Patch, "default");
        return workspace;
    }

    private static async Task<(ClusterWorkspace Workspace, RequestRecorder Recorder)> CreateAdminWorkspaceAsync<TResource>(
        TResource resource,
        Func<DelegatingHandler>? additionalHandlerFactory = null)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var recorder = new RequestRecorder();
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.AuthenticatedUser = "system:admin";
            config.InitialResources = [resource];
            config.HttpHandlers = [];
            config.HttpHandlerFactory = () =>
            {
                List<DelegatingHandler> handlers = [new RecordingHandler(recorder)];
                if (additionalHandlerFactory is not null)
                {
                    handlers.Add(additionalHandlerFactory());
                }

                return handlers;
            };
        });

        workspace.Runtime.Client.ShouldNotBeNull($"status={workspace.Runtime.Status}; connected={workspace.Runtime.Connected}; error={workspace.Runtime.LastError}");
        return (workspace, recorder);
    }

    private static async Task WaitForUiAsync()
    {
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
    }

    private sealed record RequestRecord(HttpMethod Method, Uri? Uri, string? Body);

    private sealed class RequestRecorder
    {
        private readonly ConcurrentQueue<RequestRecord> _requests = new();

        public IReadOnlyList<RequestRecord> Requests => _requests.ToArray();

        internal void Add(RequestRecord request)
        {
            _requests.Enqueue(request);
        }
    }

    private sealed class RecordingHandler : DelegatingHandler
    {
        private readonly RequestRecorder _recorder;

        internal RecordingHandler(RequestRecorder recorder)
        {
            _recorder = recorder;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _recorder.Add(new RequestRecord(request.Method, request.RequestUri, body));
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class PatchFailureHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Patch)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(
                        """{"kind":"Status","status":"Failure","message":"patch denied","code":403}""",
                        Encoding.UTF8,
                        "application/json"),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class BlockingPatchHandler : DelegatingHandler
    {
        private readonly BlockingPatchGate _gate;

        internal BlockingPatchHandler(BlockingPatchGate gate)
        {
            _gate = gate;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Patch)
            {
                _gate.RequestReceivedSource.TrySetResult(true);
                await _gate.ReleaseSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            _gate.ReleaseSource.TrySetResult(true);
            base.Dispose(disposing);
        }
    }

    private sealed class BlockingPatchGate
    {
        internal TaskCompletionSource<bool> RequestReceivedSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal TaskCompletionSource<bool> ReleaseSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal Task RequestReceived => RequestReceivedSource.Task;

        internal void Release()
        {
            ReleaseSource.TrySetResult(true);
        }
    }
}
