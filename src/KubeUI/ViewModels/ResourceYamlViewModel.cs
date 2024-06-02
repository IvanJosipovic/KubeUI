using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DemoResourceYamlViewModel : ResourceYamlViewModel
{
    public DemoResourceYamlViewModel()
    {
        Object = new V1Pod()
        {
            Metadata = new()
            {
                Name = "Demo",
                NamespaceProperty = "NS"
            },
            Spec = new()
            {
                Containers = new List<V1Container>()
                {
                    new()
                    {
                        Image = "test"
                    },
                    new()
                    {
                        Image = "test2"
                    }
                }
            }
        };
    }
}

public partial class ResourceYamlViewModel : ViewModelBase
{
    [ObservableProperty]
    private Cluster _cluster;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(YamlDocument))]
    private object _object;

    private TextDocument _yamlDocument;

    public TextDocument YamlDocument {
        get {
            _yamlDocument ??= new TextDocument(Client.Serialization.KubernetesYaml.Serialize(Object));
            return _yamlDocument;
        }
        set { _yamlDocument = value; }
    }

    public ResourceYamlViewModel()
    {
        Title = "Yaml";
        Id = "Yaml";
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _yamlDocument.UndoStack.Undo();
    }

    private bool CanUndo()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(_yamlDocument.Text);
        using MemoryStream stream = new MemoryStream(byteArray);
        Cluster.ImportYaml(stream);
    }

    private bool CanSave()
    {
        return true;
    }
}
