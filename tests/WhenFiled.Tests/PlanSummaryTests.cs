using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class PlanSummaryTests
{
    private static PlanItem Item(PlanStatus status) =>
        new(new ScannedFile(@"C:\in\a.jpg", 1, new DateResult(null, DateSource.None)),
            status, null, null, "", null, null);

    private static List<PlanItem> Items(int move, int noDate, int other) =>
        [.. Enumerable.Repeat(Item(PlanStatus.WillMove), move),
         .. Enumerable.Repeat(Item(PlanStatus.NoDate), noDate),
         .. Enumerable.Repeat(Item(PlanStatus.WontMove), other)];

    [Fact]
    public void Counts_and_texts_match_the_mock()
    {
        var s = PlanSummary.From(Items(1071, 24, 7));
        Assert.Equal("Sort 1,071 files", s.SortButton);
        Assert.True(s.CanSort);
        Assert.Equal("Scan finished. Nothing has been moved yet.", s.BannerTitle);
        Assert.Equal(
            "1,102 files found: 1,071 will move · 24 have no date and will stay · 7 won’t move for other reasons",
            s.BannerDetail);
    }

    [Fact]
    public void One_file_is_singular()
    {
        Assert.Equal("Sort 1 file", PlanSummary.From(Items(1, 0, 0)).SortButton);
    }

    [Fact]
    public void One_no_date_file_is_singular_in_the_banner()
    {
        Assert.Equal(
            "1 file found: 0 will move · 1 has no date and will stay · 0 won’t move for other reasons",
            PlanSummary.From(Items(0, 1, 0)).BannerDetail);
    }

    [Fact]
    public void Nothing_to_move_turns_Sort_off()
    {
        var s = PlanSummary.From(Items(0, 24, 7));
        Assert.False(s.CanSort);
        Assert.Equal("Sort 0 files", s.SortButton);
    }
}
