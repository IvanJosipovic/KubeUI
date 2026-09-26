using Avalonia.Controls;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Documentation.Tests.Infra;

internal sealed record WalkthroughStart(
    Func<ClusterWorkspace, WalkthroughDemoResources, IServiceProvider, Control> CreateView,
    bool ConnectToCluster);

internal sealed record WalkthroughViewModelContext<TViewModel>(
    ClusterWorkspace Workspace,
    WalkthroughDemoResources DemoResources,
    TViewModel ViewModel)
    where TViewModel : class;

internal enum WalkthroughActionKind
{
    Click,
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
    TimeSpan DelayBefore = default);

internal sealed record WalkthroughStep(
    string Narration,
    IReadOnlyList<WalkthroughAction> Actions,
    Func<Control, bool>? ReadyWhen);

internal sealed class KubeUIWalkthrough
{
    private readonly List<WalkthroughStep> _steps = [];
    private readonly List<WalkthroughAction> _actions = [];
    private WalkthroughStart? _start;
    private string _clusterName = "docs-demo";
    private string? _narration;
    private Func<Control, bool>? _readyWhen;
    private TimeSpan _delayBeforeNextAction;

    private KubeUIWalkthrough(string theme, string clipName)
    {
        Theme = theme;
        ClipName = clipName;
    }

    public string Theme { get; }

    public string ClipName { get; }

    public static KubeUIWalkthrough Create(string theme, string clipName) => new(theme, clipName);

    public KubeUIWalkthrough FakeCluster(string name)
    {
        _clusterName = name;
        return this;
    }

    public KubeUIWalkthrough StartAt<TView, TViewModel>(bool connectToCluster = true)
        where TView : Control
        where TViewModel : class, IInitializeCluster =>
        StartAt<TView, TViewModel>(
            x => x.ViewModel.Initialize(x.Workspace),
            connectToCluster);

    public KubeUIWalkthrough StartAt<TView, TViewModel>(
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

    public KubeUIWalkthrough Speak(string narration, Func<Control, bool>? readyWhen = null)
    {
        AddCurrentStep();
        _narration = narration;
        _readyWhen = readyWhen;
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
            start,
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

        if (_actions.Count == 0)
        {
            throw new InvalidOperationException("Walkthrough narration has no actions.");
        }

        _steps.Add(new WalkthroughStep(_narration, [.. _actions], _readyWhen));
        _actions.Clear();
        _narration = null;
        _readyWhen = null;
        _delayBeforeNextAction = TimeSpan.Zero;
    }
}
