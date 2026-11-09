namespace WhenFiled.Core.Rules;

public static class MediaFilter
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".heic", ".png", ".webp", ".mp4", ".mov", ".3gp",
    };

    public static bool IsMedia(string fileName, long sizeBytes) =>
        sizeBytes > 0 && Extensions.Contains(Path.GetExtension(fileName));
}
