using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class PlannerTests
{
    private const string Dest = @"D:\Sorted";
    private const string July = @"D:\Sorted\2019\07";
    private static readonly DateResult Exif = new(new DateOnly(2019, 7, 4), DateSource.Exif);

    private sealed class FakeView : IDestinationView
    {
        public Dictionary<string, HashSet<string>> Names { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Identical { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void Has(string folder, params string[] names) =>
            Names[folder] = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> FileNames(string folder) =>
            Names.TryGetValue(folder, out var n) ? n : [];

        public bool IsSameContent(string sourcePath, string destinationPath) =>
            Identical.Contains(sourcePath + "|" + destinationPath);
    }

    private static ScannedFile File(string path, DateResult? date = null) =>
        new(path, 100, date ?? Exif);

    private static IReadOnlyList<PlanItem> Plan(FakeView view, params ScannedFile[] files) =>
        Planner.Plan(files, Dest, view);

    [Theory]
    [InlineData("a.jpg")]
    [InlineData("写真.jpg")]
    [InlineData("Café 😀.jpg")]
    public void Free_name_is_kept_exactly(string name)
    {
        var item = Assert.Single(Plan(new FakeView(), File(@"C:\in\" + name)));
        Assert.Equal(PlanStatus.WillMove, item.Status);
        Assert.Equal(name, item.TargetName);
        Assert.Equal(July, item.TargetFolder);
        Assert.Equal(@"2019\07\", item.GoesTo);
    }

    [Fact]
    public void Identical_file_already_there_is_not_moved()
    {
        var view = new FakeView();
        view.Has(July, "a.jpg");
        view.Identical.Add(@"C:\in\a.jpg|" + July + @"\a.jpg");
        var item = Assert.Single(Plan(view, File(@"C:\in\a.jpg")));
        Assert.Equal(PlanStatus.WontMove, item.Status);
        Assert.Equal("Already in destination", item.Reason);
    }

    [Fact]
    public void Different_file_already_there_gets_2()
    {
        var view = new FakeView();
        view.Has(July, "a.jpg");
        var item = Assert.Single(Plan(view, File(@"C:\in\a.jpg")));
        Assert.Equal(PlanStatus.WillMove, item.Status);
        Assert.Equal("a (2).jpg", item.TargetName);
        Assert.Equal("(name already taken)", item.Note);
    }

    [Fact]
    public void If_2_is_taken_too_it_gets_3()
    {
        var view = new FakeView();
        view.Has(July, "a.jpg", "a (2).jpg");
        var item = Assert.Single(Plan(view, File(@"C:\in\a.jpg")));
        Assert.Equal("a (3).jpg", item.TargetName);
    }

    [Fact]
    public void Names_clash_ignoring_case()
    {
        var view = new FakeView();
        view.Has(July, "a.jpg");
        var item = Assert.Single(Plan(view, File(@"C:\in\A.JPG")));
        Assert.Equal("A (2).JPG", item.TargetName);
    }

    [Fact]
    public void Same_name_twice_in_one_plan_is_numbered_in_path_order()
    {
        var first = File(@"C:\in\1\a.jpg");
        var second = File(@"C:\in\2\a.jpg");
        foreach (var order in new[] { new[] { first, second }, new[] { second, first } })
        {
            var items = Plan(new FakeView(), order);
            Assert.Equal("a.jpg", items.Single(i => i.File == first).TargetName);
            Assert.Equal("a (2).jpg", items.Single(i => i.File == second).TargetName);
        }
    }

    [Fact]
    public void No_date_stays_in_its_own_folder()
    {
        var none = new DateResult(null, DateSource.None);
        var item = Assert.Single(Plan(new FakeView(), File(@"C:\in\sub\holiday.jpg", none)));
        Assert.Equal(PlanStatus.NoDate, item.Status);
        Assert.Null(item.TargetFolder);
        Assert.Equal(@"Stays in C:\in\sub", item.GoesTo);
    }

    [Fact]
    public void Too_long_target_is_not_moved()
    {
        var item = Assert.Single(Plan(new FakeView(), File(@"C:\in\" + new string('x', 250) + ".jpg")));
        Assert.Equal(PlanStatus.WontMove, item.Status);
        Assert.Equal("Path too long", item.Reason);
    }

    [Fact]
    public void Read_problem_from_scan_is_not_moved()
    {
        var f = new ScannedFile(@"C:\in\a.jpg", 100, Exif, "Access denied");
        var item = Assert.Single(Plan(new FakeView(), f));
        Assert.Equal(PlanStatus.WontMove, item.Status);
        Assert.Equal("Access denied", item.Reason);
    }
}
