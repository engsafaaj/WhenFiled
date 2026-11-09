using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class TargetPathTests
{
    private static readonly DateOnly Date = new(2019, 7, 4);

    [Fact]
    public void Folder_is_destination_year_month() =>
        Assert.Equal(@"D:\Sorted\2019\07", TargetPath.Folder(@"D:\Sorted", Date));

    [Fact]
    public void Trailing_backslash_on_destination_is_fine() =>
        Assert.Equal(@"D:\Sorted\2019\07", TargetPath.Folder(@"D:\Sorted\", Date));

    [Fact]
    public void Folder_is_shown_as_year_month_with_backslash() =>
        Assert.Equal(@"2019\07\", TargetPath.Shown(Date));

    [Fact]
    public void Path_of_259_characters_is_allowed() =>
        Assert.False(TargetPath.IsTooLong(new string('x', 259)));

    [Fact]
    public void Path_of_260_characters_is_too_long()
    {
        Assert.True(TargetPath.IsTooLong(new string('x', 260)));
        Assert.Equal("Path too long", TargetPath.TooLongReason);
    }
}
