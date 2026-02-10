using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Dock.Model.Core;
using FluentIcons.Avalonia;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;

namespace KubeUI.Views;

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
                var cluster = Application.Current.GetRequiredService<ClusterManager>().GetDefault();
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

            PART_Grid.Selection = vm.SelectionModel;
            PART_Grid.SortingModel = vm.SortingModel;
            PART_Grid.FilteringModel = vm.FilteringModel;
            PART_Grid.SearchModel = vm.SearchModel;
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

        PART_Grid.ContextMenu ??= new ContextMenu();

        PART_Grid.ContextMenu.Items.Clear();

        foreach (var item in viewModel.ResourceConfig.DefaultMenuItems())
        {
            PART_Grid.ContextMenu.Items.Add(CreateMenuItem(item));
        }

        if (viewModel.ResourceConfig.MenuItems != null)
        {
            PART_Grid.ContextMenu.Items.Add(new Separator());

            foreach (var item in viewModel.ResourceConfig.MenuItems())
            {
                PART_Grid.ContextMenu.Items.Add(CreateMenuItem(item));
            }
        }
    }

    private MenuItem CreateMenuItem(ResourceMenuItem menu, int level = 0)
    {
        var menuItem = new MenuItem();

        if (menu.Header != null)
        {
            menuItem.Header = menu.Header;
        }
        else if (menu.HeaderBinding != null)
        {
            menuItem.Bind(HeaderedSelectingItemsControl.HeaderProperty, menu.HeaderBinding);
        }

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            menuItem.Bind(MenuItem.CommandProperty, new Binding(nameof(ResourceListViewModel<>.ResourceConfig) + "." + menu.CommandPath) { Source = DataContext });
        }

        if (!string.IsNullOrEmpty(menu.CommandParameterPath))
        {
            if (menu.CommandParameterAddSelectedItem == true)
            {
                // Create the MultiBinding
                var multiBinding = new MultiBinding
                {
                    Mode = BindingMode.OneWay
                };

                // Add the individual bindings
                multiBinding.Bindings.Add(new Binding("SelectedItem"));
                multiBinding.Bindings.Add(new Binding(menu.CommandParameterPath)
                {
                    Source = PART_Grid,
                });

                menuItem.Bind(MenuItem.CommandParameterProperty, multiBinding);
            }
            else
            {
                menuItem.Bind(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
                {
                    Source = PART_Grid,
                });
            }
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            menuItem.Bind(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath));
        }

        if (!string.IsNullOrEmpty(menu.IconResource))
        {
            menuItem.Icon = new PathIcon() { Data = (Geometry)Application.Current.FindResource(menu.IconResource) };
        }

        if (menu.FluentIcon.HasValue)
        {
            menuItem.Icon = new FluentIcon()
            {
                Icon = menu.FluentIcon.Value
            };
        }

        if (menu.ItemTemplate != null)
        {
            menuItem.Styles.AddRange(GenerateStyles(menu.ItemTemplate, level));
        }

        if (menu.MenuItems != null)
        {
            foreach (var item in menu.MenuItems)
            {
                menuItem.Items.Add(CreateMenuItem(item, level + 1));
            }
        }

        return menuItem;
    }

    private List<Style> GenerateStyles(ResourceMenuItem menu, int level = 0)
    {
        var styles = new List<Style>();

        Selector func(Selector? x)
        {
            var selector = x.OfType<MenuItem>().Descendant().OfType<MenuItem>();

            for (int i = 0; i < level; i++)
            {
                selector = selector.Descendant().OfType<MenuItem>();
            }

            return selector;
        }

        var style = new Style(func);

        if (menu.Header != null)
        {
            style.Add(new Setter(HeaderedSelectingItemsControl.HeaderProperty, menu.Header));
        }
        else if (menu.HeaderBinding != null)
        {
            style.Add(new Setter(HeaderedSelectingItemsControl.HeaderProperty, menu.HeaderBinding));
        }

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            style.Add(new Setter(MenuItem.CommandProperty, new Binding(nameof(ResourceListViewModel<>.ResourceConfig) + "." + menu.CommandPath) { Source = DataContext }));
        }

        if (!string.IsNullOrEmpty(menu.CommandParameterPath))
        {
            if (menu.CommandParameterAddSelectedItem == true)
            {
                // Create the MultiBinding
                var multiBinding = new MultiBinding
                {
                    Mode = BindingMode.OneWay
                };

                // Add the individual bindings
                multiBinding.Bindings.Add(new Binding("SelectedItem")
                {
                    Source = PART_Grid,
                });
                multiBinding.Bindings.Add(new Binding(menu.CommandParameterPath));

                style.Add(new Setter(MenuItem.CommandParameterProperty, multiBinding));
            }
            else
            {
                style.Add(new Setter(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
                {
                    Source = PART_Grid,
                }));
            }
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            style.Add(new Setter(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath)));
        }

        styles.Add(style);

        if (menu.ItemTemplate != null)
        {
            styles.AddRange(GenerateStyles(menu.ItemTemplate, level + 1));
        }

        return styles;
    }
}
