using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls.Notifications;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Infrastructure;

namespace KubeUI.Avalonia.Infrastructure;

public static class Utilities
{
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

        if (type == typeof(V1EndpointSlice))
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

        return fooRef.Invoke(null, [json, null]) ?? throw new InvalidOperationException("Deserialization returned null.");
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
                            var detail = GetMeaningfulExceptionMessage(item);
                            Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(message, detail, type)));
                        }
                    }
                }
                else if (ex is HttpOperationException opEx)
                {
                    var status = KubernetesJson.Deserialize<V1Status>(opEx.Response.Content);

                    if (status != null)
                    {
                        var detail = FormatKubernetesStatusMessage(status, opEx.Message);
                        Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(status.Reason, detail, type, TimeSpan.FromSeconds(30))));
                    }
                }
                else
                {
                    var detail = GetMeaningfulExceptionMessage(ex);
                    Dispatcher.UIThread.Post(() => notificationManage.Show(new Notification(message, detail, type)));
                }
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2, "Error sending Notification");
            }
        }

        logger.LogError(ex, message);
    }

    internal static string GetMeaningfulExceptionMessage(Exception ex)
    {
        return GetMeaningfulException(ex).Message;
    }

    internal static string GetUserFacingErrorMessage(Exception ex)
    {
        var meaningfulException = GetMeaningfulException(ex);
        if (TryGetKubernetesStatusMessage(meaningfulException, out var statusMessage))
        {
            return statusMessage;
        }

        return meaningfulException.Message;
    }

    internal static Exception GetMeaningfulException(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null
               && IsWrapperException(current))
        {
            current = current.InnerException;
        }

        return current;
    }

    private static bool TryGetKubernetesStatusMessage(Exception ex, out string message)
    {
        if (ex is not HttpOperationException opEx)
        {
            message = string.Empty;
            return false;
        }

        try
        {
            var status = KubernetesJson.Deserialize<V1Status>(opEx.Response.Content);
            if (status == null)
            {
                message = string.Empty;
                return false;
            }

            message = FormatKubernetesStatusMessage(status, opEx.Message);

            return true;
        }
        catch
        {
            message = string.Empty;
            return false;
        }
    }

    internal static string FormatKubernetesStatusMessage(V1Status status, string fallbackMessage)
    {
        var causes = status.Details?.Causes?
            .Select(FormatKubernetesStatusCause)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (!string.IsNullOrWhiteSpace(status.Message))
        {
            if (causes is { Length: > 0 })
            {
                var header = ExtractKubernetesStatusHeader(status.Message);
                if (!string.IsNullOrWhiteSpace(header))
                {
                    return $"{header}\n{string.Join("\n", causes)}";
                }
            }

            return status.Message;
        }

        if (causes is { Length: > 0 })
        {
            return string.Join("\n", causes);
        }

        return status.Reason ?? fallbackMessage;
    }

    private static string FormatKubernetesStatusCause(V1StatusCause cause)
    {
        if (!string.IsNullOrWhiteSpace(cause.Field) && !string.IsNullOrWhiteSpace(cause.Message))
        {
            return $"{cause.Field}: {cause.Message}";
        }

        if (!string.IsNullOrWhiteSpace(cause.Message))
        {
            return cause.Message;
        }

        if (!string.IsNullOrWhiteSpace(cause.Field))
        {
            return cause.Field;
        }

        return string.Empty;
    }

    private static string ExtractKubernetesStatusHeader(string message)
    {
        var bracketIndex = message.IndexOf('[');
        if (bracketIndex <= 0)
        {
            return message.Trim();
        }

        return message[..bracketIndex].TrimEnd();
    }

    private static bool IsWrapperException(Exception ex)
    {
        return ex is AggregateException
            or TargetInvocationException
            || ex.Message.Contains("Exception during deserialization", StringComparison.Ordinal)
            || ex.Message.Contains("Exception during serialization", StringComparison.Ordinal);
    }

}

