using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
            if (TreeView.SelectedItem is NavigationLink navLink)
            {
                // Get top level from the current control. Alternatively, you can use Window reference instead.
                var topLevel = TopLevel.GetTopLevel(this);
                if (navLink.Id == "load-yaml")
                {
                    Task.Run(async () =>
                    {
                        // Start async operation to open the dialog.
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Load Yaml",
                            AllowMultiple = true,
                            FileTypeFilter = new List<FilePickerFileType>() { new("Yaml") { Patterns = ["*.yaml", ".yml"] } }
                        });

                        foreach (var file in files)
                        {
                            var stream = await file.OpenReadAsync();
                            navLink.Cluster.ImportYaml(stream);
                        }

                    });

                    return;
                }
                else if (navLink.Id == "load-folder")
                {
                    Task.Run(async () =>
                    {
                        // Start async operation to open the dialog.
                        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                        {
                            Title = "Load Yamls in Folder",
                            AllowMultiple = false
                        });

                        foreach (var file in folders)
                        {
                            navLink.Cluster.ImportFolder(file.TryGetLocalPath());
                        }
                    });

                    return;
                }
            }

            ((NavigationViewModel)DataContext).TreeView_SelectionChanged(TreeView.SelectedItem);
        }
    }
}
