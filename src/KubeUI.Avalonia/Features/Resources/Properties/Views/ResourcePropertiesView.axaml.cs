using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Features.Resources.Properties.Views;

public partial class ResourcePropertiesView : UserControl
{
    private INotifyPropertyChanged? _viewModel;
    private bool _isDetached;

    public ResourcePropertiesView()
    {
        InitializeComponent();
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
        if (DataContext == null)
        {
            ClearItems();
            return;
        }

        // Determine if DataContext is a generic ResourcePropertiesViewModel<T>
        // If so, extract T and invoke Reload<T>() via reflection
        var dcType = DataContext.GetType();

        if (dcType.IsGenericType)
        {
            var genericArgs = dcType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                var t = genericArgs[0];

                if (typeof(IKubernetesObject<V1ObjectMeta>).IsAssignableFrom(t))
                {
                    var reloadMethod = typeof(ResourcePropertiesView)
                        .GetMethod(nameof(Reload), BindingFlags.NonPublic | BindingFlags.Instance);

                    if (reloadMethod?.IsGenericMethodDefinition == true)
                    {
                        var genericReload = reloadMethod.MakeGenericMethod(t);
                        genericReload.Invoke(this, null);
                        return;
                    }
                }
            }
        }

        // Fallback if not matching expected generic pattern
        ClearItems();
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

                PART_Items.Children.Clear();
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

    private void Reload<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (_isDetached)
        {
            return;
        }

        ClearItems();

        if (DataContext is not ResourcePropertiesViewModel<T> viewModel)
        {
            return;
        }

        if (viewModel.Object?.Metadata == null)
            return;

        var obj = viewModel.Object;

        PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Name, Value = obj.Metadata.Name });
        if (viewModel.ResourceConfig?.IsNamespaced == true)
        {
            PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Namespace, Value = obj.Metadata.NamespaceProperty });
        }

        PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Created, Value = obj.Metadata.CreationTimestamp });

        if (viewModel.ResourceConfig == null)
        {
            return;
        }

        var extras = viewModel.ResourceConfig.Properties(obj);
        if (extras != null)
        {
            foreach (var c in extras.Where(c => c != null))
            {
                c.DataContext = obj;
                c.HorizontalAlignment = HorizontalAlignment.Stretch;
                PART_Items.Children.Add(c);

                if (viewModel.Cluster != null)
                {
                    InitializeClusterControls(c, viewModel.Cluster);
                }
            }
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

            PART_Items.Children.Add(eventsView);
            eventsView.Initialize(viewModel.Cluster);
        }

        QueueScrollToTop();
    }

    private void QueueScrollToTop()
    {
        QueueScrollToTop(0);
    }

    private void QueueScrollToTop(int attempt)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (PART_ScrollViewer is not { } scrollViewer)
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

    private static void InitializeClusterControls(Control control, ClusterWorkspaceViewModel cluster)
    {
        foreach (var current in EnumerateLogicalControls(control))
        {
            if (current is IInitializeCluster init)
            {
                init.Initialize(cluster);
            }
        }
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

