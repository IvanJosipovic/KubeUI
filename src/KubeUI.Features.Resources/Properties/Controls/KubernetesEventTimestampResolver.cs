using k8s.Models;

namespace KubeUI.Features.Resources.Properties.Controls;

/// <summary>Applies Kubernetes Event timestamp precedence without formatting UI text.</summary>
public static class KubernetesEventTimestampResolver
{
    /// <summary>Gets the event time, falling back through legacy, event, and creation timestamps.</summary>
    /// <param name="event">Kubernetes Event to inspect.</param>
    /// <returns>Last timestamp, event time, creation timestamp, or null when none is available.</returns>
    public static DateTime? Resolve(Corev1Event @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.LastTimestamp ?? @event.EventTime ?? @event.Metadata?.CreationTimestamp;
    }
}
