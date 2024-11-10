using k8s.Models;
using k8s;
using Avalonia.Data.Converters;
using Ursa.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using KubeUI.Client.Informer;

namespace KubeUI.Views;

public sealed class ResourceListView<T> : MyViewBase<ResourceListViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    DataGrid _grid;

    private readonly ILogger<ResourceListView<T>> _logger;

    public ResourceListView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView<T>>>();
    }

    private void GenerateGrid()
    {
        _grid.Columns.Clear();

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in ViewModel.ViewDefinition.Columns)
        {
            try
            {
                var columnDisplay = (Func<T, string>)columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<T, string>.Display)).GetValue(columnDefinition);

                var columnField = columnDefinition.GetType().GetProperty(nameof(ResourceListViewDefinitionColumn<T, string>.Field)).GetValue(columnDefinition);

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
                    column = new DataGridTextColumn
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

                _grid.Columns.Add(column);

                if (columnDefinition.Sort != SortDirection.None)
                {
                    Dispatcher.UIThread.Post(() => column.Sort(columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }

        _grid.ContextMenu ??= new ContextMenu();

        _grid.ContextMenu.Items.Clear();

        if (ViewModel.ViewDefinition.DefaultMenuItems)
        {
            _grid.ContextMenu.Items.Add(CreateMenuItem(new()
            {
                Header = "View",
                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewCommand),
                CommandParameterPath = "SelectedItem",
                IconResource = "ic_fluent_panel_right_filled",
            }));
            _grid.ContextMenu.Items.Add(CreateMenuItem(new()
            {
                Header = "View Yaml",
                CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewYamlCommand),
                CommandParameterPath = "SelectedItem",
                IconResource = "code_regular",
            }));
            _grid.ContextMenu.Items.Add(CreateMenuItem(new()
            {
                Header = "Delete",
                CommandPath = nameof(ResourceListViewModel<V1Pod>.DeleteCommand),
                CommandParameterPath = "SelectedItems",
                IconResource = "delete_regular",
            }));
        }

        if (ViewModel.ViewDefinition.MenuItems != null)
        {
            _grid.ContextMenu.Items.Add(new Separator());

            foreach (var item in ViewModel.ViewDefinition.MenuItems)
            {
                _grid.ContextMenu.Items.Add(CreateMenuItem(item));
            }
        }
    }

    private MenuItem CreateMenuItem(ResourceListViewMenuItem menu, int level = 0)
    {
        var menuItem = new MenuItem();

        if (menu.Header != null)
        {
            menuItem.Header = menu.Header;
        }
        else if (menu.HeaderBinding != null)
        {
            menuItem.Bind(Avalonia.Controls.Primitives.HeaderedSelectingItemsControl.HeaderProperty, menu.HeaderBinding);
        }

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            menuItem.Bind(MenuItem.CommandProperty, new Binding(menu.CommandPath) { Source = DataContext });
        }

        if (!string.IsNullOrEmpty(menu.CommandParameterPath))
        {
            menuItem.Bind(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
            {
                Source = _grid,
            });
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            menuItem.Bind(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath));
        }

        if (menu.ItemSourceBinding != null)
        {
            menuItem.Bind(ItemsControl.ItemsSourceProperty, menu.ItemSourceBinding);
        }

        if (!string.IsNullOrEmpty(menu.IconResource))
        {
            menuItem.Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource(menu.IconResource) });
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

    private List<Style> GenerateStyles(ResourceListViewMenuItem menu, int level = 0)
    {
        var styles = new List<Style>();

        Func<Selector?, Selector>? func = null;

        func = x =>
        {
            var selector = x.OfType<MenuItem>().Descendant().OfType<MenuItem>();

            for (int i = 0; i < level; i++)
            {
                selector = selector.Descendant().OfType<MenuItem>();
            }

            return selector;
        };

        var style = new Style(func);

        if (menu.Header != null)
        {
            style.Add(new Setter(Avalonia.Controls.Primitives.HeaderedSelectingItemsControl.HeaderProperty, menu.Header));
        }
        else if (menu.HeaderBinding != null)
        {
            style.Add(new Setter(Avalonia.Controls.Primitives.HeaderedSelectingItemsControl.HeaderProperty, menu.HeaderBinding));
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

        if (menu.ItemTemplate != null)
        {
            styles.AddRange(GenerateStyles(menu.ItemTemplate, level + 1));
        }

        return styles;
    }

    protected override StyleGroup? BuildStyles()
    {
        return ViewModel.ViewDefinition.SetStyle.Invoke();
    }

    protected override object Build(ResourceListViewModel<T>? vm)
    {
        var controls = new Grid()
            .Rows("Auto,*")
            .Children([
                new Grid()
                    .Row(0)
                    .Cols("Auto,Auto,*")
                    .Children([
                        new Button()
                            .Col(0)
                            .Command(vm.NewResourceCommand)
                            .IsVisible(@vm.ViewDefinition.ShowNewResource)
                            .ToolTip(Assets.Resources.ResourceListView_NewResource)
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("add_square_regular") }),
                        new Label()
                            .Col(1)
                            .Width(200)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Content(@vm.DataGridObjects.Count, null, new FuncValueConverter<int, string>((x) => string.Format("Items: {0}", x))),
                        new Grid()
                            .Col(2)
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Cols("*,*")
                            .Children([
                                new TextBox()
                                    .Col(0)
                                    .Width(300)
                                    .Background(Brushes.Transparent)
                                    .HorizontalAlignment(HorizontalAlignment.Right)
                                    .VerticalAlignment(VerticalAlignment.Stretch)
                                    .VerticalContentAlignment(VerticalAlignment.Center)
                                    .Text(@vm.SearchQuery)
                                    .Watermark("Search"),

                                new MultiComboBox()
                                    .Col(1)
                                    .Width(300)
                                    .MaxHeight(20)
                                    .HorizontalAlignment(HorizontalAlignment.Right)
                                    .Classes("ClearButton")
                                    .IsVisible(@vm.ViewDefinition.ShowNamespaces)
                                    .ItemsSource(@vm.Cluster.Namespaces.Values)
                                    .SelectedItems(@vm.Cluster.SelectedNamespaces)
                                    .SelectedItemTemplate(new FuncDataTemplate<V1Namespace?>((x,y) => new Label().Content(@x?.Metadata.Name)))
                                    .ItemTemplate(new FuncDataTemplate<V1Namespace?>((x,y) => new Label().Content(@x?.Metadata.Name)))
                                    .Watermark(Assets.Resources.ResourceListView_SelectNamespace)
                            ]),
                        ]),
                new DataGrid()
                    .Ref(out _grid)
                    .Row(1)
                    .ItemsSource(@vm.DataGridObjects, BindingMode.OneWay)
                    .SelectedItem(@vm.SelectedItem)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .MinColumnWidth(90)
                    .RowHeight(32)
                    //.OnTapped((x) =>
                    //{
                    //    if(vm.ViewCommand.CanExecute(_grid.SelectedItem))
                    //    {
                    //        vm.ViewCommand.Execute(_grid.SelectedItem);
                    //    }
                    //})
                    ,
            ]);

        GenerateGrid();

        return controls;
    }
}

public readonly struct MyFuncComparer<TObj, TPtop> : IComparer
{
    private readonly Func<TObj, TPtop> _cmp;

    public MyFuncComparer(Func<TObj, TPtop> cmp)
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

        var sourceItem = ((KeyValuePair<NamespacedName, TObj>)x).Value;
        var destItem = ((KeyValuePair<NamespacedName, TObj>)y).Value;

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
        else if (srcProperty is decimal src6 && destProperty is decimal dest6)
        {
            return src6.CompareTo(dest6);
        }
        else if (srcProperty is IntstrIntOrString src7 && destProperty is IntstrIntOrString dest7)
        {
            return src7.Value.CompareTo(dest7.Value);
        }

        throw new NotImplementedException();
    }
}
