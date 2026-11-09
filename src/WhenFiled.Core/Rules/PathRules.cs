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

    private static string WithSlash(string path) => path.TrimEnd('\\', '/') + "\\";
}
