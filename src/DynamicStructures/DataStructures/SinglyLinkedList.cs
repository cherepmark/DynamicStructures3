using System.Diagnostics.CodeAnalysis;

namespace DynamicStructures.DataStructures;

/// <summary>
/// Собственный односвязный список. Все операции выполняются через ссылки на узлы;
/// стандартные LinkedList&lt;T&gt; и List&lt;T&gt; не используются.
/// </summary>
public sealed class SinglyLinkedList<T>
{
    public sealed class Node
    {
        internal Node(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public Node? Next { get; internal set; }
    }

    public Node? Head { get; internal set; }

    public Node? Tail { get; internal set; }

    public int Count { get; internal set; }

    public bool IsEmpty => Count == 0;

    public void AddFirst(T value)
    {
        var node = new Node(value) { Next = Head };
        Head = node;
        Tail ??= node;
        Count++;
    }

    public void AddLast(T value)
    {
        AddLastNode(value);
    }

    public Node AddLastNode(T value)
    {
        var node = new Node(value);
        if (Tail is null)
        {
            Head = node;
            Tail = node;
        }
        else
        {
            Tail.Next = node;
            Tail = node;
        }

        Count++;
        return node;
    }

    public bool RemoveFirst([MaybeNullWhen(false)] out T value)
    {
        if (Head is null)
        {
            value = default;
            return false;
        }

        var node = Head;
        Head = node.Next;
        if (Head is null)
        {
            Tail = null;
        }

        node.Next = null;
        Count--;
        value = node.Value;
        return true;
    }

    public Node NodeAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Индекс должен быть в диапазоне 0..{Count - 1}.");
        }

        var current = Head;
        for (var i = 0; i < index; i++)
        {
            current = current!.Next;
        }

        return current!;
    }

    /// <summary>Вставляет значение по индексу 0..Count (Count означает дописать в конец).</summary>
    public void InsertAt(int index, T value)
    {
        if (index < 0 || index > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Индекс должен быть в диапазоне 0..{Count}.");
        }

        if (index == 0)
        {
            AddFirst(value);
            return;
        }

        if (index == Count)
        {
            AddLast(value);
            return;
        }

        var previous = NodeAt(index - 1);
        var node = new Node(value) { Next = previous.Next };
        previous.Next = node;
        Count++;
    }

    /// <summary>Удаляет узел по индексу и возвращает его значение.</summary>
    public bool RemoveAt(int index, [MaybeNullWhen(false)] out T value)
    {
        if (index < 0 || index >= Count)
        {
            value = default;
            return false;
        }

        if (index == 0)
        {
            return RemoveFirst(out value);
        }

        var previous = NodeAt(index - 1);
        var node = previous.Next!;
        previous.Next = node.Next;
        if (ReferenceEquals(Tail, node))
        {
            Tail = previous;
        }

        node.Next = null;
        Count--;
        value = node.Value;
        return true;
    }

    /// <summary>Удаляет узел, следующий за previous (previous == null — удаляет голову).</summary>
    internal void Unlink(Node? previous)
    {
        var node = previous is null ? Head : previous.Next;
        if (node is null)
        {
            return;
        }

        if (previous is null)
        {
            Head = node.Next;
        }
        else
        {
            previous.Next = node.Next;
        }

        if (ReferenceEquals(Tail, node))
        {
            Tail = previous;
        }

        node.Next = null;
        Count--;
    }

    /// <summary>Удаляет все узлы, не изменяя сами узлы (вызывающий забирает их ссылки).</summary>
    internal void DetachContents()
    {
        Head = null;
        Tail = null;
        Count = 0;
    }

    /// <summary>Присваивает готовую цепочку узлов (используется разделением списка).</summary>
    internal void Adopt(Node? head, Node? tail, int count)
    {
        if (count == 0)
        {
            if (head is not null || tail is not null)
            {
                throw new ArgumentException("Для пустого списка head и tail должны быть null.");
            }
        }
        else if (head is null || tail is null)
        {
            throw new ArgumentException("Для непустого списка head и tail обязательны.");
        }

        Head = head;
        Tail = tail;
        Count = count;
    }

    public void Clear()
    {
        var current = Head;
        while (current is not null)
        {
            var next = current.Next;
            current.Next = null;
            current = next;
        }

        Head = null;
        Tail = null;
        Count = 0;
    }

    public T[] ToArray()
    {
        var result = new T[Count];
        var index = 0;
        for (var current = Head; current is not null; current = current.Next)
        {
            if (index < result.Length)
            {
                result[index] = current.Value;
            }

            index++;
        }

        return result;
    }

    public bool HasCycle()
    {
        var slow = Head;
        var fast = Head;

        while (fast is not null && fast.Next is not null)
        {
            slow = slow!.Next;
            fast = fast.Next.Next;
            if (ReferenceEquals(slow, fast))
            {
                return true;
            }
        }

        return false;
    }

    public void WriteTo(TextWriter writer, string separator = " ")
    {
        ArgumentNullException.ThrowIfNull(writer);

        var first = true;
        for (var current = Head; current is not null; current = current.Next)
        {
            if (!first)
            {
                writer.Write(separator);
            }

            writer.Write(current.Value);
            first = false;
        }
    }

    public IEnumerable<T> Items()
    {
        for (var current = Head; current is not null; current = current.Next)
        {
            yield return current.Value;
        }
    }

    public IEnumerator<T> GetEnumerator() => Items().GetEnumerator();
}
