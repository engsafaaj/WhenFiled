namespace WhenFiled.Core.Rules;

/// <summary>Raw values read by Scanner. The rules parse and check them (D6).</summary>
public record RawDates(string? ExifDateTimeOriginal, DateTime? VideoCreatedUtc);
