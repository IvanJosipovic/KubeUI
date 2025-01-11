using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using AvaloniaEdit;
using static AvaloniaEdit.TextMate.TextMate;
using TextMateSharp.Grammars;
using Avalonia.Markup.Xaml.MarkupExtensions;
using XtermSharp;

namespace KubeUI.Views;

public sealed class PodConsoleView : MyViewBase<PodConsoleViewModel>
{
    private readonly ILogger<PodConsoleView> _logger;

    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public PodConsoleView(ILogger<PodConsoleView> logger)
    {
        _logger = logger;
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

    protected override object Build(PodConsoleViewModel? vm)
    {
        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children([
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Children([
                        new Label()
                            .Margin(0,0,4,0)
                            .Content("Pod: "),
                        new Label()
                            .Margin(0,0,4,0)
                            .Content(new MultiBinding(){
                                Bindings = [
                                        new Binding("Object.Metadata.NamespaceProperty"),
                                        new Binding("Object.Metadata.Name"),
                                        new Binding(nameof(PodConsoleViewModel.ContainerName))
                                ],
                                StringFormat = "{0}/{1}/{2}"
                            })
                    ]),
                new TextEditor()
                    .Ref(out var editor)
                    .Row(1)
                    .Set(x => {
                        _textMateInstallation = editor.InstallTextMate(_registryOptions, false);

                        x.Options.AllowScrollBelowDocument = false;
                        x.Options.ShowBoxForControlCharacters = false;
                        x.Options.EnableHyperlinks = false;
                        x.Options.EnableEmailHyperlinks = false;
                        x.TextArea.Caret.CaretBrush = Brushes.Transparent;
                        x.TextArea.Caret.Hide();
                    })
                    .OnTextChanged((e) => {
                        editor.ScrollToEnd();
                    })
                    .Document(@vm.Console, BindingMode.OneWay)
                    .FontFamily(new FontFamily("Consolas,Menlo,Monospace"))
                    .FontSize(14.0)
                    .FontWeight(FontWeight.Normal)
                    .IsReadOnly(true)
                    .ShowLineNumbers(false)
                    .Background(new DynamicResourceExtension("SystemAltHighColor"))
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .OnKeyUp((e) => {
                        if (DataContext is PodConsoleViewModel dc1)
                        {
                            if (e.KeyModifiers is KeyModifiers.Control)
                            {
                                if (e.Key == Key.C)
                                {
                                    editor.Copy();
                                    return;
                                }
                                if (e.Key == Key.V)
                                {
                                    _ = dc1.Paste();
                                    return;
                                }
                            }

                            switch (e.Key) {
                                case Key.Escape:
                                    dc1.Send(0x1b);
                                    break;
                                case Key.Space:
                                    dc1.Send(0x20);
                                    break;
                                case Key.Delete:
                                    dc1.Send(EscapeSequences.CmdDelKey);
                                    break;
                                case Key.Back:
                                    dc1.Send(0x7f);
                                    break;
                                case Key.Up:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveUpApp : EscapeSequences.MoveUpNormal);
                                    break;
                                case Key.Down:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveDownApp : EscapeSequences.MoveDownNormal);
                                    break;
                                case Key.Left:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveLeftApp : EscapeSequences.MoveLeftNormal);
                                    break;
                                case Key.Right:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveRightApp : EscapeSequences.MoveRightNormal);
                                    break;
                                case Key.PageUp:
                                    if (vm.Terminal.ApplicationCursor)
                                        dc1.Send(EscapeSequences.CmdPageUp);
                                    else {
                                        // TODO: view should scroll one page up.
                                    }
                                    break;
                                case Key.PageDown:
                                    if (vm.Terminal.ApplicationCursor)
                                        dc1.Send(EscapeSequences.CmdPageDown);
                                    else {
                                                            // TODO: view should scroll one page down
                                    }
                                                        break;
                                case Key.Home:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveHomeApp : EscapeSequences.MoveHomeNormal);
                                    break;
                                case Key.End:
                                    dc1.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveEndApp : EscapeSequences.MoveEndNormal);
                                    break;
                                case Key.Insert:
                                    break;
                                case Key.F1:
                                    dc1.Send(EscapeSequences.CmdF [0]);
                                    break;
                                case Key.F2:
                                    dc1.Send(EscapeSequences.CmdF [1]);
                                    break;
                                case Key.F3:
                                    dc1.Send(EscapeSequences.CmdF [2]);
                                    break;
                                case Key.F4:
                                    dc1.Send(EscapeSequences.CmdF [3]);
                                    break;
                                case Key.F5:
                                    dc1.Send(EscapeSequences.CmdF [4]);
                                    break;
                                case Key.F6:
                                    dc1.Send(EscapeSequences.CmdF [5]);
                                    break;
                                case Key.F7:
                                    dc1.Send(EscapeSequences.CmdF [6]);
                                    break;
                                case Key.F8:
                                    dc1.Send(EscapeSequences.CmdF [7]);
                                    break;
                                case Key.F9:
                                    dc1.Send(EscapeSequences.CmdF [8]);
                                    break;
                                case Key.F10:
                                    dc1.Send(EscapeSequences.CmdF [9]);
                                    break;
                                case Key.OemBackTab:
                                    dc1.Send(EscapeSequences.CmdBackTab);
                                    break;
                                default:
                                    dc1.Send(e.KeySymbol);
                                    //if (keyEvent.Key >= Key.ControlA && keyEvent.Key <= Key.ControlZ) {
                                    //    dc1.Send((byte)keyEvent.Key);
                                    //    break;
                                    //}
                                    //if (keyEvent.IsAlt) {
                                    //    dc1.Send(0x1b);
                                    //}
                                    //var rune = (Rune)(uint)keyEvent.Key;
                                    //var len = Rune.RuneLen (rune);
                                    //if (len > 0) {
                                    //    var buff = new byte [len];
                                    //    var n = Rune.EncodeRune (rune, buff);
                                    //    dc1.Send(buff);
                                    //} else {
                                    //    dc1.Send((byte)keyEvent.Key);
                                    //}
                                    break;
                                }

                            e.Handled = true;
                        }
                    })
                    .ContextMenu(new ContextMenu()
                                    .Items([
                                        new MenuItem()
                                            .OnClick((_) => editor.Copy())
                                            .Header(Assets.Resources.Action_Copy)
                                            .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                            .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                                        new MenuItem()
                                            .Command(vm.PasteCommand)
                                            .Header(Assets.Resources.Action_Paste)
                                            .InputGesture(new KeyGesture(Key.V, KeyModifiers.Control))
                                            .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("clipboard_paste_regular") }),
                                    ])
                    ),
            ]);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }
}
