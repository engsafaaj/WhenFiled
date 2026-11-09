using System.Globalization;

namespace WhenFiled.Core.Rules;

/// <summary>Counts and texts for the preview, in the mock's wording (D20, D21).</summary>
public record PlanSummary(int ToMove, int NoDate, int Other)
{
    public int Found => ToMove + NoDate + Other;
    public bool CanSort => ToMove > 0;
    public string SortButton => $"Sort {Format(ToMove)} {(ToMove == 1 ? "file" : "files")}";
    public string BannerTitle => "Scan finished. Nothing has been moved yet.";

    public string BannerDetail =>
        $"{Format(Found)} files found: {Format(ToMove)} will move · " +
        $"{Format(NoDate)} have no date and will stay · " +
        $"{Format(Other)} won’t move for other reasons";

    public static PlanSummary From(IEnumerable<PlanItem> items)
    {
        int move = 0, none = 0, other = 0;
        foreach (var item in items)
        {
            if (item.Status == PlanStatus.WillMove) move++;
            else if (item.Status == PlanStatus.NoDate) none++;
            else other++;
        }
        return new PlanSummary(move, none, other);
    }

    public static string Format(int n) => n.ToString("N0", CultureInfo.InvariantCulture);
}
