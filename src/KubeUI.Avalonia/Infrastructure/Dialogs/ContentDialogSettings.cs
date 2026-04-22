using FluentAvalonia.UI.Controls;

namespace KubeUI.Avalonia.Infrastructure.Dialogs;

public sealed class ContentDialogSettings
{
    public string? Title { get; set; }

    public object? Content { get; set; }

    public string? PrimaryButtonText { get; set; }

    public string? SecondaryButtonText { get; set; }

    public bool IsPrimaryButtonEnabled { get; set; } = true;

    public bool IsSecondaryButtonEnabled { get; set; } = true;

    public FAContentDialogButton DefaultButton { get; set; } = FAContentDialogButton.None;
}
