namespace WhenFiled.Core.Rules;

public static class DateResolver
{
    /// <summary>
    /// Picks the first valid date in the spec's order: EXIF, video, file name, else none.
    /// A date outside the valid range falls through to the next source (D10).
    /// </summary>
    public static DateResult Resolve(string fileName, RawDates raw, DateOnly today, TimeZoneInfo zone)
    {
        var exif = ExifDate.Parse(raw.ExifDateTimeOriginal);
        if (exif is { } e && ValidDate.IsValid(e, today))
            return new DateResult(e, DateSource.Exif);

        if (raw.VideoCreatedUtc is { } utc)
        {
            var video = VideoDate.ToLocalDate(utc, zone);
            if (ValidDate.IsValid(video, today))
                return new DateResult(video, DateSource.Video);
        }

        if (FileNameDate.Find(fileName, today) is { } named)
            return new DateResult(named, DateSource.FileName);

        return new DateResult(null, DateSource.None);
    }
}
