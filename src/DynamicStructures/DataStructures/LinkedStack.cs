using System.Diagnostics.CodeAnalysis;

namespace DynamicStructures.DataStructures;

/// <summary>
/// Стек на собственном односвязном списке. Верх стека — голова списка.
/// </summary>
public sealed class LinkedStack<T>
{
    private readonly SinglyLinkedList<T> _items = new();

    public int Count => _items.Count;

    public bool IsEmpty => _items.IsEmpty;

    public void Push(T value) => _items.AddFirst(value);

    public bool TryPop([MaybeNullWhen(false)] out T value) => _items.RemoveFirst(out value);

    public T Pop()
    {
        if (!TryPop(out var value))
        {
            throw new InvalidOperationException("Стек пуст: операция Pop невозможна.");
        }

        return value;
    }

    public bool TryTop([MaybeNullWhen(false)] out T value)
    {
        if (_items.Head is null)
        {
            value = default;
            return false;
        }

        value = _items.Head.Value;
        return true;
    }

    public bool TryPeek([MaybeNullWhen(false)] out T value) => TryTop(out value);

    public T Top()
    {
        if (!TryTop(out var value))
        {
            throw new InvalidOperationException("Стек пуст: операция Top невозможна.");
        }

        return value;
    }

    /// <summary>Снимок содержимого от вершины к основанию.</summary>
    public T[] ToArrayTopFirst() => _items.ToArray();

    public void WriteTo(TextWriter writer, string separator = " ") => _items.WriteTo(writer, separator);

    public void Print(TextWriter writer, string separator = " ") => _items.WriteTo(writer, separator);

    public void Clear() => _items.Clear();
}
