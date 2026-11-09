using System.Globalization;

namespace WhenFiled.Core.Rules;

public static class ExifDate
{
    /// <summary>
    /// Reads the date from EXIF "yyyy:MM:dd HH:mm:ss". EXIF is already local time (D7).
    /// Returns null for blank, zero or impossible dates. Range is checked by the resolver.
    /// </summary>
    public static DateOnly? Parse(string? raw)
    {
        var text = raw?.Trim('\0', ' ');
        if (string.IsNullOrEmpty(text) || text.Length < 10)
            return null;

        return DateOnly.TryParseExact(text[..10], "yyyy:MM:dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}
