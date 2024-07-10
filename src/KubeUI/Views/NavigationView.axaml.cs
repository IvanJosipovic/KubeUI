using KubeUI.ViewModels;

namespace KubeUI.Views;

public partial class NavigationView : UserControl
{
    public NavigationView()
    {
        InitializeComponent();
    }

    private void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1 && DataContext is NavigationViewModel model)
        {
            model.TreeView_SelectionChanged(e.AddedItems[0]);
        }
    }
}
