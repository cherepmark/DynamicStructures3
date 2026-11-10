using DynamicStructures.DataStructures;
using DynamicStructures.LinkedListTasks;

namespace DynamicStructures.Demo;

/// <summary>Наглядный прогон всех 12 операций над собственным связным списком.</summary>
public static class LinkedListDemo
{
    public static void RunAll(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteLine("=== Все 12 операций со связным списком ===");

        var reverse = Make(1, 2, 3, 4);
        LinkedListAlgorithms.Reverse(reverse);
        writer.WriteLine($"1. Разворот: [1 2 3 4] -> {Text(reverse)}");

        var moveLast = Make(1, 2, 3, 4);
        LinkedListAlgorithms.MoveLastToFront(moveLast);
        writer.WriteLine($"2a. Последний в начало: [1 2 3 4] -> {Text(moveLast)}");

        var moveFirst = Make(1, 2, 3, 4);
        LinkedListAlgorithms.MoveFirstToLast(moveFirst);
        writer.WriteLine($"2b. Первый в конец: [1 2 3 4] -> {Text(moveFirst)}");

        var distinct = Make(1, 2, 1, 3, 3, 4, -1, -1);
        writer.WriteLine($"3. Различных в [1 2 1 3 3 4 -1 -1]: {LinkedListAlgorithms.CountDistinct(distinct)}");

        var nonUnique = Make(1, 2, 2, 3, 3, 3, 4, -1, -1);
        var removed = LinkedListAlgorithms.RemoveNonUnique(nonUnique);
        writer.WriteLine($"4. Удаление неуникальных: удалено {removed}, осталось {Text(nonUnique)}");

        var inserted = Make(1, 2, 3);
        var insertedOk = LinkedListAlgorithms.InsertCopyAfterFirst(inserted, 2);
        writer.WriteLine($"5. Копия после первого 2: успех={insertedOk}, {Text(inserted)}, цикл={inserted.HasCycle()}");

        var sorted = Make(1, 3, 5, 7);
        LinkedListAlgorithms.InsertIntoSorted(sorted, 4);
        writer.WriteLine($"6. Вставка 4 в [1 3 5 7]: {Text(sorted)}");

        var removedAll = Make(1, 2, 3, 2, 4);
        var countRemoved = LinkedListAlgorithms.RemoveAll(removedAll, 2);
        writer.WriteLine($"7. Удаление всех 2: удалено {countRemoved}, осталось {Text(removedAll)}");

        var before = Make(1, 2, 3, 4);
        var beforeOk = LinkedListAlgorithms.InsertBeforeFirst(before, 3, 99);
        writer.WriteLine($"8. Вставить 99 перед первой 3: успех={beforeOk}, {Text(before)}");

        var left = Make(1, 2, 3);
        var right = Make(4, 5);
        LinkedListAlgorithms.Concatenate(left, right);
        writer.WriteLine($"9. Склеивание: L={Text(left)}, E={Text(right)} (E не изменён)");

        var splitSource = Make(1, 2, 3, 4, 5);
        var (first, second) = LinkedListAlgorithms.SplitAtFirst(splitSource, 3);
        writer.WriteLine($"10. Разделение по 3: First={Text(first)}, Second={Text(second)}");

        var duplicate = Make(1, 2, 3);
        LinkedListAlgorithms.Duplicate(duplicate);
        writer.WriteLine($"11. Удвоение: {Text(duplicate)}, цикл={duplicate.HasCycle()}");

        var swap = Make(1, 2, 3, 4, 5);
        LinkedListAlgorithms.SwapAt(swap, 1, 3);
        writer.WriteLine($"12. Обмен индексов 1 и 3 в [1 2 3 4 5]: {Text(swap)}");

        var absent = Make(1, 2, 3);
        var absentOk = LinkedListAlgorithms.InsertBeforeFirst(absent, 77, 8);
        var absentSplit = Make(1, 2, 3);
        var (absentFirst, absentSecond) = LinkedListAlgorithms.SplitAtFirst(absentSplit, 77);
        writer.WriteLine(
            $"Особые случаи: вставка перед отсутствующей 77 -> {absentOk}; " +
            $"разделение по отсутствующей 77 -> First={Text(absentFirst)}, Second={Text(absentSecond)}");
    }

    private static SinglyLinkedList<int> Make(params int[] values)
    {
        var list = new SinglyLinkedList<int>();
        foreach (var value in values)
        {
            list.AddLast(value);
        }

        return list;
    }

    private static string Text<T>(SinglyLinkedList<T> list) =>
        "[" + string.Join(" ", list.Items()) + "]";
}
