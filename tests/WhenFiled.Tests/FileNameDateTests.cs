using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class FileNameDateTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    [Theory]
    [InlineData("IMG-20160315-WA0020.jpg", 2016, 3, 15)]
    [InlineData("Screenshot_20230102-101010.png", 2023, 1, 2)]
    [InlineData("VID_20210514.mp4", 2021, 5, 14)]
    [InlineData("PXL_20230102_101010123.jpg", 2023, 1, 2)]
    [InlineData("2019-07-04 10.11.12.jpg", 2019, 7, 4)]
    public void Finds_the_date(string name, int y, int m, int d) =>
        Assert.Equal(new DateOnly(y, m, d), FileNameDate.Find(name, Today));

    [Theory]
    [InlineData("holiday.jpg")]
    [InlineData("IMG_4821.jpg")]
    [InlineData("20161332.jpg")]
    [InlineData("20160230.jpg")]
    [InlineData("1577836800000.jpg")]
    public void Finds_no_date(string name) =>
        Assert.Null(FileNameDate.Find(name, Today));
}
