using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Client;

namespace KubeUI.Controls;

public interface IDisplayFunc
{
    void SetDisplayFunc(Func<object, string> selector);
}

public sealed partial class ResourceTextCell : UserControl, IInitializeCluster, IDisplayFunc
{
    private ICluster? _cluster;

    private Func<object, string>? _displayFunc;

    private IKubernetesObject<V1ObjectMeta>? _viewModel;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public ResourceTextCell()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            _displayFunc = (x) => ((V1Namespace)x).Name();
            DataContext = new V1Namespace()
            {
                Metadata = new()
                {
                    CreationTimestamp = DateTime.UtcNow,
                    Name = "MyName",
                }
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        SetPrettyString();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _cluster?.OnChange -= _cluster_OnChange;
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }

    public void SetDisplayFunc(Func<object, string> selector)
    {
        _displayFunc = selector;
    }

    private void SetPrettyString()
    {
        if (DataContext is IKubernetesObject<V1ObjectMeta> obj)
        {
            _viewModel = obj;

            try
            {
                PrettyString = _displayFunc?.Invoke(obj) ?? string.Empty;
            }
            catch (Exception)
            {
                PrettyString = string.Empty;
            }
        }
        else
        {
            PrettyString = string.Empty;
        }
    }

    private void _cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (_viewModel?.Kind == resource.Kind
            && _viewModel.ApiVersion == resource.ApiVersion
            && _viewModel?.Name() == resource.Name()
            && _viewModel?.Namespace() == resource.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = resource;
                SetPrettyString();
            }, DispatcherPriority.Normal);
        }
    }
}
