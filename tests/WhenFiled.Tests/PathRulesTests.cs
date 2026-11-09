using WhenFiled.Core.Rules;

namespace WhenFiled.Tests;

public class PathRulesTests
{
    [Theory]
    [InlineData(@"D:\A", @"D:\A\B", FolderCheck.DestinationInsideSource)]
    [InlineData(@"D:\A", @"D:\A", FolderCheck.DestinationIsSource)]
    [InlineData(@"D:\A", @"D:\AB", FolderCheck.Ok)]
    [InlineData(@"D:\A\", @"d:\a\b", FolderCheck.DestinationInsideSource)]
    [InlineData(@"D:\A", @"E:\B", FolderCheck.DifferentDrives)]
    public void Folders_are_checked(string source, string destination, FolderCheck expected) =>
        Assert.Equal(expected, PathRules.Check(source, destination));
}
