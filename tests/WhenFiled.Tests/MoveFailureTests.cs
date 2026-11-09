using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class MoveFailureTests
{
    private static IOException Io(uint win32) => new("x", unchecked((int)(0x80070000 | win32)));

    [Fact]
    public void Access_denied() =>
        Assert.Equal("Access denied", MoveFailure.ReasonFor(new UnauthorizedAccessException()));

    [Theory]
    [InlineData(32u)] // sharing violation
    [InlineData(33u)] // lock violation
    public void Sharing_or_lock_violation(uint code) =>
        Assert.Equal("Still changing, scan again", MoveFailure.ReasonFor(Io(code)));

    [Theory]
    [InlineData(80u)]  // file exists
    [InlineData(183u)] // already exists
    public void File_already_exists(uint code) =>
        Assert.Equal("Destination changed, scan again", MoveFailure.ReasonFor(Io(code)));

    [Fact]
    public void Anything_else_gives_the_message() =>
        Assert.Equal("Couldn't move: disk on fire",
            MoveFailure.ReasonFor(new InvalidOperationException("disk on fire")));
}
