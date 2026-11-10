using DynamicStructures.DataStructures;

namespace DynamicStructures.Applications;

/// <summary>
/// История редактора на собственном стеке: каждое изменение кладёт предыдущее
/// состояние, Undo снимает последнее состояние (LIFO).
/// </summary>
public sealed class EditorHistory
{
    private readonly LinkedStack<string> _undoStates = new();

    public string Current { get; private set; } = string.Empty;

    public int UndoDepth => _undoStates.Count;

    public void Apply(string newText)
    {
        ArgumentNullException.ThrowIfNull(newText);

        _undoStates.Push(Current);
        Current = newText;
    }

    public bool Undo()
    {
        if (!_undoStates.TryPop(out var previous))
        {
            return false;
        }

        Current = previous;
        return true;
    }

    public string[] SnapshotsTopFirst() => _undoStates.ToArrayTopFirst();

    public void Clear()
    {
        _undoStates.Clear();
        Current = string.Empty;
    }
}
