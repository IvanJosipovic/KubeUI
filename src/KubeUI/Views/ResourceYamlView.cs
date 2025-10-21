using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using KubeUI.Client;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed class ResourceYamlView : MyViewBase<ResourceYamlViewModel>
{
    private readonly ILogger<ResourceYamlView> _logger;

    private readonly ISettingsService _settingsService;

    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    private FoldingManager _foldingManager;

    private TextEditor _textEditor;

    public ResourceYamlView(ILogger<ResourceYamlView> logger, ISettingsService settingsService)
    {
        _logger = logger;

        _settingsService = settingsService;

        _registryOptions = new RegistryOptions(Application.Current.ActualThemeVariant == ThemeVariant.Light ? ThemeName.Light : ThemeName.DarkPlus);

        Application.Current.ActualThemeVariantChanged += Current_ActualThemeVariantChanged;
    }

    private void Current_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        }
        else if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
        }
    }

    protected override object Build(ResourceYamlViewModel? vm)
    {
        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto, *")
            .Children([
                    // In Read Only Mode
                    new StackPanel()
                        .Row(0)
                        .IsVisible(@vm.EditMode, converter: Utilities.InverseBooleanConverter)
                        .Orientation(Orientation.Horizontal)
                        .Children([
                                new Button()
                                    .Command(@vm.SetEditModeCommand)
                                    .ToolTip(Assets.Resources.ResourceYamlViewer_Edit)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("document_edit_regular") }),
                                new ToggleButton()
                                    .IsChecked(@vm.HideNoisyFields, BindingMode.OneWay)
                                    .Command(@vm.SetHideNoisyFieldsCommand)
                                    .ToolTip(Assets.Resources.ResourceYamlViewer_HideNoisyFields)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("eye_hide_regular") }),
                                new ToggleButton()
                                    .IsChecked(@vm.WordWrap)
                                    .ToolTip(Assets.Resources.ResourceYamlView_WordWrap)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("text_wrap_regular") }),
                        ]),

                    // In Edit Mode
                    new StackPanel()
                        .Row(0)
                        .IsVisible(@vm.EditMode)
                        .Orientation(Orientation.Horizontal)
                        .Children([
                                new Button()
                                    .Command(@vm.SaveCommand)
                                    .ToolTip(Assets.Resources.ResourceYamlViewer_Save)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("save_regular") }),
                                new Button()
                                    .Command(@vm.SetEditModeCommand)
                                    .ToolTip(Assets.Resources.ResourceYamlViewer_Cancel)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("dismiss_regular") }),
                                new ToggleButton()
                                    .IsChecked(@vm.WordWrap)
                                    .ToolTip(Assets.Resources.ResourceYamlView_WordWrap)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("text_wrap_regular") }),
                        ]),

                    new TextEditor()
                        .Ref(out _textEditor)
                        .Row(1)
                        .Document(@vm.YamlDocument, BindingMode.OneWay)
                        .Set(x => {
                            x.Options.AllowScrollBelowDocument = false;
                            x.Options.ShowBoxForControlCharacters = false;
                            x.Options.EnableHyperlinks = false;
                            x.Options.EnableEmailHyperlinks = false;
                        })
                        .KeyBindings([
                            new () {
                                Gesture = new KeyGesture(Key.Z, KeyModifiers.Control),
                                Command = vm.UndoCommand,
                            }
                        ])
                        .OnTextChanged((x) => {
                            YamlFoldingStrategy.UpdateFoldings(_foldingManager, _textEditor.Document);
                        })
                        .FontFamily(new FontFamily("Cascadia Mono"))
                        .FontSize(Convert.ToDouble(_settingsService.Settings.ConsoleFontSize))
                        .FontWeight(FontWeight.Normal)
                        .IsReadOnly(@vm.EditMode, converter: Utilities.InverseBooleanConverter)
                        .ShowLineNumbers(true)
                        .Background(new DynamicResourceExtension("SystemAltHighColor"))
                        .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                        .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                        .WordWrap(@vm.WordWrap)
                        .ContextMenu(new ContextMenu()
                                        .Items([
                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Cut())
                                                .Header(Assets.Resources.Action_Cut)
                                                .InputGesture(new KeyGesture(Key.X, KeyModifiers.Control))
                                                .IsVisible(@vm.EditMode)
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("cut_regular") }),
                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Copy())
                                                .Header(Assets.Resources.Action_Copy)
                                                .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Paste())
                                                .Header(Assets.Resources.Action_Paste)
                                                .InputGesture(new KeyGesture(Key.V, KeyModifiers.Control))
                                                .IsVisible(@vm.EditMode)
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("clipboard_paste_regular") }),
                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Delete())
                                                .Header(Assets.Resources.Action_Delete)
                                                .InputGesture(new KeyGesture(Key.Back))
                                                .IsVisible(@vm.EditMode)
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("delete_regular") }),

                                            new Separator()
                                                .IsVisible(@vm.EditMode),

                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Undo())
                                                .Header(Assets.Resources.Action_Undo)
                                                .InputGesture(new KeyGesture(Key.Z, KeyModifiers.Control))
                                                .IsVisible(@vm.EditMode)
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                                            new MenuItem()
                                                .OnClick((x) => _textEditor.Redo())
                                                .Header(Assets.Resources.Action_Redo)
                                                .InputGesture(new KeyGesture(Key.Y, KeyModifiers.Control))
                                                .IsVisible(@vm.EditMode)
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_redo_regular") }),
                                        ])
                        ),
                ]);
    }

    public void SetOffset()
    {
        if (ViewModel != null && _textEditor.GetScrollViewer() is ScrollViewer sc)
        {
            sc.Offset = ViewModel.ScrollOffset;
        }
    }

    public void GetOffset()
    {
        if (ViewModel != null && _textEditor.GetScrollViewer() is ScrollViewer sc)
        {
            ViewModel.ScrollOffset = sc.Offset;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _textMateInstallation = _textEditor.InstallTextMate(_registryOptions, true);
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));

        _foldingManager = FoldingManager.Install(_textEditor.TextArea);

        if (ViewModel?.AllFoldings != null)
        {
            try
            {
                _foldingManager.UpdateFoldings(ViewModel.AllFoldings, -1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error loading foldings: {ex}", ex);
            }
        }

        YamlFoldingStrategy.UpdateFoldings(_foldingManager, _textEditor.Document);

        base.OnLoaded(e);
        SetOffset();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;

        _textMateInstallation.Dispose();

        GetOffset();

        ViewModel.AllFoldings = [.. _foldingManager.AllFoldings.Select(x =>
        {
            var tag = (NewFolding)x.Tag;
            tag.DefaultClosed = x.IsFolded;
            return tag;
        })];

        FoldingManager.Uninstall(_foldingManager);

        base.OnUnloaded(e);
    }
}

