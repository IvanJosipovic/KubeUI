using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class PortForwarderListViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private Cluster _cluster;

    [ObservableProperty]
    private PortForwarder? _selectedItem;

    public PortForwarderListViewModel()
    {
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
    }

    [RelayCommand(CanExecute = nameof(CanRemovePortForward))]
    private async Task RemovePortForward(PortForwarder pf)
    {
        ContentDialogSettings settings = new()
        {
            Content = $"Remove the Port Forward? {pf.Namespace}/{pf.PodName}:{pf.ContainerPort}\n\nAre you sure?",
            Title = "Warning",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            Cluster.RemovePortForward(pf);
        }
    }

    private bool CanRemovePortForward(PortForwarder pf)
    {
        return pf != null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenPortForward))]
    private async Task OpenPortForward(PortForwarder pf)
    {
        var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
        await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    }

    private bool CanOpenPortForward(PortForwarder pf)
    {
        return pf != null;
    }
}
