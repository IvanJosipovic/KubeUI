using System.Reflection;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Views;

public partial class ResourcePropertiesView : UserControl
{
    public ResourcePropertiesView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        AttachAndReload();
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

    private void ClearItems() => PART_Items.Children.Clear();

    private void Reload<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ClearItems();

        var _vm = (ResourcePropertiesViewModel<T>)DataContext;

        if (_vm?.Object?.Metadata == null)
            return;

        var obj = _vm.Object;

        PART_Items.Children.Add(new PropertyItem { Key = "Name", Value = obj.Metadata.Name });
        PART_Items.Children.Add(new PropertyItem { Key = "Namespace", Value = obj.Metadata.NamespaceProperty });

        var created = obj.Metadata.CreationTimestamp;
        PART_Items.Children.Add(new PropertyItem { Key = "Created", Value = created.HasValue ? created.Value.ToLocalTime().ToString() : "" });

        var extras = _vm.ResourceConfig.Properties(obj);
        if (extras != null)
        {
            foreach (var c in extras.Where(c => c != null))
            {
                c.DataContext = obj;
                PART_Items.Children.Add(c);
            }
        }
    }
}

