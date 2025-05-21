using Avalonia.Controls.Models.TreeDataGrid;
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
    public partial FlatTreeDataGridSource<PortForwarder> Source { get; set; }

    public PortForwarderListViewModel()
    {
        _dialogService = Application.Current.GetRequiredService<IDialogService>();
        Title = Assets.Resources.PortForwarderListViewModel_Title;
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task Remove(PortForwarder pf)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.PortForwarderListViewModel_Remove_Title,
            Content = string.Format(Assets.Resources.PortForwarderListViewModel_Remove_Content, pf.Namespace, pf.Name, pf.Port),
            PrimaryButtonText = Assets.Resources.PortForwarderListViewModel_Remove_Primary,
            SecondaryButtonText = Assets.Resources.PortForwarderListViewModel_Remove_Secondary,
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

        Source = new FlatTreeDataGridSource<PortForwarder>(Cluster.PortForwarders)
        {
            Columns =
            {
                new TextColumn<PortForwarder, string>("Type", x => x.Type),
                new TextColumn<PortForwarder, string>("Name", x => x.Name, new GridLength(1, GridUnitType.Star)),
                new TextColumn<PortForwarder, string>("Namespace", x => x.Namespace),
                new TextColumn<PortForwarder, int>("Port", x => x.Port),
                new TextColumn<PortForwarder, int>("Local Port", x => x.LocalPort),
                new TextColumn<PortForwarder, int>("Connections", x => x.Connections),
                new TextColumn<PortForwarder, string>("Status", x => x.Status),
            },
        };

        ((ITreeDataGridSource)Source).SortBy(Source.Columns[1], ListSortDirection.Ascending);
    }
}
