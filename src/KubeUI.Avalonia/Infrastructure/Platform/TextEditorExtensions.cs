using AvaloniaEdit;
using Avalonia.VisualTree;

namespace KubeUI.Avalonia.Infrastructure.Platform;

public static class TextEditorExtensions
{
    extension(TextEditor source)
    {
        public ScrollViewer? GetScrollViewer() => source.FindDescendantOfType<ScrollViewer>();
    }
}
