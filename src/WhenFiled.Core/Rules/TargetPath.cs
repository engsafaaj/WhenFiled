namespace WhenFiled.Core.Rules;

/// <summary>Target folder and the long-path limit (D13).</summary>
public static class TargetPath
{
    public const string TooLongReason = "Path too long";
    private const int Limit = 260;

    /// <summary>The full folder, such as D:\Sorted\2019\07.</summary>
    public static string Folder(string destination, DateOnly date) =>
        $@"{destination.TrimEnd('\\', '/')}\{date:yyyy}\{date:MM}";

    /// <summary>The folder as the window shows it, such as 2019\07\.</summary>
    public static string Shown(DateOnly date) => $@"{date:yyyy}\{date:MM}\";

    public static bool IsTooLong(string fullPath) => fullPath.Length >= Limit;
}
