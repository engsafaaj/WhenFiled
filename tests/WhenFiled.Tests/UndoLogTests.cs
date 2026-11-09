using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class UndoLogTests
{
    [Fact]
    public void Moves_come_back_in_reverse_order()
    {
        var log = new UndoLog();
        log.RecordMove(@"C:\in\a.jpg", @"D:\s\2019\07\a.jpg");
        log.RecordMove(@"C:\in\b.jpg", @"D:\s\2019\07\b.jpg");
        Assert.Equal(
            [(@"C:\in\b.jpg", @"D:\s\2019\07\b.jpg"), (@"C:\in\a.jpg", @"D:\s\2019\07\a.jpg")],
            log.MovesNewestFirst);
        Assert.True(log.HasMoves);
    }

    [Fact]
    public void Folders_come_back_deepest_first()
    {
        var log = new UndoLog();
        log.RecordFolder(@"D:\s\2019");
        log.RecordFolder(@"D:\s\2019\07");
        log.RecordFolder(@"D:\s\2020");
        log.RecordFolder(@"D:\s\2020\05");
        Assert.Equal([@"D:\s\2019\07", @"D:\s\2020\05", @"D:\s\2019", @"D:\s\2020"],
            log.FoldersDeepestFirst);
    }

    [Fact]
    public void Clear_empties_the_log()
    {
        var log = new UndoLog();
        log.RecordMove("a", "b");
        log.RecordFolder("c");
        log.Clear();
        Assert.False(log.HasMoves);
        Assert.Empty(log.FoldersDeepestFirst);
    }
}
