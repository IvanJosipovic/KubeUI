using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;
using k8s;
using k8s.Models;

namespace KubeUI.Converters;

public class ObjectOwnerNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IKubernetesObject<V1ObjectMeta> obj)
        {
            return obj.Metadata.OwnerReferences.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A";
        }

        return "N/A";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
