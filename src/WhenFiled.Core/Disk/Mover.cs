using WhenFiled.Core.Rules;

namespace WhenFiled.Core.Disk;

public record NotMoved(string Path, string Reason);
public record MoveResult(int Moved, IReadOnlyList<NotMoved> NotMoved);
public record UndoResult(int Restored, IReadOnlyList<NotMoved> NotRestored);

/// <summary>
/// Moves files and undoes it (D3, D16-D19). Never deletes, copies or overwrites a file:
/// moves use the 2-argument File.Move, which throws if the target exists.
/// </summary>
public sealed class Mover
{
    private UndoLog _log = new();

    public bool CanUndo => _log.HasMoves;

    /// <summary>Moves every WillMove item. A problem with one file skips only that file.</summary>
    public MoveResult Sort(IEnumerable<PlanItem> items)
    {
        _log = new UndoLog(); // each Sort starts a fresh log (D19)
        var notMoved = new List<NotMoved>();
        var moved = 0;

        foreach (var item in items.Where(i => i.Status == PlanStatus.WillMove))
        {
            var reason = MoveOne(item);
            if (reason is null) moved++;
            else notMoved.Add(new NotMoved(item.File.Path, reason));
        }
        return new MoveResult(moved, notMoved);
    }

    /// <summary>Moves files back newest first, then removes the empty folders Sort created (D19).</summary>
    public UndoResult Undo()
    {
        var notRestored = new List<NotMoved>();
        var restored = 0;

        foreach (var (from, to) in _log.MovesNewestFirst)
        {
            var reason = RestoreOne(from, to);
            if (reason is null) restored++;
            else notRestored.Add(new NotMoved(from, reason));
        }

        foreach (var folder in _log.FoldersDeepestFirst)
        {
            try { Directory.Delete(folder); } // 1 argument: only an empty folder goes
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }

        _log.Clear();
        return new UndoResult(restored, notRestored);
    }

    private static string? RestoreOne(string from, string to)
    {
        try
        {
            if (File.Exists(from) || Directory.Exists(from))
                return "Original place is taken";
            if (!File.Exists(to))
                return "No longer in destination";

            Directory.CreateDirectory(Path.GetDirectoryName(from)!);
            File.Move(to, from);
            return null;
        }
        catch (Exception e)
        {
            return MoveFailure.ReasonFor(e);
        }
    }

    /// <summary>Returns null when the file moved, otherwise the reason it didn't.</summary>
    private string? MoveOne(PlanItem item)
    {
        var source = item.File.Path;
        var folder = item.TargetFolder!;
        var target = Path.Combine(folder, item.TargetName!);
        try
        {
            // D17: re-check just before moving.
            if (new FileInfo(source).Length != item.File.Size)
                return "Still changing, scan again";
            using (new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None)) { }
            if (File.Exists(target) || Directory.Exists(target))
                return "Destination changed, scan again";

            CreateFolders(folder);
            File.Move(source, target); // throws if the target exists
            _log.RecordMove(source, target);
            return null;
        }
        catch (Exception e)
        {
            return MoveFailure.ReasonFor(e);
        }
    }

    /// <summary>Creates the Year and Month folders, logging each one that is new.</summary>
    private void CreateFolders(string folder)
    {
        var missing = new Stack<string>();
        for (var p = folder; p is not null && !Directory.Exists(p); p = Path.GetDirectoryName(p))
            missing.Push(p);
        foreach (var p in missing)
        {
            Directory.CreateDirectory(p);
            _log.RecordFolder(p);
        }
    }
}
