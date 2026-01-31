using System.Globalization;
using DynamicData;
using k8s;
using k8s.Models;
using Kubernetes.Controller.Client;
using KubeUI.Client;
using KubeUI.Converters;

namespace KubeUI.Controls;

public partial class PodContainerCell : UserControl, IInitializeCluster
{
    private ICluster? _cluster;

    private V1Pod? _viewModel;

    [GeneratedDirectProperty]
    public partial ObservableCollection<ViewModel> ContainerStatuses { get; set; } = [];

    public PodContainerCell()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Pod()
            {
                Status = new()
                {
                    ContainerStatuses = new ObservableCollection<V1ContainerStatus>()
                    {
                        new()
                        {
                            Name = "Stopped",
                            Started = false,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = "Starting",
                            Started = true,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = "Ready",
                            Started = true,
                            Ready = true,
                        },
                    },
                    InitContainerStatuses = new ObservableCollection<V1ContainerStatus>()
                    {
                        new()
                        {
                            Name = "Init-Stopped",
                            Started = false,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = "Init-Starting",
                            Started = true,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = "Init-Ready",
                            Started = true,
                            Ready = true,
                        },
                    }
                }
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        PopulateData();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _cluster?.OnChange -= _cluster_OnChange;
    }

    private void PopulateData()
    {
        if(DataContext is V1Pod pod)
        {
            _viewModel = pod;

            var coll = new ObservableCollection<ViewModel>();
            if (pod?.Status?.ContainerStatuses != null)
            {
                coll.AddRange(pod.Status.ContainerStatuses.Select(x => new ViewModel()
                {
                    Name = x.Name,
                    Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), null, CultureInfo.InvariantCulture)
                }));
            }
            if (pod?.Status?.InitContainerStatuses != null)
            {
                coll.AddRange(pod.Status.InitContainerStatuses.Select(x => new ViewModel()
                {
                    Name = x.Name,
                    Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), null, CultureInfo.InvariantCulture)
                }));
            }
            ContainerStatuses = coll;
        }
    }

    private void _cluster_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (_viewModel?.Name() == arg3.Name() && _viewModel?.Namespace() == arg3.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = arg3;
                PopulateData();
            }, DispatcherPriority.Normal);
        }
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }

    public partial class ViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; }

        [ObservableProperty]
        public partial IBrush Brush { get; set; }
    };
}
