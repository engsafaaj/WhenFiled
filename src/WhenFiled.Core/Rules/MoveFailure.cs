namespace WhenFiled.Core.Rules;

/// <summary>Maps a move exception to the reason the window shows (D18).</summary>
public static class MoveFailure
{
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;
    private const int ErrorFileExists = 80;
    private const int ErrorAlreadyExists = 183;

    public static string ReasonFor(Exception e)
    {
        if (e is UnauthorizedAccessException)
            return "Access denied";

        if (e is IOException)
        {
            switch (e.HResult & 0xFFFF)
            {
                case ErrorSharingViolation or ErrorLockViolation:
                    return "Still changing, scan again";
                case ErrorFileExists or ErrorAlreadyExists:
                    return "Destination changed, scan again";
            }
        }

        return "Couldn't move: " + e.Message;
    }
}
