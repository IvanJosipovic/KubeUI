using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using KubeUI.Client;
using Microsoft.Extensions.DependencyInjection;
using XtermSharp;

namespace KubeUI.Views;

public sealed partial class PodConsoleView : UserControl
{
    private readonly ILogger<PodConsoleView> _logger;
    private readonly ISettingsService _settingsService;

    public PodConsoleViewModel? ViewModel => DataContext as PodConsoleViewModel;

    public PodConsoleView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<PodConsoleView>>();
        _settingsService = Application.Current.GetRequiredService<ISettingsService>();

        InitializeComponent();

        TextEditorControl = this.FindControl<TextEditor>("TextEditorControl");

        if (TextEditorControl != null)
        {
            InitializeEditor();
        }
    }

    private void InitializeEditor()
    {
        if (TextEditorControl == null) return;

        // Remove caret navigation key bindings
        foreach (var cb in TextEditorControl.TextArea.DefaultInputHandler.CaretNavigation.CommandBindings
                     .Where(x => x.Command.Gesture != null).ToList())
        {
            TextEditorControl.TextArea.DefaultInputHandler.CaretNavigation.CommandBindings.Remove(cb);
        }

        TextEditorControl.TextArea.DefaultInputHandler.CaretNavigation.KeyBindings.Clear();

        // Key handling
        TextEditorControl.TextArea.KeyDown += (s, e) =>
        {
            // Block default processing so we fully control input
            e.Handled = true;
        };

        TextEditorControl.TextArea.KeyUp += (s, e) =>
        {
            if (ViewModel is null)
                return;

            try
            {
                HandleTerminalKey(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing terminal key");
            }
            finally
            {
                e.Handled = true;
            }
        };

        // Caret update after text changes
        TextEditorControl.TextChanged += (_, _) =>
        {
            if (ViewModel?.Terminal is { } term)
            {
                // Match original caret positioning logic
                TextEditorControl.TextArea.Caret.Line = term.Buffer.Y - term.Buffer.YDisp + term.Buffer.YBase + 1;
                TextEditorControl.TextArea.Caret.Column = term.Buffer.X + 1;
            }
        };

        // Add colorizer
        if (ViewModel?.Terminal is { } terminal)
        {
            TextEditorControl.TextArea.TextView.LineTransformers.Add(new ConsoleColorizer(terminal));
        }
    }

    private void HandleTerminalKey(KeyEventArgs e)
    {
        var vm = ViewModel;
        if (vm == null)
            return;

        // CTRL combinations
        if (e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.A: vm.Send((byte)0x01); return;
                case Key.B: vm.Send((byte)0x02); return;
                case Key.C: vm.Send((byte)0x03); return;
                case Key.D: vm.Send((byte)0x04); return;
                case Key.E: vm.Send((byte)0x05); return;
                case Key.F: vm.Send((byte)0x06); return;
                case Key.G: vm.Send((byte)0x07); return;
                case Key.H: vm.Send((byte)0x08); return;
                case Key.I: vm.Send((byte)0x09); return;
                case Key.J: vm.Send((byte)0x0A); return;
                case Key.K: vm.Send((byte)0x0B); return;
                case Key.L: vm.Send((byte)0x0C); return;
                case Key.M: vm.Send((byte)0x0D); return;
                case Key.N: vm.Send((byte)0x0E); return;
                case Key.O: vm.Send((byte)0x0F); return;
                case Key.P: vm.Send((byte)0x10); return;
                case Key.Q: vm.Send((byte)0x11); return;
                case Key.R: vm.Send((byte)0x12); return;
                case Key.S: vm.Send((byte)0x13); return;
                case Key.T: vm.Send((byte)0x14); return;
                case Key.U: vm.Send((byte)0x15); return;
                case Key.V:
                    _ = vm.Paste();
                    return;
                case Key.W: vm.Send((byte)0x17); return;
                case Key.X: vm.Send((byte)0x18); return;
                case Key.Y: vm.Send((byte)0x19); return;
                case Key.Z: vm.Send((byte)0x1A); return;
                case Key.D1: vm.Send((byte)0x31); return;
                case Key.D2: vm.Send((byte)0x32); return;
                case Key.D3: vm.Send((byte)0x33); return;
                case Key.D4: vm.Send((byte)0x34); return;
                case Key.D5: vm.Send((byte)0x35); return;
                case Key.D6: vm.Send((byte)0x36); return;
                case Key.D7: vm.Send((byte)0x37); return;
                case Key.D8: vm.Send((byte)0x38); return;
                case Key.D9: vm.Send((byte)0x39); return;
                case Key.D0: vm.Send((byte)0x30); return;
                case Key.OemOpenBrackets: vm.Send((byte)0x1B); return;
                case Key.OemBackslash: vm.Send((byte)0x1C); return;
                case Key.OemCloseBrackets: vm.Send((byte)0x1D); return;
                case Key.Space: vm.Send((byte)0x00); return;
                case Key.OemMinus: vm.Send((byte)0x1F); return;
                default:
                    vm.Send(e.KeySymbol);
                    return;
            }
        }

        // ALT sends ESC prefix
        if (e.KeyModifiers == KeyModifiers.Alt)
        {
            vm.Send((byte)0x1B);
            vm.Send(e.KeySymbol);
            return;
        }

        // Other keys
        switch (e.Key)
        {
            case Key.Escape:
                vm.Send((byte)0x1B);
                break;
            case Key.Space:
                vm.Send((byte)0x20);
                break;
            case Key.Delete:
                vm.Send(EscapeSequences.CmdDelKey);
                break;
            case Key.Back:
                vm.Send((byte)0x7F);
                break;
            case Key.Up:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveUpApp : EscapeSequences.MoveUpNormal);
                break;
            case Key.Down:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveDownApp : EscapeSequences.MoveDownNormal);
                break;
            case Key.Left:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveLeftApp : EscapeSequences.MoveLeftNormal);
                break;
            case Key.Right:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveRightApp : EscapeSequences.MoveRightNormal);
                break;
            case Key.PageUp:
                if (vm.Terminal.ApplicationCursor)
                    vm.Send(EscapeSequences.CmdPageUp);
                // else: scrolling not yet implemented
                break;
            case Key.PageDown:
                if (vm.Terminal.ApplicationCursor)
                    vm.Send(EscapeSequences.CmdPageDown);
                // else: scrolling not yet implemented
                break;
            case Key.Home:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveHomeApp : EscapeSequences.MoveHomeNormal);
                break;
            case Key.End:
                vm.Send(vm.Terminal.ApplicationCursor ? EscapeSequences.MoveEndApp : EscapeSequences.MoveEndNormal);
                break;
            case Key.F1: vm.Send(EscapeSequences.CmdF[0]); break;
            case Key.F2: vm.Send(EscapeSequences.CmdF[1]); break;
            case Key.F3: vm.Send(EscapeSequences.CmdF[2]); break;
            case Key.F4: vm.Send(EscapeSequences.CmdF[3]); break;
            case Key.F5: vm.Send(EscapeSequences.CmdF[4]); break;
            case Key.F6: vm.Send(EscapeSequences.CmdF[5]); break;
            case Key.F7: vm.Send(EscapeSequences.CmdF[6]); break;
            case Key.F8: vm.Send(EscapeSequences.CmdF[7]); break;
            case Key.F9: vm.Send(EscapeSequences.CmdF[8]); break;
            case Key.F10: vm.Send(EscapeSequences.CmdF[9]); break;
            case Key.OemBackTab:
                vm.Send(EscapeSequences.CmdBackTab);
                break;
            case Key.Tab:
                vm.Send(EscapeSequences.CmdTab);
                break;
            default:
                vm.Send(e.KeySymbol);
                break;
        }
    }

    private void CopyMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        TextEditorControl?.Copy();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        try
        {
            ViewModel?.Connect().GetAwaiter();
            if (TextEditorControl != null)
            {
                ViewModel?.Resize(TextEditorControl.Bounds.Width, TextEditorControl.Bounds.Height);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to console");
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (TextEditorControl != null)
        {
            ViewModel?.Resize(TextEditorControl.Bounds.Width, TextEditorControl.Bounds.Height);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Re-add colorizer if Terminal changes
        if (TextEditorControl != null && ViewModel?.Terminal is { } term)
        {
            if (!TextEditorControl.TextArea.TextView.LineTransformers.OfType<ConsoleColorizer>().Any())
            {
                TextEditorControl.TextArea.TextView.LineTransformers.Add(new ConsoleColorizer(term));
            }
        }
    }
}
