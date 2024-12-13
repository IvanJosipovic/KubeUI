using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class PortForwarderListViewModel : ViewModelBase, IInitializeCluster
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial PortForwarder? SelectedItem { get; set; }

    public PortForwarderListViewModel()
    {
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
        Title = Resources.PortForwarderListViewModel_Title;
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task Remove(PortForwarder pf)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.PortForwarderListViewModel_Remove_Title,
            Content = string.Format(Resources.PortForwarderListViewModel_Remove_Content, pf.Namespace, pf.Name, pf.Port),
            PrimaryButtonText = Resources.PortForwarderListViewModel_Remove_Primary,
            SecondaryButtonText = Resources.PortForwarderListViewModel_Remove_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            Cluster.RemovePortForward(pf);
        }
    }

    private bool CanRemove(PortForwarder pf)
    {
        return pf != null;
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private async Task Open(PortForwarder pf)
    {
        var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
        await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    }

    private bool CanOpen(PortForwarder pf)
    {
        return pf != null;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Id = cluster.Name + nameof(PortForwarderListViewModel);
    }
}
