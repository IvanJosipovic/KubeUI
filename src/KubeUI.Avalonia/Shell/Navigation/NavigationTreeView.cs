using Avalonia.Controls;
using Avalonia.Input;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationTreeView : TreeView
{
    protected override Type StyleKeyOverride => typeof(TreeView);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationTreeViewItem();
    }
}

internal sealed class NavigationTreeViewItem : TreeViewItem
{
    protected override Type StyleKeyOverride => typeof(TreeViewItem);

    protected override void OnHeaderDoubleTapped(TappedEventArgs e)
    {
        // Selection handles expansion on each click, so the native double-tap toggle would toggle twice.
        e.Handled = true;
    }
}
