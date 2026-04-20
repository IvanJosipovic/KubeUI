using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

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

    private T ResolveService<T>() where T : class
    {
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var service = services.GetRequiredService<T>();
        if (service is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return service;
    }

    private static async Task WaitForValidationDebounceAsync(Func<bool>? predicate = null, int timeoutMs = 2500)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            await Task.Delay(25);
            Dispatcher.UIThread.RunJobs();
            if (predicate == null || predicate())
            {
                return;
            }
        }
        while (sw.ElapsedMilliseconds < timeoutMs);

        Dispatcher.UIThread.RunJobs();
        (predicate?.Invoke() ?? true).ShouldBeTrue();
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

        vm.YamlDocument.Text.ShouldNotContain("updated: \"true\"");

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

        vm.YamlDocument.Text.ShouldContain("updated: \"true\"");

        foldingManager = GetFoldingManager(behavior);
        foldingManager.ShouldNotBeNull();
        foldingManager.AllFoldings.First().IsFolded.ShouldBeTrue();

    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ShowsCompletion_WhenCompletionIsRequested()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
        Dispatcher.UIThread.RunJobs();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldNotBeNull();
        completionWindow!.IsOpen.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void ResourceYamlView_ShowsDocumentationPopupOnHoverOverFieldName()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("spec", StringComparison.Ordinal) + 1;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        var popup = GetDocumentationWindow(editor);
        popup.ShouldNotBeNull();
        popup!.ShouldBeOfType<StackPanel>();
        ToolTip.GetIsOpen(editor).ShouldBeTrue();

        var panel = (StackPanel)popup;
        var title = panel.Children.OfType<TextBlock>().FirstOrDefault();
        title.ShouldNotBeNull();
        title!.Text.ShouldBe("spec");

        var valueOffset = editor.Document!.Text.IndexOf("value", StringComparison.Ordinal) + 1;
        shown = InvokeHoverTooltip(editor, valueOffset, onlyWhenOpen: true);
        shown.ShouldBeFalse();
        Dispatcher.UIThread.RunJobs();

        ToolTip.GetIsOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_ShowsDocumentationPopupOnNestedFieldName()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupOnFieldNamePastTenthLine()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    }

    [AvaloniaFact]
    public void ResourceYamlView_ShowsDocumentationPopupOnNestedFieldPastTenthLine()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupOnKeyAtEndOfLine()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupOnSequenceItemFieldName()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_UpdatesDocumentationPopupWhenHoverMovesBetweenFields()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupAfterBlankLinesAndComments()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_DoesNotShowDocumentationPopupOnColonOrValueBoundary()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var colonOffset = editor.Document!.Text.IndexOf("imagePullPolicy:", StringComparison.Ordinal) + "imagePullPolicy".Length;
        editor.Document.GetLineByOffset(colonOffset).LineNumber.ShouldBeGreaterThan(10);
        var shown = InvokeHoverTooltip(editor, colonOffset);
        shown.ShouldBeFalse();

        var valueOffset = editor.Document.Text.IndexOf("IfNotPresent", StringComparison.Ordinal) + 1;
        shown = InvokeHoverTooltip(editor, valueOffset);
        shown.ShouldBeFalse();

        Dispatcher.UIThread.RunJobs();

        ToolTip.GetIsOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_ClosesDocumentationPopupWhenScrolled()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var fieldNameOffset = editor.Document!.Text.IndexOf("imagePullPolicy", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltip(editor, fieldNameOffset);
        shown.ShouldBeTrue();

        Dispatcher.UIThread.RunJobs();

        ToolTip.GetIsOpen(editor).ShouldBeTrue();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        scrollViewer.Offset = new Vector(0, 80);
        Dispatcher.UIThread.RunJobs();

        ToolTip.GetIsOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_ShowsDocumentationPopupFromRenderedPointOnRootField()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupFromRenderedPointOnNestedSequenceField()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_ShowsDocumentationPopupFromRenderedPointPastTenthLine()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
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
    public void ResourceYamlView_DoesNotShowDocumentationPopupFromRenderedPointOnValue()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var valueOffset = editor.Document!.Text.IndexOf("IfNotPresent", StringComparison.Ordinal) + 2;
        var shown = InvokeHoverTooltipAtPoint(editor, GetPointForOffset(editor, valueOffset));
        shown.ShouldBeFalse();

        Dispatcher.UIThread.RunJobs();

        ToolTip.GetIsOpen(editor).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifest()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

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
    public void ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifestWithoutTrailingNewline()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

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
    public void ResourceYamlView_ShowsDocumentationPopupForImagePullPolicyInCalicoControllerManifestAfterScroll()
    {
        var window = CreateWindow(width: 800, height: 250);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
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
    public void ResourceYamlView_ResolvesViewportPointToImagePullPolicyOffsetAfterScroll()
    {
        var window = CreateWindow(width: 800, height: 250);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
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
    public void ResourceYamlView_CreatesDocumentationTipForImagePullPolicyOffsetAfterScroll()
    {
        var window = CreateWindow(width: 800, height: 250);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        var scrollViewer = editor.GetScrollViewer();
        scrollViewer.ShouldNotBeNull();
        WaitFor(() =>
        {
            Dispatcher.UIThread.RunJobs();
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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.TextArea.PerformTextInput("\n");
        Dispatcher.UIThread.RunJobs();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsSequenceMarker_WhenEnterIsPressedOnSequenceProperty()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.LastIndexOf("containers:", StringComparison.Ordinal) + "containers:".Length;

        editor.TextArea.PerformTextInput("\n");
        Dispatcher.UIThread.RunJobs();

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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.LastIndexOf("command:", StringComparison.Ordinal) + "command:".Length;

        editor.TextArea.PerformTextInput("\n");
        Dispatcher.UIThread.RunJobs();

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
    public async Task ResourceYamlView_ContinuesListItem_WhenEnterIsPressedAtEndOfSequenceEntry()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("\n");
        Dispatcher.UIThread.RunJobs();

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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("\n");
        Dispatcher.UIThread.RunJobs();

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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        editor.TextArea.PerformTextInput("s");
        Dispatcher.UIThread.RunJobs();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsStarterSequence_WhenSelectingListCompletion()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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
                  ar
            """.ReplaceLineEndings("\n");

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        vm.RequestCompletionCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var behavior = Interaction.GetBehaviors(editor).OfType<YamlEditorBehavior>().Single();
        var completionWindow = GetCompletionWindow(behavior);
        completionWindow.ShouldNotBeNull();

        var completionData = completionWindow!.CompletionList.CompletionData
            .OfType<YamlCompletionData>()
            .Single(data => data.Text == "args");

        completionData.Complete(
            editor.TextArea,
            new TextSegment
            {
                StartOffset = completionWindow.StartOffset,
                Length = completionWindow.EndOffset - completionWindow.StartOffset,
            },
            EventArgs.Empty);
        Dispatcher.UIThread.RunJobs();

        editor.Text.ShouldBe(
            "apiVersion: v1\n"
            + "kind: Pod\n"
            + "metadata:\n"
            + "  name: temp\n"
            + "  namespace: default\n"
            + "spec:\n"
            + "  containers:\n"
            + "    - name: test\n"
            + "      args:\n"
            + "        - ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_InsertsStarterObjectBlock_WhenSelectingObjectCompletion()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.CaretOffset = editor.Text.Length;

        vm.RequestCompletionCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

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
        Dispatcher.UIThread.RunJobs();

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
        var service = ResolveService<IYamlValidationService>();

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
    public void YamlSyntaxValidationService_ReturnsDiagnostic_ForUnknownKubernetesField()
    {
        var service = ResolveService<IYamlValidationService>();
        var cluster = CreateTestWorkspace();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n"), cluster.ModelCache);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Severity.ShouldBe(YamlDiagnosticSeverity.Error);
        diagnostics[0].Message.ShouldContain("unknownField");
    }

    [AvaloniaFact]
    public void YamlSyntaxValidationService_AnchorsTypedScalarConversionError_ToValue()
    {
        var service = ResolveService<IYamlValidationService>();
        var cluster = CreateTestWorkspace();

        var diagnostics = service.Validate("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: temp
              namespace: default
            spec:
              activeDeadlineSeconds: a
            """.ReplaceLineEndings("\n"), cluster.ModelCache);

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
        var service = ResolveService<IYamlValidationService>();

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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("expected", StringComparison.OrdinalIgnoreCase));
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Validation failed");
        vm.ActionResultMessage.ShouldContain("expected");
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(InfoBarSeverity.Error);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_TracksStrictKubernetesDiagnostics()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
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
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.ValidationDiagnostics[0].Message.ShouldContain("unknownField");
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Validation failed");
        vm.ActionResultMessage.ShouldContain("unknownField");

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("unknownField", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_TracksUnknownTypeDiagnostics()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

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

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();

        GetDiagnosticMessages(editor).ShouldContain(message => message.Contains("MadeUpKind", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_SaveShowsInlineFailure_WhenYamlIsInvalid()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.YamlDocument.Text = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: [test
            """.ReplaceLineEndings("\n");
        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        await vm.SaveCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Save failed");
        vm.ActionResultMessage.ShouldContain("line");
        vm.ActionResultMessage.ShouldContain("column");
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(InfoBarSeverity.Error);
        TestApp.LastNotification.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DisablesSaveAndDryRun_WhenValidationErrorsExist()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

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
    public async Task ResourceYamlView_DebouncesValidationWhileTypingPartialPropertyName()
    {
        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
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
            spec:
              con
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        vm.ValidationDiagnostics.ShouldBeEmpty();
        vm.HasActionResult.ShouldBeFalse();

        await WaitForValidationDebounceAsync(() => vm.ValidationDiagnostics.Count == 1);

        vm.ValidationDiagnostics.Count.ShouldBe(1);
        vm.HasActionFailureResult.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DryRunShowsInlineSuccess_WhenYamlIsValid()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
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
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        vm.DryRunCommand.CanExecute(null).ShouldBeTrue();

        await vm.DryRunCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        TestApp.LastNotification.ShouldBeNull();
        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run succeeded");
        vm.ActionResultMessage.ShouldBe("The server accepted the manifest using dry-run.");
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(InfoBarSeverity.Success);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_DryRunShowsInlineFailure_WhenServerValidationFails()
    {
        var window = CreateWindow(width: 800, height: 600);

        var runtime = new TestCluster
        {
            DryRunYamlBehavior = _ => Task.FromException(new InvalidOperationException("Server-side validation failed.")),
        };
        var cluster = runtime.CreateWorkspace();
        _disposables.Add(cluster);

        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
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
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        await vm.DryRunCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        TestApp.LastNotification.ShouldBeNull();
        vm.HasActionFailureResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run failed");
        vm.ActionResultMessage.ShouldBe("Server-side validation failed.");
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.Severity.ShouldBe(InfoBarSeverity.Error);
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_KeepsActionResultVisible_WhenYamlChanges()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
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
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        await vm.DryRunCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        vm.HasActionResult.ShouldBeTrue();

        vm.YamlDocument.Insert(vm.YamlDocument.TextLength, "\n# note");
        Dispatcher.UIThread.RunJobs();

        vm.HasActionResult.ShouldBeTrue();
        vm.ActionResultTitle.ShouldBe("Dry run succeeded");
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_ClearsActionResult_WhenDismissed()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
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
            spec:
              containers:
                - name: app
                  image: nginx
            """.ReplaceLineEndings("\n");
        Dispatcher.UIThread.RunJobs();

        await vm.DryRunCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        vm.HasActionSuccessResult.ShouldBeTrue();
        vm.DismissActionResultCommand.CanExecute(null).ShouldBeTrue();
        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeTrue();
        actionBar.IsClosable.ShouldBeTrue();

        vm.DismissActionResultCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        vm.HasActionResult.ShouldBeFalse();
        vm.DismissActionResultCommand.CanExecute(null).ShouldBeFalse();
        actionBar.IsOpen.ShouldBeFalse();
        actionBar.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ResourceYamlView_HidesActionResultBar_WhenThereIsNoMessage()
    {
        var vm = ResolveService<ResourceYamlViewModel>();
        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;

        var actionBar = view.FindControl<InfoBar>("ActionResultBar");
        actionBar.ShouldNotBeNull();
        actionBar.IsOpen.ShouldBeFalse();
        actionBar.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void YamlEditorBehavior_UnindentsEmptyLineByTwoSpaces()
    {
        var editor = new AvaloniaEdit.TextEditor
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
        var editor = new AvaloniaEdit.TextEditor
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
        var editor = new AvaloniaEdit.TextEditor
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
        var editor = new AvaloniaEdit.TextEditor
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
        var editor = new AvaloniaEdit.TextEditor
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
        var editor = new AvaloniaEdit.TextEditor
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
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });
        vm.EditMode = true;

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.Options.IndentationSize.ShouldBe(2);
        editor.Options.ConvertTabsToSpaces.ShouldBeTrue();
        editor.Options.GetIndentationString(1).ShouldBe("  ");
    }

    [AvaloniaFact]
    public async Task ResourceYamlView_LeavesScrollBelowDocumentEnabled()
    {
        var window = CreateWindow(width: 800, height: 600);

        var cluster = CreateTestWorkspace();
        var vm = ResolveService<ResourceYamlViewModel>();
        vm.Initialize(cluster, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default",
            },
        });

        var view = ResolveService<ResourceYamlView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        var editor = view.FindControl<AvaloniaEdit.TextEditor>("Editor");
        editor.ShouldNotBeNull();
        editor.Options.AllowScrollBelowDocument.ShouldBeTrue();
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

    private static CompletionWindow? GetCompletionWindow(YamlEditorBehavior behavior)
    {
        var field = typeof(YamlEditorBehavior).GetField("_completionWindow", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(behavior) as CompletionWindow;
    }

    private static bool InvokeHoverTooltip(AvaloniaEdit.TextEditor editor, int offset, bool onlyWhenOpen = false)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryShowHoverTooltipAtOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (bool)method.Invoke(behavior, [offset, onlyWhenOpen])!;
    }

    private static bool InvokeHoverTooltipAtPoint(AvaloniaEdit.TextEditor editor, Point point, bool onlyWhenOpen = false)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryShowHoverTooltipAtPoint", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (bool)method.Invoke(behavior, [point, onlyWhenOpen])!;
    }

    private static int? TryGetHoverOffset(AvaloniaEdit.TextEditor editor, Point point)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryGetPointerOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        object?[] args = [point, 0];
        var resolved = (bool)method.Invoke(behavior, args)!;
        return resolved ? (int)args[1] : null;
    }

    private static object? TryCreateHoverDocumentationTip(AvaloniaEdit.TextEditor editor, int offset)
    {
        var behavior = Interaction.GetBehaviors(editor).OfType<YamlHoverToolTipBehavior>().Single();
        var method = typeof(YamlHoverToolTipBehavior).GetMethod("TryCreateDocumentationTip", BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        object?[] args = [offset, null!];
        var created = (bool)method.Invoke(behavior, args)!;
        return created ? args[1] : null;
    }

    private static Point GetPointForOffset(AvaloniaEdit.TextEditor editor, int offset)
    {
        var location = editor.Document!.GetLocation(offset);
        var point = editor.TextArea.TextView.GetVisualPosition(new TextViewPosition(location.Line, location.Column), VisualYPosition.LineMiddle);
        return new Point(point.X + 2, point.Y);
    }

    private static Point GetViewportPointForOffset(AvaloniaEdit.TextEditor editor, int offset)
    {
        var point = GetPointForOffset(editor, offset);
        return point - editor.TextArea.TextView.ScrollOffset;
    }

    private static object? GetDocumentationWindow(AvaloniaEdit.TextEditor editor)
    {
        return global::Avalonia.Controls.ToolTip.GetTip(editor);
    }

    private static IReadOnlyList<string> GetDiagnosticMessages(AvaloniaEdit.TextEditor editor)
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
            if (predicate())
                return;
            System.Threading.Thread.Sleep(10);
        }
        predicate().ShouldBeTrue();
    }
}
