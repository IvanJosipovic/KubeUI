using Avalonia;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed record ResourceEventItem(
    string Message,
    string Source,
    int Count,
    string SubObject,
    string LastSeen,
    bool IsWarning)
{
    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public Thickness CardPadding => HasMessage
        ? new Thickness(8)
        : new Thickness(8, 2, 8, 8);
}
