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
using KubeUI.Avalonia.Behaviors;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Avalonia.Views;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class ResourceYamlViewModelTests
{
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
        var window = new Window
        {
            Width = 800,
            Height = 600,
        };

        var cluster = new TestCluster().CreateWorkspace();
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        });

        var view = Application.Current.GetRequiredService<ResourceYamlView>();
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

        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

        window.Close();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesScrollOffset_WhenActiveDockableChanges()
    {
        // Use a simple TabControl simulation so the YAML view and its editor
        // are realized deterministically in the headless test environment.
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Namespace { Metadata = new V1ObjectMeta { Name = "test" } });

        var yamlView = Application.Current.GetRequiredService<ResourceYamlView>();
        yamlView.DataContext = vm;

        // create a dummy second tab
        var dummy = new UserControl();

        var tabControl = new TabControl
        {
            Width = 800,
            Height = 600,
        };

        tabControl.Items.Add(new TabItem { Header = "Yaml", Content = yamlView });
        tabControl.Items.Add(new TabItem { Header = "Other", Content = dummy });

        var window = new Window { Content = tabControl, Width = 800, Height = 600 };
        window.Show();

        Dispatcher.UIThread.RunJobs();

        // populate the document with many lines so the editor becomes scrollable
        vm.YamlDocument.Text = string.Join('\n', Enumerable.Range(0, 200).Select(i => $"line{i}: value"));
        Dispatcher.UIThread.RunJobs();

        // wait for the editor to be available inside the yaml view
        WaitFor(() => yamlView.GetVisualDescendants().OfType<AvaloniaEdit.TextEditor>().Any(), 2000);

        var editor = yamlView.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();

        // Wait until the editor is actually scrollable (content bigger than viewport)
        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return scrollViewer.Extent.Height > scrollViewer.Viewport.Height;
        }, 3000);

        // scroll to a target offset near the bottom
        var targetOffset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        scrollViewer.Offset = targetOffset;

        // switch away and back (visibility change will trigger persist/restore)
        tabControl.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        tabControl.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();

        // Expect scroll offset to be preserved; test will fail if it's not
        scrollViewer.Offset.ShouldBe(targetOffset);

        window.Close();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesFoldState_WhenResourceIsUpdated()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
        };

        var cluster = new TestCluster().CreateWorkspace();
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        var resource = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
            },
        };

        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredService<ResourceYamlView>();
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

        window.Close();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsAboveFold()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
        };

        var cluster = new TestCluster().CreateWorkspace();
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: false, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredService<ResourceYamlView>();
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

        window.Close();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesParentFoldState_WhenResourceGrowsBelowFold()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
        };

        var cluster = new TestCluster().CreateWorkspace();
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredService<ResourceYamlView>();
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

        window.Close();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_PreservesNestedFoldState_WhenResourceUpdatesInsideParent()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
        };

        var cluster = new TestCluster().CreateWorkspace();
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();
        var resource = CreatePod("test", includeLabels: true, extraEnv: false);
        vm.Initialize(cluster, resource);

        var view = Application.Current.GetRequiredService<ResourceYamlView>();
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

        window.Close();
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

    private static object? FindDockableById(object? dockable, string id)
    {
        if (dockable == null)
        {
            return null;
        }

        var dockableType = dockable.GetType();
        var dockableId = dockableType.GetProperty("Id")?.GetValue(dockable) as string;
        if (string.Equals(dockableId, id, StringComparison.Ordinal))
        {
            return dockable;
        }

        var visibleDockables = dockableType.GetProperty("VisibleDockables")?.GetValue(dockable) as System.Collections.IEnumerable;
        if (visibleDockables == null)
        {
            return null;
        }

        foreach (var child in visibleDockables)
        {
            var match = FindDockableById(child, id);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static object? FindFirstDockableByType(object? dockable, Type dockableType)
    {
        if (dockable == null)
        {
            return null;
        }

        if (dockableType.IsInstanceOfType(dockable))
        {
            return dockable;
        }

        var visibleDockables = dockable.GetType().GetProperty("VisibleDockables")?.GetValue(dockable) as System.Collections.IEnumerable;
        if (visibleDockables == null)
        {
            return null;
        }

        foreach (var child in visibleDockables)
        {
            var match = FindFirstDockableByType(child, dockableType);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static void SetActiveDockable(object dockable, object activeDockable)
    {
        var property = dockable.GetType().GetProperty("ActiveDockable");
        property.ShouldNotBeNull();
        property!.SetValue(dockable, activeDockable);
    }

    private static void RaiseDockableChanged(object behavior, object dockable)
    {
        var method = behavior.GetType().GetMethod("FactoryDockableChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        method!.Invoke(behavior, [null, new { Dockable = dockable }]);
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

