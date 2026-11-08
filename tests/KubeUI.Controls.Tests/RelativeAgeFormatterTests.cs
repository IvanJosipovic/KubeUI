using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class RelativeAgeFormatterTests
{
    [Fact]
    public void FormatPrettyAge_preserves_existing_unit_boundaries_and_rounding()
    {
        var now = new DateTime(2026, 3, 29, 0, 34, 52, DateTimeKind.Utc);

        RelativeAgeFormatter.FormatPrettyAge(now, now).ShouldBe("0ms");
        RelativeAgeFormatter.FormatPrettyAge(now.AddMilliseconds(-4), now).ShouldBe("4ms");
        RelativeAgeFormatter.FormatPrettyAge(now.AddSeconds(-74), now).ShouldBe("1m14s");
        RelativeAgeFormatter.FormatPrettyAge(now.AddHours(-2), now).ShouldBe("2h");
        RelativeAgeFormatter.FormatPrettyAge(now.AddDays(-3), now).ShouldBe("3d");
        RelativeAgeFormatter.FormatPrettyAge(now.AddDays(-730), now).ShouldBe("2y");
        RelativeAgeFormatter.FormatPrettyAge(now.AddSeconds(1), now).ShouldBe("0ms");
    }

    [Fact]
    public void FormatPrettyAge_treats_unspecified_timestamp_as_utc()
    {
        var now = new DateTime(2026, 3, 29, 0, 34, 52, DateTimeKind.Utc);
        var unspecified = DateTime.SpecifyKind(now.AddMinutes(-2), DateTimeKind.Unspecified);

        RelativeAgeFormatter.FormatPrettyAge(unspecified, now).ShouldBe("2m0s");
    }

    [Fact]
    public void FormatPrettyLastSeen_handles_missing_same_day_and_previous_day_timestamps()
    {
        var now = new DateTime(2026, 3, 29, 0, 34, 52, DateTimeKind.Utc);
        var localNow = now.ToLocalTime();
        var sameDayLocal = localNow.Date.AddTicks(localNow.TimeOfDay.Ticks / 2);
        var previousDayLocal = localNow.Date.AddTicks(-1);
        var sameDayUtc = DateTime.SpecifyKind(sameDayLocal, DateTimeKind.Local).ToUniversalTime();
        var previousDayUtc = DateTime.SpecifyKind(previousDayLocal, DateTimeKind.Local).ToUniversalTime();

        RelativeAgeFormatter.FormatPrettyLastSeen(null, now).ShouldBeEmpty();
        RelativeAgeFormatter.FormatPrettyLastSeen(sameDayUtc, now)
            .ShouldContain($"({sameDayLocal.ToString("t")})");
        RelativeAgeFormatter.FormatPrettyLastSeen(previousDayUtc, now)
            .ShouldContain($"({previousDayLocal.ToString("g")})");
    }
}
