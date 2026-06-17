using System.Reflection;
using AvaloniaEdit;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public static class TextEditorExtensions
{
    private static readonly PropertyInfo s_scrollViewerProperty = typeof(TextEditor).GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic);

    extension(TextEditor source)
    {
        public ScrollViewer? GetScrollViewer()
        {
            if (s_scrollViewerProperty.GetValue(source) is ScrollViewer sc)
                return sc;

            return null;
        }
    }
}
