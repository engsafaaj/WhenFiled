namespace WhenFiled.Core.Rules;

public enum FolderCheck { Ok, DestinationIsSource, DestinationInsideSource, DifferentDrives }

/// <summary>Folder checks before Scan (D12). Compares full paths, ignoring case.</summary>
public static class PathRules
{
    public static FolderCheck Check(string source, string destination)
    {
        var src = WithSlash(source);
        var dst = WithSlash(destination);

        var srcRoot = Path.GetPathRoot(src) ?? "";
        var dstRoot = Path.GetPathRoot(dst) ?? "";
        if (!string.Equals(srcRoot, dstRoot, StringComparison.OrdinalIgnoreCase))
            return FolderCheck.DifferentDrives;

        if (string.Equals(src, dst, StringComparison.OrdinalIgnoreCase))
            return FolderCheck.DestinationIsSource;

        if (dst.StartsWith(src, StringComparison.OrdinalIgnoreCase))
            return FolderCheck.DestinationInsideSource;

        return FolderCheck.Ok;
    }

    /// <summary>The text for the window when Scan refuses to start, or null when Ok.</summary>
    public static string? Refusal(FolderCheck check, string source) => check switch
    {
        FolderCheck.DestinationInsideSource =>
            $"The destination can’t be inside the source folder. Choose a destination outside {source}.",
        FolderCheck.DestinationIsSource =>
            "The destination can’t be the same folder as the source. Choose a different destination.",
        FolderCheck.DifferentDrives =>
            "The source and destination must be on the same drive. WhenFiled moves files, it doesn’t copy them.",
        _ => null,
    };

    private static string WithSlash(string path) => path.TrimEnd('\\', '/') + "\\";
}
