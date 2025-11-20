using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
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

        if (DataContext != null)
        {
            GetGenericMethod(nameof(GenerateGrid))?.Invoke(this, null);
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

        PART_Grid.Columns.Clear();

        if (viewModel.ResourceConfig.ListStyle() != null)
        {
            PART_Grid.Styles.Add(viewModel.ResourceConfig.ListStyle());
        }

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in viewModel.ResourceConfig.Columns())
        {
            try
            {
                var columnDisplay = (Func<T, string>)columnDefinition.GetType().GetProperty(nameof(ResourceListColumn<,>.Display)).GetValue(columnDefinition);

                var columnField = columnDefinition.GetType().GetProperty(nameof(ResourceListColumn<,>.Field)).GetValue(columnDefinition);

                // Create Sort FuncComparer
                var colType = columnField.GetType().GenericTypeArguments[1];
                var sortConverterType = typeof(MyFuncComparer<,>).MakeGenericType(typeof(T), colType);
                var sortConverter = (IComparer)Activator.CreateInstance(sortConverterType, columnField);

                DataGridColumn column = null;

                if (columnDefinition.CustomControl != null)
                {
                    column = new DataGridTemplateColumn()
                    {
                        Header = columnDefinition.Name,
                        CustomSortComparer = sortConverter,
                        CanUserSort = true,
                        CellTemplate = new FuncDataTemplate<T>((item, _) =>
                        {
                            var control = Application.Current.GetRequiredService(columnDefinition.CustomControl) as Control;
                            control.DataContext = item;

                            if (control is IInitializeCluster init)
                            {
                                init.Initialize(viewModel.Cluster);
                            }

                            return control;
                        }),
                    };
                }
                else
                {
                    // Create Display FuncValueConverter
                    column = new MyDataGridTextColumn
                    {
                        Binding = new Binding()
                        {
                            Converter = new FuncValueConverter<T, string>(x =>
                            {
                                if (x == null)
                                    return "";
                                // Use columnDisplay if not null, otherwise use columnField as a Func<T, string>
                                if (columnDisplay != null)
                                    return columnDisplay(x);
                                else if (columnField is Func<T, string> fieldFunc)
                                    return fieldFunc(x);
                                else
                                    throw new Exception("Column is miss-configured");
                            }),
                            Mode = BindingMode.OneWay
                        },
                        Header = columnDefinition.Name,
                        CanUserSort = true,
                        CustomSortComparer = sortConverter,
                    };
                }

                if (!string.IsNullOrEmpty(columnDefinition.Width) && converter.ConvertFromString(columnDefinition.Width) is DataGridLength width)
                {
                    column.Width = width;
                }

                PART_Grid.Columns.Add(column);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
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

    private void SetSort<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (DataContext == null)
        {
            return;
        }

        var viewModel = (ResourceListViewModel<T>)DataContext!;

        if (string.IsNullOrEmpty(viewModel.SortColumnName))
        {
            var columnDefinition = viewModel.ResourceConfig.Columns().First(x => x.Sort != SortDirection.None);

            var column = PART_Grid.Columns.First(x => x.Header.ToString() == columnDefinition.Name);

            Dispatcher.UIThread.Post(() =>
            {
                if (column != null && columnDefinition != null)
                {
                    column.Sort(columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                }
            });
        }
        else
        {
            var column = PART_Grid.Columns.First(x => x.Header.ToString() == viewModel.SortColumnName);

            Dispatcher.UIThread.Post(() =>
            {
                if (viewModel != null)
                {
                    column.Sort(viewModel.SortDirection == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                }
            });
        }
    }

    private void PART_Grid_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var ViewModel = (IResourceListViewModel)DataContext;

        if (e.Property.Name == "CollectionView")
        {
            GetGenericMethod(nameof(SetSort))?.Invoke(this, null);
        }
    }

    private void PART_Grid_Sorting(object? sender, DataGridColumnEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var ViewModel = (IResourceListViewModel)DataContext;

            var direction = e.Column.SortDescription;

            if (direction != null)
            {
                ViewModel.SortColumnName = e.Column.Header.ToString();
                ViewModel.SortDirection = direction.Value == ListSortDirection.Ascending ? SortDirection.Ascending : SortDirection.Descending;
            }
        }, DispatcherPriority.Background);
    }
}

public static class DataGridExtensions
{
    extension(DataGridColumn col)
    {
        public ListSortDirection? SortDescription => GetSortDescription(col).Direction;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetSortDescription")]
    public static extern DataGridSortDescription GetSortDescription(DataGridColumn column);
}
