using System.Globalization;
using Avalonia.Data.Converters;
using k8s.Models;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Converters;

public class ContainerStatusToBrushConverter : IValueConverter
{
    public static ContainerStatusToBrushConverter Instance() => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not V1ContainerStatus status)
            return ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush");

        var param = parameter?.ToString();
        var isEphemeral = param == "ephemeral";
        var isInit = param == "init";

            // Ready & Started
            if (status.Ready && status.Started == true)
                return isEphemeral
                    ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralReadyBrush")
                    : isInit
                        ? ApplicationBrushResources.GetBrush("ContainerStatusInitReadyBrush")
                        : ApplicationBrushResources.GetBrush("ContainerStatusReadyBrush");

            // Started but not ready
            if (!status.Ready && status.Started == true)
                return isEphemeral
                    ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralRunningBrush")
                    : isInit
                        ? ApplicationBrushResources.GetBrush("ContainerStatusInitRunningBrush")
                        : ApplicationBrushResources.GetBrush("ContainerStatusRunningBrush");

            // Waiting state
            if (status.State?.Waiting != null)
                return isEphemeral
                    ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralWaitingBrush")
                    : isInit
                        ? ApplicationBrushResources.GetBrush("ContainerStatusInitWaitingBrush")
                        : ApplicationBrushResources.GetBrush("ContainerStatusWaitingBrush");

            // Running state (container is running but may not be Ready)
            if (status.State?.Running != null)
            {
                // If Ready is true prefer the ready color; otherwise indicate running-but-not-ready
                if (status.Ready && status.Started == true)
                    return isEphemeral
                        ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralReadyBrush")
                        : isInit
                            ? ApplicationBrushResources.GetBrush("ContainerStatusInitReadyBrush")
                            : ApplicationBrushResources.GetBrush("ContainerStatusReadyBrush");

                return isEphemeral
                    ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralRunningBrush")
                    : isInit
                        ? ApplicationBrushResources.GetBrush("ContainerStatusInitRunningBrush")
                        : ApplicationBrushResources.GetBrush("ContainerStatusRunningBrush");
            }

            // Terminated state
            var terminated = status.State?.Terminated;
            if (terminated != null)
            {
                if (terminated.Reason == "Completed")
                    return ApplicationBrushResources.GetBrush("ContainerStatusCompletedBrush");

                return isEphemeral
                    ? ApplicationBrushResources.GetBrush("ContainerStatusEphemeralWaitingBrush")
                    : isInit
                        ? ApplicationBrushResources.GetBrush("ContainerStatusInitWaitingBrush")
                        : ApplicationBrushResources.GetBrush("ContainerStatusWaitingBrush");
            }

            // Fallback
            return ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

