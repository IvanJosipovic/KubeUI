using Avalonia.Controls;
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
        if (TreeView.SelectedItem != null)
        {
            ((NavigationViewModel)DataContext!).TreeView_SelectionChanged(TreeView.SelectedItem);
        }
    }
}
