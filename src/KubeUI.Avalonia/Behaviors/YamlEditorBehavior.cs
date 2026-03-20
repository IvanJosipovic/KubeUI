using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Behaviors;

public sealed class YamlEditorBehavior : Behavior<TextEditor>
{
    private Installation? _textMateInstallation;
    private RegistryOptions _registryOptions = null!;
    private FoldingManager? _foldingManager;
    private ResourceYamlViewModel? _currentViewModel;
    private readonly Dictionary<string, Queue<bool>> _savedFoldStates = new(StringComparer.Ordinal);
    private bool _isRefreshingFromViewModel;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        _registryOptions = new RegistryOptions(Application.Current!.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        // Keep behavior lifecycle tied to ViewModel attach/detach only.

        AssociatedObject.TextChanged += Editor_TextChanged;
        Application.Current!.ActualThemeVariantChanged += ThemeChanged;

        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.AttachedToVisualTree += AttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += DetachedFromVisualTree;

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            _currentViewModel = vm;
            SubscribeViewModel(vm);
            InitializeEditor(vm);
        }
    }

    // Factory event wiring removed. Behavior relies on DataContext attach/detach
    // and visual tree attach/detach for fold-state persistence.
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        var nextViewModel = AssociatedObject.DataContext as ResourceYamlViewModel;
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            if (nextViewModel != null)
            {
                InitializeEditor(nextViewModel);
            }

            return;
        }

        var previousViewModel = _currentViewModel;
        PersistFoldingState(previousViewModel, persistToViewModel: true);
        UnsubscribeViewModel(_currentViewModel);
        _currentViewModel = nextViewModel;

        if (nextViewModel != null && !ReferenceEquals(previousViewModel, nextViewModel))
        {
            _savedFoldStates.Clear();
            LoadSavedFoldStates(nextViewModel);
        }

        SubscribeViewModel(_currentViewModel);

        if (nextViewModel != null)
        {
            InitializeEditor(nextViewModel);
        }
    }

    private void InitializeEditor(ResourceYamlViewModel vm)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        AssociatedObject.Document = vm.YamlDocument;

        if (_textMateInstallation == null)
        {
            _textMateInstallation = AssociatedObject.InstallTextMate(_registryOptions, true);
            _textMateInstallation.SetGrammar(_registryOptions
                .GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));
        }

        if (_foldingManager == null)
        {
            _foldingManager = FoldingManager.Install(AssociatedObject.TextArea);
        }

        UpdateFoldings();
    }

    private void AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceYamlViewModel)
        {
            UpdateFoldings();
        }
    }

    private void DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PersistFoldingState(_currentViewModel, persistToViewModel: true);
    }

    protected override void OnDetaching()
    {
        PersistFoldingState(_currentViewModel, persistToViewModel: true);

        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.AttachedToVisualTree -= AttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= DetachedFromVisualTree;
            AssociatedObject.TextChanged -= Editor_TextChanged;
        }

        Application.Current!.ActualThemeVariantChanged -= ThemeChanged;

        _textMateInstallation?.Dispose();
        _textMateInstallation = null;

        if (_foldingManager != null)
        {
            FoldingManager.Uninstall(_foldingManager);
            _foldingManager = null;
        }

        UnsubscribeViewModel(_currentViewModel);
        _currentViewModel = null;

        base.OnDetaching();
    }

    private void ThemeChanged(object? sender, EventArgs e)
    {
        if (_textMateInstallation == null)
        {
            return;
        }

        if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        }
        else
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
        }
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (!_isRefreshingFromViewModel)
        {
            PersistFoldingState(_currentViewModel);
        }

        UpdateFoldings();
        _isRefreshingFromViewModel = false;
    }

    private void PersistFoldingState(ResourceYamlViewModel? vm, bool persistToViewModel = false)
    {
        if (vm == null || _foldingManager == null)
        {
            return;
        }

        _savedFoldStates.Clear();
        List<NewFolding>? foldings = persistToViewModel ? [] : null;

        foreach (var folding in _foldingManager.AllFoldings)
        {
            var tag = (NewFolding)folding.Tag;
            tag.DefaultClosed = folding.IsFolded;

            var title = tag.Name.TrimEnd();
            if (!_savedFoldStates.TryGetValue(title, out var states))
            {
                states = new Queue<bool>();
                _savedFoldStates.Add(title, states);
            }

            states.Enqueue(folding.IsFolded);
            foldings?.Add(tag);
        }

        if (foldings != null)
        {
            vm.AllFoldings = foldings;
        }
    }

    private void SubscribeViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void UnsubscribeViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged -= ViewModelOnPropertyChanged;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResourceYamlViewModel.Object)
            or nameof(ResourceYamlViewModel.EditMode)
            or nameof(ResourceYamlViewModel.HideNoisyFields))
        {
            PersistFoldingState(_currentViewModel);
            _isRefreshingFromViewModel = true;
        }
    }

    private void UpdateFoldings()
    {
        if (_foldingManager == null || AssociatedObject == null)
        {
            return;
        }

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            try
            {
                if (_savedFoldStates.Count == 0)
                {
                    LoadSavedFoldStates(vm);
                }

                var newFoldings = YamlFoldingStrategy.CreateNewFoldings(AssociatedObject.Document!, out var firstErrorOffset)
                    .ToList();

                _foldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                RestoreFoldStates();
            }
            catch (Exception ex)
            {
                var logger = Application.Current?.GetRequiredService<ILogger<YamlEditorBehavior>>();
                logger?.LogWarning(ex, "Error loading foldings");
            }
        }
    }

    private void LoadSavedFoldStates(ResourceYamlViewModel vm)
    {
        if (vm.AllFoldings == null)
        {
            return;
        }

        foreach (var folding in vm.AllFoldings)
        {
            var title = folding.Name.TrimEnd();
            if (!_savedFoldStates.TryGetValue(title, out var states))
            {
                states = new Queue<bool>();
                _savedFoldStates.Add(title, states);
            }

            states.Enqueue(folding.DefaultClosed);
        }
    }

    private void RestoreFoldStates()
    {
        foreach (var folding in _foldingManager!.AllFoldings)
        {
            var title = folding.Title.TrimEnd();
            if (_savedFoldStates.TryGetValue(title, out var states) && states.Count > 0)
            {
                folding.IsFolded = states.Dequeue();
            }
        }
    }
}
