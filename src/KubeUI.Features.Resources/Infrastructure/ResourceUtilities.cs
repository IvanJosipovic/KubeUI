using System.Text.Json;
using Avalonia.Controls.Notifications;
using k8s.Autorest;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Features.Resources.Infrastructure;

/// <summary>Shared error handling for Kubernetes resource actions and editors.</summary>
public static class ResourceUtilities
{
    /// <summary>Logs an action failure and optionally reports its Kubernetes details to the user.</summary>
    /// <param name="logger">Logger for the failed action.</param>
    /// <param name="notificationManager">Notification surface.</param>
    /// <param name="exception">Failure to report.</param>
    /// <param name="message">Short action description.</param>
    /// <param name="type">Notification severity.</param>
    /// <param name="sendNotification">Whether to display a notification.</param>
    public static void HandleException(
        ILogger logger,
        INotificationManager notificationManager,
        Exception exception,
        string message,
        NotificationType type = NotificationType.Error,
        bool sendNotification = false)
    {
        if (sendNotification)
        {
            try
            {
                if (exception is AggregateException aggregate)
                {
                    foreach (var item in aggregate.InnerExceptions)
                    {
                        if (item is HttpOperationException operationException)
                        {
                            var status = JsonSerializer.Deserialize(
                                operationException.Response.Content,
                                KubernetesJsonStaticContext.Default.V1Status);
                            if (status is not null)
                            {
                                var details = status.Message + "\n\n" +
                                    string.Join("\n", status.Details?.Causes?.Select(static cause => cause.Message) ?? []);
                                Dispatcher.UIThread.Post(() => notificationManager.Show(
                                    new Notification(status.Reason, details, type, TimeSpan.FromSeconds(30))));
                            }
                        }
                        else
                        {
                            var detail = GetMeaningfulExceptionMessage(item);
                            Dispatcher.UIThread.Post(() => notificationManager.Show(new Notification(message, detail, type)));
                        }
                    }
                }
                else if (exception is HttpOperationException operationException)
                {
                    var status = JsonSerializer.Deserialize(
                        operationException.Response.Content,
                        KubernetesJsonStaticContext.Default.V1Status);
                    if (status is not null)
                    {
                        var detail = FormatKubernetesStatusMessage(status, operationException.Message);
                        Dispatcher.UIThread.Post(() => notificationManager.Show(
                            new Notification(status.Reason, detail, type, TimeSpan.FromSeconds(30))));
                    }
                }
                else
                {
                    var detail = GetMeaningfulExceptionMessage(exception);
                    Dispatcher.UIThread.Post(() => notificationManager.Show(new Notification(message, detail, type)));
                }
            }
            catch (Exception notificationException)
            {
                logger.LogError(notificationException, "Error sending Notification");
            }
        }

        logger.LogError(exception, message);
    }

    /// <summary>Gets the innermost useful message from a wrapped exception.</summary>
    public static string GetMeaningfulExceptionMessage(Exception exception)
        => GetMeaningfulException(exception).Message;

    /// <summary>Gets a user-facing Kubernetes status message when available.</summary>
    public static string GetUserFacingErrorMessage(Exception exception)
    {
        var meaningfulException = GetMeaningfulException(exception);
        return TryGetKubernetesStatusMessage(meaningfulException, out var statusMessage)
            ? statusMessage
            : meaningfulException.Message;
    }

    /// <summary>Unwraps serialization and aggregate exceptions used by Kubernetes clients.</summary>
    public static Exception GetMeaningfulException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var current = exception;
        while (current.InnerException is not null && IsWrapperException(current))
        {
            current = current.InnerException;
        }

        return current;
    }

    /// <summary>Formats a Kubernetes status and its field causes for display.</summary>
    public static string FormatKubernetesStatusMessage(V1Status status, string fallbackMessage)
    {
        ArgumentNullException.ThrowIfNull(status);
        var causes = status.Details?.Causes?
            .Select(FormatKubernetesStatusCause)
            .Where(static cause => !string.IsNullOrWhiteSpace(cause))
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

        return causes is { Length: > 0 }
            ? string.Join("\n", causes)
            : status.Reason ?? fallbackMessage;
    }

    private static bool TryGetKubernetesStatusMessage(Exception exception, out string message)
    {
        if (exception is not HttpOperationException operationException)
        {
            message = string.Empty;
            return false;
        }

        try
        {
            var status = JsonSerializer.Deserialize(operationException.Response.Content, KubernetesJsonStaticContext.Default.V1Status);
            if (status is null)
            {
                message = string.Empty;
                return false;
            }

            message = FormatKubernetesStatusMessage(status, operationException.Message);
            return true;
        }
        catch
        {
            message = string.Empty;
            return false;
        }
    }

    private static string FormatKubernetesStatusCause(V1StatusCause cause)
    {
        if (!string.IsNullOrWhiteSpace(cause.Field) && !string.IsNullOrWhiteSpace(cause.Message))
        {
            return $"{cause.Field}: {cause.Message}";
        }

        return !string.IsNullOrWhiteSpace(cause.Message)
            ? cause.Message
            : cause.Field ?? string.Empty;
    }

    private static string ExtractKubernetesStatusHeader(string message)
    {
        var bracketIndex = message.IndexOf('[');
        return bracketIndex <= 0
            ? message.Trim()
            : message[..bracketIndex].TrimEnd();
    }

    private static bool IsWrapperException(Exception exception)
        => exception is AggregateException
            || exception.Message.Contains("Exception during deserialization", StringComparison.Ordinal)
            || exception.Message.Contains("Exception during serialization", StringComparison.Ordinal);
}
