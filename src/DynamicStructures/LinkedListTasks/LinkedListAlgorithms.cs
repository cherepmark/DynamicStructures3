using DynamicStructures.DataStructures;

namespace DynamicStructures.LinkedListTasks;

/// <summary>
/// 12 заданий обработки собственного односвязного списка. Готовые List&lt;T&gt;
/// и LinkedList&lt;T&gt; не используются; порядок меняется ссылками узлов.
/// </summary>
public static class LinkedListAlgorithms
{
    /// <summary>1. Разворот списка изменением ссылок Next.</summary>
    public static void Reverse<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        var oldHead = list.Head;
        SinglyLinkedList<T>.Node? previous = null;
        var current = list.Head;

        while (current is not null)
        {
            var next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }

        list.Head = previous;
        list.Tail = oldHead;
    }

    /// <summary>2а. Перенести последний элемент в начало.</summary>
    public static void MoveLastToFront<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Count < 2)
        {
            return;
        }

        var penultimate = list.Head!;
        while (!ReferenceEquals(penultimate.Next, list.Tail))
        {
            penultimate = penultimate.Next!;
        }

        var last = list.Tail!;
        penultimate.Next = null;
        last.Next = list.Head;
        list.Head = last;
        list.Tail = penultimate;
    }

    /// <summary>2б. Перенести первый элемент в конец.</summary>
    public static void MoveFirstToLast<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Count < 2)
        {
            return;
        }

        var first = list.Head!;
        list.Head = first.Next;
        first.Next = null;
        list.Tail!.Next = first;
        list.Tail = first;
    }

    /// <summary>3. Количество различных элементов (без HashSet, O(n^2)).</summary>
    public static int CountDistinct<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        var comparer = EqualityComparer<T>.Default;
        var distinct = 0;

        for (var current = list.Head; current is not null; current = current.Next)
        {
            var seenBefore = false;
            for (var previous = list.Head; !ReferenceEquals(previous, current); previous = previous!.Next)
            {
                if (comparer.Equals(previous!.Value, current.Value))
                {
                    seenBefore = true;
                    break;
                }
            }

            if (!seenBefore)
            {
                distinct++;
            }
        }

        return distinct;
    }

    /// <summary>4. Удалить все вхождения значений, встречающихся более одного раза.</summary>
    public static int RemoveNonUnique<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Count == 0)
        {
            return 0;
        }

        var nodes = new SinglyLinkedList<T>.Node[list.Count];
        var remove = new bool[list.Count];
        var index = 0;
        for (var current = list.Head; current is not null; current = current.Next)
        {
            nodes[index++] = current;
        }

        var removed = 0;
        for (var i = 0; i < nodes.Length; i++)
        {
            if (CountOccurrences(list, nodes[i].Value) > 1)
            {
                remove[i] = true;
                removed++;
            }
        }

        SinglyLinkedList<T>.Node? previous = null;
        var node = list.Head;
        index = 0;
        while (node is not null)
        {
            var next = node.Next;
            if (remove[index])
            {
                list.Unlink(previous);
            }
            else
            {
                previous = node;
            }

            node = next;
            index++;
        }

        return removed;
    }

    /// <summary>5. После первого вхождения marker вставить независимую копию всего списка.</summary>
    public static bool InsertCopyAfterFirst<T>(SinglyLinkedList<T> list, T marker)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Head is null)
        {
            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        SinglyLinkedList<T>.Node? target = null;
        for (var current = list.Head; current is not null; current = current.Next)
        {
            if (comparer.Equals(current.Value, marker))
            {
                target = current;
                break;
            }
        }

        if (target is null)
        {
            return false;
        }

        var (copyHead, copyTail, copyCount) = CopyChain(list.Head);
        copyTail!.Next = target.Next;
        target.Next = copyHead;
        if (ReferenceEquals(list.Tail, target))
        {
            list.Tail = copyTail;
        }

        list.Count += copyCount;
        return true;
    }

    /// <summary>6. Вставка в непустой список, отсортированный по неубыванию.</summary>
    public static void InsertIntoSorted(SinglyLinkedList<int> list, int value)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (list.Head is null)
        {
            list.AddLast(value);
            return;
        }

        SinglyLinkedList<int>.Node? previous = null;
        var current = list.Head;
        while (current is not null && current.Value <= value)
        {
            previous = current;
            current = current.Next;
        }

        if (previous is null)
        {
            list.AddFirst(value);
            return;
        }

        if (current is null)
        {
            list.AddLast(value);
            return;
        }

        var node = new SinglyLinkedList<int>.Node(value) { Next = current };
        previous.Next = node;
        list.Count++;
    }

    /// <summary>7. Удалить все вхождения значения.</summary>
    public static int RemoveAll<T>(SinglyLinkedList<T> list, T value)
    {
        ArgumentNullException.ThrowIfNull(list);
        var comparer = EqualityComparer<T>.Default;
        var removed = 0;
        SinglyLinkedList<T>.Node? previous = null;
        var current = list.Head;

        while (current is not null)
        {
            var next = current.Next;
            if (comparer.Equals(current.Value, value))
            {
                list.Unlink(previous);
                removed++;
            }
            else
            {
                previous = current;
            }

            current = next;
        }

        return removed;
    }

    /// <summary>8. Вставить newValue перед первым вхождением target.</summary>
    public static bool InsertBeforeFirst<T>(SinglyLinkedList<T> list, T target, T newValue)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Head is null)
        {
            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        SinglyLinkedList<T>.Node? previous = null;
        var current = list.Head;

        while (current is not null && !comparer.Equals(current.Value, target))
        {
            previous = current;
            current = current.Next;
        }

        if (current is null)
        {
            return false;
        }

        if (previous is null)
        {
            list.AddFirst(newValue);
            return true;
        }

        var node = new SinglyLinkedList<T>.Node(newValue) { Next = current };
        previous.Next = node;
        list.Count++;
        return true;
    }

    /// <summary>
    /// 9. Дописать к left независимые копии элементов right. right не изменяется,
    /// поэтому корректно работает и при left == right.
    /// </summary>
    public static void Concatenate<T>(SinglyLinkedList<T> left, SinglyLinkedList<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (right.Head is null)
        {
            return;
        }

        var (copyHead, copyTail, copyCount) = CopyChain(right.Head);
        if (left.Tail is null)
        {
            left.Head = copyHead;
            left.Tail = copyTail;
        }
        else
        {
            left.Tail.Next = copyHead;
            left.Tail = copyTail;
        }

        left.Count += copyCount;
    }

    /// <summary>
    /// 10. Разделить список по первому вхождению value.
    /// Первый список — элементы до вхождения, второй — начиная с вхождения.
    /// Если value нет, второй список пуст, первый (тот же source) не изменяется.
    /// </summary>
    public static (SinglyLinkedList<int> First, SinglyLinkedList<int> Second) SplitAtFirst(
        SinglyLinkedList<int> source,
        int value)
    {
        ArgumentNullException.ThrowIfNull(source);

        SinglyLinkedList<int>.Node? previous = null;
        var target = source.Head;
        var index = 0;

        while (target is not null && target.Value != value)
        {
            previous = target;
            target = target.Next;
            index++;
        }

        if (target is null)
        {
            return (source, new SinglyLinkedList<int>());
        }

        var oldTail = source.Tail;
        var secondCount = source.Count - index;

        if (previous is null)
        {
            source.DetachContents();
        }
        else
        {
            previous.Next = null;
            source.Tail = previous;
            source.Count = index;
        }

        var second = new SinglyLinkedList<int>();
        second.Adopt(target, oldTail, secondCount);
        return (source, second);
    }

    /// <summary>11. Дописать независимую копию списка в конец.</summary>
    public static void Duplicate<T>(SinglyLinkedList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Head is null)
        {
            return;
        }

        var (copyHead, copyTail, copyCount) = CopyChain(list.Head);
        list.Tail!.Next = copyHead;
        list.Tail = copyTail;
        list.Count += copyCount;
    }

    /// <summary>12. Поменять местами элементы с индексами firstIndex и secondIndex (0-based).</summary>
    public static void SwapAt<T>(SinglyLinkedList<T> list, int firstIndex, int secondIndex)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (firstIndex < 0 || firstIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(firstIndex));
        }

        if (secondIndex < 0 || secondIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(secondIndex));
        }

        if (firstIndex == secondIndex)
        {
            return;
        }

        if (firstIndex > secondIndex)
        {
            (firstIndex, secondIndex) = (secondIndex, firstIndex);
        }

        var previousFirst = firstIndex == 0 ? null : list.NodeAt(firstIndex - 1);
        var first = previousFirst?.Next ?? list.Head!;
        var previousSecond = secondIndex == firstIndex + 1 ? first : list.NodeAt(secondIndex - 1);
        var second = previousSecond.Next!;

        var afterFirst = first.Next;
        var afterSecond = second.Next;

        if (previousFirst is null)
        {
            list.Head = second;
        }
        else
        {
            previousFirst.Next = second;
        }

        if (secondIndex == firstIndex + 1)
        {
            second.Next = first;
            first.Next = afterSecond;
        }
        else
        {
            previousSecond.Next = first;
            second.Next = afterFirst;
            first.Next = afterSecond;
        }

        if (afterSecond is null)
        {
            list.Tail = first;
        }
    }

    private static int CountOccurrences<T>(SinglyLinkedList<T> list, T value)
    {
        var comparer = EqualityComparer<T>.Default;
        var count = 0;
        for (var current = list.Head; current is not null; current = current.Next)
        {
            if (comparer.Equals(current.Value, value))
            {
                count++;
            }
        }

        return count;
    }

    private static (SinglyLinkedList<T>.Node? Head, SinglyLinkedList<T>.Node? Tail, int Count) CopyChain<T>(
        SinglyLinkedList<T>.Node? sourceHead)
    {
        SinglyLinkedList<T>.Node? head = null;
        SinglyLinkedList<T>.Node? tail = null;
        var count = 0;

        for (var current = sourceHead; current is not null; current = current.Next)
        {
            var copy = new SinglyLinkedList<T>.Node(current.Value);
            if (head is null)
            {
                head = copy;
                tail = copy;
            }
            else
            {
                tail!.Next = copy;
                tail = copy;
            }

            count++;
        }

        return (head, tail, count);
    }
}
