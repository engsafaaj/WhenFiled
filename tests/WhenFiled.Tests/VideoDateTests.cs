using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class VideoDateTests
{
    private static TimeZoneInfo Zone(int hours) =>
        TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(hours), "test", "test");

    [Fact]
    public void Utc_zone_keeps_the_date() =>
        Assert.Equal(new DateOnly(2021, 5, 14),
            VideoDate.ToLocalDate(new DateTime(2021, 5, 14, 10, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Utc));

    [Fact]
    public void Late_utc_evening_moves_to_next_day_in_utc_plus_2() =>
        Assert.Equal(new DateOnly(2021, 6, 1),
            VideoDate.ToLocalDate(new DateTime(2021, 5, 31, 22, 30, 0, DateTimeKind.Utc), Zone(2)));
}
