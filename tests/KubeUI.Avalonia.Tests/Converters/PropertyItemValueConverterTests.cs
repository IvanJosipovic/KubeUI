using System.Globalization;
using KubeUI.Avalonia.Converters;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Converters;

public sealed class PropertyItemValueConverterTests
{
    private readonly PropertyItemValueConverter _converter = new();

    [Fact]
    public void converts_utc_datetime_to_local_time()
    {
        var utc = new DateTime(2025, 03, 28, 22, 0, 0, DateTimeKind.Utc);

        var result = _converter.Convert(utc, typeof(string), null, CultureInfo.InvariantCulture);

        result.ShouldBe(utc.ToLocalTime().ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void converts_datetimeoffset_to_local_time()
    {
        var dto = new DateTimeOffset(2025, 03, 28, 22, 0, 0, TimeSpan.Zero);

        var result = _converter.Convert(dto, typeof(string), null, CultureInfo.InvariantCulture);

        result.ShouldBe(dto.ToLocalTime().ToString(CultureInfo.InvariantCulture));
    }
}
