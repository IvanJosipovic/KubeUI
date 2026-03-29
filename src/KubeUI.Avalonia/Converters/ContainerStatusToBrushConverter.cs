using System.Globalization;
using Avalonia.Data.Converters;
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
            // Ready & Started
            if (status.Ready && status.Started == true)
                return Brushes.LimeGreen;

            // Started but not ready
            if (!status.Ready && status.Started == true)
                return Brushes.Orange;

            // Waiting state
            if (status.State?.Waiting != null)
                return Brushes.Orange;

            // Terminated state
            var terminated = status.State?.Terminated;
            if (terminated != null)
            {
                if (terminated.Reason == "Completed")
                    return Brushes.Gray;

                return Brushes.Orange;
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

