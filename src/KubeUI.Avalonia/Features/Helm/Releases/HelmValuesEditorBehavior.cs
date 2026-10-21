using System.ComponentModel;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace KubeUI.Avalonia.Features.Helm.Releases;

internal sealed class HelmValuesEditorBehavior : Behavior<TextEditor>
{
    private HelmReleaseEditorViewModel? _viewModel;
    private bool _updatingEditor;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.DataContextChanged += DataContextChanged;
        AssociatedObject.TextChanged += EditorTextChanged;
        SetViewModel(AssociatedObject.DataContext as HelmReleaseEditorViewModel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DataContextChanged -= DataContextChanged;
        AssociatedObject.TextChanged -= EditorTextChanged;
        SetViewModel(null);
        base.OnDetaching();
    }

    private void DataContextChanged(object? sender, EventArgs args)
        => SetViewModel(AssociatedObject.DataContext as HelmReleaseEditorViewModel);

    private void SetViewModel(HelmReleaseEditorViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
            UpdateEditor(_viewModel.ValuesYaml);
        }
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(HelmReleaseEditorViewModel.ValuesYaml) && _viewModel is not null)
        {
            UpdateEditor(_viewModel.ValuesYaml);
        }
    }

    private void EditorTextChanged(object? sender, EventArgs args)
    {
        if (!_updatingEditor && _viewModel is not null)
        {
            _viewModel.ValuesYaml = AssociatedObject.Text;
        }
    }

    private void UpdateEditor(string text)
    {
        if (string.Equals(AssociatedObject.Text, text, StringComparison.Ordinal))
        {
            return;
        }

        _updatingEditor = true;
        try
        {
            AssociatedObject.Text = text;
        }
        finally
        {
            _updatingEditor = false;
        }
    }
}
