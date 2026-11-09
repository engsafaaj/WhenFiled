using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using WhenFiled.Core.Rules;

namespace WhenFiled.Core.Disk;

public sealed partial class Scanner
{
    /// <summary>
    /// Reads the raw EXIF text and video time (D5, D6). Unreadable metadata counts as none (D11).
    /// A locked or refused file still throws, so the caller can report it.
    /// </summary>
    private static RawDates ReadRawDates(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        IReadOnlyList<MetadataExtractor.Directory> directories;
        try
        {
            directories = ImageMetadataReader.ReadMetadata(stream);
        }
        // Any parser error on a corrupt file means "no date". Only a real read problem
        // (locked, refused, I/O) is left to throw so the caller can report it.
        catch (Exception e) when (e is EndOfStreamException
            || e is not (IOException or UnauthorizedAccessException))
        {
            return new RawDates(null, null);
        }

        var exif = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault()
            ?.GetString(ExifDirectoryBase.TagDateTimeOriginal);

        DateTime? video = null;
        var header = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
        if (header is not null && header.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created))
            video = DateTime.SpecifyKind(created, DateTimeKind.Utc);

        return new RawDates(exif, video);
    }
}
