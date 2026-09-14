using System.Globalization;
using SharedConverters = KubeUI.Avalonia.Converters.Converters;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Converters;

public sealed class ConvertersTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void not_inverts_boolean_value(bool value, bool expected)
    {
        var result = SharedConverters.Not.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("value", true)]
    public void has_text_reports_non_whitespace_text(string? value, bool expected)
    {
        var result = SharedConverters.HasText.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

        result.ShouldBe(expected);
    }
}
