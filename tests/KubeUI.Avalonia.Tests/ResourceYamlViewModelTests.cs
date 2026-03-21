using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Avalonia.Behaviors;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Avalonia.Views;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class ResourceYamlViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];
    private readonly List<Window> _windows = [];

    public override void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Content = null;
            window.Close();
        }

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    private Window CreateWindow(double width = 1200, double height = 900, object? content = null)
    {
        var window = new Window
        {
            Content = content,
            Width = width,
            Height = height,
        };

        _windows.Add(window);
        return window;
    }

    private ClusterWorkspaceViewModel CreateTestWorkspace()
    {
        var cluster = new TestCluster().CreateWorkspace();
        _disposables.Add(cluster);
        return cluster;
    }

    private T ResolveService<T>()
    {
        var service = Application.Current.GetRequiredService<T>();
        if (service is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return service;
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
        var cluster = CreateTestWorkspace();
        var factory = ResolveService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = CreateWindow(content: dockControl);
        window.Show();

        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        });

        var otherDockable = ResolveService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        vm.YamlDocument.Text = """
            spec:
              nested:
                child: value
            """.ReplaceLineEndings("\n");

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var editor = WaitForValue(() => FindVisibleYamlEditor(window, vm), 3000);
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Count().ShouldBeGreaterThan(0);

        foldingManager.AllFoldings.First().IsFolded = true;

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredEditor = WaitForValue(() => FindVisibleYamlEditor(window, vm), 3000);
        restoredEditor.ShouldNotBeNull();

        behavior = Interaction.GetBehaviors(restoredEditor).OfType<YamlEditorBehavior>().Single();
        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesScrollOffset_WhenActiveDockableChanges()
    {
        var cluster = CreateTestWorkspace();
        var factory = ResolveService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = CreateWindow(content: dockControl);
        window.Show();

        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace { Metadata = new V1ObjectMeta { Name = "test" } });
        vm.YamlDocument.Text = string.Join('\n', Enumerable.Range(0, 400).Select(i => $"line{i}: value"));

        var otherDockable = ResolveService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var editor = WaitForValue(() => FindVisibleYamlEditor(window, vm), 3000);
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();

        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        var targetOffset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();
        WaitFor(() => vm.ScrollOffset == targetOffset, 3000);

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        vm.ScrollOffset.ShouldBe(targetOffset);

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredEditor = WaitForValue(() => FindVisibleYamlEditor(window, vm), 3000);
        restoredEditor.ShouldNotBeNull();

        var restoredScrollViewer = restoredEditor.GetScrollViewer();
        restoredScrollViewer.ShouldNotBeNull();

        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return restoredScrollViewer.Extent.Height > restoredScrollViewer.Viewport.Height;
        }, 3000);

        var restored = SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return restoredScrollViewer.Offset == targetOffset;
        }, 3000);

        if (!restored)
        {
            throw new ShouldAssertException(
                $"Expected restored offset {targetOffset} but got {restoredScrollViewer.Offset}. "
                + $"Saved view-model offset is {vm.ScrollOffset}. "
                + $"Dock reused editor instance: {ReferenceEquals(editor, restoredEditor)}.");
        }

        vm.ScrollOffset.ShouldBe(targetOffset);

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesFoldState_WhenResourceIsUpdated()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();

        var resource = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        };

        vm.Initialize(cluster, resource);

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        vm.YamlDocument.Text = """
            spec:
              nested:
                child: value
            """.ReplaceLineEndings("\n");

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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

        await cluster.AddOrUpdateResource(updatedResource);
        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsAboveFold()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: false, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var specFold = foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "spec:");
        specFold.IsFolded = true;

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: false));
        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "spec:").IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsBelowFold()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var metadataFold = foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "metadata:");
        metadataFold.IsFolded = true;

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.TrimEnd() == "metadata:").IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesNestedFoldState_WhenResourceUpdatesInsideParent()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();

        var nestedFold = foldingManager.AllFoldings.Single(x => x.Title.Trim() == "containers:");
        nestedFold.IsFolded = true;

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Title.Trim() == "containers:").IsFolded.ShouldBeTrue();

    }

    private static FoldingManager? GetFoldingManager(YamlEditorBehavior behavior)
    {
        var field = typeof(YamlEditorBehavior).GetField("_foldingManager", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as FoldingManager;
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

    private static AvaloniaEdit.TextEditor? FindVisibleYamlEditor(Visual root, ResourceYamlViewModel vm)
    {
        return root.GetVisualDescendants()
            .OfType<AvaloniaEdit.TextEditor>()
            .FirstOrDefault(editor => editor.IsVisible && ReferenceEquals(editor.DataContext, vm));
    }

    private static T WaitForValue<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        T? value = null;
        WaitFor(() =>
        {
            value = getter();
            return value != null;
        }, timeoutMs);
        return value!;
    }

    private static void WaitFor(Func<bool> predicate, int timeoutMs = 1000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate()) return;
            System.Threading.Thread.Sleep(10);
        }
        predicate().ShouldBeTrue();
    }
}

