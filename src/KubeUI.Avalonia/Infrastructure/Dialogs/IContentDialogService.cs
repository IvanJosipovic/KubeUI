using System.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace KubeUI.Avalonia.Infrastructure.Dialogs;

public interface IContentDialogService
{
    Task<FAContentDialogResult> ShowContentDialogAsync(INotifyPropertyChanged? owner, ContentDialogSettings settings);
}
