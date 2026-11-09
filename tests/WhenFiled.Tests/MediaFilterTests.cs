using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class MediaFilterTests
{
    [Theory]
    [InlineData("a.jpg")]
    [InlineData("a.JPEG")]
    [InlineData("a.Heic")]
    [InlineData("a.png")]
    [InlineData("a.WEBP")]
    [InlineData("a.mp4")]
    [InlineData("a.MOV")]
    [InlineData("a.3gp")]
    public void Accepts_media_extensions_in_any_case(string name) =>
        Assert.True(MediaFilter.IsMedia(name, 100));

    [Theory]
    [InlineData("notes.txt")]
    [InlineData("Thumbs.db")]
    [InlineData("x.pdf")]
    [InlineData("photo.jpg.txt")]
    public void Rejects_other_files(string name) =>
        Assert.False(MediaFilter.IsMedia(name, 100));

    [Fact]
    public void Rejects_zero_byte_file() =>
        Assert.False(MediaFilter.IsMedia("empty.jpg", 0));
}
