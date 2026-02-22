using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls.Notifications;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace KubeUI;

public static class Utilities
{
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

    private static readonly MethodInfo s_deserializeJson = typeof(KubernetesJson).GetMethod(nameof(KubernetesJson.Deserialize), BindingFlags.Static | BindingFlags.Public, [typeof(string), typeof(JsonSerializerOptions)]);

    public static object DeserializeKubeJson(string json, Type type)
    {
        var fooRef = s_deserializeJson.MakeGenericMethod(type);

        return fooRef.Invoke(null, [json, null]);
    }

    public static void HandleException(ILogger logger, INotificationManager notificationManage, Exception ex, string message, NotificationType type = NotificationType.Error, bool sendNotification = false)
    {
        if (sendNotification)
        {
            try
            {
                if (ex is AggregateException aggregate)
                {
                    foreach (var item in aggregate.InnerExceptions)
                    {
                        if (item is HttpOperationException opEx)
                        {
                            var status = KubernetesJson.Deserialize<V1Status>(opEx.Response.Content);

                            if (status != null)
                            {
                                Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(status.Reason, status.Message + "\n\n" + status?.Details?.Causes?.Select(x => x.Message).Aggregate((x, y) => x + "\n" + y) ?? "", type, TimeSpan.FromSeconds(30))));
                            }
                        }
                        else
                        {
                            Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(message, item.Message, type)));
                        }
                    }
                }
                else if (ex is HttpOperationException opEx)
                {
                    var status = KubernetesJson.Deserialize<V1Status>(opEx.Response.Content);

                    if (status != null)
                    {
                        Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(status.Reason, status.Message + "\n\n" + status?.Details?.Causes?.Select(x => x.Message).Aggregate((x, y) => x + "\n" + y) ?? "", type, TimeSpan.FromSeconds(30))));
                    }
                }
                else
                {
                    Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(message, ex.Message, type)));
                }
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2, "Error sending Notification");
            }
        }

        logger.LogError(ex, message);
    }

    public static IBinding FuncBinding<T>(Expression<Func<T, object>> func)
    {
        return new Binding(PathBuilder(func));
    }

    /// <summary>
    /// Get the path of a property from a lambda expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string PathBuilder<T>(Expression<Func<T, object>> func)
    {
        if (func.Body is MemberExpression body)
        {
            var str = body.ToString();
            return str.Substring(str.IndexOf(".") + 1);
        }

        if (func.Body is UnaryExpression unary)
        {
            var str = unary.Operand.ToString();
            return str.Substring(str.IndexOf(".") + 1);
        }

        throw new Exception("Unknown Expression Type");
    }
}
