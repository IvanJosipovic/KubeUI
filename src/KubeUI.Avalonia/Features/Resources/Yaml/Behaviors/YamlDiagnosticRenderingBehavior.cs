using System.ComponentModel;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlDiagnosticRenderingBehavior : Behavior<TextEditor>
{
    private static readonly AttachedProperty<YamlDiagnosticRenderer?> s_rendererProperty =
        AvaloniaProperty.RegisterAttached<YamlDiagnosticRenderingBehavior, TextEditor, YamlDiagnosticRenderer?>("Renderer");

    private readonly YamlDiagnosticRenderer _renderer = new();
    private ResourceYamlViewModel? _currentViewModel;

    internal static YamlDiagnosticRenderer? GetRenderer(TextEditor editor)
    {
        return editor.GetValue(s_rendererProperty);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        AssociatedObject.SetValue(s_rendererProperty, _renderer);
        AssociatedObject.TextArea.TextView.BackgroundRenderers.Add(_renderer);
        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.TextChanged += OnTextChanged;

        UpdateCurrentViewModel(AssociatedObject.DataContext as ResourceYamlViewModel);
        UpdateValidationDiagnostics();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.TextArea.TextView.BackgroundRenderers.Remove(_renderer);
            AssociatedObject.ClearValue(s_rendererProperty);
        }

        DetachViewModel(_currentViewModel);
        _currentViewModel = null;

        base.OnDetaching();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as ResourceYamlViewModel);
        UpdateValidationDiagnostics();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        UpdateValidationDiagnostics();
    }

    private void UpdateCurrentViewModel(ResourceYamlViewModel? nextViewModel)
    {
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            return;
        }

        DetachViewModel(_currentViewModel);
        _currentViewModel = nextViewModel;
        AttachViewModel(nextViewModel);
    }

    private void AttachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void DetachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged -= ViewModelOnPropertyChanged;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResourceYamlViewModel.ValidationDiagnostics))
        {
            UpdateValidationDiagnostics();
        }
    }

    private void UpdateValidationDiagnostics()
    {
        _renderer.Update(AssociatedObject?.Document, _currentViewModel?.ValidationDiagnostics ?? []);
    }
}
