using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using AvaloniaEdit;
using static AvaloniaEdit.TextMate.TextMate;
using TextMateSharp.Grammars;
using Avalonia.Markup.Xaml.MarkupExtensions;
using XtermSharp;
using AvaloniaEdit.Highlighting;

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
                            }),
                        //new Label()
                        //    .Margin(0,0,4,0)
                        //    .Content(new MultiBinding(){
                        //        Bindings = [
                        //                new Binding("Width"),
                        //                new Binding("Height"),
                        //        ],
                        //        StringFormat = "{0}x{1}"
                        //    }),
                        //new Label()
                        //    .Margin(0,0,4,0)
                        //    .Content(new MultiBinding(){
                        //        Bindings = [
                        //                new Binding("Terminal.Cols"),
                        //                new Binding("Terminal.Rows"),
                        //        ],
                        //        StringFormat = "{0}x{1}"
                        //    })
                    ]),
                    new Grid()
                        .Children([

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

                                    x.TextArea.OnKeyDown((e) => {
                                        e.Handled = true;
                                    });

                                    x.TextArea.OnKeyUp((e) => {
                                        if (DataContext is PodConsoleViewModel dc1)
                                        {
                                            if (e.KeyModifiers is KeyModifiers.Control)
                                            {
                                                switch (e.Key)
                                                {
                                                    case Key.A:
                                                        dc1.Send(0x01);  // Ctrl+A
                                                        break;
                                                    case Key.B:
                                                        dc1.Send(0x02);  // Ctrl+B
                                                        break;
                                                    case Key.C:
                                                        dc1.Send(0x03);  // Ctrl+C
                                                        break;
                                                    case Key.D:
                                                        dc1.Send(0x04);  // Ctrl+D
                                                        break;
                                                    case Key.E:
                                                        dc1.Send(0x05);  // Ctrl+E
                                                        break;
                                                    case Key.F:
                                                        dc1.Send(0x06);  // Ctrl+F
                                                        break;
                                                    case Key.G:
                                                        dc1.Send(0x07);  // Ctrl+G
                                                        break;
                                                    case Key.H:
                                                        dc1.Send(0x08);  // Ctrl+H
                                                        break;
                                                    case Key.I:
                                                        dc1.Send(0x09);  // Ctrl+I (Tab)
                                                        break;
                                                    case Key.J:
                                                        dc1.Send(0x0A);  // Ctrl+J (Line Feed)
                                                        break;
                                                    case Key.K:
                                                        dc1.Send(0x0B);  // Ctrl+K
                                                        break;
                                                    case Key.L:
                                                        dc1.Send(0x0C);  // Ctrl+L
                                                        break;
                                                    case Key.M:
                                                        dc1.Send(0x0D);  // Ctrl+M (Carriage Return)
                                                        break;
                                                    case Key.N:
                                                        dc1.Send(0x0E);  // Ctrl+N
                                                        break;
                                                    case Key.O:
                                                        dc1.Send(0x0F);  // Ctrl+O
                                                        break;
                                                    case Key.P:
                                                        dc1.Send(0x10);  // Ctrl+P
                                                        break;
                                                    case Key.Q:
                                                        dc1.Send(0x11);  // Ctrl+Q
                                                        break;
                                                    case Key.R:
                                                        dc1.Send(0x12);  // Ctrl+R
                                                        break;
                                                    case Key.S:
                                                        dc1.Send(0x13);  // Ctrl+S
                                                        break;
                                                    case Key.T:
                                                        dc1.Send(0x14);  // Ctrl+T
                                                        break;
                                                    case Key.U:
                                                        dc1.Send(0x15);  // Ctrl+U
                                                        break;
                                                    case Key.V:
                                                        _ = dc1.Paste();
                                                        //dc1.Send(0x16);  // Ctrl+V
                                                        break;
                                                    case Key.W:
                                                        dc1.Send(0x17);  // Ctrl+W
                                                        break;
                                                    case Key.X:
                                                        dc1.Send(0x18);  // Ctrl+X
                                                        break;
                                                    case Key.Y:
                                                        dc1.Send(0x19);  // Ctrl+Y
                                                        break;
                                                    case Key.Z:
                                                        dc1.Send(0x1A);  // Ctrl+Z
                                                        break;
                                                    case Key.D1: // Ctrl+1
                                                        dc1.Send(0x31);  // ASCII '1'
                                                        break;
                                                    case Key.D2: // Ctrl+2
                                                        dc1.Send(0x32);  // ASCII '2'
                                                        break;
                                                    case Key.D3: // Ctrl+3
                                                        dc1.Send(0x33);  // ASCII '3'
                                                        break;
                                                    case Key.D4: // Ctrl+4
                                                        dc1.Send(0x34);  // ASCII '4'
                                                        break;
                                                    case Key.D5: // Ctrl+5
                                                        dc1.Send(0x35);  // ASCII '5'
                                                        break;
                                                    case Key.D6: // Ctrl+6
                                                        dc1.Send(0x36);  // ASCII '6'
                                                        break;
                                                    case Key.D7: // Ctrl+7
                                                        dc1.Send(0x37);  // ASCII '7'
                                                        break;
                                                    case Key.D8: // Ctrl+8
                                                        dc1.Send(0x38);  // ASCII '8'
                                                        break;
                                                    case Key.D9: // Ctrl+9
                                                        dc1.Send(0x39);  // ASCII '9'
                                                        break;
                                                    case Key.D0: // Ctrl+0
                                                        dc1.Send(0x30);  // ASCII '0'
                                                        break;
                                                    case Key.OemOpenBrackets: // Ctrl+[
                                                        dc1.Send(0x1B);
                                                        break;
                                                    case Key.OemBackslash: // Ctrl+\
                                                        dc1.Send(0x1C);
                                                        break;
                                                    case Key.OemCloseBrackets: // Ctrl+]
                                                        dc1.Send(0x1D);
                                                        break;
                                                    case Key.Space: // Ctrl+Space
                                                        dc1.Send(0x00);
                                                        break;
                                                    case Key.OemMinus: // Ctrl+_
                                                        dc1.Send(0x1F);
                                                        break;
                                                    default:
                                                        dc1.Send(e.KeySymbol);
                                                        break;
                                                }
                                            }
                                            if (e.KeyModifiers is KeyModifiers.Alt)
                                            {
                                                dc1.Send(0x1B);
                                                dc1.Send(e.KeySymbol);
                                            }
                                            else
                                            {
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
                                                        {
                                                            dc1.Send(EscapeSequences.CmdPageUp);
                                                        } else {
                                                            // TODO: view should scroll one page up.
                                                        }
                                                        break;
                                                    case Key.PageDown:
                                                        if (vm.Terminal.ApplicationCursor)
                                                        {
                                                            dc1.Send(EscapeSequences.CmdPageDown);
                                                        } else {
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
                                                    case Key.Tab:
                                                        dc1.Send(EscapeSequences.CmdTab);
                                                        break;
                                                    default:
                                                        dc1.Send(e.KeySymbol);
                                                        break;
                                                    }
                                            }

                                            e.Handled = true;
                                        }
                                    });

                                    // Add Coloring
                                    x.TextArea.TextView.LineTransformers.Add(new RichTextColorizer(vm.ConsoleColor));

                                    // Remove Caret keyboard navigation
                                    foreach (var command in x.TextArea.DefaultInputHandler.CaretNavigation.CommandBindings.Where(x => x.Command.Gesture != null).ToList())
                                    {
                                        x.TextArea.DefaultInputHandler.CaretNavigation.CommandBindings.Remove(command);
                                    }

                                    x.TextArea.DefaultInputHandler.CaretNavigation.KeyBindings.Clear();
                                })
                                .OnTextChanged((e) => {
                                    editor.TextArea.Caret.Line =  vm.Terminal.Buffer.Y - vm.Terminal.Buffer.YDisp + vm.Terminal.Buffer.YBase + 1;
                                    editor.TextArea.Caret.Column = vm.Terminal.Buffer.X + 1;
                                })
                                .Document(@vm.Console, BindingMode.OneWay)
                                .FontFamily(@vm.FontFamily)
                                .FontSize(@vm.FontSize)
                                .IsReadOnly(true)
                                .ShowLineNumbers(false)
                                .Background(new DynamicResourceExtension("SystemAltHighColor"))
                                .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
                                .VerticalScrollBarVisibility(ScrollBarVisibility.Disabled)
                                .ContextMenu(new ContextMenu()
                                                .Items([
                                                    new MenuItem()
                                                        .OnClick((_) => editor.Copy())
                                                        .Header(Assets.Resources.Action_Copy)
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        try
        {
            ViewModel.Connect().GetAwaiter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Console1");
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }
}
