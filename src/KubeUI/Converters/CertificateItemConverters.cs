using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Data.Converters;

namespace KubeUI.Converters;

public sealed class CertificateSanDnsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is X509Certificate2 cert)
        {
            var san = cert.Extensions
                .OfType<X509SubjectAlternativeNameExtension>()
                .FirstOrDefault();
            if (san != null)
                return san.EnumerateDnsNames().ToArray();
        }
        return Array.Empty<string>();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class CertificateSanIpConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is X509Certificate2 cert)
        {
            var san = cert.Extensions
                .OfType<X509SubjectAlternativeNameExtension>()
                .FirstOrDefault();
            if (san != null)
                return san.EnumerateIPAddresses().Select(ip => ip.ToString()).ToArray();
        }
        return Array.Empty<string>();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class LocalDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? dt.ToLocalTime().ToString() : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class CertificateExpiryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is X509Certificate2 cert)
        {
            var expires = cert.NotAfter.ToLocalTime();
            var days = (expires - DateTime.Now).Days;
            return $"{expires} (Valid:{days} days)";
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
