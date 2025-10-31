using System.Globalization;
using Avalonia.Data.Converters;
using k8s;
using k8s.Models;

namespace KubeUI.Converters;

public class AgeConverter : IValueConverter
{
    public static readonly AgeConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IKubernetesObject<V1ObjectMeta> obj && obj.Metadata.CreationTimestamp.HasValue)
        {
            var Delta = DateTime.UtcNow - obj.Metadata.CreationTimestamp.Value;

            if (Delta.TotalMilliseconds <= 0)
                return "0ms";
            else if (Delta.TotalDays >= 365)
                return $"{(Delta.TotalDays / 365):N0}y";
            else if (Delta.TotalDays >= 1)
                return $"{Delta.TotalDays:N0}d";
            else if (Delta.TotalHours >= 1)
                return $"{Delta.TotalHours:N0}h";
            else if (Delta.TotalMinutes >= 1)
                return $"{Delta.TotalMinutes:N0}m{Delta.Seconds:N0}s";
            else if (Delta.TotalSeconds >= 1)
                return $"{Delta.TotalSeconds:N0}s";
            else
                return $"{Delta.TotalMilliseconds:N0}ms";
        }

        return "N/A";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
