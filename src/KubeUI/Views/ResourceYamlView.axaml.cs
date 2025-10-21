using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using KubeUI.Client;
using Microsoft.Extensions.Logging;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed partial class ResourceYamlView : UserControl
{
    private readonly ILogger<ResourceYamlView> _logger;
    private readonly ISettingsService _settingsService;

    private Installation? _textMateInstallation;
    private RegistryOptions _registryOptions = null!;
    private FoldingManager? _foldingManager;

    public ResourceYamlView()
        : this(
            Application.Current.GetRequiredService<ILogger<ResourceYamlView>>(),
            Application.Current.GetRequiredService<ISettingsService>())
    {
        // Parameterless for XAML / designer.
    }

    public ResourceYamlView(ILogger<ResourceYamlView> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _registryOptions = new RegistryOptions(Application.Current!.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        _textMateInstallation = Editor.InstallTextMate(_registryOptions, true);
        _textMateInstallation.SetGrammar(_registryOptions
            .GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));

        _foldingManager = FoldingManager.Install(Editor.TextArea);

        UpdateFoldings();

        if (DataContext is ResourceYamlViewModel vm && vm.AllFoldings != null)
        {
            try
            {
                _foldingManager.UpdateFoldings(vm.AllFoldings, -1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading foldings");
            }
        }

        Application.Current!.ActualThemeVariantChanged += ThemeChanged;
        Editor.TextChanged += Editor_TextChanged;

        // Restore scroll position
        if (DataContext is ResourceYamlViewModel vm2)
            SetOffset(vm2.ScrollOffset);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Application.Current!.ActualThemeVariantChanged -= ThemeChanged;
        Editor.TextChanged -= Editor_TextChanged;

        _textMateInstallation?.Dispose();
        _textMateInstallation = null;

        if (DataContext is ResourceYamlViewModel vm &&
            _foldingManager is not null)
        {
            vm.AllFoldings = [.. _foldingManager.AllFoldings
                .Select(f =>
                {
                    var tag = (NewFolding)f.Tag;
                    tag.DefaultClosed = f.IsFolded;
                    return tag;
                })];

            vm.ScrollOffset = GetOffset();
        }

        if (_foldingManager != null)
        {
            FoldingManager.Uninstall(_foldingManager);
            _foldingManager = null;
        }
    }

    private void ThemeChanged(object? sender, EventArgs e)
    {
        if (_textMateInstallation == null) return;

        if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        else
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
    }

    private void Editor_TextChanged(object? sender, EventArgs e) => UpdateFoldings();

    private void UpdateFoldings()
    {
        if (_foldingManager == null) return;
        YamlFoldingStrategy.UpdateFoldings(_foldingManager, Editor.Document);
    }

    private void SetOffset(Vector offset)
    {
        if (Editor.GetScrollViewer() is ScrollViewer sc)
            sc.Offset = offset;
    }

    private Vector GetOffset()
    {
        if (Editor.GetScrollViewer() is ScrollViewer sc)
            return sc.Offset;
        return Vector.Zero;
    }

    // Context menu handlers
    private void CutClick(object? s, RoutedEventArgs e) => Editor.Cut();
    private void CopyClick(object? s, RoutedEventArgs e) => Editor.Copy();
    private void PasteClick(object? s, RoutedEventArgs e) => Editor.Paste();
    private void DeleteClick(object? s, RoutedEventArgs e) => Editor.Delete();
    private void UndoClick(object? s, RoutedEventArgs e) => Editor.Undo();
    private void RedoClick(object? s, RoutedEventArgs e) => Editor.Redo();
}

internal static class YamlFoldingStrategy
{
    public static void UpdateFoldings(FoldingManager? manager, TextDocument? document)
    {
        if (manager == null || document == null) return;
        var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        try
        {
            var foldMarkers = new List<NewFolding>();
            var lines = document.Text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var currentLine = lines[i];
                var currentLineIndent = CountIndents(currentLine);
                var lineCount = i;
                NewFolding? fold = null;

                while (IsNextLineFoldable(lines, lineCount, currentLineIndent))
                {
                    fold ??= new NewFolding
                    {
                        StartOffset = document.GetOffset(i + 1, 1),
                        Name = currentLine
                    };
                    lineCount++;
                }

                if (fold != null)
                {
                    fold.EndOffset = document.GetOffset(lineCount + 1, lines[lineCount].Length + 1);
                    foldMarkers.Add(fold);
                }
            }

            firstErrorOffset = -1;
            foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldMarkers;
        }
        catch
        {
            firstErrorOffset = 0;
            return [];
        }
    }

    private static bool IsNextLineFoldable(string[] lines, int i, int currentLineIndent)
    {
        var nextLine = i + 1 < lines.Length ? lines[i + 1] : null;
        if (nextLine == null) return false;

        var nextIndent = CountIndents(nextLine);
        if (nextLine == "\n" || nextLine == "\r" ||
            (currentLineIndent < nextLine.Length &&
             nextLine[currentLineIndent] == '-' &&
             !lines[i].Trim().StartsWith("-")))
            return true;

        return nextIndent > currentLineIndent;
    }

    private static int CountIndents(string line)
    {
        int indent = 0;
        foreach (var ch in line)
        {
            if (ch == ' ') indent++;
            else break;
        }
        return indent;
    }
}
