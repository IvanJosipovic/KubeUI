using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Properties.ViewModels;

/// <summary>
/// Represents one event card in the resource properties events section.
/// </summary>
public sealed partial class ResourceEventItemViewModel : ViewModelBase
{
    public ResourceEventItemViewModel(
        string message,
        string source,
        int count,
        string subObject,
        string lastSeen,
        bool isWarning)
    {
        Message = message;
        Source = source;
        Count = count;
        SubObject = subObject;
        LastSeen = lastSeen;
        IsWarning = isWarning;
    }

    public string Message { get; }

    public string Source { get; }

    public int Count { get; }

    public string SubObject { get; }

    public string LastSeen { get; }

    public bool IsWarning { get; }
}
