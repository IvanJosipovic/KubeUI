using Avalonia.VisualTree;
using AvaloniaEdit;

namespace KubeUI.Features.Resources.Infrastructure;

public static class TextEditorExtensions
{
    extension(TextEditor source)
    {
        public ScrollViewer? GetScrollViewer() => source.FindDescendantOfType<ScrollViewer>();
    }
}
