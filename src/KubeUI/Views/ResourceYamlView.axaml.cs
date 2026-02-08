using Avalonia.Styling;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using k8s.Models;
using KubeUI.Client;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed partial class ResourceYamlView : UserControl
{
    private readonly ILogger<ResourceYamlView> _logger;
    private readonly ISettingsService _settingsService;

    private Installation? _textMateInstallation;
    private readonly RegistryOptions _registryOptions = null!;
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

        _registryOptions = new RegistryOptions(Application.Current!.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        Editor.TextChanged += Editor_TextChanged;
        Application.Current!.ActualThemeVariantChanged += ThemeChanged;

#if DEBUG
        if (Design.IsDesignMode)
        {
            var cluster = Application.Current.GetRequiredService<ClusterManager>().GetDefault();
            cluster.Connect();
            var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

            var obj = new V1Namespace()
            {
                Metadata = new()
                {
                    Name = "test"
                }
            };

            vm.Initialize(cluster, obj);

            DataContext = vm;
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ResourceYamlViewModel vm)
        {
            Editor.Document = vm.YamlDocument;

            _textMateInstallation = Editor.InstallTextMate(_registryOptions, true);
            _textMateInstallation.SetGrammar(_registryOptions
                .GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));

            _foldingManager = FoldingManager.Install(Editor.TextArea);

            UpdateFoldings();


        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is ResourceYamlViewModel vm)
        {
            SetOffset(vm.ScrollOffset);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

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
        if (DataContext is ResourceYamlViewModel vm)
        {
            if (vm.AllFoldings != null)
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
            YamlFoldingStrategy.UpdateFoldings(_foldingManager, Editor.Document);
        }
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
    // Plan (pseudocode):
    // 1) For each line, skip blank/comment-only lines as fold starts.
    // 2) Find the next non-blank/comment line.
    // 3) If the next meaningful line is not more indented, no fold.
    // 4) If it is more indented, create a fold that spans until the last line
    //    that is either blank/comment or has indent greater than the base indent.
    // 5) Ensure end offsets are safe even when trailing blank/comment lines exist.

    public static void UpdateFoldings(FoldingManager? manager, TextDocument? document)
    {
        if (manager == null || document == null) return;
        var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    private static int CountLeadingSpaces(ReadOnlyMemory<char> memory, out bool isSequenceItem)
    {
        var span = memory.Span;
        var count = 0;
        while (count < span.Length && span[count] == ' ')
            count++;

        isSequenceItem = count < span.Length && span[count] == '-' &&
            (count + 1 == span.Length || span[count + 1] == ' ');

        return count;
    }

    private static bool IsMappingKey(ReadOnlyMemory<char> memory)
    {
        var span = memory.Span;
        var hashIndex = span.IndexOf('#');
        if (hashIndex >= 0)
            span = span[..hashIndex];

        span = span.TrimEnd();
        return span.Length > 0 && span[^1] == ':';
    }

    private static bool IsBlankLine(ReadOnlyMemory<char> memory) =>
        memory.Span.Trim().Length == 0;

    private static bool IsCommentLine(ReadOnlyMemory<char> memory) =>
        memory.Span.TrimStart().StartsWith("#");

    private static string GetTrimmedLineString(ReadOnlyMemory<char> memory)
    {
        var span = memory.Span;
        var start = 0;
        while (start < span.Length && char.IsWhiteSpace(span[start]))
            start++;
        return start == 0 ? span.ToString() : new string(span[start..]);
    }

    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        try
        {
            var foldMarkers = new List<NewFolding>();

            foreach (var line in document.Lines)
            {
                var text = document.GetTextAsMemory(line.Offset, line.Length);
                if (IsBlankLine(text) || IsCommentLine(text))
                    continue;

                var baseIndent = CountLeadingSpaces(text, out var baseIsSequenceItem);
                var baseIsMappingKey = IsMappingKey(text);
                var baseAllowsSameIndentSequence = baseIsMappingKey && !baseIsSequenceItem;
                var nextLine = line.NextLine;

                // Find the first meaningful next line.
                while (nextLine != null)
                {
                    var nextText = document.GetTextAsMemory(nextLine.Offset, nextLine.Length);
                    if (IsBlankLine(nextText) || IsCommentLine(nextText))
                    {
                        nextLine = nextLine.NextLine;
                        continue;
                    }
                    break;
                }

                if (nextLine == null)
                    continue;

                var nextLineText = document.GetTextAsMemory(nextLine.Offset, nextLine.Length);
                var nextIndent = CountLeadingSpaces(nextLineText, out var nextIsSequenceItem);

                var isFoldStart = nextIndent > baseIndent;
                if (!isFoldStart && baseAllowsSameIndentSequence &&
                    nextIndent == baseIndent && nextIsSequenceItem)
                {
                    isFoldStart = true;
                }

                if (!isFoldStart)
                    continue;

                var fold = new NewFolding
                {
                    StartOffset = line.Offset,
                    Name = GetTrimmedLineString(text)
                };

                var endLine = nextLine;
                var scan = nextLine.NextLine;

                while (scan != null)
                {
                    var scanText = document.GetTextAsMemory(scan.Offset, scan.Length);
                    if (IsBlankLine(scanText) || IsCommentLine(scanText))
                    {
                        endLine = scan;
                        scan = scan.NextLine;
                        continue;
                    }

                    var scanIndent = CountLeadingSpaces(scanText, out var scanIsSequenceItem);
                    if (scanIndent < baseIndent)
                        break;
                    if (scanIndent == baseIndent)
                    {
                        if (!baseAllowsSameIndentSequence || !scanIsSequenceItem)
                            break;
                    }

                    endLine = scan;
                    scan = scan.NextLine;
                }

                fold.EndOffset = endLine.EndOffset;
                foldMarkers.Add(fold);
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
}
