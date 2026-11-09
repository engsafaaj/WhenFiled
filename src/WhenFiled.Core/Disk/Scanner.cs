using WhenFiled.Core.Rules;

namespace WhenFiled.Core.Disk;

public record ScanProgress(int FilesChecked, int MediaFound, string CurrentPath);

/// <summary>Refusal is set when Scan didn't start. Otherwise Items is the plan.</summary>
public record ScanResult(string? Refusal, IReadOnlyList<PlanItem> Items);

/// <summary>Reads only: checks folders, lists files, reads dates, builds the plan (D3, D6).</summary>
public sealed partial class Scanner : IDestinationView
{
    private const int ProgressEvery = 50;

    public ScanResult Scan(
        string source, string destination, DateOnly today, TimeZoneInfo zone,
        IProgress<ScanProgress>? progress)
    {
        if (!Directory.Exists(source))
            return Refused("The source folder doesn’t exist.");

        var check = PathRules.Check(source, destination);
        if (PathRules.Refusal(check, source) is { } text)
            return Refused(text);

        if (!Directory.Exists(destination))
            return Refused("The destination folder doesn’t exist.");

        var files = new List<ScannedFile>();
        var checkedCount = 0;
        foreach (var path in ListFiles(source))
        {
            checkedCount++;
            var scanned = ReadFile(path, today, zone);
            if (scanned is not null)
                files.Add(scanned);
            if (checkedCount % ProgressEvery == 0)
                progress?.Report(new ScanProgress(checkedCount, files.Count, path));
        }
        progress?.Report(new ScanProgress(checkedCount, files.Count, ""));

        return new ScanResult(null, Planner.Plan(files, destination, this));
    }

    private static ScanResult Refused(string text) => new(text, []);
}
