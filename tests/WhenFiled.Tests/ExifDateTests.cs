using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class ExifDateTests
{
    [Fact]
    public void Parses_the_date_part() =>
        Assert.Equal(new DateOnly(2019, 7, 4), ExifDate.Parse("2019:07:04 10:11:12"));

    [Fact]
    public void Ignores_a_trailing_nul() =>
        Assert.Equal(new DateOnly(2019, 7, 4), ExifDate.Parse("2019:07:04 10:11:12\0"));

    [Theory]
    [InlineData("0000:00:00 00:00:00")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("2019:02:30 10:00:00")]
    public void Returns_null_when_there_is_no_date(string? raw) =>
        Assert.Null(ExifDate.Parse(raw));
}
