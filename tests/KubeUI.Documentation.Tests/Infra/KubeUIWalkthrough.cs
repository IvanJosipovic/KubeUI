using Avalonia.Controls;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Testing.Kubernetes.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Documentation.Tests.Infra;

internal sealed record WalkthroughStart(
    Func<ClusterWorkspace, WalkthroughDemoResources, IServiceProvider, Control> CreateView,
    bool ConnectToCluster);

internal sealed record WalkthroughIntro(string Title, string Narration);

internal sealed record WalkthroughViewModelContext<TViewModel>(
    ClusterWorkspace Workspace,
    WalkthroughDemoResources DemoResources,
    TViewModel ViewModel)
    where TViewModel : class;

internal enum WalkthroughActionKind
{
    Click,
    RightClick,
    Hover,
    MovePointer,
    SetText,
    InsertText,
    TypeText,
    BackspaceText,
    ExploreCompletion,
}

internal sealed record WalkthroughAction(
    WalkthroughActionKind Kind,
    string Target,
    string? Value = null,
    TimeSpan DelayBefore = default,
    Func<Control, bool>? ReadyWhen = null);

internal sealed record WalkthroughStep(
    string Narration,
    IReadOnlyList<WalkthroughAction> Actions,
    Func<Control, bool>? ReadyWhen,
    Func<Control, string>? ReadyDescription);

internal sealed class KubeUIWalkthrough
{
    private readonly List<WalkthroughStep> _steps = [];
    private readonly List<WalkthroughAction> _actions = [];
    private WalkthroughStart? _start;
    private WalkthroughIntro? _intro;
    private string _clusterName = "docs-demo";
    private Action<TestClusterConfig>? _configureFakeCluster;
    private string? _narration;
    private Func<Control, bool>? _readyWhen;
    private Func<Control, string>? _readyDescription;
    private TimeSpan _delayBeforeNextAction;

    private KubeUIWalkthrough(string theme, string clipName)
    {
        Theme = theme;
        ClipName = clipName;
    }

    public string Theme { get; }

    public string ClipName { get; }

    public static KubeUIWalkthrough Create(string theme, string clipName) => new(theme, clipName);

    public KubeUIWalkthrough FakeCluster(string name, Action<TestClusterConfig>? configure = null)
    {
        _clusterName = name;
        _configureFakeCluster = configure;
        return this;
    }

    public KubeUIWalkthrough Intro(string title, string narration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(narration);
        if (_intro is not null)
        {
            throw new InvalidOperationException("A walkthrough can have only one intro.");
        }

        if (_steps.Count > 0 || _narration is not null)
        {
            throw new InvalidOperationException("Add the walkthrough intro before narrated steps.");
        }

        _intro = new WalkthroughIntro(title, narration);
        return this;
    }

    public KubeUIWalkthrough StartAt<TView, TViewModel>(bool connectToCluster = true)
        where TView : Control
        where TViewModel : class, IInitializeCluster =>
        LoadView<TView, TViewModel>(
            x => x.ViewModel.Initialize(x.Workspace),
            connectToCluster);

