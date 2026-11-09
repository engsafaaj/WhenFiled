using System.Text.RegularExpressions;

namespace WhenFiled.Core.Rules;

public static partial class FileNameDate
{
    // YYYYMMDD, or YYYY-MM-DD / YYYY_MM_DD with the same separator twice.
    // No digit may touch either end of the match (D8).
    [GeneratedRegex(@"(?<!\d)(?:(?<y>\d{4})(?<m>\d{2})(?<d>\d{2})|(?<y>\d{4})(?<s>[-_])(?<m>\d{2})\k<s>(?<d>\d{2}))(?!\d)")]
    private static partial Regex Pattern();

    /// <summary>The first real, in-range date in the name, or null (D8).</summary>
    public static DateOnly? Find(string fileName, DateOnly today)
    {
        var match = Pattern().Match(fileName);
        while (match.Success)
        {
            var y = int.Parse(match.Groups["y"].Value);
            var m = int.Parse(match.Groups["m"].Value);
            var d = int.Parse(match.Groups["d"].Value);

            if (y >= 1 && m is >= 1 and <= 12 && d >= 1 && d <= DateTime.DaysInMonth(y, m))
            {
                var date = new DateOnly(y, m, d);
                if (ValidDate.IsValid(date, today))
                    return date;
            }
            match = Pattern().Match(fileName, match.Index + 1);
        }
        return null;
    }
}
