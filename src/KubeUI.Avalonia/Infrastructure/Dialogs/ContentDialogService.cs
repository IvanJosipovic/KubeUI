using System.ComponentModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using KubeUI.Avalonia.Infrastructure.Platform;

namespace KubeUI.Avalonia.Infrastructure.Dialogs;

public sealed class ContentDialogService : IContentDialogService
{
    public async Task<FAContentDialogResult> ShowContentDialogAsync(INotifyPropertyChanged? owner, ContentDialogSettings settings)
    {
        var topLevel = App.TopLevel ?? TopLevelAccessor.GetRequired();

        var dialog = new FAContentDialog
        {
            Title = settings.Title,
            Content = settings.Content,
            PrimaryButtonText = settings.PrimaryButtonText,
            SecondaryButtonText = settings.SecondaryButtonText,
            IsPrimaryButtonEnabled = settings.IsPrimaryButtonEnabled,
            IsSecondaryButtonEnabled = settings.IsSecondaryButtonEnabled,
            DefaultButton = settings.DefaultButton,
        };

        return await dialog.ShowAsync(topLevel).ConfigureAwait(false);
    }
}
