using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class ValidDateTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    [Theory]
    [InlineData(1989, 12, 31, false)]
    [InlineData(1990, 1, 1, true)]
    [InlineData(2026, 11, 10, true)]
    [InlineData(2026, 11, 11, false)]
    public void Range_includes_both_ends(int y, int m, int d, bool expected) =>
        Assert.Equal(expected, ValidDate.IsValid(new DateOnly(y, m, d), Today));
}
