using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class DateResolverTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);
    private static readonly DateTime VideoTime = new(2021, 5, 14, 10, 0, 0, DateTimeKind.Utc);

    private static DateResult Resolve(string name, string? exif, DateTime? video) =>
        DateResolver.Resolve(name, new RawDates(exif, video), Today, TimeZoneInfo.Utc);

    [Fact]
    public void Exif_beats_video_and_file_name()
    {
        var r = Resolve("IMG-20160315-WA0020.jpg", "2019:07:04 10:11:12", VideoTime);
        Assert.Equal(new DateResult(new DateOnly(2019, 7, 4), DateSource.Exif), r);
    }

    [Fact]
    public void Video_beats_file_name()
    {
        var r = Resolve("VID_20160315.mp4", null, VideoTime);
        Assert.Equal(new DateResult(new DateOnly(2021, 5, 14), DateSource.Video), r);
    }

    [Fact]
    public void Zero_exif_date_falls_through_to_file_name()
    {
        var r = Resolve("IMG-20160315-WA0020.jpg", "0000:00:00 00:00:00", null);
        Assert.Equal(new DateResult(new DateOnly(2016, 3, 15), DateSource.FileName), r);
    }

    [Fact]
    public void Future_exif_date_falls_through()
    {
        var r = Resolve("IMG-20160315-WA0020.jpg", "2030:01:01 00:00:00", null);
        Assert.Equal(new DateResult(new DateOnly(2016, 3, 15), DateSource.FileName), r);
    }

    [Fact]
    public void Nothing_found_gives_none()
    {
        var r = Resolve("holiday.jpg", null, null);
        Assert.Equal(new DateResult(null, DateSource.None), r);
    }
}
