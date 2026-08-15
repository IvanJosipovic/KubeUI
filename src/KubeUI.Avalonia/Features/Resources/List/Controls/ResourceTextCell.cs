using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.List.Controls;

public interface IDisplayFunc
{
    void SetDisplayFunc(Func<object, string> selector);
}

public sealed partial class ResourceTextCell : UserControl, IInitializeCluster, IDisplayFunc
{
    public ClusterWorkspace? Cluster { get; private set; }

    private Func<object, string>? _displayFunc;

    private IKubernetesObject<V1ObjectMeta>? _viewModel;
    private bool _runtimeChangeSubscribed;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public ResourceTextCell()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        Content = CreateCellTextBlock();

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

    private TextBlock CreateCellTextBlock()
    {
        return new TextBlock()
            .Name("CellTextBlock")
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(this, x => x.PrettyString)
            .ToolTip_Tip(this, x => x.PrettyString);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        SubscribeToRuntimeChanges();
        SetPrettyString();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToRuntimeChanges();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromRuntimeChanges();
        base.OnDetachedFromVisualTree(e);
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        SubscribeToRuntimeChanges();
    }

    private void SubscribeToRuntimeChanges()
    {
        if (!_runtimeChangeSubscribed && Cluster is not null)
        {
            Cluster.Runtime.OnChange += _cluster_OnChange;
            _runtimeChangeSubscribed = true;
        }
    }

    private void UnsubscribeFromRuntimeChanges()
    {
        if (_runtimeChangeSubscribed && Cluster is not null)
        {
            Cluster.Runtime.OnChange -= _cluster_OnChange;
            _runtimeChangeSubscribed = false;
        }
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
            UpdatePrettyString(obj);
        }
        else
        {
            _viewModel = null;
            PrettyString = string.Empty;
        }
    }

    private void UpdatePrettyString(IKubernetesObject<V1ObjectMeta> obj)
    {
        try
        {
            PrettyString = _displayFunc?.Invoke(obj) ?? string.Empty;
        }
        catch (Exception)
        {
            PrettyString = string.Empty;
        }
    }

    private void _cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (eventType is WatchEventType.Added or WatchEventType.Modified
            && _viewModel is not null
            && _viewModel.ApiVersion == resource.ApiVersion
            && _viewModel.Kind == resource.Kind
            && _viewModel.Name() == resource.Name()
            && _viewModel.Namespace() == resource.Namespace())
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                SetPrettyString();
            }
            else
            {
                Dispatcher.UIThread.Invoke(SetPrettyString);
            }
        }
    }
}

