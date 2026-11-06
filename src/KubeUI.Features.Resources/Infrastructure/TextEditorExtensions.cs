using AvaloniaEdit;
using Avalonia.VisualTree;

namespace KubeUI.Features.Resources.Infrastructure;

public static class TextEditorExtensions
{
    extension(TextEditor source)
    {
        public ScrollViewer? GetScrollViewer() => source.FindDescendantOfType<ScrollViewer>();
    }
}
