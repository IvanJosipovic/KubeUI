using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Features.Resources.Properties;

public partial class ResourcePropertiesView<T> : ViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private StackPanel _itemsPanel = null!;
    private StackPanel _actionsPanel = null!;

    private ScrollViewer? _scrollViewer;
    private INotifyPropertyChanged? _viewModel;
    private bool _isDetached;
    private Control[]? _propertyControls;
    private ResourceConfigBase<T>? _propertyConfig;
    private T? _propertyResource;
    private bool _propertyControlsInitialized;

    protected override object Build(ResourcePropertiesViewModel<T> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        _itemsPanel = new StackPanel()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Name("PART_Items", Scope);

        _actionsPanel = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(2)
            .Margin(2)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .Name("PART_Actions", Scope);

        _scrollViewer = new ScrollViewer()
            .Name("PART_ScrollViewer", Scope)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(_itemsPanel);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                _actionsPanel,
                _scrollViewer.Row(1));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SubscribeToViewModel();
        ReloadNowOrLater();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isDetached = false;
        ReloadNowOrLater();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isDetached = true;
        UnsubscribeFromViewModel();
    }

    private void AttachAndReload()
    {
        ReloadProperties();
    }

    private void SubscribeToViewModel()
    {
        UnsubscribeFromViewModel();

        if (DataContext is INotifyPropertyChanged notifyPropertyChanged)
        {
            _viewModel = notifyPropertyChanged;
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void UnsubscribeFromViewModel()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModel = null;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "Object" or "ResourceConfig" or "Cluster")
        {
            ReloadNowOrLater();
        }

        if (e.PropertyName == nameof(ResourcePropertiesViewModel<T>.Actions))
        {
            ReloadActions();
        }
    }

    protected virtual void ClearItems()
    {
        void action()
        {
            try
            {
                if (_isDetached)
                {
                    return;
                }

                _itemsPanel.Children.Clear();
            }
            catch
            {
                // Swallow any exceptions here to avoid crashing the UI thread during detach/race conditions.
                // The state will be reconciled on the next valid reload.
            }
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action, DispatcherPriority.Background);
        }
    }

    private void ReloadNowOrLater()
    {
        if (_isDetached)
        {
            return;
        }

        if (VisualRoot == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(AttachAndReload, DispatcherPriority.Background);
    }

    private void ReloadProperties()
    {
        if (_isDetached)
        {
            return;
        }

        ClearItems();
        ReloadActions();

        if (DataContext is not ResourcePropertiesViewModel<T> viewModel)
        {
            return;
        }

        if (viewModel.Object?.Metadata == null)
            return;

        var obj = viewModel.Object;

        _itemsPanel.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Name, Value = obj.Metadata.Name });
        if (viewModel.ResourceConfig?.IsNamespaced == true)
        {
            _itemsPanel.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Namespace, Value = obj.Metadata.NamespaceProperty });
        }

        _itemsPanel.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Created, Value = obj.Metadata.CreationTimestamp });

        if (viewModel.ResourceConfig == null)
        {
            return;
        }

        var extras = GetPropertyControls(viewModel.ResourceConfig, obj, out var reused);
        foreach (var c in extras)
        {
            if (!reused)
            {
                c.DataContext = obj;
            }

            c.HorizontalAlignment = HorizontalAlignment.Stretch;
            _itemsPanel.Children.Add(c);

            if (!reused && viewModel.Cluster != null)
            {
                InitializeClusterControls(c, viewModel.Cluster);
            }
        }

        if (!reused)
        {
            _propertyControlsInitialized = viewModel.Cluster != null;
        }
        else if (!_propertyControlsInitialized && viewModel.Cluster != null)
        {
            foreach (var c in extras)
            {
                InitializeClusterControls(c, viewModel.Cluster);
            }

            _propertyControlsInitialized = true;
        }

        if (typeof(T) != typeof(Corev1Event)
            && viewModel.Cluster != null
            && viewModel.Cluster.CanReadEvents(obj))
        {
            var eventsView = new ResourceEventsView
            {
                DataContext = obj,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            _itemsPanel.Children.Add(eventsView);
            eventsView.Initialize(viewModel.Cluster);
        }

        QueueScrollToTop();
    }

    private void ReloadActions()
    {
        _actionsPanel.Children.Clear();

        if (DataContext is not ResourcePropertiesViewModel<T> viewModel)
        {
            _actionsPanel.IsVisible = false;
            return;
        }

        foreach (var action in viewModel.Actions)
        {
            if (!action.IsVisible || action.IsSeparator || !action.ShowInPropertiesView)
            {
                continue;
            }

            var button = new Button
            {
                Width = 32,
                Height = 32,
                Padding = new Thickness(4),
                Content = ResourceActionPresenter.CreateIcon(action),
            };
            ToolTip.SetTip(button, action.Title);

            if (action.Items is { Count: > 0 } children)
            {
                button.Flyout = ResourceActionPresenter.CreateFlyout(children);
            }
            else if (action.Command != null)
            {
                button.Command = action.Command;
                button.CommandParameter = action.CommandParameter;
            }
            else
            {
                continue;
            }

            _actionsPanel.Children.Add(button);
        }

        _actionsPanel.IsVisible = _actionsPanel.Children.Count > 0;
    }

    private void QueueScrollToTop()
    {
        QueueScrollToTop(0);
    }

    private void QueueScrollToTop(int attempt)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_scrollViewer is not { } scrollViewer)
            {
                return;
            }

            if (scrollViewer.Extent.Height <= scrollViewer.Viewport.Height && attempt < 4)
            {
                QueueScrollToTop(attempt + 1);
                return;
            }

            scrollViewer.Offset = default;
            scrollViewer.ScrollToHome();
        }, DispatcherPriority.Loaded);
    }

    private static void InitializeClusterControls(Control control, ClusterWorkspace cluster)
    {
        foreach (var current in EnumerateLogicalControls(control))
        {
            if (current is IInitializeCluster init)
            {
                init.Initialize(cluster);
            }
        }
    }

    private Control[] GetPropertyControls(ResourceConfigBase<T> resourceConfig, T resource, out bool reused)
    {
        if (_propertyControls is { Length: > 0 } controls
            && ReferenceEquals(_propertyConfig, resourceConfig)
            && _propertyResource is not null
            && IsSameResource(_propertyResource, resource)
            && controls.All(control => control is IResourcePropertiesRefreshable<T>))
        {
            foreach (var control in controls.OfType<IResourcePropertiesRefreshable<T>>())
            {
                control.Refresh(resource);
            }

            _propertyResource = resource;
            reused = true;
            return controls;
        }

        var created = resourceConfig.Properties(resource)
            .Where(static control => control is not null)
            .ToArray();
        _propertyControls = created;
        _propertyConfig = resourceConfig;
        _propertyResource = resource;
        _propertyControlsInitialized = false;
        reused = false;
        return created;
    }

    private static bool IsSameResource(T left, T right)
    {
        return string.Equals(left.ApiVersion, right.ApiVersion, StringComparison.Ordinal)
            && string.Equals(left.Kind, right.Kind, StringComparison.Ordinal)
            && string.Equals(left.Metadata?.Name, right.Metadata?.Name, StringComparison.Ordinal)
            && string.Equals(left.Metadata?.NamespaceProperty, right.Metadata?.NamespaceProperty, StringComparison.Ordinal);
    }

    private static IEnumerable<Control> EnumerateLogicalControls(Control root)
    {
        var stack = new Stack<Control>();
        var seen = new HashSet<Control>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!seen.Add(current))
            {
                continue;
            }

            yield return current;

            switch (current)
            {
                case Panel panel:
                    foreach (var child in panel.Children.OfType<Control>())
                    {
                        stack.Push(child);
                    }
                    break;
                case Decorator decorator when decorator.Child is Control child:
                    stack.Push(child);
                    break;
                case ContentControl contentControl when contentControl.Content is Control child:
                    stack.Push(child);
                    break;
            }

            if (current is not ILogical logical)
            {
                continue;
            }

            foreach (var child in logical.LogicalChildren.OfType<Control>())
            {
                stack.Push(child);
            }
        }
    }
}
