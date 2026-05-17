using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PortForwarderListViewModel : ViewModelBase, IInitializeCluster
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    [ObservableProperty]
    public partial PortForwarder? SelectedItem { get; set; }

    public PortForwarderListViewModel(ISettingsService settings, IDialogService dialogService)
    {
        Settings = settings;
        _dialogService = dialogService;
        Title = Assets.Resources.PortForwarderListView_Title;
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task Remove(PortForwarder pf)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.PortForwarderListView_Remove_Title,
            Content = string.Format(Assets.Resources.PortForwarderListView_Remove_Content, pf.Namespace, pf.Name, pf.Port),
            PrimaryButtonText = Assets.Resources.PortForwarderListView_Remove_Primary,
            SecondaryButtonText = Assets.Resources.PortForwarderListView_Remove_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == FAContentDialogResult.Primary)
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
        await App.TopLevel!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    }

    private bool CanOpen(PortForwarder pf)
    {
        return pf != null;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        Cluster = cluster;
        Id = cluster.Name + nameof(PortForwarderListViewModel);
    }
}



