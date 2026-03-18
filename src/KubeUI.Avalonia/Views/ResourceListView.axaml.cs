using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Views;

public partial class ResourceListView : UserControl
{
    private readonly ILogger<ResourceListView> _logger;

    public ResourceListView()
    {
        InitializeComponent();

        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView>>();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();
                await cluster.SeedResource<V1Pod>();

                var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>() as IDockable;

                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IResourceListViewModel vm)
        {
            GetGenericMethod(nameof(GenerateGrid))?.Invoke(this, null);

            PART_Grid.SelectionModelFactory = vm.SelectionModelFactory;
            PART_Grid.SortingAdapterFactory = vm.SortingAdapterFactory;
            PART_Grid.FilteringAdapterFactory = vm.FilteringAdapterFactory;
            PART_Grid.SearchAdapterFactory = vm.SearchAdapterFactory;
        }
    }

    private MethodInfo? GetGenericMethod(string name)
    {
        var dcType = DataContext?.GetType();

        if (dcType?.IsGenericType == true)
        {
            var genericArgs = dcType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                var t = genericArgs[0];

                if (typeof(IKubernetesObject<V1ObjectMeta>).IsAssignableFrom(t))
                {
                    var method = GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (method?.IsGenericMethodDefinition == true)
                    {
                        var genericMethod = method.MakeGenericMethod(t);
                        return genericMethod;
                    }
                }
            }
        }

        return null;
    }

    private void GenerateGrid<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var viewModel = (ResourceListViewModel<T>)DataContext!;
        if (viewModel.ResourceConfig.ListStyle() != null)
        {
            PART_Grid.Styles.Add(viewModel.ResourceConfig.ListStyle());
        }
    }
}



