namespace WhenFiled.Core.Rules;

public static class ValidDate
{
    public static readonly DateOnly Earliest = new(1990, 1, 1);

    /// <summary>From 1990-01-01 up to and including tomorrow (D10).</summary>
    public static bool IsValid(DateOnly date, DateOnly today) =>
        date >= Earliest && date <= today.AddDays(1);
}
