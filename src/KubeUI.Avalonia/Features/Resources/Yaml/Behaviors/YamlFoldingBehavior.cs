using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using KubeUI.Avalonia.Features.Resources.Yaml;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

/// <summary>Enables nested YAML folding for editors that do not use resource-specific YAML editing.</summary>
internal sealed class YamlFoldingBehavior : Behavior<TextEditor>
{
    private FoldingManager? _foldingManager;
    private LeakSafeFoldingMargin? _foldingMargin;
    private DispatcherTimer? _updateTimer;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null)
        {
            return;
        }

        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _updateTimer.Tick += UpdateTimerOnTick;
        AssociatedObject.TextChanged += TextChanged;
        AssociatedObject.AttachedToVisualTree += AttachedToVisualTree;

        _foldingManager = FoldingManager.Install(AssociatedObject.TextArea);
        var defaultMargin = AssociatedObject.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        _foldingMargin = new LeakSafeFoldingMargin { FoldingManager = defaultMargin.FoldingManager };
        var marginIndex = AssociatedObject.TextArea.LeftMargins.IndexOf(defaultMargin);
        AssociatedObject.TextArea.LeftMargins.RemoveAt(marginIndex);
        AssociatedObject.TextArea.LeftMargins.Insert(marginIndex, _foldingMargin);
        UpdateFoldings();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TextChanged -= TextChanged;
            AssociatedObject.AttachedToVisualTree -= AttachedToVisualTree;
        }

        if (_updateTimer is not null)
        {
            _updateTimer.Stop();
            _updateTimer.Tick -= UpdateTimerOnTick;
            _updateTimer = null;
        }

        if (_foldingManager is not null)
        {
            if (_foldingMargin is not null)
            {
                LeakSafeFoldingMargin.ClearLogicalChildren(_foldingMargin);
                AssociatedObject?.TextArea.LeftMargins.Remove(_foldingMargin);
                _foldingMargin = null;
            }

            FoldingManager.Uninstall(_foldingManager);
            _foldingManager = null;
        }

        base.OnDetaching();
    }

    private void TextChanged(object? sender, EventArgs e)
    {
        if (_updateTimer is null)
        {
            return;
        }

        _updateTimer.Stop();
        _updateTimer.Start();
    }

    private void AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) => UpdateFoldings();

    private void UpdateTimerOnTick(object? sender, EventArgs e)
    {
        _updateTimer?.Stop();
        UpdateFoldings();
    }

    private void UpdateFoldings()
    {
        if (_foldingManager is null || AssociatedObject?.Document is not { } document)
        {
            return;
        }

        _foldingManager.UpdateFoldings(YamlFoldingStrategy.CreateNewFoldings(document, out var firstErrorOffset), firstErrorOffset);
    }
}
