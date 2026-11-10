using System.Diagnostics.CodeAnalysis;

namespace DynamicStructures.DataStructures;

/// <summary>Тонкая оболочка над стандартным System.Collections.Generic.Queue&lt;T&gt;.</summary>
public sealed class StandardQueue<T> : IQueue<T>
{
    private readonly Queue<T> _items = new();

    public string ImplementationName => "StandardQueue";

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public void Enqueue(T item) => _items.Enqueue(item);

    public bool TryDequeue([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items.Dequeue();
        return true;
    }

    public bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items.Peek();
        return true;
    }

    public T[] Snapshot() => _items.ToArray();

    public void WriteTo(TextWriter writer, string separator = " ")
    {
        ArgumentNullException.ThrowIfNull(writer);
        var first = true;
        foreach (var item in _items)
        {
            if (!first)
            {
                writer.Write(separator);
            }

            writer.Write(item);
            first = false;
        }
    }
}
