using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Controls;
using KubeUI.Resources;
using Ursa.Controls;

namespace KubeUI.Views;

public sealed class ResourceListView<T> : MyViewBase<ResourceListViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    DataGrid _grid;

    private readonly ILogger<ResourceListView<T>> _logger;

    private readonly ISettingsService _settingsService;

    public ResourceListView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListView<T>>>();
        _settingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    private void GenerateGrid()
    {
        _grid.Columns.Clear();

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

                _grid.Columns.Add(column);

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

        _grid.ContextMenu ??= new ContextMenu();

        _grid.ContextMenu.Items.Clear();

        foreach (var item in ViewModel.ResourceConfig.DefaultMenuItems())
        {
            _grid.ContextMenu.Items.Add(CreateMenuItem(item));
        }

        if (ViewModel.ResourceConfig.MenuItems != null)
        {
            _grid.ContextMenu.Items.Add(new Separator());

            foreach (var item in ViewModel.ResourceConfig.MenuItems())
            {
                _grid.ContextMenu.Items.Add(CreateMenuItem(item));
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
                    Source = _grid,
                });

                menuItem.Bind(MenuItem.CommandParameterProperty, multiBinding);
            }
            else
            {
                menuItem.Bind(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
                {
                    Source = _grid,
                });
            }
        }

        if (!string.IsNullOrEmpty(menu.ItemSourcePath))
        {
            menuItem.Bind(ItemsControl.ItemsSourceProperty, new Binding(menu.ItemSourcePath));
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

    private List<Style> GenerateStyles(ResourceMenuItem menu, int level = 0)
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
            style.Add(new Setter(MenuItem.CommandProperty, new Binding(nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + menu.CommandPath) { Source = DataContext }));
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
                    Source = _grid,
                });
                multiBinding.Bindings.Add(new Binding(menu.CommandParameterPath));

                style.Add(new Setter(MenuItem.CommandParameterProperty, multiBinding));
            }
            else
            {
                style.Add(new Setter(MenuItem.CommandParameterProperty, new Binding(menu.CommandParameterPath)
                {
                    Source = _grid,
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

    protected override StyleGroup? BuildStyles()
    {
        return ViewModel?.ResourceConfig.ListStyle();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        SaveState();
    }

    private void SaveState()
    {
        foreach (var sortColumn in _grid.Columns)
        {
            var headerCell = sortColumn.GetType().GetProperty("HeaderCell", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(sortColumn) as DataGridColumnHeader;

            var direction = headerCell.GetType().GetProperty("CurrentSortingState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(headerCell) as ListSortDirection?;

            if (direction != null)
            {
                ViewModel.SortColumnName = sortColumn.Header.ToString();
                ViewModel.SortDirection = direction == ListSortDirection.Ascending ? SortDirection.Ascending : SortDirection.Descending;
            }
        }
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
                            .Command(vm.ResourceConfig.NewResourceCommand)
                            .IsVisible(@vm.ResourceConfig.ShowNewResource)
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
                                    .Width(200)
                                    .Background(Brushes.Transparent)
                                    .HorizontalAlignment(HorizontalAlignment.Right)
                                    .VerticalAlignment(VerticalAlignment.Stretch)
                                    .VerticalContentAlignment(VerticalAlignment.Center)
                                    .Text(@vm.SearchQuery)
                                    .Watermark("Search"),

                                new MultiComboBox()
                                    .Col(1)
                                    .MaxHeight(20)
                                    .Width(200)
                                    .HorizontalAlignment(HorizontalAlignment.Right)
                                    .Classes("ClearButton")
                                    .IsVisible(@vm.ResourceConfig.IsNamespaced)
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
                    .RowHeight(Convert.ToDouble(_settingsService.Settings.ListRowHeight))
                    .OnDoubleTapped((x) =>
                    {
                        if ((x.Source is Visual control) && control.FindAncestorOfType<DataGridCell>(true) == null)
                        {
                            return;
                        }

                        if (_grid.SelectedItem == null) return;

                        if(vm.ResourceConfig.ViewCommand.CanExecute(((KeyValuePair<NamespacedName, T>)_grid.SelectedItem).Value))
                        {
                            vm.ResourceConfig.ViewCommand.Execute(((KeyValuePair<NamespacedName, T>)_grid.SelectedItem).Value);
                        }
                    })
                    .KeyBindings([
                        new KeyBinding()
                            .Gesture(new KeyGesture(Key.Enter))
                            .Command(vm.ResourceConfig.ViewCommand)
                            .CommandParameter(new Binding("SelectedItem.Value") { Source = vm }),
                        ])
                    .Styles([
                        new Style<DataGridCell>()
                            .Setter(DataGridCell.FontSizeProperty, Convert.ToDouble(_settingsService.Settings.FontSize))
                            .Setter(DataGridCell.MinHeightProperty, Convert.ToDouble(_settingsService.Settings.ListRowHeight)),
                        new Style<DataGridColumnHeader>()
                            .Setter(DataGridColumnHeader.FontSizeProperty, Convert.ToDouble(_settingsService.Settings.FontSize))
                            .Setter(DataGridColumnHeader.MinHeightProperty, Convert.ToDouble(_settingsService.Settings.ListRowHeight)),
                    ]),
            ]);

        GenerateGrid();

        return controls;
    }
}
