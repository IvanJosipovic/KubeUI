using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Model.Core;
using DynamicData;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Resources;
using KubeUI.ViewModels;
using OpenTelemetry.Trace;
using Semi.Avalonia;

namespace KubeUI.Views;

public partial class ResourceListView : UserControl
{
    private readonly ILogger<ResourceListView> _logger;

    public ResourceListView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView>>();

        InitializeComponent();

#if DEBUG
        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>() as IDockable;

        var cluster = Application.Current.GetRequiredService<ClusterManager>().GetDefault();

        cluster.Connect();
        cluster.Seed<V1Pod>(false);

        if (vm is IInitializeCluster init)
        {
            init.Initialize(cluster);
        }

        DataContext = vm;
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Determine if DataContext is a generic ResourcePropertiesViewModel<T>
        // If so, extract T and invoke Reload<T>() via reflection
        var dcType = DataContext?.GetType();

        if (dcType?.IsGenericType == true)
        {
            var genericArgs = dcType.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                var t = genericArgs[0];

                if (typeof(IKubernetesObject<V1ObjectMeta>).IsAssignableFrom(t))
                {
                    var reloadMethod = typeof(ResourceListView)
                        .GetMethod(nameof(GenerateGrid), BindingFlags.NonPublic | BindingFlags.Instance);

                    if (reloadMethod?.IsGenericMethodDefinition == true)
                    {
                        var genericReload = reloadMethod.MakeGenericMethod(t);
                        genericReload.Invoke(this, null);
                        return;
                    }
                }
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        SaveState();
    }

    private void GenerateGrid<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var ViewModel = (ResourceListViewModel<T>)DataContext!;

        PART_Grid.Columns.Clear();

        if (ViewModel.ResourceConfig.ListStyle() != null)
        {
            PART_Grid.Styles.Add(ViewModel.ResourceConfig.ListStyle());
        }

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in ViewModel.ResourceConfig.Columns())
        {
            try
            {
                var columnDisplay = (Func<T, string>)columnDefinition.GetType().GetProperty(nameof(ResourceListColumn<T, string>.Display)).GetValue(columnDefinition);

                var columnField = columnDefinition.GetType().GetProperty(nameof(ResourceListColumn<T, string>.Field)).GetValue(columnDefinition);

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
                        CellTemplate = new FuncDataTemplate<KeyValuePair<NamespacedName, T>>((item, _) =>
                        {
                            var control = Application.Current.GetRequiredService(columnDefinition.CustomControl) as Control;
                            control.DataContext = item.Value;

                            if (control is IInitializeCluster init)
                            {
                                init.Initialize(ViewModel.Cluster);
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
                            Path = "Value",
                            Converter = new FuncValueConverter<T, string>(columnDisplay ?? (Func<T, string>)columnField),
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

                if (string.IsNullOrEmpty(ViewModel.SortColumnName))
                {
                    if (columnDefinition.Sort != SortDirection.None)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (column != null && columnDefinition != null)
                            {
                                column.Sort(columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                            }
                        });
                    }
                }
                else
                {
                    if (column.Header.ToString() == ViewModel.SortColumnName)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (ViewModel != null)
                            {
                                column.Sort(ViewModel.SortDirection == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }

        PART_Grid.ContextMenu ??= new ContextMenu();

        PART_Grid.ContextMenu.Items.Clear();

        foreach (var item in ViewModel.ResourceConfig.DefaultMenuItems())
        {
            PART_Grid.ContextMenu.Items.Add(CreateMenuItem(item));
        }

        if (ViewModel.ResourceConfig.MenuItems != null)
        {
            PART_Grid.ContextMenu.Items.Add(new Separator());

            foreach (var item in ViewModel.ResourceConfig.MenuItems())
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
            menuItem.Bind(MenuItem.CommandProperty, new Binding(nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + menu.CommandPath) { Source = DataContext });
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
                multiBinding.Bindings.Add(new Binding("SelectedItem.Value"));
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
                multiBinding.Bindings.Add(new Binding("SelectedItem.Value")
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

    private void SaveState()
    {
        var ViewModel = (IResourceListViewModel)DataContext;
        ViewModel.SortColumnName = string.Empty;
        ViewModel.SortDirection = SortDirection.None;

        foreach (var sortColumn in PART_Grid.Columns)
        {
            var headerCell = (DataGridColumnHeader)sortColumn.GetType().GetProperty("HeaderCell", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sortColumn);

            var direction = headerCell.Classes.Contains(":sortascending") ? SortDirection.Ascending : headerCell.Classes.Contains(":sortdescending") ? SortDirection.Descending : SortDirection.None;

            if (direction != SortDirection.None)
            {
                ViewModel.SortColumnName = sortColumn.Header.ToString();
                ViewModel.SortDirection = direction;
            }
        }
    }
}
