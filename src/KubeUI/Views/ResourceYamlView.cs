using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed class ResourceYamlView : MyViewBase<ResourceYamlViewModel>
{
    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public ResourceYamlView()
    {
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
                                    .Command(@vm.SetHideNoisyFieldsCommand)
                                    .Set(ToggleButton.IsCheckedProperty, @vm.HideNoisyFields, BindingMode.OneWay)
                                    .ToolTip(Assets.Resources.ResourceYamlViewer_HideNoisyFields)
                                    .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("eye_hide_regular") }),
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
                            ]),

                    new TextEditor()
                        .Ref(out var editor)
                        .Row(1)
                        .Set(x => {
                            _textMateInstallation = x.InstallTextMate(_registryOptions, false);
                            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));

                            x.Options.AllowScrollBelowDocument = false;
                            x.Options.ShowBoxForControlCharacters = false;
                            x.Options.EnableHyperlinks = false;
                            x.Options.EnableEmailHyperlinks = false;
                            return x;
                        })
                        .Set(TextEditor.DocumentProperty, @vm.YamlDocument, BindingMode.OneWay)
                        .Set(TextEditor.FontFamilyProperty, new FontFamily("Consolas,Menlo,Monospace"))
                        .Set(TextEditor.FontSizeProperty, 14.0)
                        .Set(TextEditor.FontWeightProperty, FontWeight.Normal)
                        .Set(TextEditor.IsReadOnlyProperty, @vm.EditMode, converter: Utilities.InverseBooleanConverter)
                        .Set(TextEditor.ShowLineNumbersProperty, true)
                        .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                        .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                        .ContextMenu([
                                new MenuItem()
                                    .OnClick((x) => editor.Cut())
                                    .Header(Assets.Resources.Action_Cut)
                                    .InputGesture(new KeyGesture(Key.X, KeyModifiers.Control))
                                    .IsVisible(@vm.EditMode)
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("cut_regular") }),
                                new MenuItem()
                                    .OnClick((x) => editor.Copy())
                                    .Header(Assets.Resources.Action_Copy)
                                    .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                                new MenuItem()
                                    .OnClick((x) => editor.Paste())
                                    .Header(Assets.Resources.Action_Paste)
                                    .InputGesture(new KeyGesture(Key.V, KeyModifiers.Control))
                                    .IsVisible(@vm.EditMode)
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("clipboard_paste_regular") }),
                                new MenuItem()
                                    .OnClick((x) => editor.Delete())
                                    .Header(Assets.Resources.Action_Delete)
                                    .InputGesture(new KeyGesture(Key.Back))
                                    .IsVisible(@vm.EditMode)
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("delete_regular") }),

                                new Separator()
                                    .IsVisible(@vm.EditMode),

                                new MenuItem()
                                    .OnClick((x) => editor.Undo())
                                    .Header(Assets.Resources.Action_Undo)
                                    .InputGesture(new KeyGesture(Key.Z, KeyModifiers.Control))
                                    .IsVisible(@vm.EditMode)
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                                new MenuItem()
                                    .OnClick((x) => editor.Redo())
                                    .Header(Assets.Resources.Action_Redo)
                                    .InputGesture(new KeyGesture(Key.Y, KeyModifiers.Control))
                                    .IsVisible(@vm.EditMode)
                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_redo_regular") }),
                            ]),
                ]);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }
}
