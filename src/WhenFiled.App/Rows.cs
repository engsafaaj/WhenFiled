using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WhenFiled.Core.Rules;

namespace WhenFiled.App;

/// <summary>A group heading in the preview. Brushes are shared, so equal groups compare equal.</summary>
public record GroupInfo(string Title, string? Note, Brush Brush, Brush Fore)
{
    public Visibility NoteVisibility => Note is null ? Visibility.Collapsed : Visibility.Visible;
}

public record PreviewRow(string Name, string DateText, string FromText, string GoesText, GroupInfo Group);

public record FailedRow(string Name, string Reason);

public static class RowBuilder
{
    private static Brush Frozen(string hex)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        return brush;
    }

    private static readonly Brush Grey = Frozen("#F0F0F0");
    private static readonly Brush Text = Frozen("#1B1B1B");
    private static readonly Brush WarnBg = Frozen("#FFF4CE");
    private static readonly Brush WarnFore = Frozen("#9D5D00");

    private const string NoDateNote =
        "They stay where they are. To sort them, rename them with a date (like 20190704) " +
        "and scan again, or sort them by hand.";

    /// <summary>From labels as the user sees them (D22).</summary>
    public static string FromLabel(DateSource from) => from switch
    {
        DateSource.Exif => "Photo info",
        DateSource.Video => "Video",
        DateSource.FileName => "File name",
        _ => "None",
    };

    public static List<PreviewRow> Preview(IReadOnlyList<PlanItem> items, PlanSummary summary)
    {
        var move = new GroupInfo($"Will move ({PlanSummary.Format(summary.ToMove)})", null, Grey, Text);
        var none = new GroupInfo(
            $"⚠ No date – these {PlanSummary.Format(summary.NoDate)} files will stay where they are",
            NoDateNote, WarnBg, WarnFore);
        var other = new GroupInfo("Won’t move (other reasons)", null, Grey, Text);

        return items
            .OrderBy(i => i.Status) // WillMove, NoDate, WontMove. The sort is stable, so path order stays.
            .Select(i => new PreviewRow(
                System.IO.Path.GetFileName(i.File.Path),
                i.File.Date.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "—",
                FromLabel(i.File.Date.From),
                i.Note is null ? i.GoesTo : i.GoesTo + " " + i.Note,
                i.Status switch
                {
                    PlanStatus.WillMove => move,
                    PlanStatus.NoDate => none,
                    _ => other,
                }))
            .ToList();
    }
}