    public KubeUIWalkthrough LoadView<TView, TViewModel>(
        Action<WalkthroughViewModelContext<TViewModel>> configureViewModel,
        bool connectToCluster = true)
        where TView : Control
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(configureViewModel);
        _start = new WalkthroughStart(
            (workspace, demoResources, services) =>
            {
                var viewModel = services.GetRequiredService<TViewModel>();
                configureViewModel(
                    new WalkthroughViewModelContext<TViewModel>(workspace, demoResources, viewModel));
                var view = services.GetRequiredService<TView>();
                view.DataContext = viewModel;
                return view;
            },
            connectToCluster);
        return this;
    }

    public KubeUIWalkthrough Speak(
        string narration,
        Func<Control, bool>? readyWhen = null,
        Func<Control, string>? readyDescription = null)
    {
        AddCurrentStep();
        _narration = narration;
        _readyWhen = readyWhen;
        _readyDescription = readyDescription;
        return this;
    }

    public KubeUIWalkthrough DelayBeforeNextAction(TimeSpan delay)
    {
        if (_narration is null)
        {
            throw new InvalidOperationException("Add narration before delaying an action.");
        }

        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Action delay cannot be negative.");
        }

        _delayBeforeNextAction = delay;
        return this;
    }

    public KubeUIWalkthrough OpenCluster(string name)
    {
        AddAction(new(WalkthroughActionKind.Click, "cluster", name));
        return this;
    }

    public KubeUIWalkthrough SelectNavigation(string name)
    {
        AddAction(new(WalkthroughActionKind.Click, "navigation", name));
        return this;
    }

    public KubeUIWalkthrough SelectPod(string name)
    {
        AddAction(new(WalkthroughActionKind.Click, "pod", name));
        return this;
    }

    public KubeUIWalkthrough SelectPodLogs(string name, Func<Control, bool> logsReadyWhen)
    {
        ArgumentNullException.ThrowIfNull(logsReadyWhen);
        AddAction(new(WalkthroughActionKind.RightClick, "pod", name));
        AddAction(new(
            WalkthroughActionKind.Click,
            "context-menu-item",
            "View Logs",
            ReadyWhen: root => KubeUIWalkthroughRecorder.HasContextMenuItem(root, "Open New Logs View")));
        AddAction(new(
            WalkthroughActionKind.Click,
            "context-menu-item",
            "Open New Logs View",
            ReadyWhen: logsReadyWhen));
        return this;
    }

    public KubeUIWalkthrough RightClickPod(string name)
    {
        AddAction(new(WalkthroughActionKind.RightClick, "pod", name));
        return this;
    }

    public KubeUIWalkthrough SelectContextMenuItem(string name, Func<Control, bool>? readyWhen = null)
    {
        AddAction(new(WalkthroughActionKind.Click, "context-menu-item", name, ReadyWhen: readyWhen));
        return this;
    }

    public KubeUIWalkthrough JumpToLogController(string resourceKind)
    {
        AddAction(new(WalkthroughActionKind.Click, "logs-controller", resourceKind));
        return this;
    }

    public KubeUIWalkthrough ClickEditor()
    {
        AddAction(new(WalkthroughActionKind.Click, "yaml-editor"));
        return this;
    }

    public KubeUIWalkthrough ClickRelationshipSurface()
    {
        AddAction(new(WalkthroughActionKind.Click, "relationship-surface"));
        return this;
    }

    public KubeUIWalkthrough HoverYamlHeader(string name)
    {
        AddAction(new(WalkthroughActionKind.Hover, "yaml-header", name));
        return this;
    }

    public KubeUIWalkthrough ClickYamlEditMode()
    {
        AddAction(new(WalkthroughActionKind.Click, "yaml-edit-mode"));
        return this;
    }

    public KubeUIWalkthrough MoveCursorBelowYamlMetadata()
    {
        AddAction(new(WalkthroughActionKind.MovePointer, "yaml-below-metadata"));
        return this;
    }

    public KubeUIWalkthrough ExploreYamlCompletion(string field)
    {
        AddAction(new(WalkthroughActionKind.ExploreCompletion, "yaml-editor", field));
        return this;
    }

    public KubeUIWalkthrough InsertYamlText(string text)
    {
        AddAction(new(WalkthroughActionKind.InsertText, "yaml-editor", text));
        return this;
    }

    public KubeUIWalkthrough TypeYamlText(string text)
    {
        AddAction(new(WalkthroughActionKind.TypeText, "yaml-editor", text));
        return this;
    }

    public KubeUIWalkthrough BackspaceYamlText(string expectedSuffix)
    {
        AddAction(new(WalkthroughActionKind.BackspaceText, "yaml-editor", expectedSuffix));
        return this;
    }

    public KubeUIWalkthrough SetYamlText(string yaml)
    {
        AddAction(new(WalkthroughActionKind.SetText, "yaml-editor", yaml));
        return this;
    }

    public KubeUIWalkthrough DryRunYaml()
    {
        AddAction(new(WalkthroughActionKind.Click, "yaml-dry-run"));
        return this;
    }

    public KubeUIWalkthrough SaveYaml()
    {
        AddAction(new(WalkthroughActionKind.Click, "yaml-save"));
        return this;
    }

    public async Task RecordAsync()
    {
        AddCurrentStep();
        var start = _start
            ?? throw new InvalidOperationException("Choose a starting view before recording.");
        await KubeUIWalkthroughRecorder.RecordAsync(
            Theme,
            ClipName,
            _clusterName,
            _configureFakeCluster,
            start,
            _intro,
            _steps);
    }

    private void AddAction(WalkthroughAction action)
    {
        if (_narration is null)
        {
            throw new InvalidOperationException("Add narration before its actions.");
        }

        _actions.Add(action with { DelayBefore = _delayBeforeNextAction });
        _delayBeforeNextAction = TimeSpan.Zero;
    }

    private void AddCurrentStep()
    {
        if (_narration is null)
        {
            return;
        }

        _steps.Add(new WalkthroughStep(_narration, [.. _actions], _readyWhen, _readyDescription));
        _actions.Clear();
        _narration = null;
        _readyWhen = null;
        _readyDescription = null;
        _delayBeforeNextAction = TimeSpan.Zero;
    }
}
