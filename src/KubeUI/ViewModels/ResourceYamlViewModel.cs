using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using k8s;
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

public partial class ResourceYamlViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private Cluster? _cluster;

    [ObservableProperty]
    private IKubernetesObject<V1ObjectMeta>? _object;

    [ObservableProperty]
    private TextDocument? _yamlDocument;

    [ObservableProperty]
    private bool _editMode;

    [ObservableProperty]
    private bool _hideNoisyFields = true;

    private void SetYamlDocument()
    {
        if (!EditMode && HideNoisyFields)
        {
            var ObjectClone = CloneObject(Object);

            ObjectClone.Metadata.ManagedFields = null;

            ObjectClone?.Metadata?.Annotations?.Remove("kubectl.kubernetes.io/last-applied-configuration");

            YamlDocument = new TextDocument(Client.Serialization.KubernetesYaml.Serialize(ObjectClone));
        }
        else
        {
            YamlDocument = new TextDocument(Client.Serialization.KubernetesYaml.Serialize(Object));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Object) || e.PropertyName == nameof(EditMode) || e.PropertyName == nameof(HideNoisyFields))
        {
            SetYamlDocument();
        }

        if (e.PropertyName == nameof(Cluster))
        {
            Cluster.OnChange += Cluster_OnChange;
        }
    }

    private async void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Object != null
            && Object.Kind == resource.Kind
            && Object.ApiVersion == resource.ApiVersion
            && Object.Metadata.Name == resource.Metadata.Name
            && Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty)
        {
            await Dispatcher.UIThread.InvokeAsync(() => Object = resource);
        }
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

    [RelayCommand(CanExecute = nameof(CanSetHideNoisyFields))]
    private void SetHideNoisyFields()
    {
        HideNoisyFields = !HideNoisyFields;
    }

    private bool CanSetHideNoisyFields()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanSetEditMode))]
    private void SetEditMode()
    {
        EditMode = !EditMode;
    }

    private bool CanSetEditMode()
    {
        return true;
    }

    public IKubernetesObject<V1ObjectMeta> CloneObject(object obj)
    {
        var source = Client.Serialization.KubernetesYaml.Serialize(obj);

        return (IKubernetesObject<V1ObjectMeta>)Client.Serialization.KubernetesYaml.Deserializer.Deserialize(source, obj.GetType())!;
    }

    public void Dispose()
    {
        if (Cluster != null)
        {
            Cluster.OnChange -= Cluster_OnChange;
        }
    }
}
