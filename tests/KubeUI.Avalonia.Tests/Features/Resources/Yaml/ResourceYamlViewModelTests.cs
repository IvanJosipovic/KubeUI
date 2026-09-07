using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using k8s.Models;
using KubernetesJson = k8s.KubernetesJson;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Serialization;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public class ResourceYamlViewModelTests
{
    private static async Task WaitForValidationDebounceAsync(Func<bool>? predicate = null, int timeoutMs = 2500)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync();
            if (predicate == null || predicate())
            {
                return;
            }
        }
        while (sw.ElapsedMilliseconds < timeoutMs);

        await TestApplicationExtensions.WaitForUiAsync();
        (predicate?.Invoke() ?? true).ShouldBeTrue();
    }

    private static async Task WaitForUiAsync(Func<bool> predicate, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (await Dispatcher.UIThread.InvokeAsync(predicate))
            {
                return;
            }

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
        }

        (await Dispatcher.UIThread.InvokeAsync(predicate)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Utilities_CloneObject_PreservesNestedTypedResourceFields()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "nginx:1.27",
                        Command = ["/bin/sh", "-c"],
                    },
                ],
            },
        };

        var clone = Utilities.CloneObject(pod);
        clone.ShouldBeOfType<V1Pod>();
        var yaml = KubernetesYaml.Serialize(clone);

        yaml.ShouldContain("spec:");
        yaml.ShouldContain("containers:");
        yaml.ShouldContain("image: nginx:1.27");
        yaml.ShouldContain("command:");
        yaml.ShouldContain("- /bin/sh");
    }

    [Fact]
    public void Utilities_CloneObject_PreservesGenericResourceTypeAndDocument()
    {
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "example.com/v1alpha1",
              "kind": "Widget",
              "metadata": { "name": "test" },
              "spec": { "replicas": 3 }
            }
            """)!;

        var clone = Utilities.CloneObject(resource);

        clone.ShouldBeOfType<GenericKubernetesObject>();
        clone.ShouldNotBeSameAs(resource);
        clone.Properties["spec"].GetProperty("replicas").GetInt32().ShouldBe(3);
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_CreatesFoldingsForNestedMappings()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2:
              prop2Nested:
                prop2NestedProp1: val0
            prop3:
            """.ReplaceLineEndings("\n");


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(2);
        foldings[0].Name.TrimEnd().ShouldBe($"prop2:");
        foldings[1].Name.TrimEnd().ShouldBe($"  prop2Nested:");
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_IdentifiesMultilineNoisyMetadataFields()
    {
        var text = new TextDocument("""
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                example.com/keep: value
                kubectl.kubernetes.io/last-applied-configuration: |
                  {"apiVersion":"v1","kind":"Pod"}
              managedFields:
                - manager: kubectl
                  operation: Apply
            """.ReplaceLineEndings("\n"));

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Select(folding => folding.Name.Trim()).ShouldBe(
        [
            "metadata:",
            "annotations:",
            "kubectl.kubernetes.io/last-applied-configuration: |",
            "managedFields:",
            "- manager: kubectl",
        ]);
        var noisyFoldings = foldings
            .Where(folding => YamlFoldingStrategy.IsNoisyFieldFolding(text, folding))
            .Select(folding => folding.Name.Trim())
            .ToArray();

        noisyFoldings.ShouldBe(
        [
            "kubectl.kubernetes.io/last-applied-configuration: |",
            "managedFields:",
        ]);
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_DoesNotIdentifySingleLineLastAppliedConfiguration()
    {
        var text = new TextDocument("""
            metadata:
              annotations:
                kubectl.kubernetes.io/last-applied-configuration: compact
            """.ReplaceLineEndings("\n"));

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();

        foldings.Any(folding => YamlFoldingStrategy.IsNoisyFieldFolding(text, folding)).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesNoisyFieldsInSerializedYaml()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "v1",
              "kind": "Pod",
              "metadata": {
                "name": "test",
                "managedFields": [{"manager": "kubectl", "operation": "Apply"}],
                "annotations": {
                  "kubectl.kubernetes.io/last-applied-configuration": "compact"
                }
              }
            }
            """)!;
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();

        vm.Initialize(cluster, resource);

        vm.YamlDocument.Text.ShouldContain("managedFields:");
        vm.YamlDocument.Text.ShouldContain("kubectl.kubernetes.io/last-applied-configuration:");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_HideNoisyFieldsToggleControlsNoisyFoldState()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                ManagedFields =
                [
                    new V1ManagedFieldsEntry
                    {
                        Manager = "kubectl",
                        Operation = "Apply",
                    },
                ],
                Annotations = new Dictionary<string, string>
                {
                    ["example.com/keep"] = "value",
                },
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        await WaitForUiAsync(
            () => foldingManager.AllFoldings.Any(folding =>
                YamlFoldingStrategy.IsNoisyFieldFolding(editor.Document!, folding.StartOffset, folding.EndOffset)));

        FoldingSection[] GetNoisyFoldings() => foldingManager.AllFoldings
            .Where(folding => YamlFoldingStrategy.IsNoisyFieldFolding(editor.Document!, folding.StartOffset, folding.EndOffset))
            .ToArray();

        var noisyFoldings = GetNoisyFoldings();
        noisyFoldings.ShouldNotBeEmpty();
        noisyFoldings.ShouldAllBe(folding => folding.IsFolded);

        vm.HideNoisyFields = false;
        await WaitForUiAsync(() => GetNoisyFoldings().All(folding => !folding.IsFolded));

        vm.HideNoisyFields = true;
        await WaitForUiAsync(() => GetNoisyFoldings().All(folding => folding.IsFolded));
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_RepeatedDocumentRefreshesDoNotRetainDetachedFoldingMarkers()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod { Metadata = new V1ObjectMeta { Name = "test" } });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var foldingMargin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        for (var refreshIndex = 0; refreshIndex < 3; refreshIndex++)
        {
            var foldingTitlePrefix = $"value-{refreshIndex}-";
            vm.YamlDocument.Text = string.Join(
                "\n",
                Enumerable.Range(0, 300).Select(i => $"{foldingTitlePrefix}{i}:\n  nested: {i}"));
            await WaitForUiAsync(
                () => foldingManager.AllFoldings.Count() == 300
                    && foldingManager.AllFoldings.All(folding =>
                        folding.Title.StartsWith(foldingTitlePrefix, StringComparison.Ordinal)));
            foldingManager.AllFoldings.Count().ShouldBe(300);

            for (var lineIndex = 0; lineIndex < 200; lineIndex++)
            {
                editor.ScrollToLine(lineIndex);
                await TestApplicationExtensions.WaitForUiAsync();
            }

            ((ILogical)foldingMargin).LogicalChildren
                .Count()
                .ShouldBeLessThan(100);
        }
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_FoldsNewNoisyFieldsImmediatelyWhenObjectRefreshes()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test" },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        vm.Object = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                ManagedFields =
                [
                    new V1ManagedFieldsEntry
                    {
                        Manager = "kubectl",
                        Operation = "Apply",
                    },
                ],
            },
        };

        await WaitForUiAsync(
            () => foldingManager.AllFoldings.Any(folding =>
                YamlFoldingStrategy.IsNoisyFieldFolding(editor.Document!, folding.StartOffset, folding.EndOffset)));

        var noisyFoldings = foldingManager.AllFoldings
            .Where(folding => YamlFoldingStrategy.IsNoisyFieldFolding(
                editor.Document!, folding.StartOffset, folding.EndOffset))
            .ToArray();

        noisyFoldings.ShouldNotBeEmpty();
        noisyFoldings.ShouldAllBe(folding => folding.IsFolded);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesUserFoldingsWhenObjectRefreshes()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, CreatePod("before", includeLabels: false, extraEnv: true));

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        await WaitForUiAsync(
            () => foldingManager.AllFoldings.Any(folding => folding.Title.TrimEnd() == "spec:"));

        foldingManager.AllFoldings.Single(folding => folding.Title.TrimEnd() == "spec:").IsFolded = true;

        vm.Object = CreatePod("after", includeLabels: false, extraEnv: true);

        await WaitForUiAsync(
            () => editor.Document!.Text.Contains("after", StringComparison.Ordinal)
                && foldingManager.AllFoldings.Any(folding => folding.Title.TrimEnd() == "spec:"));

        foldingManager.AllFoldings
            .Single(folding => folding.Title.TrimEnd() == "spec:")
            .IsFolded
            .ShouldBeTrue();
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_CreatesFoldingForMappingWithSequenceChildren()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
            - prop1Nested2:
            - prop1Nested3:
            """.ReplaceLineEndings("\n");


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(1);
        foldings[0].Name.TrimEnd().ShouldBe($"prop1:");
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_CreatesFoldingsForNestedSequences()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
              - prop1Nested1Prop1: val0
              - prop1Nested1Prop2: val1
                prop1Nested1Prop2Nested: val2
            - prop1Nested2:
              - prop1Nested2Prop1: val3
              - prop1Nested2Prop2: val4
            - prop1Nested3:
            """.ReplaceLineEndings("\n");


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(4);
        foldings[0].Name.TrimEnd().ShouldBe($"prop1:");
        foldings[1].Name.TrimEnd().ShouldBe($"- prop1Nested1:");
        foldings[2].Name.TrimEnd().ShouldBe($"  - prop1Nested1Prop2: val1");

        foldings[3].Name.TrimEnd().ShouldBe($"- prop1Nested2:");
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_IgnoresBlankLinesAndComments()
    {
        var text = new TextDocument();
        text.Text = """
            # header

            prop1:
              # comment
              prop1Nested: val

            prop2: val
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(1);
        foldings[0].Name.TrimEnd().ShouldBe($"prop1:");
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_DoesNotCreateFoldingsForFlatMappings()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2: val
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void YamlEditorScrollBehavior_TreatsNearlyEqualOffsetsAsEqual()
    {
        var method = typeof(YamlEditorScrollBehavior).GetMethod("AreOffsetsClose", BindingFlags.Static | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        var closeResult = (bool)method.Invoke(null, [new Vector(10, 20), new Vector(10.25, 20.25)])!;
        closeResult.ShouldBeTrue();

        var farResult = (bool)method.Invoke(null, [new Vector(10, 20), new Vector(11, 20)])!;
        farResult.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_DoesNotCreateFoldingsForListItemsWithoutChildren()
    {
        var text = new TextDocument();
        text.Text = """
            - item1
            - item2
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void YamlFoldingStrategy_CreatesFoldingForListItemWithChildren()
    {
        var text = new TextDocument();
        text.Text = """
            - item1:
              child1: val
            - item2
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(1);
        foldings[0].Name.TrimEnd().ShouldBe($"- item1:");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesFoldState_WhenActiveDockableChanges()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        using var window = Application.Current.CreateTestWindow(content: dockControl);
        window.Show();

        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        });

        var otherDockable = Application.Current.GetRequiredTestService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        vm.YamlDocument.Text = """
            spec:
              nested:
                child: value
            """.ReplaceLineEndings("\n");

        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var editor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, vm), 3000);
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Count().ShouldBeGreaterThan(0);

        foldingManager.AllFoldings.First().IsFolded = true;

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredEditor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, vm), 3000);
        restoredEditor.ShouldNotBeNull();

        behavior = Interaction.GetBehaviors(restoredEditor).OfType<YamlEditorBehavior>().Single();
        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotReplaceUnsavedYaml_WhenResourceUpdatesDuringEdit()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Namespace
            metadata:
              name: local-edit
            """.ReplaceLineEndings("\n");

        await cluster.Runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                Labels = new Dictionary<string, string>
                {
                    ["server-update"] = "true",
                },
            },
        });

        await WaitForUiAsync(
            () => vm.Object?.Metadata.Labels?.ContainsKey("server-update") == true);

        vm.YamlDocument.Text.ShouldContain("name: local-edit");

        vm.EditMode = false;
        await WaitForUiAsync(() => vm.YamlDocument.Text.Contains("server-update", StringComparison.Ordinal));
        vm.YamlDocument.Text.ShouldNotContain("name: local-edit");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesScrollOffset_WhenActiveDockableChanges()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        using var window = Application.Current.CreateTestWindow(content: dockControl);
        window.Show();

        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace { Metadata = new V1ObjectMeta { Name = "test" } });
        vm.YamlDocument.Text = string.Join('\n', Enumerable.Range(0, 400).Select(i => $"line{i}: value"));

        var otherDockable = Application.Current.GetRequiredTestService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var editor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, vm), 3000);
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();

        await WaitForAsync(() =>
        {
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        var targetOffset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        scrollViewer.Offset = targetOffset;
        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => vm.ScrollOffset == targetOffset, 3000);

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.ScrollOffset.ShouldBe(targetOffset);

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredEditor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, vm), 3000);
        restoredEditor.ShouldNotBeNull();

        var restoredScrollViewer = restoredEditor.GetScrollViewer();
        restoredScrollViewer.ShouldNotBeNull();

        await WaitForAsync(() =>
        {
            return restoredScrollViewer.Extent.Height > restoredScrollViewer.Viewport.Height;
        }, 3000);

        await WaitForAsync(() => restoredScrollViewer.Offset == targetOffset, 3000);

        vm.ScrollOffset.ShouldBe(targetOffset);

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesFoldState_WhenResourceIsUpdated()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();

        var resource = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        };

        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        vm.YamlDocument.Text = """
            spec:
              nested:
                child: value
            """.ReplaceLineEndings("\n");

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text.ShouldNotContain("updated: \"true\"");

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Count().ShouldBeGreaterThan(0);
        foldingManager.AllFoldings.First().IsFolded = true;

        var updatedResource = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        };

        updatedResource.Metadata.Labels = new Dictionary<string, string>
        {
            ["updated"] = "true",
        };

        await cluster.Runtime.AddOrUpdateResource(updatedResource);
        await WaitForUiAsync(
            () => vm.YamlDocument.Text.Contains("updated: \"true\"", StringComparison.OrdinalIgnoreCase));
        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text.ShouldContain("updated: \"true\"");

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsCompletion_WhenCompletionIsRequested()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var keyBinding = editor.KeyBindings.OfType<KeyBinding>()
            .Single(binding => binding.Command == vm.RequestCompletionCommand);
        keyBinding.Command.ShouldBe(vm.RequestCompletionCommand);
        keyBinding.Gesture.ShouldBeOfType<KeyGesture>()
            .ShouldSatisfyAllConditions(gesture =>
            {
                gesture.Key.ShouldBe(Key.Space);
                gesture.KeyModifiers.ShouldBe(KeyModifiers.Control);
            });

        vm.RequestCompletionCommand.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldNotBeNull();
        completionWindow!.IsOpen.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotShowEmptyCompletion_WhenTypedPrefixHasNoMatches()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = "metadata:\n  name: temp\n  sdsdsds";

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;
        editor.TextArea.PerformTextInput("s");
        Dispatcher.UIThread.RunJobs();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        GetCompletionWindow(behavior).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ClosesCompletion_WhenTextIsDeleted()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();

        vm.RequestCompletionCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        GetCompletionWindow(behavior).ShouldNotBeNull();

        editor.Document!.Remove(0, 1);
        Dispatcher.UIThread.RunJobs();

        GetCompletionWindow(behavior).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DebouncesFoldingUpdates_WhenTextChanges()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        var initialTitles = foldingManager.AllFoldings.Select(folding => folding.Title).ToArray();

        vm.YamlDocument.Text = "root:\n  nested:\n    value: test";
        await TestApplicationExtensions.WaitForUiAsync();

        var timerField = typeof(YamlEditorBehavior).GetField("_foldingUpdateTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        var timer = timerField?.GetValue(behavior) as DispatcherTimer;
        timer.ShouldNotBeNull();
        timer!.IsEnabled.ShouldBeTrue();
        foldingManager.AllFoldings.Select(folding => folding.Title).ShouldBe(initialTitles);

        await WaitForUiAsync(
            () => foldingManager.AllFoldings.Any(folding => folding.Title.TrimEnd() == "root:"),
            timeoutMs: 1000);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnHoverOverFieldName()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            spec: value
            metadata:
              name: test
              namespace: default
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var popup = GetDocumentationWindow(editor);
        popup.ShouldNotBeNull();
        popup!.ShouldBeOfType<StackPanel>();
        IsDocumentationPopupOpen(editor).ShouldBeTrue();

        var panel = (StackPanel)popup;
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("spec");

        var valueOffset = editor.Document!.Text.IndexOf("value", StringComparison.Ordinal) + 1;
        shown = InvokeHoverTooltip(editor, valueOffset, onlyWhenOpen: true);
        shown.ShouldBeFalse();
        Dispatcher.UIThread.RunJobs();

        IsDocumentationPopupOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupForPodMetadataNameAndNamespace()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor").ShouldNotBeNull();
        foreach (var field in new[] { "name", "namespace" })
        {
            var offset = editor.Document!.Text.IndexOf(field, StringComparison.Ordinal) + 1;
            InvokeHoverTooltip(editor, offset).ShouldBeTrue(field);
            GetDocumentationWindow(editor).ShouldNotBeNull(field);
            IsDocumentationPopupOpen(editor).ShouldBeTrue(field);
        }
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupForMetadataNameAtEndOfDocument()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nmetadata:\n  name: temp";
        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor").ShouldNotBeNull();
        var offset = editor.Document!.Text.IndexOf("name", StringComparison.Ordinal) + 1;
        var editorPoint = GetPointForOffset(editor, offset);
        var windowPoint = editor.TextArea.TextView.TranslatePoint(editorPoint, window);
        windowPoint.ShouldNotBeNull();

        window.MouseMove(windowPoint!.Value);

        await WaitForAsync(() => IsDocumentationPopupOpen(editor), 1000);
        GetDocumentationWindow(editor).ShouldNotBeNull();
        var popup = GetDocumentationPopup(editor).ShouldNotBeNull();
        popup.PlacementTarget.ShouldBe(editor.TextArea.TextView);
        popup.Placement.ShouldBe(PlacementMode.AnchorAndGravity);
        popup.PlacementAnchor.ShouldBe(PopupAnchor.TopLeft);
        popup.PlacementGravity.ShouldBe(PopupGravity.BottomRight);
        popup.PlacementConstraintAdjustment.ShouldBe(
            PopupPositionerConstraintAdjustment.SlideX | PopupPositionerConstraintAdjustment.SlideY);
        popup.PlacementRect.ShouldNotBeNull().X.ShouldBeGreaterThan(0);
        popup.HorizontalOffset.ShouldBe(8);
        popup.VerticalOffset.ShouldBe(0);

        var surface = popup.Child.ShouldBeOfType<ContentControl>();
        surface.IsHitTestVisible.ShouldBeFalse();
        Application.Current.TryGetResource("SystemRegionBrush", Application.Current.ActualThemeVariant, out var background).ShouldBeTrue();
        surface.Background.ShouldBe(background);
        surface.Foreground.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsTooltipForBuiltInResourceSchema()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);
        var cluster = await Application.Current.CreateClusterAsync();
        var kind = GroupApiVersionKind.From<V1Pod>();
        cluster.Runtime.ModelCatalog.OpenApiSchemas.GetSchema(kind).ShouldNotBeNull();

        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nspec:\n";
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor").ShouldNotBeNull();
        var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;

        InvokeHoverTooltipAtPoint(editor, GetPointForOffset(editor, offset)).ShouldBeTrue();
        var tip = GetDocumentationWindow(editor).ShouldBeOfType<StackPanel>();
        tip.Children.OfType<TextBlock>().Select(x => x.Text).ShouldContain("Pod specification");
        tip.Children.OfType<TextBlock>().Select(x => x.Text).ShouldContain("object");
        IsDocumentationPopupOpen(editor).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsTooltipForBuiltInResourceThroughWindowPointerMove()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nspec:\n";
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor").ShouldNotBeNull();
        var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
        var textView = editor.TextArea.TextView;
        var editorPoint = GetPointForOffset(editor, offset);
        var windowPoint = textView.TranslatePoint(editorPoint, window);
        windowPoint.ShouldNotBeNull();

        window.MouseMove(windowPoint!.Value);

        await WaitForAsync(() => IsDocumentationPopupOpen(editor), 1000);
        IsDocumentationPopupOpen(editor).ShouldBeTrue();
        GetDocumentationWindow(editor).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsTooltipForBuiltInResourceWhenPresentedByDock()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        cluster.Runtime.ModelCatalog.OpenApiSchemas.GetSchema(GroupApiVersionKind.From<V1Pod>()).ShouldNotBeNull();

        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var dockControl = new DockControl { Layout = layout };

        using var window = Application.Current.CreateTestWindow(content: dockControl);
        window.Show();

        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        });
        vm.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nspec:\n";
        factory.AddToDocuments(vm);
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);

        var editor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, vm), 3000);
        editor.ShouldNotBeNull();
        var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
        var editorPoint = GetPointForOffset(editor, offset);
        var windowPoint = editor.TextArea.TextView.TranslatePoint(editorPoint, window);
        windowPoint.ShouldNotBeNull();
        TextView? hitTextView = null;
        await WaitForAsync(() =>
        {
            hitTextView = window.InputHitTest(windowPoint!.Value) as TextView;
            return hitTextView is not null;
        }, 3000);
        hitTextView.ShouldNotBeNull();

        window.MouseMove(windowPoint!.Value);

        await WaitForAsync(() => IsDocumentationPopupOpen(editor), 1000);
        IsDocumentationPopupOpen(editor).ShouldBeTrue();
        GetDocumentationWindow(editor).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task MainView_ShowsTooltipForV1PodYaml()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var mainViewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var previousLayout = mainViewModel.Layout;
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        mainViewModel.Layout = layout;

        using var window = Application.Current.CreateTestWindow(content: new MainView
        {
            DataContext = mainViewModel,
        });
        window.Show();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        };
        var yamlViewModel = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        try
        {
            yamlViewModel.Initialize(cluster, pod);
            yamlViewModel.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nspec:\n";
            factory.AddToBottom(yamlViewModel);

            var editor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, yamlViewModel), 3000);
            var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
            var windowPoint = editor.TextArea.TextView.TranslatePoint(GetPointForOffset(editor, offset), window);
            windowPoint.ShouldNotBeNull();
            window.MouseMove(windowPoint.Value);

            await WaitForAsync(() => IsDocumentationPopupOpen(editor), 1000);
            GetDocumentationWindow(editor).ShouldNotBeNull();
            var popup = GetDocumentationPopup(editor).ShouldNotBeNull();
            popup.PlacementTarget.ShouldBe(editor.TextArea.TextView);
            popup.Placement.ShouldBe(PlacementMode.AnchorAndGravity);
        }
        finally
        {
            factory.RemoveDockable(yamlViewModel, collapse: false);
            yamlViewModel.Dispose();
            if (previousLayout is not null)
            {
                factory.InitLayout(previousLayout);
            }

            mainViewModel.Layout = previousLayout;
        }
    }

    [AvaloniaFact]
    public async Task MainWindow_ShowsTooltipForV1PodYaml()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var mainViewModel = Application.Current.GetRequiredTestService<MainViewModel>();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var previousLayout = mainViewModel.Layout;
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        mainViewModel.Layout = layout;

        var window = new MainWindow
        {
            Width = 1200,
            Height = 800,
            DataContext = mainViewModel,
        };
        window.Show();

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "test", NamespaceProperty = "default" },
        };
        var yamlViewModel = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        try
        {
            yamlViewModel.Initialize(cluster, pod);
            yamlViewModel.YamlDocument.Text = "apiVersion: v1\nkind: Pod\nspec:\n";
            factory.AddToBottom(yamlViewModel);

            var editor = await WaitForValueAsync(() => FindVisibleYamlEditor(window, yamlViewModel), 3000);
            var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
            var windowPoint = editor.TextArea.TextView.TranslatePoint(GetPointForOffset(editor, offset), window);
            windowPoint.ShouldNotBeNull();
            window.MouseMove(windowPoint.Value);

            await WaitForAsync(() => IsDocumentationPopupOpen(editor), 1000);
            GetDocumentationWindow(editor).ShouldNotBeNull();
            var popup = GetDocumentationPopup(editor).ShouldNotBeNull();
            popup.PlacementTarget.ShouldBe(editor.TextArea.TextView);
            popup.Placement.ShouldBe(PlacementMode.AnchorAndGravity);
        }
        finally
        {
            factory.RemoveDockable(yamlViewModel, collapse: false);
            yamlViewModel.Dispose();
            window.DataContext = null;
            window.Content = null;
            window.Close();
            mainViewModel.Layout = previousLayout;
        }
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsTooltipForCustomResourceSchema()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);
        var cluster = await Application.Current.CreateClusterAsync();
        var kind = new GroupApiVersionKind("example.com", "v1", "Widget", "widgets");
        cluster.Runtime.ModelCatalog.RegisterCustomResourceDefinition(kind);
        cluster.Runtime.ModelCatalog.OpenApiSchemas.GetSchema(kind).ShouldNotBeNull();

        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("{\"apiVersion\":\"example.com/v1\",\"kind\":\"Widget\",\"metadata\":{\"name\":\"test\"}}");
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = "apiVersion: example.com/v1\nkind: Widget\nspec:\n";
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor").ShouldNotBeNull();
        var offset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;

        InvokeHoverTooltip(editor, offset).ShouldBeTrue();
        GetDocumentationWindow(editor).ShouldNotBeNull();
        IsDocumentationPopupOpen(editor).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnNestedFieldName()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              ownerReferences:
              - apiVersion: apps/v1
                kind: ReplicaSet
                name: cert-manager-566988c7b9
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("ownerReferences", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("ownerReferences");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnFieldNamePastTenthLine()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              containers:
                - name: one
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document.Text.IndexOf("containers", StringComparison.Ordinal) + 1;
        editor.Document!.GetLineByOffset(fieldNameOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("containers");
        panel.Children.OfType<TextBlock>().Skip(1).First().Text.ShouldBe("array");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnNestedFieldPastTenthLine()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
              ownerReferences:
                - apiVersion: apps/v1
                  kind: ReplicaSet
                  name: cert-manager-566988c7b9
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var lineNumber = editor.Document!.GetLineByOffset(editor.Document.Text.IndexOf("ownerReferences", StringComparison.Ordinal)).LineNumber;
        lineNumber.ShouldBeGreaterThan(10);

        var fieldNameOffset = editor.Document.Text.IndexOf("ownerReferences", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("ownerReferences");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnKeyAtEndOfLine()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              containers:
                - name: cert-manager
                  imagePullPolicy: IfNotPresent
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        editor.Document.GetLineByOffset(fieldNameOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("imagePullPolicy");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupOnSequenceItemFieldName()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              containers:
                - name: cert-manager
                  image: nginx
                  env:
                    - name: FIRST
                      value: one
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("name: FIRST", StringComparison.Ordinal) + 1;
        editor.Document.GetLineByOffset(fieldNameOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("name");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_UpdatesDocumentationPopupWhenHoverMovesBetweenFields()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              containers:
                - name: cert-manager
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var nameOffset = editor.Document!.Text.IndexOf("name: cert-manager", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltip(editor, nameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("name");

        var imageOffset = editor.Document.Text.IndexOf("image: nginx", StringComparison.Ordinal) + 1;
        shown = InvokeHoverTooltip(editor, imageOffset, onlyWhenOpen: true);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        panel = tip.ShouldBeOfType<StackPanel>();
        title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("image");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupAfterBlankLinesAndComments()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            # lots of leading comments
            # to force the target lower in the document

            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              containers:
                - name: cert-manager
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("containers", StringComparison.Ordinal) + 1;
        editor.Document.GetLineByOffset(fieldNameOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        var panel = tip.ShouldBeOfType<StackPanel>();
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("containers");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotShowDocumentationPopupOnColonOrValueBoundary()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
            spec:
              imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var colonOffset = editor.Document!.Text.IndexOf("imagePullPolicy:", StringComparison.Ordinal) + "imagePullPolicy".Length;
        editor.Document.GetLineByOffset(colonOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, colonOffset);
        shown.ShouldBeFalse();

        var valueOffset = editor.Document.Text.IndexOf("IfNotPresent", StringComparison.Ordinal) + 1;
        shown = InvokeHoverTooltip(editor, valueOffset);
        shown.ShouldBeFalse();

        Dispatcher.UIThread.RunJobs();

        IsDocumentationPopupOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ClosesDocumentationPopupWhenScrolled()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: fa2328c666789a14eecd7a5ad558b972b510008d547a5d745bd10ccf00e16fb0
                cni.projectcalico.org/podIP: 10.1.43.176/32
                cni.projectcalico.org/podIPs: 10.1.43.176/32
                kubectl.kubernetes.io/default-container: alertmanager
                kubectl.kubernetes.io/restartedAt: 2024-12-21T11:27:54Z
              creationTimestamp: "2025-12-18T03:18:16Z"
              generateName: alertmanager-prometheus-kube-prometheus-alertmanager-
              generation: 1
              labels:
                alertmanager: prometheus-kube-prometheus-alertmanager
                app.kubernetes.io/instance: prometheus-kube-prometheus-alertmanager
                app.kubernetes.io/managed-by: prometheus-operator
                app.kubernetes.io/name: alertmanager
                app.kubernetes.io/version: 0.27.0
                apps.kubernetes.io/pod-index: "0"
                controller-revision-hash: alertmanager-prometheus-kube-prometheus-alertmanager-7bfd55984
                statefulset.kubernetes.io/pod-name: alertmanager-prometheus-kube-prometheus-alertmanager-0
              name: alertmanager-prometheus-kube-prometheus-alertmanager-0
              namespace: monitoring
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: StatefulSet
                name: alertmanager-prometheus-kube-prometheus-alertmanager
                uid: b8a36710-6e1d-4391-b059-e2cf435acc99
              resourceVersion: "801283915"
              uid: 2aeb93fe-692d-41e1-a62c-69fccb4fceef
            spec:
              containers:
              - args:
                - --config.file=/etc/alertmanager/config_out/alertmanager.env.yaml
                - --storage.path=/alertmanager
                - --data.retention=120h
                - --cluster.listen-address=
                - --web.listen-address=:9093
                - --web.external-url=http://prometheus-kube-prometheus-alertmanager.monitoring:9093
                - --web.route-prefix=/
                - --cluster.label=monitoring/prometheus-kube-prometheus-alertmanager
                - --cluster.peer=alertmanager-prometheus-kube-prometheus-alertmanager-0.alertmanager-operated:9094
                - --cluster.reconnect-timeout=5m
                - --web.config.file=/etc/alertmanager/web_config/web-config.yaml
                env:
                - name: POD_IP
                  valueFrom:
                    fieldRef:
                      apiVersion: v1
                      fieldPath: status.podIP
                image: quay.io/prometheus/alertmanager:v0.27.0
                imagePullPolicy: IfNotPresent
                name: alertmanager
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        IsDocumentationPopupOpen(editor).ShouldBeTrue();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        scrollViewer.Offset = new Vector(0, 80);
        Dispatcher.UIThread.RunJobs();

        IsDocumentationPopupOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupFromRenderedPointOnRootField()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var specOffset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltipAtPoint(editor, GetPointForOffset(editor, specOffset));
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("spec");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupFromRenderedPointOnNestedSequenceField()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
            spec:
              containers:
                - name: app
                  env:
                    - name: FIRST
                      value: one
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var nameOffset = editor.Document!.Text.IndexOf("name: FIRST", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltipAtPoint(editor, GetPointForOffset(editor, nameOffset));
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("name");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupFromRenderedPointPastTenthLine()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                a: "1"
                b: "2"
                c: "3"
                d: "4"
                e: "5"
                f: "6"
                g: "7"
                h: "8"
            spec:
              containers:
                - name: app
                  imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldOffset = editor.Document!.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        editor.Document.GetLineByOffset(fieldOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltipAtPoint(editor, GetViewportPointForOffset(editor, fieldOffset));
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("imagePullPolicy");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotShowDocumentationPopupFromRenderedPointOnValue()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            spec:
              imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var valueOffset = editor.Document!.Text.IndexOf("IfNotPresent", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltipAtPoint(editor, GetPointForOffset(editor, valueOffset));
        shown.ShouldBeFalse();

        Dispatcher.UIThread.RunJobs();

        IsDocumentationPopupOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifest()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldOffset = editor.Document!.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltip(editor, fieldOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("imagePullPolicy");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifestWithoutTrailingNewline()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = (
            """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """)
            .ReplaceLineEndings("\n")
            .TrimEnd('\r', '\n');
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldOffset = editor.Document!.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltip(editor, fieldOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("imagePullPolicy");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifestAfterScroll()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 250);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        await WaitForAsync(() =>
        {
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        Dispatcher.UIThread.RunJobs();

        var fieldOffset = editor.Document!.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltipAtPoint(editor, GetViewportPointForOffset(editor, fieldOffset));
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var tip = GetDocumentationWindow(editor);
        tip.ShouldNotBeNull();
        tip.ShouldBeOfType<StackPanel>()
            .Children.OfType<TextBlock>()
            .First().Text.ShouldBe("imagePullPolicy");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ResolvesViewportPointToImagePullPolicyOffsetAfterScroll()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 250);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        await WaitForAsync(() =>
        {
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        Dispatcher.UIThread.RunJobs();

        var fieldOffset = editor.Document!.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var viewportPoint = GetViewportPointForOffset(editor, fieldOffset);
        var resolvedOffset = TryGetHoverOffset(editor, viewportPoint);

        resolvedOffset.ShouldBe(fieldOffset);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_CreatesDocumentationTipForImagePullPolicyOffsetAfterScroll()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 250);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              annotations:
                cni.projectcalico.org/containerID: 32e3ced3c8334f980a2979d270e291671975b77f359837a46efb7de7ea80fbdf
                cni.projectcalico.org/podIP: 10.1.43.214/32
                cni.projectcalico.org/podIPs: 10.1.43.214/32
              creationTimestamp: "2025-12-18T03:18:00Z"
              generateName: calico-kube-controllers-6d7fffdff7-
              generation: 1
              labels:
                k8s-app: calico-kube-controllers
                pod-template-hash: 6d7fffdff7
              name: calico-kube-controllers-6d7fffdff7-m67z2
              namespace: kube-system
              ownerReferences:
              - apiVersion: apps/v1
                blockOwnerDeletion: true
                controller: true
                kind: ReplicaSet
                name: calico-kube-controllers-6d7fffdff7
                uid: 09983864-0770-4948-ad18-81f9d9c2a408
              resourceVersion: "801284056"
              uid: a98d0cf5-ee3e-4107-814b-21a877a2f052
            spec:
              containers:
              - env:
                - name: ENABLED_CONTROLLERS
                  value: node
                - name: DATASTORE_TYPE
                  value: kubernetes
                image: docker.io/calico/kube-controllers:v3.29.3
                imagePullPolicy: IfNotPresent
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        await WaitForAsync(() =>
        {
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        scrollViewer.Offset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        Dispatcher.UIThread.RunJobs();

        var fieldOffset = editor.Document!.Text.LastIndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var tip = TryCreateHoverDocumentationTip(editor, fieldOffset);

        tip.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotShowCompletion_WhenEnterCreatesNewLine()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsSequenceMarker_WhenEnterIsPressedOnSequenceProperty()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.LastIndexOf("containers:", StringComparison.Ordinal) + "containers:".Length;

        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_AlignsNestedSequenceMarker_WhenEnterIsPressedOnSequenceItemProperty()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - command:
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.LastIndexOf("command:", StringComparison.Ordinal) + "command:".Length;

        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - command:\n"
            + "        - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ContinuesSequenceItemMapping_WhenEnterIsPressedAfterValue()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - imagePullPolicy: Always
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - imagePullPolicy: Always\n"
            + "      ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ForcesNewSequenceItem_WhenControlEnterIsPressedAfterValue()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - imagePullPolicy: Always
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;
        editor.TextArea.Focus();

        window.KeyPress(Key.Enter, RawInputModifiers.Control, PhysicalKey.Enter, null);
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - imagePullPolicy: Always\n"
            + "    - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ContinuesListItem_WhenEnterIsPressedAtEndOfSequenceEntry()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.ValidationDebounceDelay = TimeSpan.Zero;
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                - test
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - test\n"
            + "    - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ExitsList_WhenEnterIsPressedOnBlankSequenceEntry()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            spec:
              containers:
                -
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "spec:\n"
            + "  containers:\n"
            + "  ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DoesNotShowCompletion_WhenTypingScalarSequenceItem()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - command:
                  -
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("s");
        await TestApplicationExtensions.WaitForUiAsync();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsStarterSequence_WhenSelectingListCompletion()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              containers:
                - name: test
                  co
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        vm.RequestCompletionCommand.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldNotBeNull();

        var completionData = completionWindow!.CompletionList.CompletionData
            .OfType<YamlCompletionData>()
            .Single(data => data.Text == "command");

        completionData.Complete(
            editor.TextArea,
            new TextSegment
            {
                StartOffset = completionWindow.StartOffset,
                Length = completionWindow.EndOffset - completionWindow.StartOffset,
            },
            EventArgs.Empty);
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - name: test\n"
            + "      command:\n"
            + "        - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsStarterObjectBlock_WhenSelectingObjectCompletion()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            sp
            """.ReplaceLineEndings("\n");

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        vm.RequestCompletionCommand.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldNotBeNull();

        var completionData = completionWindow!.CompletionList.CompletionData
            .OfType<YamlCompletionData>()
            .Single(data => data.Text == "spec");

        completionData.Complete(
            editor.TextArea,
            new TextSegment
            {
                StartOffset = completionWindow.StartOffset,
                Length = completionWindow.EndOffset - completionWindow.StartOffset,
            },
            EventArgs.Empty);
        await TestApplicationExtensions.WaitForUiAsync();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  ");
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_ReturnsDiagnostic_ForMalformedYaml()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: [test
            """.ReplaceLineEndings("\n"));

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Severity.ShouldBe(YamlDiagnosticSeverity.Error);
        diagnostics[0].Message.ShouldContain("expected");
        diagnostics[0].StartLine.ShouldBeGreaterThan(0);
        diagnostics[0].StartColumn.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AnchorsDuplicateKeyDiagnostic_ToDuplicateKey()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();

        var diagnostics = service.Validate("""
            apiVersion: azure.upbound.io/v1beta1
            kind: ResourceGroup
            metadata:
              name: temp
              namespace: default
            spec:
              forProvider:
                location: test
                managedBy: tes
                tags:
                  test: val
                  test: 2
                  test: 4
            """.ReplaceLineEndings("\n"));

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("duplicate key test");
        diagnostics[0].StartLine.ShouldBe(12);
        diagnostics[0].StartColumn.ShouldBe(7);
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AnchorsDuplicateKeyInSequenceMapping_ToDuplicateKey()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
            spec:
              containers:
                - name: app
                  image: first
                  image: second
            """.ReplaceLineEndings("\n"));

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("duplicate key image");
        diagnostics[0].StartLine.ShouldBe(9);
        diagnostics[0].StartColumn.ShouldBe(7);
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AcceptsCrdInstanceWithJsonModel()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();
        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        var crd = KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);
        var version = crd.Spec.Versions.First(version => version.Served && version.Storage).Name;
        var kind = new GroupApiVersionKind(crd.Spec.Group, version, crd.Spec.Names.Kind, crd.Spec.Names.Plural);
        try
        {
            cluster.Runtime.ModelCatalog.RegisterCustomResourceDefinition(kind);

            cluster.Runtime.ModelCatalog.IsCustomResource(kind).ShouldBeTrue();

            var diagnostics = service.Validate(KubernetesTestData.CustomResourceYaml, cluster.Runtime.ModelCatalog);

            diagnostics.ShouldBeEmpty();
        }
        finally
        {
            cluster.Runtime.ModelCatalog.RemoveCustomResourceDefinition(kind);
        }
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AcceptsRegisteredCnpgCluster()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();
        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var kind = new GroupApiVersionKind("postgresql.cnpg.io", "v1", "Cluster", "clusters");
        const string yaml = """
            apiVersion: postgresql.cnpg.io/v1
            kind: Cluster
            metadata:
              name: app
            """;

        try
        {
            cluster.Runtime.ModelCatalog.RegisterCustomResourceDefinition(kind);

            service.Validate(yaml.ReplaceLineEndings("\n"), cluster.Runtime.ModelCatalog).ShouldBeEmpty();
        }
        finally
        {
            cluster.Runtime.ModelCatalog.RemoveCustomResourceDefinition(kind);
        }
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_ReturnsDiagnostic_ForUnknownKubernetesField()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();
        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n"), cluster.Runtime.ModelCatalog);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Severity.ShouldBe(YamlDiagnosticSeverity.Error);
        diagnostics[0].Message.ShouldContain("unknownField");
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AnchorsTypedScalarConversionError_ToValue()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();
        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              activeDeadlineSeconds: a
            """.ReplaceLineEndings("\n"), cluster.Runtime.ModelCatalog);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Severity.ShouldBe(YamlDiagnosticSeverity.Error);
        diagnostics[0].Message.ShouldContain("correct format");
        diagnostics[0].StartLine.ShouldBe(7);
        diagnostics[0].StartColumn.ShouldBeGreaterThan(25);
        diagnostics[0].EndLine.ShouldBe(7);
        diagnostics[0].EndColumn.ShouldBeGreaterThanOrEqualTo(diagnostics[0].StartColumn);
    }

    [AvaloniaFact]
    public void Utilities_FormatKubernetesStatusMessage_PrefersStatusMessage()
    {
        var status = new V1Status
        {
            Message = "Namespace \"temp\" is invalid: [spec.finalizers: Invalid value: \"\": name part must be non-empty]",
            Reason = "Invalid",
            Details = new V1StatusDetails
            {
                Causes =
                [
                    new V1StatusCause
                    {
                        Message = "Invalid value: \"\": name part must be non-empty",
                        Field = "spec.finalizers",
                    },
                ],
            },
        };

        var message = Utilities.FormatKubernetesStatusMessage(status, "fallback");

        message.ShouldBe("Namespace \"temp\" is invalid:\nspec.finalizers: Invalid value: \"\": name part must be non-empty");
    }

    [AvaloniaFact]
    public void Utilities_FormatKubernetesStatusMessage_FormatsCausesWithFieldPath_WhenStatusMessageMissing()
    {
        var status = new V1Status
        {
            Reason = "Invalid",
            Details = new V1StatusDetails
            {
                Causes =
                [
                    new V1StatusCause
                    {
                        Message = "Invalid value: \"\": name part must be non-empty",
                        Field = "spec.finalizers",
                    },
                    new V1StatusCause
                    {
                        Message = "must be a number",
                        Field = "spec.activeDeadlineSeconds",
                    },
                ],
            },
        };

        var message = Utilities.FormatKubernetesStatusMessage(status, "fallback");

        message.ShouldBe("spec.finalizers: Invalid value: \"\": name part must be non-empty\nspec.activeDeadlineSeconds: must be a number");
    }

    [AvaloniaFact]
    public void Utilities_FormatKubernetesStatusMessage_FormatsStructuredInvalidStatusMessage_AsHeaderAndLines()
    {
        var status = new V1Status
        {
            Message = "Deployment.apps \"temp\" is invalid: [spec.selector: Required value, spec.template.metadata.labels: Invalid value: null: `selector` does not match template `labels`, spec.template.spec.containers: Required value]",
            Reason = "Invalid",
            Details = new V1StatusDetails
            {
                Causes =
                [
                    new V1StatusCause
                    {
                        Message = "Required value",
                        Field = "spec.selector",
                    },
                    new V1StatusCause
                    {
                        Message = "Invalid value: null: `selector` does not match template `labels`",
                        Field = "spec.template.metadata.labels",
                    },
                    new V1StatusCause
                    {
                        Message = "Required value",
                        Field = "spec.template.spec.containers",
                    },
                ],
            },
        };

        var message = Utilities.FormatKubernetesStatusMessage(status, "fallback");

        message.ShouldBe(
            "Deployment.apps \"temp\" is invalid:\n" +
            "spec.selector: Required value\n" +
            "spec.template.metadata.labels: Invalid value: null: `selector` does not match template `labels`\n" +
            "spec.template.spec.containers: Required value");
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AnchorsUnknownTypeDiagnostic_ToKindHeader()
    {
        var service = Application.Current.GetRequiredTestService<IYamlValidationService>();

        var diagnostics = service.Validate("""
            apiVersion: example.io/v1
            kind: MadeUpKind
            metadata:
              name: test
            """.ReplaceLineEndings("\n"));

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("example.io/v1/MadeUpKind");
        diagnostics[0].StartLine.ShouldBe(2);
        diagnostics[0].StartColumn.ShouldBe(1);
        diagnostics[0].EndLine.ShouldBe(2);
        diagnostics[0].EndColumn.ShouldBe(5);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_TracksYamlSyntaxDiagnostics()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: [test
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.ValidationDiagnostics[0].Message.ShouldNotContain("Exception during serialization");
        vm.ValidationDiagnostics[0].Message.ShouldNotContain("Exception during deserialization");

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("expected", StringComparison.OrdinalIgnoreCase));
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Validation failed");
        vm.ActionResultMessage.ShouldContain("expected");
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(FAInfoBarSeverity.Error);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_TracksStrictKubernetesDiagnostics()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.ValidationDiagnostics[0].Message.ShouldContain("unknownField");
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Validation failed");
        vm.ActionResultMessage.ShouldContain("unknownField");

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("unknownField", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_TracksUnknownTypeDiagnostics()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: example.io/v1
            kind: MadeUpKind
            metadata:
              name: test
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.ValidationDiagnostics[0].StartLine.ShouldBe(2);
        vm.ValidationDiagnostics[0].Message.ShouldContain("MadeUpKind");
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Validation failed");
        vm.ActionResultMessage.ShouldContain("MadeUpKind");

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("MadeUpKind", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_SaveShowsInlineFailure_WhenYamlIsInvalid()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: [test
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        await vm.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Save failed");
        vm.ActionResultMessage.ShouldContain("line");
        vm.ActionResultMessage.ShouldContain("column");
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(FAInfoBarSeverity.Error);
        (Application.Current as TestApp)?.Notification.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DisablesSaveAndDryRun_WhenValidationErrorsExist()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: [test
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.SaveCommand.CanExecute(null).ShouldBeFalse();
        vm.DryRunCommand.CanExecute(null).ShouldBeFalse();

        var buttons = view.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Single(x => x.Command == vm.SaveCommand).IsEnabled.ShouldBeFalse();
        buttons.Single(x => x.Command == vm.DryRunCommand).IsEnabled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DebouncesValidationWhileTypingInvalidYaml()
    {
        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;
        vm.ValidationDebounceDelay = TimeSpan.FromMilliseconds(200);

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
              labels: [test
            """.ReplaceLineEndings("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        vm.ValidationDiagnostics.ShouldBeEmpty();
        vm.HasActionResult.ShouldBeFalse();

        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.HasActionFailureResult.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DryRunShowsInlineSuccess_WhenYamlIsValid()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        vm.DryRunCommand.CanExecute(null).ShouldBeTrue();

        await vm.DryRunCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        (Application.Current as TestApp)?.Notification.ShouldBeNull();
        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run succeeded");
        vm.ActionResultMessage.ShouldBe("The server accepted the manifest using dry-run.");
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(FAInfoBarSeverity.Success);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DryRunShowsInlineFailure_WhenServerValidationFails()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var services = Application.Current.GetTestServices();
        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();

        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec: {}
            """.ReplaceLineEndings("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        await vm.DryRunCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        (Application.Current as TestApp)?.Notification.ShouldBeNull();
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run failed");
        vm.ActionResultMessage.ShouldNotBeNullOrWhiteSpace();
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(FAInfoBarSeverity.Error);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_KeepsActionResultVisible_WhenYamlChanges()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        await vm.DryRunCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.HasActionResult.ShouldBeTrue();

        vm.YamlDocument.Insert(vm.YamlDocument.TextLength, "\n# note");
        await TestApplicationExtensions.WaitForUiAsync();

        vm.HasActionResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run succeeded");
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ClearsActionResult_WhenDismissed()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await cluster.Connect();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        await TestApplicationExtensions.WaitForUiAsync();

        await vm.DryRunCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.DismissActionResultCommand.CanExecute(null).ShouldBeTrue();
        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.IsClosable.ShouldBeTrue();

        vm.DismissActionResultCommand.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.HasActionResult.ShouldBeFalse();
        vm.DismissActionResultCommand.CanExecute(null).ShouldBeFalse();
        actionBar.IsOpen.ShouldBeFalse();
        actionBar.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_HidesActionResultBar_WhenThereIsNoMessage()
    {
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;

        var actionBar = view.FindControl<FAInfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeFalse();
        actionBar.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_HideNoisyFieldsToggle_BindsDirectlyToViewModel()
    {
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;

        var toggleButton = view.FindControl<ToggleButton>("HideNoisyFieldsToggle");
        toggleButton.ShouldNotBeNull();
        toggleButton.Command.ShouldBeNull();
        toggleButton.IsChecked.ShouldBe(true);

        toggleButton.IsChecked = false;
        Dispatcher.UIThread.RunJobs();

        vm.HideNoisyFields.ShouldBeFalse();
        toggleButton.IsChecked.ShouldBe(false);
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_UnindentsEmptyLineByTwoSpaces()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("        "),
        };
        editor.TextArea.Caret.Offset = editor.Document.TextLength;

        var handled = YamlEditorBehavior.TryUnindentEmptyLine(editor.TextArea);

        handled.ShouldBeTrue();
        editor.Text.ShouldBe("      ");
        editor.TextArea.Caret.Offset.ShouldBe(6);
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_DoesNotUnindentNonEmptyLine()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("      a"),
        };
        editor.TextArea.Caret.Offset = editor.Document.TextLength;

        var handled = YamlEditorBehavior.TryUnindentEmptyLine(editor.TextArea);

        handled.ShouldBeFalse();
        editor.Text.ShouldBe("      a");
        editor.TextArea.Caret.Offset.ShouldBe(editor.Document.TextLength);
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_UnindentsEmptyLineForShiftTabPath()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("    "),
        };
        editor.TextArea.Caret.Offset = editor.Document.TextLength;

        var handled = YamlEditorBehavior.TryUnindentEmptyLine(editor.TextArea);

        handled.ShouldBeTrue();
        editor.Text.ShouldBe("  ");
        editor.TextArea.Caret.Offset.ShouldBe(2);
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_UnindentsCurrentLineForShiftTabPath()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("    value"),
        };
        editor.TextArea.Caret.Offset = editor.Document.TextLength;

        var handled = YamlEditorBehavior.TryUnindentSelectedLines(editor.TextArea);

        handled.ShouldBeTrue();
        editor.Text.ShouldBe("  value");
        editor.TextArea.Caret.Offset.ShouldBe(7);
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_IndentsSelectedLinesByTwoSpaces()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("a\nb\n"),
        };
        editor.Select(0, editor.Text.Length);

        var handled = YamlEditorBehavior.TryIndentSelectedLines(editor.TextArea);

        handled.ShouldBeTrue();
        editor.Text.ShouldBe("  a\n  b\n");
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_UnindentsSelectedLinesByTwoSpaces()
    {
        var editor = new TextEditor
        {
            Document = new TextDocument("  a\n  b\n"),
        };
        editor.Select(0, editor.Text.Length);

        var handled = YamlEditorBehavior.TryUnindentSelectedLines(editor.TextArea);

        handled.ShouldBeTrue();
        editor.Text.ShouldBe("a\nb\n");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_UsesTwoSpaceIndentationOptions()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.Options.IndentationSize.ShouldBe(2);
        editor.Options.ConvertTabsToSpaces.ShouldBeTrue();
        editor.Options.GetIndentationString(1).ShouldBe("  ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_LeavesScrollBelowDocumentEnabled()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.Options.AllowScrollBelowDocument.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsAboveFold()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: false, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var specFold = foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "spec:");
        specFold.IsFolded = true;

        await cluster.Runtime.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: false));
        await TestApplicationExtensions.WaitForUiAsync();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "spec:").IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsBelowFold()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var metadataFold = foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "metadata:");
        metadataFold.IsFolded = true;

        await cluster.Runtime.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        await TestApplicationExtensions.WaitForUiAsync();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "metadata:").IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesNestedFoldState_WhenResourceUpdatesInsideParent()
    {
        using var window = Application.Current.CreateTestWindow(width: 800, height: 600);

        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredTestService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        var editor = view.FindControl<TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var nestedFold = foldingManager.AllFoldings.Single(x => x.Title.Trim() == "containers:");
        nestedFold.IsFolded = true;

        await cluster.Runtime.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        await TestApplicationExtensions.WaitForUiAsync();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.Trim() == "containers:").IsFolded.ShouldBeTrue();

    }

    private static FoldingManager? GetFoldingManager(YamlEditorBehavior behavior)
    {
        var field = typeof(YamlEditorBehavior).GetField("_foldingManager", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as FoldingManager;
    }

    private static CompletionWindow? GetCompletionWindow(YamlEditorBehavior behavior)
    {
        var field = typeof(YamlEditorBehavior).GetField("_completionWindow", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as CompletionWindow;
    }

    private static bool InvokeHoverTooltip(TextEditor editor, int offset, bool onlyWhenOpen = false)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryShowHoverTooltipAtOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (bool)method.Invoke(behavior, [offset, onlyWhenOpen])!;
    }

    private static bool InvokeHoverTooltipAtPoint(TextEditor editor, Point point, bool onlyWhenOpen = false)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryShowHoverTooltipAtPoint", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (bool)method.Invoke(behavior, [point, onlyWhenOpen])!;
    }

    private static int? TryGetHoverOffset(TextEditor editor, Point point)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryGetPointerOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        object?[] args = [point, 0];
        var resolved = (bool)method.Invoke(behavior, args)!;
        return resolved ? (int)args[1] : null;
    }

    private static object? TryCreateHoverDocumentationTip(TextEditor editor, int offset)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryCreateDocumentationTip", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        object?[] args = [offset, null!];
        var created = (bool)method.Invoke(behavior, args)!;
        return created ? args[1] : null;
    }

    private static Point GetPointForOffset(TextEditor editor, int offset)
    {
        editor.UpdateLayout();
        editor.TextArea.TextView.UpdateLayout();
        var location = editor.Document!.GetLocation(offset);
        var point = editor.TextArea.TextView.GetVisualPosition(new TextViewPosition(location.Line, location.Column), VisualYPosition.LineMiddle);
        return new Point(point.X + 2, point.Y);
    }

    private static Point GetViewportPointForOffset(TextEditor editor, int offset)
    {
        var point = GetPointForOffset(editor, offset);
        return point - editor.TextArea.TextView.ScrollOffset;
    }

    private static Popup? GetDocumentationPopup(TextEditor editor)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var field = typeof(YamlHoverToolTipBehavior).GetField("_hoverPopup", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as Popup;
    }

    private static bool IsDocumentationPopupOpen(TextEditor editor)
    {
        return GetDocumentationPopup(editor)?.IsOpen == true;
    }

    private static object? GetDocumentationWindow(TextEditor editor)
    {
        return (GetDocumentationPopup(editor)?.Child as ContentControl)?.Content;
    }

    private static IReadOnlyList<string> GetDiagnosticMessages(TextEditor editor)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlDiagnosticRenderingBehavior>().Single();
        var field = typeof(YamlDiagnosticRenderingBehavior).GetField("_renderer", BindingFlags.Instance | BindingFlags.NonPublic);
        var renderer = field?.GetValue(behavior);
        var property = renderer?.GetType().GetProperty("Messages", BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(renderer) as IReadOnlyList<string> ?? [];
    }

    private static V1Pod CreatePod(string name, bool includeLabels, bool extraEnv)
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers = new List<V1Container>
                {
                    new()
                    {
                        Name = "app",
                        Image = "nginx",
                        Env = extraEnv
                            ? new List<V1EnvVar>
                            {
                                new() { Name = "A", Value = "1" },
                                new() { Name = "B", Value = "2" },
                            }
                            : new List<V1EnvVar>(),
                    },
                },
            },
        };

        if (includeLabels)
        {
            pod.Metadata.Labels = new Dictionary<string, string>
            {
                ["app"] = "kubeui",
            };
        }

        return pod;
    }

    private static TextEditor? FindVisibleYamlEditor(Visual root, ResourceYamlViewModel vm)
    {
        return root.GetVisualDescendants()
            .OfType<TextEditor>()
            .FirstOrDefault(editor => editor.IsVisible && ReferenceEquals(editor.DataContext, vm));
    }

    private static async Task<T> WaitForValueAsync<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        T? value = null;
        await WaitForAsync(() =>
        {
            value = getter();
            return value != null;
        }, timeoutMs);
        return value!;
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 1000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            await TestApplicationExtensions.WaitForUiAsync();
            if (predicate())
                return;
            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
        }
        predicate().ShouldBeTrue();
    }
}
