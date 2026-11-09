using System.Security.Cryptography;
using WhenFiled.Core.Rules;

namespace WhenFiled.Core.Disk;

public sealed partial class Scanner
{
    private static readonly EnumerationOptions Options = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        // D15: skip hidden, system and reparse-point files and folders.
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint,
    };

    private static IEnumerable<string> ListFiles(string source) =>
        Directory.EnumerateFiles(source, "*", Options);

    /// <summary>Returns null for files that aren't photos or videos (MediaFilter).</summary>
    private static ScannedFile? ReadFile(string path, DateOnly today, TimeZoneInfo zone)
    {
        long size;
        try { size = new FileInfo(path).Length; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }

        var name = Path.GetFileName(path);
        if (!MediaFilter.IsMedia(name, size))
            return null;

        try
        {
            var raw = ReadRawDates(path);
            return new ScannedFile(path, size, DateResolver.Resolve(name, raw, today, zone));
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(path, size, "Access denied");
        }
        catch (IOException)
        {
            return Problem(path, size, "Still changing, scan again");
        }
    }

    private static ScannedFile Problem(string path, long size, string reason) =>
        new(path, size, new DateResult(null, DateSource.None), reason);

    // IDestinationView (D2)

    public IReadOnlyCollection<string> FileNames(string folder) =>
        Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder).Select(p => Path.GetFileName(p)).ToList()
            : [];

    public bool IsSameContent(string sourcePath, string destinationPath)
    {
        try
        {
            if (new FileInfo(sourcePath).Length != new FileInfo(destinationPath).Length)
                return false;
            return Sha256(sourcePath).SequenceEqual(Sha256(destinationPath));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false; // unsure means different: the file just gets a (2) name
        }
    }

    private static byte[] Sha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return SHA256.HashData(stream);
    }
}
