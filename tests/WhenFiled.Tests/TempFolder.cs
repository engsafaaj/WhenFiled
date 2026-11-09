using System.Security.Cryptography;

namespace WhenFiled.Tests;

/// <summary>A new folder under the system temp folder. Dispose deletes only this folder (CLAUDE.md rule 1).</summary>
public sealed class TempFolder : IDisposable
{
    public string Root { get; } =
        Path.Combine(Path.GetTempPath(), "WhenFiledTests", Guid.NewGuid().ToString("N"));

    public TempFolder() => Directory.CreateDirectory(Root);

    public string Dir(string relative)
    {
        var path = Path.Combine(Root, relative);
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Makes a file with made-up bytes and returns its full path.</summary>
    public string File(string relative, string content = "photo bytes")
    {
        var path = Path.Combine(Root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(path, content);
        return path;
    }

    public static string Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(System.IO.File.ReadAllBytes(path)));

    public void Dispose()
    {
        foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
            System.IO.File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(Root, recursive: true);
    }
}
