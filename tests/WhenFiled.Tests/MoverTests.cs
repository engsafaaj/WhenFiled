using WhenFiled.Core.Disk;
using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class MoverTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private static IReadOnlyList<PlanItem> Scan(string src, string dst) =>
        new Scanner().Scan(src, dst, Today, TimeZoneInfo.Utc, null).Items;

    [Fact]
    public void Sort_moves_files_into_year_month_folders()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "A");
        var before = TempFolder.Hash(a);

        var mover = new Mover();
        var result = mover.Sort(Scan(src, dst));

        var target = Path.Combine(dst, "2016", "03", "IMG-20160315-a.jpg");
        Assert.Equal(1, result.Moved);
        Assert.Empty(result.NotMoved);
        Assert.False(File.Exists(a));
        Assert.Equal(before, TempFolder.Hash(target));
    }

    [Fact]
    public void Never_overwrites_a_target_that_appears_after_Scan()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "mine");
        var plan = Scan(src, dst);
        var target = t.File(@"dst\2016\03\IMG-20160315-a.jpg", "theirs");

        var result = new Mover().Sort(plan);

        Assert.Equal(0, result.Moved);
        var failed = Assert.Single(result.NotMoved);
        Assert.Equal("Destination changed, scan again", failed.Reason);
        Assert.Equal("mine", File.ReadAllText(a));
        Assert.Equal("theirs", File.ReadAllText(target));
    }

    [Fact]
    public void Read_only_file_is_moved_and_stays_read_only()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg");
        File.SetAttributes(a, FileAttributes.ReadOnly);

        var result = new Mover().Sort(Scan(src, dst));

        var target = Path.Combine(dst, "2016", "03", "IMG-20160315-a.jpg");
        Assert.Equal(1, result.Moved);
        Assert.True(File.GetAttributes(target).HasFlag(FileAttributes.ReadOnly));
    }

    [Fact]
    public void File_held_open_is_not_moved_and_the_next_file_still_moves()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var busy = t.File(@"src\IMG-20160315-a.jpg", "busy");
        t.File(@"src\IMG-20160316-b.jpg", "free");
        var plan = Scan(src, dst);

        MoveResult result;
        using (new FileStream(busy, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            result = new Mover().Sort(plan);

        Assert.Equal(1, result.Moved);
        var failed = Assert.Single(result.NotMoved);
        Assert.Equal(busy, failed.Path);
        Assert.Equal("Still changing, scan again", failed.Reason);
        Assert.True(File.Exists(busy));
        Assert.True(File.Exists(Path.Combine(dst, "2016", "03", "IMG-20160316-b.jpg")));
    }

    [Fact]
    public void File_whose_size_changed_since_Scan_is_not_moved()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "short");
        var plan = Scan(src, dst);
        File.WriteAllText(a, "now much longer");

        var result = new Mover().Sort(plan);

        Assert.Equal("Still changing, scan again", Assert.Single(result.NotMoved).Reason);
        Assert.True(File.Exists(a));
    }

    [Fact]
    public void Undo_puts_every_file_back_and_removes_the_folders_Sort_created()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "A");
        var b = t.File(@"src\sub\IMG-20210514-b.mp4", "B");
        File.SetAttributes(b, FileAttributes.ReadOnly);
        var hashA = TempFolder.Hash(a);
        var hashB = TempFolder.Hash(b);
        var timeA = File.GetLastWriteTimeUtc(a);

        var mover = new Mover();
        mover.Sort(Scan(src, dst));
        Assert.True(mover.CanUndo);
        var undo = mover.Undo();

        Assert.Equal(2, undo.Restored);
        Assert.Empty(undo.NotRestored);
        Assert.Equal(hashA, TempFolder.Hash(a));
        Assert.Equal(hashB, TempFolder.Hash(b));
        Assert.Equal(timeA, File.GetLastWriteTimeUtc(a));
        Assert.True(File.GetAttributes(b).HasFlag(FileAttributes.ReadOnly));
        Assert.Empty(Directory.GetFileSystemEntries(dst));
        Assert.False(mover.CanUndo);
    }

    [Fact]
    public void Undo_keeps_a_created_folder_that_now_holds_another_file()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "A");
        var mover = new Mover();
        mover.Sort(Scan(src, dst));
        var other = t.File(@"dst\2016\03\other.jpg", "other");

        mover.Undo();

        Assert.True(File.Exists(a));
        Assert.Equal("other", File.ReadAllText(other));
    }

    [Fact]
    public void Undo_does_not_overwrite_a_file_now_at_the_original_path()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var a = t.File(@"src\IMG-20160315-a.jpg", "A");
        var mover = new Mover();
        mover.Sort(Scan(src, dst));
        File.WriteAllText(a, "new file");

        var undo = mover.Undo();

        Assert.Equal("Original place is taken", Assert.Single(undo.NotRestored).Reason);
        Assert.Equal("new file", File.ReadAllText(a));
        Assert.Equal("A", File.ReadAllText(Path.Combine(dst, "2016", "03", "IMG-20160315-a.jpg")));
    }
}
