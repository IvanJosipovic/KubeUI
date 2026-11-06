using Avalonia.Controls.Selection;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Controls.Platform;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed partial class PortForwarderListViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IPlatformServices _platformServices;
    private IObservableCache<PortForwarder, string>? _portForwarderCache;
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
        Title = L10n.Instance.PortForwarderListView_Title.CurrentValue()!;
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task Remove(PortForwarder pf)
    {
        ContentDialogSettings settings = new()
        {
            Title = L10n.Instance.PortForwarderListView_Remove_Title.CurrentValue()!,
            Content = string.Format(L10n.Instance.PortForwarderListView_Remove_Content.CurrentValue()!, pf.Namespace, pf.Name, pf.Port),
            PrimaryButtonText = L10n.Instance.PortForwarderListView_Remove_Primary.CurrentValue()!,
            SecondaryButtonText = L10n.Instance.PortForwarderListView_Remove_Secondary.CurrentValue()!,
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
            _portForwarderCache?.Dispose();
        }
        _portForwarderCache = cluster.Runtime.PortForwarders
            .ToObservableChangeSet(static item => $"{item.Namespace}\u001f{item.Type}\u001f{item.Name}\u001f{item.Port}")
            .AsObservableCache();
        _tableSource = new DynamicTableViewSource<PortForwarder, string>(
            _portForwarderCache,
            static item => $"{item.Namespace}\u001f{item.Type}\u001f{item.Name}\u001f{item.Port}",
            [
                CreateColumn("type", static item => item.Type, L10n.Instance.PortForwarderListView_Type.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 80),
                CreateColumn("name", static item => item.Name, L10n.Instance.PortForwarderListView_Name.CurrentValue()!, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("namespace", static item => item.Namespace, L10n.Instance.PortForwarderListView_Namespace.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("port", static item => item.Port, L10n.Instance.PortForwarderListView_Port.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 80),
                CreateColumn("local-port", static item => item.LocalPort, L10n.Instance.PortForwarderListView_LocalPort.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 100),
                CreateColumn("connections", static item => item.Connections, L10n.Instance.PortForwarderListView_Connections.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("status", static item => item.Status, L10n.Instance.PortForwarderListView_Status.CurrentValue()!, DynamicTableViewWidthMode.Pixel, 140)
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
        _portForwarderCache?.Dispose();
        _portForwarderCache = null;
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
