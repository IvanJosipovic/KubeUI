using Dock.Model.Mvvm.Controls;

namespace KubeUI.ViewModels;

public sealed class CustomDocumentDock : DocumentDock
{
    public CustomDocumentDock()
    {
        CanCreateDocument = false;
    }
}
