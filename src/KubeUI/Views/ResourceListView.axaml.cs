using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls;
using k8s.Models;
using k8s;
using Avalonia.Data;
using Avalonia.Data.Converters;
using KubeUI.Client.Informer;
using System.Reflection;
using Avalonia.Threading;
using Avalonia.Controls.Templates;
using KubeUI.Controls;
using Avalonia;
using System.ComponentModel;
using System.Linq;
using KubeUI.ViewModels;
using KubeUI.Client;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

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

    private void GenerateGrid<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var resourceType = typeof(T);

        var definition = GetResourceListViewDefinition<T>();

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
                    Dispatcher.UIThread.InvokeAsync(() => column.Sort(ListSortDirection.Ascending));
                }
                else if (columnSort == SortDirection.Descending)
                {
                    Dispatcher.UIThread.InvokeAsync(() => column.Sort(ListSortDirection.Descending));
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
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "View", CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewCommand), CommandParameterPath = "SelectedItems" }));
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "View Yaml", CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewYamlCommand), CommandParameterPath = "SelectedItems" }));
            Grid.ContextMenu.Items.Add(CreateMenuItem(new () { Header = "Delete", CommandPath = nameof(ResourceListViewModel<V1Pod>.DeleteCommand), CommandParameterPath = "SelectedItems" }));

            if (definition.MenuItems?.Count > 0)
            {
                Grid.ContextMenu.Items.Add(new Separator());
            }
        }

        if (definition.MenuItems != null)
        {
            foreach (var item in definition.MenuItems)
            {
                Grid.ContextMenu.Items.Add(CreateMenuItem(item));
            }
        }
    }

    private ResourceListViewDefinition<T> GetResourceListViewDefinition<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var resourceType = typeof(T);

        ResourceListViewDefinition<T> definition = new();

        if (resourceType == typeof(V1Node))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Name",
                    Field = x => x.Metadata.Name,
                    Sort = SortDirection.Ascending,
                    Width = "4*",
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "CPU",
                    Field = x => x.Status.Allocatable["cpu"].Value,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Memory",
                    Field = x => x.Status.Allocatable["memory"].Value,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Disk",
                    Field = x => x.Status.Allocatable["ephemeral-storage"].Value,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Taints",
                    Field = x => x.Metadata.Annotations.TryGetValue("scheduler.alpha.kubernetes.io/taints", out var value) ? value : "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Roles",
                    Field = x => x.Metadata.Annotations.Any(x => x.Key.StartsWith("node-role.kubernetes.io/")) ? x.Metadata.Annotations.Where(x => x.Key.StartsWith("node-role.kubernetes.io/")).Select(x => x.Value).Aggregate((x,y) => x + ", " + y) : "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Version",
                    Field = x => x.Status.NodeInfo.KubeletVersion,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Ready")?.Reason,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                }
            };
        }
        else if (resourceType == typeof(V1Namespace))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Name",
                    Field = x => x.Metadata.Name,
                    Sort = SortDirection.Ascending,
                    Width = "4*",
                },
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Labels",
                    Field = x => x.Metadata.Labels.Select(x => x.Key + "=" + x.Value).Aggregate((x,y) => x + ", " + y),
                    Width = "2*"
                },
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Namespace, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                }
            };
        }
        else if (resourceType == typeof(V1Pod))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Name",
                    Field = x => x.Metadata.Name,
                    Sort = SortDirection.Ascending,
                    Width = "4*",
                },
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Containers",
                    CustomControl = typeof(PodContainerCell),
                    Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Namespace",
                    Field = x => x.Metadata.NamespaceProperty,
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    Field = x => x.Status.ContainerStatuses.Sum(x => x.RestartCount),
                    Display = x => x.Status?.ContainerStatuses?.Sum(x => x.RestartCount).ToString(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Controlled By",
                    Field = x => x.Metadata.OwnerReferences.FirstOrDefault()?.Name,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Node",
                    Field = x => x.Spec.NodeName,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "QoS",
                    Field = x => x.Status.QosClass,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                }
            };

            definition.MenuItems = new()
            {
                new()
                {
                    Header = "View Console",
                    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                    DataTemplate = new FuncDataTemplate<object>((obj, nameScope) => new MenuItem()
                        {
                            [!MenuItem.HeaderProperty] = new Binding("Name"),
                            [!MenuItem.CommandProperty] = new Binding(nameof(ResourceListViewModel<V1Pod>.ViewConsoleCommand)) { Source = DataContext },
                            [!MenuItem.CommandParameterProperty] = new Binding("Name"),
                        }
                    )
                },
                new()
                {
                    Header = "View Logs",
                    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                    DataTemplate = new FuncDataTemplate<object>((obj, nameScope) => new MenuItem()
                        {
                            [!MenuItem.HeaderProperty] = new Binding("Name"),
                            [!MenuItem.CommandProperty] = new Binding(nameof(ResourceListViewModel<V1Pod>.ViewLogsCommand)) { Source = DataContext },
                            [!MenuItem.CommandParameterProperty] = new Binding("Name"),
                        }
                    )
                }
            };
        }
        else if (resourceType == typeof(V1Deployment))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Name",
                    Field = x => x.Metadata.Name,
                    Sort = SortDirection.Ascending,
                    Width = "2*",
                },
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Namespace",
                    Field = x => x.Metadata.NamespaceProperty,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Pods",
                    Display = x => $"{x.Status?.AvailableReplicas.GetValueOrDefault()}/{x.Spec?.Replicas}",
                    Field = x => x.Status.AvailableReplicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Replicas",
                    Display = x => x.Spec.Replicas.GetValueOrDefault().ToString(),
                    Field = x => x.Spec.Replicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Available",
                    Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Available").Status,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                }
            };
        }
        else if (resourceType == typeof(V1CustomResourceDefinition))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Name",
                    Field = x => x.Spec.Names.Kind,
                    Sort = SortDirection.Ascending,
                    Width = "2*",
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Group",
                    Field = x => x.Spec.Group,
                    Width = "*",
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Version",
                    Field = x => x.Spec.Versions.First(x => x.Storage).Name,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
                {
                    Name = "Scope",
                    Field = x => x.Spec.Scope,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                }
            };
        }
        else if (resourceType == typeof(Corev1Event))
        {
            definition.Columns = new()
            {
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Type",
                    Field = x => x.Type,
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Message",
                    Field = x => x.Message,
                    Width = "4*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Namespace",
                    Field = x => x.Metadata.NamespaceProperty,
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Involved Object",
                    Field = x => x.InvolvedObject.Name,
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, string>()
                {
                    Name = "Source",
                    Field = x => x.Source.Component,
                    Width = "*"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, int>()
                {
                    Name = "Count",
                    Display = x => (x.Count ?? 0).ToString(),
                    Field = x => x.Count ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },

                new ResourceListViewDefinitionColumn<Corev1Event, DateTime?>()
                {
                    Name = "Last Seen",
                    CustomControl = typeof(LastSeenCell),
                    Field = x => x.LastTimestamp,
                    Sort = SortDirection.Descending,
                    Width = "80"
                },
                new ResourceListViewDefinitionColumn<Corev1Event, DateTime?>()
                {
                    Name = "Age",
                    CustomControl = typeof(AgeCell),
                    Field = x => x.Metadata.CreationTimestamp,
                    Width = "80"
                },
            };
        }
        else
        {
            definition.Columns = new();

            // Add Name Column
            var nameColumn = new ResourceListViewDefinitionColumn<T, string>()
            {
                Name = "Name",
                Field = x => x.Metadata.Name,
                Sort = SortDirection.Ascending,
                Width = "2*"
            };

            definition.Columns.Add(nameColumn);

            // Check if resource type is CRD
            // TODO: Find a better way to identify CRDs
            if (resourceType.Namespace.StartsWith("KubeUI.Models"))
            {
                // Generate CRD Columns
                var cluster = ((ResourceListViewModel<T>)DataContext).Cluster;

                var metadata = GroupApiVersionKind.From(resourceType);

                var crd = cluster.GetObject<V1CustomResourceDefinition>(null, metadata.PluralNameGroup);

                var version = crd.Spec.Versions.First(x => x.Storage);

                //Check if its a namespaced crd
                if (crd.Spec.Scope == "Namespaced")
                {
                    // Add Namespace Column
                    var nsColumn = new ResourceListViewDefinitionColumn<T, string>()
                    {
                        Name = "Namespace",
                        Field = x => x.Metadata.NamespaceProperty,
                        Width = "*"
                    };

                    definition.Columns.Add(nsColumn);
                }

                if (version.AdditionalPrinterColumns != null)
                {
                    foreach (var item in version.AdditionalPrinterColumns)
                    {
                        try
                        {
                            if (item.JsonPath == ".metadata.creationTimestamp")
                            {
                                continue;
                            }

                            if (item.Type == "string")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, string>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, string>()
                                {
                                    Name = item.Name,
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "number")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, int>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, int>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString<T>(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "integer" && item.Format == "int64")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, long>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, long>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString<T>(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "integer")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, int>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, int>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString<T>(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "date")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, DateTime>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, DateTime>()
                                {
                                    Name = item.Name,
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else if (item.Type == "boolean")
                            {
                                var exp = JsonPathLINQ.JsonPathLINQ.GetExpression<T, bool>(item.JsonPath, true);

                                var colDef = new ResourceListViewDefinitionColumn<T, bool>()
                                {
                                    Name = item.Name,
                                    Display = TransformToFuncOfString<T>(exp.Body, exp.Parameters).Compile(),
                                    Field = exp.Compile(),
                                    Width = "*"
                                };

                                definition.Columns.Add(colDef);
                            }
                            else
                            {
                                _logger.LogWarning("CRD Column Type not supported: {type}", item.Type);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical(ex, "Unable to generate CRD Column: {Name}", item.Name);
                        }
                    }
                }
            }

            // Add Age Column
            var ageColumn = new ResourceListViewDefinitionColumn<T, DateTime?>()
            {
                Name = "Age",
                CustomControl = typeof(AgeCell),
                Field = x => x.Metadata.CreationTimestamp,
                Width = "80"
            };

            definition.Columns.Add(ageColumn);
        }

        return definition;
    }

    public static Expression<Func<T, string>> TransformToFuncOfString<T>(Expression expression, ReadOnlyCollection<ParameterExpression> parameters)
    {
        // Convert the body of the original expression to return a string
        var bodyAsString = Expression.Call(expression, "ToString", Type.EmptyTypes);

        // Create a new lambda expression
        return Expression.Lambda<Func<T, string>>(bodyAsString, parameters);
    }

    public class ResourceListViewDefinitionColumn<T,T2> where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        public required string Name { get; set; }

        public required Func<T, T2> Field { get; set; }

        public Func<T, string>? Display { get; set; }

        public SortDirection Sort { get; set; }

        public Type? CustomControl { get; set; }

        public string? Width { get; set; }
    }

    public enum SortDirection
    {
        None,
        Ascending,
        Descending
    }

    public class ResourceListViewMenuItem
    {
        public required string Header { get; set; }

        public string? CommandPath { get; set; }

        public string? CommandParameterPath { get; set; }

        public string? ItemSourcePath { get; set; }

        public FuncDataTemplate<object>? DataTemplate { get; set; }
    }

    public class ResourceListViewDefinition<T> where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        public List<object> Columns { get; set; }

        public List<ResourceListViewMenuItem> MenuItems { get; set; }

        public bool DefaultMenuItems { get; set; } = true;
    }

    internal readonly struct FuncComparer<T,T2> : IComparer
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
        var menuItem = new MenuItem
        {
            Header = menu.Header
        };

        if (!string.IsNullOrEmpty(menu.CommandPath))
        {
            menuItem.Bind(MenuItem.CommandProperty, new Binding(menu.CommandPath));
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

        if (menu.DataTemplate != null)
        {
            menuItem.ItemTemplate = menu.DataTemplate;
        }

        return menuItem;
    }
}
