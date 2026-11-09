namespace WhenFiled.Core.Rules;

/// <summary>One level of undo, held in memory (D19). Pure: it only remembers paths.</summary>
public sealed class UndoLog
{
    private readonly List<(string From, string To)> _moves = [];
    private readonly List<string> _folders = [];

    public bool HasMoves => _moves.Count > 0;

    public void RecordMove(string from, string to) => _moves.Add((from, to));

    public void RecordFolder(string path) => _folders.Add(path);

    public IReadOnlyList<(string From, string To)> MovesNewestFirst =>
        Enumerable.Reverse(_moves).ToList();

    /// <summary>Deepest first, so a child goes before its parent. Equal depths keep their order.</summary>
    public IReadOnlyList<string> FoldersDeepestFirst =>
        _folders.OrderByDescending(p => p.Count(c => c is '\\' or '/')).ToList();

    public void Clear()
    {
        _moves.Clear();
        _folders.Clear();
    }
}
