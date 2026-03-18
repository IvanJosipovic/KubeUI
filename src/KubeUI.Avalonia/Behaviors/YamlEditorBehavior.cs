using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Behaviors;

public sealed class YamlEditorBehavior : Behavior<TextEditor>
{
    private Installation? _textMateInstallation;
    private RegistryOptions _registryOptions = null!;
    private FoldingManager? _foldingManager;

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

        AssociatedObject.TextChanged += Editor_TextChanged;
        Application.Current!.ActualThemeVariantChanged += ThemeChanged;

        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.AttachedToVisualTree += AttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += DetachedFromVisualTree;

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            InitializeEditor(vm);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceYamlViewModel vm)
        {
            InitializeEditor(vm);
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
        if (AssociatedObject?.DataContext is ResourceYamlViewModel vm)
        {
            UpdateFoldings();

            Dispatcher.UIThread.Post(() =>
            {
                if (AssociatedObject.GetScrollViewer() is ScrollViewer sc)
                {
                    sc.Offset = vm.ScrollOffset;
                }
            }, DispatcherPriority.Normal);
        }
    }

    private void DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceYamlViewModel vm && _foldingManager != null)
        {
            vm.AllFoldings = [.. _foldingManager.AllFoldings
                .Select(f =>
                {
                    var tag = (NewFolding)f.Tag;
                    tag.DefaultClosed = f.IsFolded;
                    return tag;
                })];

            if (AssociatedObject.GetScrollViewer() is ScrollViewer sc)
            {
                vm.ScrollOffset = sc.Offset;
            }
        }
    }

    protected override void OnDetaching()
    {
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

    private void Editor_TextChanged(object? sender, EventArgs e) => UpdateFoldings();

    private void UpdateFoldings()
    {
        if (_foldingManager == null || AssociatedObject == null)
        {
            return;
        }

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            if (vm.AllFoldings != null)
            {
                try
                {
                    _foldingManager.UpdateFoldings(vm.AllFoldings, -1);
                }
                catch (Exception ex)
                {
                    var logger = Application.Current?.GetRequiredService<ILogger<YamlEditorBehavior>>();
                    logger?.LogWarning(ex, "Error loading foldings");
                }
            }

            YamlFoldingStrategy.UpdateFoldings(_foldingManager, AssociatedObject.Document);
        }
    }
}
