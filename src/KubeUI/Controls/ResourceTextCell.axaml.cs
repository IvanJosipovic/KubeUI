using k8s;
using k8s.Models;
using Kubernetes.Controller.Client;
using KubeUI.Client;

namespace KubeUI.Controls;

public interface IDisplayFunc
{
    void SetDisplayFunc(Func<object, string> selector);
}

public sealed partial class ResourceTextCell : UserControl, IInitializeCluster, IDisplayFunc, IDisposable
{
    private ICluster _cluster;

    private Func<object, string> _displayFunc;

    private IKubernetesObject<V1ObjectMeta>? _viewModel;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; }

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

        if (DataContext is IKubernetesObject<V1ObjectMeta> obj)
        {
            _viewModel = obj;
            try
            {
                PrettyString = _displayFunc.Invoke(obj);
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

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;

        _cluster.OnChange += _cluster_OnChange;
    }

    public void SetDisplayFunc(Func<object, string> selector)
    {
        _displayFunc = selector;
    }

    private void _cluster_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (_viewModel?.Name() == arg3.Name() && _viewModel?.Namespace() == arg3.Namespace())
        {
            Dispatcher.UIThread.Invoke(() => DataContext = arg3, DispatcherPriority.Background);
        }
    }

    public void Dispose()
    {
        _cluster?.OnChange -= _cluster_OnChange;
    }
}
