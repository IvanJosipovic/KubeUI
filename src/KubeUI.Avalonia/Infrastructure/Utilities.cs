using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Infrastructure;

public static class Utilities
{
    public static T CloneObject<T>(T resource)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(resource);

        return resource is GenericKubernetesObject generic
            ? (T)(object)(KubernetesJson.Deserialize<GenericKubernetesObject>(KubernetesJson.Serialize(generic))
                ?? throw new InvalidOperationException("Deserialization returned null."))
            : KubernetesJson.Deserialize<T>(KubernetesJson.Serialize(resource))
                ?? throw new InvalidOperationException("Deserialization returned null.");
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
            || ex.Message.Contains("Exception during deserialization", StringComparison.Ordinal)
            || ex.Message.Contains("Exception during serialization", StringComparison.Ordinal);
    }

    public static T Behaviors<T>(this T obj, params BehaviorCollection behaviorCollection) where T : AvaloniaObject
    {
        var collection = obj.GetValue(Interaction.BehaviorsProperty);
        collection ??= [];

        collection.AddRange(behaviorCollection);
        obj.SetValue(Interaction.BehaviorsProperty, collection);

        return obj;
    }

    public static T KeyBindings<T>(this T obj, params KeyBinding[] keyBindings) where T : IInputElement
    {
        obj.KeyBindings.AddRange(keyBindings);
        return obj;
    }
}
