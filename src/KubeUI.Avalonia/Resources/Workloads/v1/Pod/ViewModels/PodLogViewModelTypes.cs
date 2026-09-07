using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>Describes a container that can be selected for pod logs.</summary>
public sealed record PodLogContainerOption(string Name, string DisplayName, bool IsInitContainer, bool IsEphemeralContainer = false);

/// <summary>Describes a selected resource that contributes Pods to a log session.</summary>
public sealed partial class PodLogScopeSelectionItem(
    IKubernetesObject<V1ObjectMeta> resource,
    string resourceKind,
    string displayName) : ObservableObject
{
    /// <summary>Gets the selected Kubernetes resource.</summary>
    public IKubernetesObject<V1ObjectMeta> Resource { get; } = resource;

    /// <summary>Gets the selected resource kind.</summary>
    public string ResourceKind { get; } = resourceKind;

    /// <summary>Gets the resource label displayed by the selector.</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>Gets or sets the current resolution status.</summary>
    [ObservableProperty]
    public partial string ResolutionStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of Pods currently resolved from this resource.</summary>
    [ObservableProperty]
    public partial int ResolvedPodCount { get; set; }
}

internal readonly record struct PodLogOutputEntry(string PodName, string ContainerName, string Message);

internal readonly record struct PodLogContainerSelectionKey(string Name, bool IsInitContainer, bool IsEphemeralContainer);

internal enum PodLogDisplayMode
{
    None,
    Container,
    PodAndContainer,
}

internal enum PodLogSourceNodeKind
{
    Resource,
    Pod,
    Container,
}

internal sealed partial class PodLogSourceTreeNode : ObservableObject
{
    private readonly Action<PodLogSourceTreeNode, bool> _selectionChanged;
    private bool _isUpdating;

    public PodLogSourceTreeNode(
        PodLogSourceNodeKind kind,
        string key,
        string displayName,
        object value,
        bool? isChecked,
        Action<PodLogSourceTreeNode, bool> selectionChanged)
    {
        Kind = kind;
        Key = key;
        DisplayName = displayName;
        Value = value;
        _selectionChanged = selectionChanged;
        UpdateIsChecked(isChecked);
    }

    public PodLogSourceNodeKind Kind { get; }

    public string Key { get; }

    [ObservableProperty]
    public partial string DisplayName { get; private set; }

    [ObservableProperty]
    public partial object Value { get; private set; }

    public ObservableCollection<PodLogSourceTreeNode> Children { get; } = [];

    [ObservableProperty]
    public partial bool? IsChecked { get; set; }

    partial void OnIsCheckedChanged(bool? value)
    {
        if (!_isUpdating && value.HasValue)
        {
            _selectionChanged(this, value.Value);
        }
    }

    public void UpdateIsChecked(bool? value)
    {
        _isUpdating = true;
        try
        {
            IsChecked = value;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    public void Update(string displayName, object value)
    {
        DisplayName = displayName;
        Value = value;
    }
}

internal static class PodLogFileNameExtensions
{
    public static string ReplaceInvalidFileNameChars(this string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
        }

        return builder.ToString();
    }

    internal static V1OwnerReference? GetControllerReference(IKubernetesObject<V1ObjectMeta> resource)
    {
        var ownerReferences = resource.Metadata?.OwnerReferences;
        if (ownerReferences is null)
        {
            return null;
        }

        for (var i = 0; i < ownerReferences.Count; i++)
        {
            var ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences.Count > 0 ? ownerReferences[0] : null;
    }
}
