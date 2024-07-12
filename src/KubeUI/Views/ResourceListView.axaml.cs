using k8s.Models;
using k8s;
using Avalonia.Data;
using Avalonia.Data.Converters;
using KubeUI.Client.Informer;
using System.Reflection;
using Avalonia.Controls.Templates;
using KubeUI.ViewModels;
using Avalonia.Styling;

namespace KubeUI.Views;

public partial class ResourceListView : UserControl
{
    private readonly ILogger<ResourceListView> _logger;

    public ResourceListView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView>>();

        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext == null)
        {
            return;
        }

        var type = DataContext.GetType();

        var resourceType = type.GenericTypeArguments[0];

        var methodInfo = GetType().GetMethod(nameof(GenerateGrid), BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = methodInfo.MakeGenericMethod(resourceType);

        genericMethod.Invoke(this, null);
    }

    private async Task GenerateGrid<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var resourceType = typeof(T);

        var definition = ((ResourceListViewModel<T>)DataContext!).ViewDefinition;

        Grid.Columns.Clear();

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in definition.Columns)
        {
            try
            {
                var columnName = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Name)).GetValue(columnDefinition) as string;

                var columnDisplay = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Display)).GetValue(columnDefinition);

                var columnWidth = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Width)).GetValue(columnDefinition) as string;

                var columnField = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Field)).GetValue(columnDefinition);

                var columnControl = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.CustomControl)).GetValue(columnDefinition) as Type;

                var columnSort = (SortDirection)columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<V1Pod, string>.Sort)).GetValue(columnDefinition);

                // Create Sort FuncComparer
                var colType = columnField.GetType().GenericTypeArguments[1];
                var sortConverterType = typeof(FuncComparer<,>).MakeGenericType(typeof(T), colType);
                var sortConverter = Activator.CreateInstance(sortConverterType, columnField) as IComparer;

                DataGridColumn column = null;

                if (columnControl != null)
                {
                    column = new DataGridTemplateColumn()
                    {
                        Header = columnName,
                        SortMemberPath = "Value",
                        CanUserSort = true,
                        CustomSortComparer = sortConverter,
                        CellTemplate = new FuncDataTemplate<KeyValuePair<NamespacedName, T>>((item, _) =>
                        {
                            var control = Application.Current.GetRequiredService(columnControl) as Control;
                            control.DataContext = item.Value;

                            if (columnControl.GetProperty("Cluster") is PropertyInfo prop)
                            {
                                prop.SetValue(control, DataContext.GetType().GetProperty(nameof(ResourceListViewModel<V1Pod>.Cluster)).GetValue(DataContext));
                            }

                            return control;
                        }),
                    };
                }
                else
                {
                    // Create Display FuncValueConverter
                    var converterType = typeof(FuncValueConverter<,>).MakeGenericType(typeof(T), typeof(string));
                    var displayValueConverter = Activator.CreateInstance(converterType, columnDisplay ?? columnField) as IValueConverter;

                    column = new DataGridTextColumn
                    {
                        Binding = new Binding()
                        {
                            Path = "Value",
                            Converter = displayValueConverter,
                        },
                        Header = columnName,
                        SortMemberPath = "Value",
                        CustomSortComparer = sortConverter,
                        CanUserSort = true,
                    };
                }

                if (!string.IsNullOrEmpty(columnWidth) && converter.ConvertFromString(columnWidth) is DataGridLength width)
                {
                    column.Width = width;
                }

                Grid.Columns.Add(column);

                if (columnSort == SortDirection.Ascending)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => column.Sort(ListSortDirection.Ascending));
                }
                else if (columnSort == SortDirection.Descending)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => column.Sort(ListSortDirection.Descending));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }

        Grid.ContextMenu ??= new ContextMenu();

        Grid.ContextMenu.Items.Clear();

        if (definition.DefaultMenuItems)
        {
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "View", CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewCommand), CommandParameterPath = "SelectedItem" }));
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "View Yaml", CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewYamlCommand), CommandParameterPath = "SelectedItem" }));
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "Delete", CommandPath = nameof(ResourceListViewModel<V1Pod>.DeleteCommand), CommandParameterPath = "SelectedItems" }));
        }

        if (definition.MenuItems != null)
        {
            Grid.ContextMenu.Items.Add(new Separator());

            foreach (var item in definition.MenuItems)
            {
                Grid.ContextMenu.Items.Add(CreateMenuItem(item));
            }
        }
    }

    private readonly struct FuncComparer<T,T2> : IComparer
    {
        private readonly Func<T, T2> _cmp;

        public FuncComparer(Func<T, T2> cmp)
        {
            _cmp = cmp;
        }

        public int Compare(object? x, object? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            var sourceItem = ((KeyValuePair<NamespacedName, T>)x).Value;
            var destItem = ((KeyValuePair<NamespacedName, T>)y).Value;

            var srcProperty = _cmp.Invoke(sourceItem);
            var destProperty = _cmp.Invoke(destItem);

            if (srcProperty == null && destProperty == null)
            {
                return 0;
            }

            if (srcProperty == null && destProperty != null)
            {
                return -1;
            }

            if (srcProperty != null && destProperty == null)
            {
                return 1;
            }

            if (srcProperty is string src && destProperty is string dest)
            {
                return src.CompareTo(dest);
            }
            else if (srcProperty is int src2 && destProperty is int dest2)
            {
                return src2.CompareTo(dest2);
            }
            else if (srcProperty is long src3 && destProperty is long dest3)
            {
                return src3.CompareTo(dest3);
            }
            else if (srcProperty is DateTime src4 && destProperty is DateTime dest4)
            {
                return src4.CompareTo(dest4);
            }
            else if (srcProperty is bool src5 && destProperty is bool dest5)
            {
                return src5.CompareTo(dest5);
            }
            else if (srcProperty is decimal src6 && destProperty is bool dest6)
            {
                return src6.CompareTo(dest6);
            }

            throw new NotImplementedException();
        }
    }

    private MenuItem CreateMenuItem(ResourceListViewMenuItem menu)
    {
        var menuItem = new MenuItem();

        if (menu.Header != null)
        {
            menuItem.Header = menu.Header;
        }
        else if (menu.HeaderBinding != null)
        {
            menuItem.Bind(MenuItem.HeaderProperty, menu.HeaderBinding);
        }

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            menuItem.Bind(MenuItem.CommandProperty, new Binding(menu.CommandPath) { Source = DataContext });
        }

        if (!string.IsNullOrEmpty(menu.CommandParameterPath))
        {
            menuItem.Bind(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
            {
                Source = Grid,
            });
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            menuItem.Bind(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath));
        }

        if (menu.MenuItem != null)
        {
            menuItem.Styles.AddRange(GenerateStyles(menu.MenuItem));
        }

        return menuItem;
    }

    private List<Style> GenerateStyles(ResourceListViewMenuItem menu, int level = 0)
    {
        var styles = new List<Style>();

        Func<Selector?, Selector>? func = null;

        if (level == 0)
        {
            func = x => x.OfType<MenuItem>().Descendant().OfType<MenuItem>();
        }
        else if (level == 1)
        {
            func = x => x.OfType<MenuItem>().Descendant().OfType<MenuItem>().Descendant().OfType<MenuItem>();
        }
        else if (level == 2)
        {
            func = x => x.OfType<MenuItem>().Descendant().OfType<MenuItem>().Descendant().OfType<MenuItem>().Descendant().OfType<MenuItem>();
        }
        else
        {
            throw new NotImplementedException();
        }

        var style = new Style(func);

        if (menu.Header != null)
        {
            style.Add(new Setter(MenuItem.HeaderProperty, menu.Header));
        }
        else if (menu.HeaderBinding != null)
        {
            style.Add(new Setter(MenuItem.HeaderProperty, menu.HeaderBinding));
        }

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            style.Add(new Setter(MenuItem.CommandProperty, new Binding(menu.CommandPath) { Source = DataContext }));
        }

        if (!string.IsNullOrEmpty(menu.CommandParameterPath))
        {
            style.Add(new Setter(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)));
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            style.Add(new Setter(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath)));
        }

        styles.Add(style);

        if (menu.MenuItem != null)
        {
            styles.AddRange(GenerateStyles(menu.MenuItem, level + 1));
        }

        return styles;
    }
}
