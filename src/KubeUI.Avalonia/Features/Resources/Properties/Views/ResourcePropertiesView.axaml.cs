using System.Reflection;
using System.ComponentModel;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Features.Resources.Properties.Views;

public partial class ResourcePropertiesView : UserControl
{
    private INotifyPropertyChanged? _viewModel;

    public ResourcePropertiesView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SubscribeToViewModel();
        AttachAndReload();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
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
            AttachAndReload();
        }
    }

    private void ClearItems() => PART_Items.Children.Clear();

    private void Reload<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ClearItems();

        var _vm = (ResourcePropertiesViewModel<T>)DataContext;

        if (_vm?.Object?.Metadata == null)
            return;

        var obj = _vm.Object;

        PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Name, Value = obj.Metadata.Name });
        PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Namespace, Value = obj.Metadata.NamespaceProperty });

        PART_Items.Children.Add(new PropertyItem { Key = AppResources.ResourcePropertiesView_Created, Value = obj.Metadata.CreationTimestamp });

        var extras = _vm.ResourceConfig.Properties(obj);
        if (extras != null)
        {
            foreach (var c in extras.Where(c => c != null))
            {
                c.DataContext = obj;
                if (_vm.Cluster != null && c is IInitializeCluster init)
                {
                    init.Initialize(_vm.Cluster);
                }

                PART_Items.Children.Add(c);
            }
        }

        if (typeof(T) != typeof(Corev1Event) && _vm.Cluster != null)
        {
            var eventsView = new ResourceEventsView
            {
                DataContext = obj,
            };

            eventsView.Initialize(_vm.Cluster);
            PART_Items.Children.Add(eventsView);
        }
    }
}

