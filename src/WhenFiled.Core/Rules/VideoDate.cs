namespace WhenFiled.Core.Rules;

public static class VideoDate
{
    /// <summary>MP4 and MOV store UTC. Convert to the given zone, then keep the date (D7).</summary>
    public static DateOnly ToLocalDate(DateTime createdUtc, TimeZoneInfo zone)
    {
        var utc = DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, zone));
    }
}
