using Avalonia.Controls.Selection;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class PortForwarderListViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IPlatformServices _platformServices;
    private DynamicTableViewSource<PortForwarder, string>? _tableSource;

    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    [ObservableProperty]
    public partial PortForwarder? SelectedItem { get; set; }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new InvalidOperationException("Port-forwarder table source has not been initialized.");

    public PortForwarderListViewModel(IDialogService dialogService, IPlatformServices platformServices)
    {
        _dialogService = dialogService;
        _platformServices = platformServices;
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
            Cluster.Runtime.RemovePortForward(pf);
        }
    }

    private bool CanRemove(PortForwarder pf)
    {
        return pf != null;
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private async Task Open(PortForwarder pf)
    {
        await _platformServices.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    }

    private bool CanOpen(PortForwarder pf)
    {
        return pf != null;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Id = cluster.Runtime.Name + nameof(PortForwarderListViewModel);
        if (_tableSource is not null)
        {
            _tableSource.SelectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
            _tableSource.Dispose();
        }
        _tableSource = DynamicTableViewSource<PortForwarder, string>.FromObservableCollection(
            cluster.Runtime.PortForwarders,
            static item => $"{item.Namespace}\u001f{item.Type}\u001f{item.Name}\u001f{item.Port}",
            [
                CreateColumn("type", static item => item.Type, Assets.Resources.PortForwarderListView_Type!, DynamicTableViewWidthMode.Pixel, 80),
                CreateColumn("name", static item => item.Name, Assets.Resources.PortForwarderListView_Name!, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("namespace", static item => item.Namespace, Assets.Resources.PortForwarderListView_Namespace!, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("port", static item => item.Port, Assets.Resources.PortForwarderListView_Port!, DynamicTableViewWidthMode.Pixel, 80),
                CreateColumn("local-port", static item => item.LocalPort, Assets.Resources.PortForwarderListView_LocalPort!, DynamicTableViewWidthMode.Pixel, 100),
                CreateColumn("connections", static item => item.Connections, Assets.Resources.PortForwarderListView_Connections!, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("status", static item => item.Status, Assets.Resources.PortForwarderListView_Status!, DynamicTableViewWidthMode.Pixel, 140)
            ],
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference
            });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SelectionModel.SelectionChanged += SelectionModelOnSelectionChanged;
        OnPropertyChanged(nameof(TableSource));
    }

    public void Dispose()
    {
        if (_tableSource is null)
            return;
        _tableSource.SelectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
        _tableSource.Dispose();
        _tableSource = null;
    }

    partial void OnSelectedItemChanged(PortForwarder? value)
    {
        if (_tableSource is not null && !ReferenceEquals(_tableSource.SelectionModel.SelectedItem, value))
            _tableSource.SelectionModel.SelectedItem = value;
    }

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        => SelectedItem = _tableSource?.SelectionModel.SelectedItem as PortForwarder;

    private static DynamicTableViewColumn<PortForwarder> CreateColumn<TValue>(
        string key,
        Func<PortForwarder, TValue> selector,
        string header,
        DynamicTableViewWidthMode widthMode,
        double width)
    {
        var column = DynamicTableViewColumn<PortForwarder>.Create(key, header, selector);
        column.WidthMode = widthMode;
        column.Width = width;
        return column;
    }
}
