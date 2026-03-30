using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public class SafeSelectableTextBlock : SelectableTextBlock
{
    protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

    public SafeSelectableTextBlock()
    {
        CopyingToClipboard += OnCopyingToClipboard;
    }

    private void OnCopyingToClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        _ = CopySelectionSafelyAsync();
    }

    private async Task CopySelectionSafelyAsync()
    {
        try
        {
            var text = SelectedText;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var clipboard = topLevel?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                await clipboard.SetTextAsync(text);
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.SetTextAsync(text));
        }
        catch (COMException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
