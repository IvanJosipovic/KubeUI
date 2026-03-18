using Avalonia;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using Shouldly;
using k8s.Models;
using KubeUI.Avalonia.Behaviors;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Avalonia.Views;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Tests;

public class ResourceYamlViewModelTests
{
    [Fact]
    public void YamlFolding_FoldsNestedMappings()
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

    [Fact]
    public void YamlFolding_FoldsMappingWithSequenceChildren()
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

    [Fact]
    public void YamlFolding_FoldsNestedSequences()
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

    [Fact]
    public void YamlFolding_IgnoresBlankAndCommentLines()
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

    [Fact]
    public void YamlFolding_DoesNotFoldFlatMappings()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2: val
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(0);
    }

    [Fact]
    public void YamlFolding_DoesNotFoldListItemsWithoutChildren()
    {
        var text = new TextDocument();
        text.Text = """
            - item1
            - item2
            """.ReplaceLineEndings("\n");

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.ShouldBe(0);
    }

    [Fact]
    public void YamlFolding_FoldsListItemWithChildren()
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
    public async Task yaml_editor_preserves_fold_state_when_viewmodel_is_rebound()
    {
        var window = new MainWindow
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
        foldingManager.AllFoldings.Count.ShouldBeGreaterThan(0);

        foldingManager.AllFoldings.First().IsFolded = true;

        view.DataContext = null;
        Dispatcher.UIThread.RunJobs();

        view.DataContext = vm;
        Dispatcher.UIThread.RunJobs();

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task yaml_editor_preserves_scroll_offset_when_viewmodel_is_rebound()
    {
        var window = new MainWindow
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

        vm.YamlDocument.Text = string.Join('\n', Enumerable.Range(0, 200).Select(i => $"line{i}: value"));
        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();

        var targetOffset = new Vector(0, 120);
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        var savedOffset = vm.ScrollOffset;
        savedOffset.ShouldBe(targetOffset);

        view.DataContext = null;
        Dispatcher.UIThread.RunJobs();

        view.DataContext = vm;
        Dispatcher.UIThread.RunJobs();

        scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        scrollViewer.Offset.ShouldBe(targetOffset);
    }

    [AvaloniaFact]
    public async Task yaml_editor_preserves_scroll_and_folds_when_object_updates()
    {
        var window = new MainWindow
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
        foldingManager.AllFoldings.Count.ShouldBeGreaterThan(0);
        foldingManager.AllFoldings.First().IsFolded = true;

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        var targetOffset = new Vector(0, 120);
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

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

        vm.ScrollOffset.ShouldBe(targetOffset);

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task yaml_editor_preserves_fold_when_object_grows_above_saved_fold()
    {
        var window = new MainWindow
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

        var specFold = foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "spec:");
        specFold.IsFolded = true;

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        var targetOffset = new Vector(0, 100);
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: false));
        Dispatcher.UIThread.RunJobs();

        vm.ScrollOffset.ShouldBe(targetOffset);

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "spec:").IsFolded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task yaml_editor_preserves_fold_when_object_grows_below_saved_fold()
    {
        var window = new MainWindow
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

        var metadataFold = foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "metadata:");
        metadataFold.IsFolded = true;

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        var targetOffset = new Vector(0, 80);
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        Dispatcher.UIThread.RunJobs();

        vm.ScrollOffset.ShouldBe(targetOffset);

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "metadata:").IsFolded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task yaml_editor_preserves_nested_fold_when_object_updates_inside_parent()
    {
        var window = new MainWindow
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

        var nestedFold = foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "containers:");
        nestedFold.IsFolded = true;

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        var targetOffset = new Vector(0, 90);
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        await cluster.AddOrUpdateResource(CreatePod("test", includeLabels: true, extraEnv: true));
        Dispatcher.UIThread.RunJobs();

        vm.ScrollOffset.ShouldBe(targetOffset);

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.Single(x => x.Name.TrimEnd() == "containers:").IsFolded.ShouldBeTrue();
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
}