internal static class YamlFoldingStrategy
{
    /// <summary>
    /// Create <see cref="NewFolding" />s for the specified document and updates the folding manager with them.
    /// </summary>
    public static void UpdateFoldings(FoldingManager? manager, TextDocument? document)
    {
        if (manager == null || document == null)
        {
            return;
        }

        var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    /// <summary>
    /// Create <see cref="NewFolding" />s for the specified document.
    /// </summary>
    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        try
        {
            if (document == null)
            {
                firstErrorOffset = 0;
                return [];
            }

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
                        Name = currentLine,
                    };

                    lineCount++;
                }

                if (fold != null)
                {
                    fold.EndOffset = document.GetOffset(lineCount + 1, lines[lineCount].Length + 1);

                    foldMarkers.Add(fold);
                    fold = null;
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
        var currentLine = lines[i];
        var nextLine = i + 1 < lines.Length ? lines[i + 1] : null;

        if (nextLine == null)
        {
            return false;
        }

        var nextLineIndents = CountIndents(nextLine);

        if (nextLine == "\n" || nextLine == "\r" ||
            (currentLineIndent < nextLine.Length && nextLine[currentLineIndent] == '-' && !currentLine.Trim(" ").StartsWith("-")))
        {
            return true;
        }

        return nextLineIndents > currentLineIndent;
    }

    private static int CountIndents(string line)
    {
        if (line == null)
        {
            return 0;
        }

        int indentLevel = 0;

        foreach (var ch in line)
        {
            if (ch == ' ')
            {
                indentLevel++;
            }
            else
            {
                break;
            }
        }
        return indentLevel;
    }
}
