using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;

namespace KubeUI.ViewModels;

public sealed class CustomDocumentDock : DocumentDock
{
    public CustomDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var index = VisibleDockables?.Count + 1;
        var document = Application.Current.GetRequiredService<ClusterListViewModel>();
        document.Title = $"Document{index}";
        document.Id = $"Document{index}";

        //new DocumentViewModel { Id = $"Document{index}", Title = $"Document{index}" };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
