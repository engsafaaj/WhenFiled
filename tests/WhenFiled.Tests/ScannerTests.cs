using WhenFiled.Core.Disk;
using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class ScannerTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private static ScanResult Scan(string source, string destination) =>
        new Scanner().Scan(source, destination, Today, TimeZoneInfo.Utc, null);

    private sealed record Snapshot(string Hash, DateTime Modified, FileAttributes Attributes);

    private static Dictionary<string, Snapshot> Snap(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToDictionary(
            p => p, p => new Snapshot(TempFolder.Hash(p), System.IO.File.GetLastWriteTimeUtc(p),
                System.IO.File.GetAttributes(p)));

    [Fact]
    public void Scan_changes_nothing()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        t.File(@"src\IMG-20160315-WA0020.jpg");
        t.File(@"src\sub\holiday.jpg");
        t.File(@"src\notes.txt");
        var ro = t.File(@"src\VID_20210514.mp4");
        System.IO.File.SetAttributes(ro, FileAttributes.ReadOnly);
        var before = Snap(src);

        var result = Scan(src, dst);

        Assert.Null(result.Refusal);
        Assert.Equal(before, Snap(src));
        Assert.Empty(Directory.GetFileSystemEntries(dst));
    }

    [Fact]
    public void Destination_inside_source_is_refused()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        t.File(@"src\IMG-20160315-WA0020.jpg");
        var dst = Path.Combine(src, "Sorted");

        var result = Scan(src, dst);

        Assert.NotNull(result.Refusal);
        Assert.Contains("inside the source", result.Refusal);
        Assert.Empty(result.Items);
        Assert.False(Directory.Exists(dst));
    }

    [Fact]
    public void Missing_folder_is_refused()
    {
        using var t = new TempFolder();
        var result = Scan(Path.Combine(t.Root, "nope"), t.Dir("dst"));
        Assert.Contains("source folder doesn’t exist", result.Refusal);
    }

    [Fact]
    public void Files_are_planned_from_their_names_and_filtered()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        t.File(@"src\IMG-20160315-WA0020.jpg");
        t.File(@"src\sub\holiday.jpg");
        t.File(@"src\notes.txt");
        t.File(@"src\empty.jpg", "");

        var items = Scan(src, dst).Items;

        Assert.Equal(2, items.Count);
        var dated = items.Single(i => i.File.Path.EndsWith("WA0020.jpg"));
        Assert.Equal(Path.Combine(dst, "2016", "03"), dated.TargetFolder);
        Assert.Equal(DateSource.FileName, dated.File.Date.From);
        Assert.Equal(PlanStatus.NoDate, items.Single(i => i.File.Path.EndsWith("holiday.jpg")).Status);
    }

    [Fact]
    public void Destination_files_are_seen_and_compared_by_content()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        t.File(@"src\IMG-20160315-a.jpg", "same");
        t.File(@"src\IMG-20160315-b.jpg", "mine");
        t.File(@"dst\2016\03\IMG-20160315-a.jpg", "same");
        t.File(@"dst\2016\03\IMG-20160315-b.jpg", "other");

        var items = Scan(src, dst).Items;

        Assert.Equal(Planner.AlreadyThere, items.Single(i => i.File.Path.EndsWith("-a.jpg")).Reason);
        Assert.Equal("IMG-20160315-b (2).jpg", items.Single(i => i.File.Path.EndsWith("-b.jpg")).TargetName);
    }
}
