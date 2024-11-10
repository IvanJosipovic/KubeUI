using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;
using Avalonia.Input;
using k8s;
using k8s.Autorest;
using k8s.Models;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace KubeUI;

public static class Utilities
{
    public static FuncValueConverter<bool, bool> InverseBooleanConverter { get; } = new FuncValueConverter<bool, bool>(b => !b);

    public static FuncValueConverter<object, bool> NotNullConverter { get; } = new FuncValueConverter<object, bool>((x) => x != null);

    public static T GetRequiredService<T>(this Application? app)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service).GetRequiredService<T>();
        }

        throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
    }

    public static object GetRequiredService(this Application? app, Type type)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service!).GetRequiredService(type);
        }

        throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
    }

    public static T Set<T>(this T control, Action<T> func) where T : Control
    {
        func.Invoke(control);

        return control;
    }

    public static TDataGrid Columns<TDataGrid>(this TDataGrid container, params DataGridColumn[] items) where TDataGrid : DataGrid
    {
        IList items2 = container.Columns;
        if (items2 != null)
        {
            foreach (DataGridColumn value in items)
            {
                items2.Add(value);
            }
        }

        return container;
    }

    //public static TControl ContextMenu<TControl>(this TControl container, params Control[] items) where TControl : Control
    //{
    //    container.ContextMenu ??= new ContextMenu();

    //    foreach (var item in items)
    //    {
    //        container.ContextMenu.Items.Add(item);
    //    }

    //    return container;
    //}

    public static TControl ContextFlyout<TControl>(this TControl container, params Control[] items) where TControl : Control
    {
        container.ContextFlyout ??= new MenuFlyout();

        var menu = (MenuFlyout)container.ContextFlyout;

        foreach (var item in items)
        {
            menu.Items.Add(item);
        }

        return container;
    }

    public static TControl Set<TControl,TValue>(this TControl control, AvaloniaProperty property, TValue value, FuncValueConverter<TValue, object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where TControl : Control
    {
        return control._setEx(property, ps, () => control[property] = converter.TryConvert(value), bindingMode, converter, bindingSource);
    }

    public static TControl KeyBindings<TControl>(this TControl container, params KeyBinding[] items) where TControl : Control
    {
        container.KeyBindings.AddRange(items);

        return container;
    }

    public static string GetKubeAssetPath(Type type)
    {
        const string infrastructure_componentsBasePath = "/Assets/kube/infrastructure_components/unlabeled/";

        if (type == typeof(V1Node))
        {
            return infrastructure_componentsBasePath + "node.svg";
        }

        if (type == typeof(Corev1Event))
        {
            return infrastructure_componentsBasePath + "etcd.svg";
        }

        const string resourceBasePath = "/Assets/kube/resources/unlabeled/";

        if (type == typeof(V1ConfigMap))
        {
            return resourceBasePath + "cm.svg";
        }

        if (type == typeof(V1ClusterRoleBinding))
        {
            return resourceBasePath + "crb.svg";
        }

        if (type == typeof(V1CustomResourceDefinition))
        {
            return resourceBasePath + "crd.svg";
        }

        if (type == typeof(V1ClusterRole))
        {
            return resourceBasePath + "c-role.svg";
        }

        if (type == typeof(V1CronJob))
        {
            return resourceBasePath + "cronjob.svg";
        }

        if (type == typeof(V1Deployment))
        {
            return resourceBasePath + "deploy.svg";
        }

        if (type == typeof(V1DaemonSet))
        {
            return resourceBasePath + "ds.svg";
        }

        if (type == typeof(V1Endpoints) || type == typeof(V1EndpointSlice))
        {
            return resourceBasePath + "ep.svg";
        }

        if (type == typeof(V1APIGroup)) // unsure on this one
        {
            return resourceBasePath + "group.svg";
        }

        if (type == typeof(V1HorizontalPodAutoscaler) || type == typeof(V2HorizontalPodAutoscaler))
        {
            return resourceBasePath + "hpa.svg";
        }

        if (type == typeof(V1Ingress))
        {
            return resourceBasePath + "ing.svg";
        }

        if (type == typeof(V1Job))
        {
            return resourceBasePath + "job.svg";
        }

        if (type == typeof(V1LimitRange))
        {
            return resourceBasePath + "limits.svg";
        }

        if (type == typeof(V1NetworkPolicy))
        {
            return resourceBasePath + "netpol.svg";
        }

        if (type == typeof(V1Namespace))
        {
            return resourceBasePath + "ns.svg";
        }

        if (type == typeof(V1Pod))
        {
            return resourceBasePath + "pod.svg";
        }

        if (type == typeof(V1PersistentVolume))
        {
            return resourceBasePath + "pv.svg";
        }

        if (type == typeof(V1PersistentVolumeClaim))
        {
            return resourceBasePath + "pvc.svg";
        }

        if (type == typeof(V1ResourceQuota))
        {
            return resourceBasePath + "quota.svg";
        }

        if (type == typeof(V1RoleBinding))
        {
            return resourceBasePath + "rb.svg";
        }

        if (type == typeof(V1Role))
        {
            return resourceBasePath + "role.svg";
        }

        if (type == typeof(V1ReplicaSet))
        {
            return resourceBasePath + "rs.svg";
        }

        if (type == typeof(V1ServiceAccount))
        {
            return resourceBasePath + "sa.svg";
        }

        if (type == typeof(V1StorageClass))
        {
            return resourceBasePath + "sc.svg";
        }

        if (type == typeof(V1Secret))
        {
            return resourceBasePath + "secret.svg";
        }

        if (type == typeof(V1StatefulSet))
        {
            return resourceBasePath + "sts.svg";
        }

        if (type == typeof(V1Service))
        {
            return resourceBasePath + "svc.svg";
        }

        if (type == typeof(V1UserSubject)) // unsure on this one
        {
            return resourceBasePath + "user.svg";
        }

        return "/Assets/kube/blank.svg";
    }

    public static IKubernetesObject<V1ObjectMeta> CloneObject(object obj)
    {
        var json = KubernetesJson.Serialize(obj);

        return (IKubernetesObject<V1ObjectMeta>)DeserializeKubeJson(json, obj.GetType());
    }

    private static readonly MethodInfo _deserializeJson = typeof(KubernetesJson).GetMethod(nameof(KubernetesJson.Deserialize), BindingFlags.Static | BindingFlags.Public, [typeof(string), typeof(JsonSerializerOptions)]);

    public static object DeserializeKubeJson(string json, Type type)
    {
        var fooRef = _deserializeJson.MakeGenericMethod(type);

        return fooRef.Invoke(null, [json, null]);
    }

    public static void HandleException(ILogger logger, INotificationManager notificationManage, Exception ex, string message, NotificationType type = NotificationType.Error, bool sendNotification = false)
    {
        if (sendNotification)
        {
            if(ex is AggregateException aggregate)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    if (item is HttpOperationException opEx)
                    {
                        var status = KubernetesYaml.Deserialize<V1Status>(opEx.Response.Content);

                        if (status != null)
                        {
                            notificationManage.Show(new Notification(status.Reason, status.Message + "\n\n" + status?.Details?.Causes?.Select(x => x.Message).Aggregate((x, y) => x + "\n" + y) ?? "", type, TimeSpan.FromSeconds(30)));
                        }
                    }
                    else
                    {
                        notificationManage.Show(new Notification(message, item.Message, type));
                    }
                }
            }
            else if (ex is HttpOperationException opEx)
            {
                var status = KubernetesYaml.Deserialize<V1Status>(opEx.Response.Content);

                if (status != null)
                {
                    notificationManage.Show(new Notification(status.Reason, status.Message + "\n\n" + status?.Details?.Causes?.Select(x => x.Message).Aggregate((x, y) => x + "\n" + y) ?? "", type, TimeSpan.FromSeconds(30)));
                }
            }
            else
            {
                notificationManage.Show(new Notification(message, ex.Message, type));
            }
        }

        logger.LogError(ex, message);
    }
}
