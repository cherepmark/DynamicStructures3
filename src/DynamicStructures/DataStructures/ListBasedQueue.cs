using System.Diagnostics.CodeAnalysis;

namespace DynamicStructures.DataStructures;

/// <summary>
/// Очередь на основе List&lt;T&gt;. Удаление из начала выполняется через RemoveAt(0),
/// поэтому операция Dequeue имеет сложность O(n) и используется для сравнения.
/// </summary>
public sealed class ListBasedQueue<T> : IQueue<T>
{
    private readonly List<T> _items = new();

    public string ImplementationName => "ListBasedQueue";

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public void Enqueue(T item) => _items.Add(item);

    public bool TryDequeue([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items[0];
        _items.RemoveAt(0);
        return true;
    }

    public bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items[0];
        return true;
    }

    public T[] Snapshot() => _items.ToArray();

    public void WriteTo(TextWriter writer, string separator = " ")
    {
        ArgumentNullException.ThrowIfNull(writer);
        for (var i = 0; i < _items.Count; i++)
        {
            if (i > 0)
            {
                writer.Write(separator);
            }

            writer.Write(_items[i]);
        }
    }
}
