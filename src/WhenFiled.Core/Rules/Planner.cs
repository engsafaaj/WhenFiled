namespace WhenFiled.Core.Rules;

/// <summary>Decides each file's target folder and name (D13, D14).</summary>
public static class Planner
{
    public const string AlreadyThere = "Already in destination";
    public const string NameTaken = "(name already taken)";

    public static IReadOnlyList<PlanItem> Plan(
        IEnumerable<ScannedFile> files, string destination, IDestinationView view)
    {
        var taken = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var items = new List<PlanItem>();

        foreach (var file in files.OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase))
            items.Add(PlanOne(file, destination, view, taken));

        return items;
    }

    private static PlanItem PlanOne(
        ScannedFile file, string destination, IDestinationView view,
        Dictionary<string, HashSet<string>> taken)
    {
        if (file.Problem is { } problem)
            return WontMove(file, problem);

        if (file.Date.Date is not { } date)
        {
            var folder = Path.GetDirectoryName(file.Path) ?? "";
            return new PlanItem(file, PlanStatus.NoDate, null, null, "Stays in " + folder, null, null);
        }

        return PlanDated(file, date, destination, view, taken);
    }

    private static PlanItem PlanDated(
        ScannedFile file, DateOnly date, string destination, IDestinationView view,
        Dictionary<string, HashSet<string>> taken)
    {
        var folder = TargetPath.Folder(destination, date);
        var name = Path.GetFileName(file.Path);

        if (!taken.TryGetValue(folder, out var planned))
            taken[folder] = planned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existing = new HashSet<string>(view.FileNames(folder), StringComparer.OrdinalIgnoreCase);

        var finalName = name;
        if (existing.TryGetValue(name, out var existingName)
            && !planned.Contains(name)
            && view.IsSameContent(file.Path, folder + "\\" + existingName))
            return WontMove(file, AlreadyThere);

        for (var n = 2; existing.Contains(finalName) || planned.Contains(finalName); n++)
            finalName = Numbered(name, n);

        if (TargetPath.IsTooLong(folder + "\\" + finalName))
            return WontMove(file, TargetPath.TooLongReason);

        planned.Add(finalName);
        var renamed = finalName != name;
        var goesTo = TargetPath.Shown(date) + (renamed ? finalName : "");
        return new PlanItem(file, PlanStatus.WillMove, folder, finalName, goesTo,
            renamed ? NameTaken : null, null);
    }

    private static string Numbered(string name, int n) =>
        $"{Path.GetFileNameWithoutExtension(name)} ({n}){Path.GetExtension(name)}";

    private static PlanItem WontMove(ScannedFile file, string reason) =>
        new(file, PlanStatus.WontMove, null, null, reason, null, reason);
}
