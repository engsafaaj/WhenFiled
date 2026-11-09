using System.Text.RegularExpressions;

namespace WhenFiled.Core.Rules;

public static partial class FileNameDate
{
    // YYYYMMDD, or YYYY-MM-DD / YYYY_MM_DD with the same separator twice.
    // No digit may touch either end of the match (D8).
    // ASCII digits only: \d also matches Arabic-Indic and full-width digits, which int.Parse rejects.
    [GeneratedRegex(@"(?<![0-9])(?:(?<y>[0-9]{4})(?<m>[0-9]{2})(?<d>[0-9]{2})|(?<y>[0-9]{4})(?<s>[-_])(?<m>[0-9]{2})\k<s>(?<d>[0-9]{2}))(?![0-9])")]
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
