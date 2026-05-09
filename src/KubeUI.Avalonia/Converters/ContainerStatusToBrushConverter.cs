using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using k8s.Models;

namespace KubeUI.Avalonia.Converters;

public class ContainerStatusToBrushConverter : IValueConverter
{
    public static ContainerStatusToBrushConverter Instance() => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not V1ContainerStatus status)
            return Brushes.Red;

        try
        {
            var param = parameter?.ToString();
            var isEphemeral = param == "ephemeral";
            var isInit = param == "init";

            // Ready & Started
            if (status.Ready && status.Started == true)
                return isEphemeral ? Brushes.DodgerBlue : isInit ? Brushes.MediumPurple : Brushes.LimeGreen;

            // Started but not ready
            if (!status.Ready && status.Started == true)
                return isEphemeral ? Brushes.CornflowerBlue : isInit ? Brushes.MediumOrchid : Brushes.Orange;

            // Waiting state
            if (status.State?.Waiting != null)
                return isEphemeral ? Brushes.OrangeRed : isInit ? Brushes.PaleVioletRed : Brushes.Orange;

            // Running state (container is running but may not be Ready)
            if (status.State?.Running != null)
            {
                // If Ready is true prefer the ready color; otherwise indicate running-but-not-ready
                if (status.Ready && status.Started == true)
                    return isEphemeral ? Brushes.DodgerBlue : isInit ? Brushes.MediumPurple : Brushes.LimeGreen;

                return isEphemeral ? Brushes.CornflowerBlue : isInit ? Brushes.MediumOrchid : Brushes.Orange;
            }

            // Terminated state
            var terminated = status.State?.Terminated;
            if (terminated != null)
            {
                if (terminated.Reason == "Completed")
                    return Brushes.Gray;

                return isEphemeral ? Brushes.OrangeRed : isInit ? Brushes.PaleVioletRed : Brushes.Orange;
            }

            // Fallback
            return Brushes.Red;
        }
        catch
        {
            return Brushes.Red;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

