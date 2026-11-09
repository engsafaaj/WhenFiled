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

    [Fact]
    public void Refusal_texts_name_the_problem()
    {
        Assert.Null(PathRules.Refusal(FolderCheck.Ok, @"D:\A"));
        Assert.Equal(
            @"The destination can’t be inside the source folder. Choose a destination outside D:\A.",
            PathRules.Refusal(FolderCheck.DestinationInsideSource, @"D:\A"));
        Assert.Contains("same folder", PathRules.Refusal(FolderCheck.DestinationIsSource, @"D:\A"));
        Assert.Contains("same drive", PathRules.Refusal(FolderCheck.DifferentDrives, @"D:\A"));
    }
}
