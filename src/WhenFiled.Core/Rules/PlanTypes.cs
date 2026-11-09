namespace WhenFiled.Core.Rules;

/// <summary>The rules' read-only view of the destination. Scanner implements it (D2).</summary>
public interface IDestinationView
{
    /// <summary>File names in the folder. Empty when the folder doesn't exist.</summary>
    IReadOnlyCollection<string> FileNames(string folder);

    /// <summary>True when both files have the same size and the same SHA-256.</summary>
    bool IsSameContent(string sourcePath, string destinationPath);
}

/// <summary>A source file as Scanner found it. Problem is a read problem from Scan (D11).</summary>
public record ScannedFile(string Path, long Size, DateResult Date, string? Problem = null);

public enum PlanStatus { WillMove, NoDate, WontMove }

/// <summary>One row of the preview. GoesTo is the "Goes to" text; Note is the muted extra.</summary>
public record PlanItem(
    ScannedFile File,
    PlanStatus Status,
    string? TargetFolder,
    string? TargetName,
    string GoesTo,
    string? Note,
    string? Reason);
