using DynamicStructures.LinkedListTasks;

namespace DynamicStructures.Tests;

public class LinkedListAlgorithmsTests
{
    [Fact]
    public void Reverse_EmptySingleAndMultiple()
    {
        var empty = TestHelpers.List<int>();
        LinkedListAlgorithms.Reverse(empty);
        Assert.Empty(empty.ToArray());

        var single = TestHelpers.List(5);
        LinkedListAlgorithms.Reverse(single);
        Assert.Equal([5], single.ToArray());
        Assert.Equal(5, single.Head!.Value);
        Assert.Equal(5, single.Tail!.Value);

        var list = TestHelpers.List(1, 2, 3, 4);
        LinkedListAlgorithms.Reverse(list);
        Assert.Equal([4, 3, 2, 1], list.ToArray());
        Assert.Equal(4, list.Head!.Value);
        Assert.Equal(1, list.Tail!.Value);
        Assert.False(list.HasCycle());
    }

    [Fact]
    public void MoveLastToFront_HandlesEmptySingleAndMultiple()
    {
        var empty = TestHelpers.List<int>();
        LinkedListAlgorithms.MoveLastToFront(empty);
        Assert.Empty(empty.ToArray());

        var single = TestHelpers.List(1);
        LinkedListAlgorithms.MoveLastToFront(single);
        Assert.Equal([1], single.ToArray());

        var two = TestHelpers.List(1, 2);
        LinkedListAlgorithms.MoveLastToFront(two);
        Assert.Equal([2, 1], two.ToArray());
        Assert.Equal(1, two.Tail!.Value);

        var list = TestHelpers.List(1, 2, 3);
        LinkedListAlgorithms.MoveLastToFront(list);
        Assert.Equal([3, 1, 2], list.ToArray());
        Assert.Equal(2, list.Tail!.Value);
    }

    [Fact]
    public void MoveFirstToLast_HandlesEmptySingleAndMultiple()
    {
        var empty = TestHelpers.List<int>();
        LinkedListAlgorithms.MoveFirstToLast(empty);
        Assert.Empty(empty.ToArray());

        var single = TestHelpers.List(1);
        LinkedListAlgorithms.MoveFirstToLast(single);
        Assert.Equal([1], single.ToArray());

        var list = TestHelpers.List(1, 2, 3);
        LinkedListAlgorithms.MoveFirstToLast(list);
        Assert.Equal([2, 3, 1], list.ToArray());
        Assert.Equal(1, list.Tail!.Value);
    }

    [Fact]
    public void CountDistinct_Works()
    {
        Assert.Equal(0, LinkedListAlgorithms.CountDistinct(TestHelpers.List<int>()));
        Assert.Equal(1, LinkedListAlgorithms.CountDistinct(TestHelpers.List(7)));
        Assert.Equal(3, LinkedListAlgorithms.CountDistinct(TestHelpers.List(1, 2, 1, 3, 3)));
        Assert.Equal(2, LinkedListAlgorithms.CountDistinct(TestHelpers.List(-1, -1, 5)));
        Assert.Equal(2, LinkedListAlgorithms.CountDistinct(TestHelpers.List("cat", "dog", "cat")));
    }

    [Fact]
    public void RemoveNonUnique_RemovesAllRepeatedValues()
    {
        var list = TestHelpers.List(1, 2, 2, 3, 3, 3, 4);
        var removed = LinkedListAlgorithms.RemoveNonUnique(list);

        Assert.Equal(5, removed);
        Assert.Equal([1, 4], list.ToArray());
        Assert.Equal(4, list.Tail!.Value);

        var allSame = TestHelpers.List(5, 5, 5);
        Assert.Equal(3, LinkedListAlgorithms.RemoveNonUnique(allSame));
        Assert.Empty(allSame.ToArray());
        Assert.Null(allSame.Head);
        Assert.Null(allSame.Tail);

        var unique = TestHelpers.List(1, 2, 3);
        Assert.Equal(0, LinkedListAlgorithms.RemoveNonUnique(unique));
        Assert.Equal([1, 2, 3], unique.ToArray());

        var empty = TestHelpers.List<int>();
        Assert.Equal(0, LinkedListAlgorithms.RemoveNonUnique(empty));
        Assert.Empty(empty.ToArray());
    }

    [Fact]
    public void RemoveNonUnique_HandlesFirstAndLastNodes()
    {
        var list = TestHelpers.List(-1, 2, 2, 3, -1);
        Assert.Equal(4, LinkedListAlgorithms.RemoveNonUnique(list));
        Assert.Equal([3], list.ToArray());

        var headDuplicate = TestHelpers.List(1, 1, 2);
        Assert.Equal(2, LinkedListAlgorithms.RemoveNonUnique(headDuplicate));
        Assert.Equal([2], headDuplicate.ToArray());
    }

    [Fact]
    public void InsertCopyAfterFirst_InsertsIndependentCopy()
    {
        var list = TestHelpers.List(1, 2, 3);
        Assert.True(LinkedListAlgorithms.InsertCopyAfterFirst(list, 2));

        Assert.Equal([1, 2, 1, 2, 3, 3], list.ToArray());
        Assert.Equal(6, list.Count);
        Assert.False(list.HasCycle());

        list.NodeAt(1).Value = 99;
        Assert.Equal(1, list.NodeAt(2).Value);
        Assert.Equal(2, list.NodeAt(3).Value);
    }

    [Fact]
    public void InsertCopyAfterFirst_HeadTailAndMissing()
    {
        var head = TestHelpers.List(2, 3);
        Assert.True(LinkedListAlgorithms.InsertCopyAfterFirst(head, 2));
        Assert.Equal([2, 2, 3, 3], head.ToArray());

        var tail = TestHelpers.List(1, 2);
        Assert.True(LinkedListAlgorithms.InsertCopyAfterFirst(tail, 2));
        Assert.Equal([1, 2, 1, 2], tail.ToArray());
        Assert.Equal(2, tail.Tail!.Value);

        var missing = TestHelpers.List(1, 2);
        Assert.False(LinkedListAlgorithms.InsertCopyAfterFirst(missing, 77));
        Assert.Equal([1, 2], missing.ToArray());

        var empty = TestHelpers.List<int>();
        Assert.False(LinkedListAlgorithms.InsertCopyAfterFirst(empty, 1));
        Assert.Empty(empty.ToArray());
    }

    [Fact]
    public void InsertIntoSorted_HandlesAllPositions()
    {
        var empty = TestHelpers.List<int>();
        LinkedListAlgorithms.InsertIntoSorted(empty, 4);
        Assert.Equal([4], empty.ToArray());

        var head = TestHelpers.List(1, 3, 5);
        LinkedListAlgorithms.InsertIntoSorted(head, 0);
        Assert.Equal([0, 1, 3, 5], head.ToArray());

        var middle = TestHelpers.List(1, 3, 5);
        LinkedListAlgorithms.InsertIntoSorted(middle, 4);
        Assert.Equal([1, 3, 4, 5], middle.ToArray());

        var tail = TestHelpers.List(1, 3, 5);
        LinkedListAlgorithms.InsertIntoSorted(tail, 9);
        Assert.Equal([1, 3, 5, 9], tail.ToArray());
        Assert.Equal(9, tail.Tail!.Value);
    }

    [Fact]
    public void InsertIntoSorted_KeepsEqualValuesInOrder()
    {
        var list = TestHelpers.List(1, 2, 2, 3);
        LinkedListAlgorithms.InsertIntoSorted(list, 2);

        Assert.Equal([1, 2, 2, 2, 3], list.ToArray());
        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void RemoveAll_HandlesAllPositions()
    {
        var middle = TestHelpers.List(1, 2, 3, 2, 4);
        Assert.Equal(2, LinkedListAlgorithms.RemoveAll(middle, 2));
        Assert.Equal([1, 3, 4], middle.ToArray());

        var head = TestHelpers.List(2, 2, 1);
        Assert.Equal(2, LinkedListAlgorithms.RemoveAll(head, 2));
        Assert.Equal([1], head.ToArray());
        Assert.Equal(1, head.Head!.Value);

        var tail = TestHelpers.List(1, 2, 2);
        Assert.Equal(2, LinkedListAlgorithms.RemoveAll(tail, 2));
        Assert.Equal([1], tail.ToArray());
        Assert.Equal(1, tail.Tail!.Value);

        var all = TestHelpers.List(3, 3, 3);
        Assert.Equal(3, LinkedListAlgorithms.RemoveAll(all, 3));
        Assert.True(all.IsEmpty);
        Assert.Null(all.Tail);

        var missing = TestHelpers.List(1, 2);
        Assert.Equal(0, LinkedListAlgorithms.RemoveAll(missing, 9));
        Assert.Equal([1, 2], missing.ToArray());

        Assert.Equal(0, LinkedListAlgorithms.RemoveAll(TestHelpers.List<int>(), 1));
    }

    [Fact]
    public void InsertBeforeFirst_HandlesHeadMiddleTailAndMissing()
    {
        var head = TestHelpers.List(1, 2, 3);
        Assert.True(LinkedListAlgorithms.InsertBeforeFirst(head, 1, 99));
        Assert.Equal([99, 1, 2, 3], head.ToArray());
        Assert.Equal(99, head.Head!.Value);

        var middle = TestHelpers.List(1, 2, 3);
        Assert.True(LinkedListAlgorithms.InsertBeforeFirst(middle, 2, 99));
        Assert.Equal([1, 99, 2, 3], middle.ToArray());

        var tail = TestHelpers.List(1, 2, 3);
        Assert.True(LinkedListAlgorithms.InsertBeforeFirst(tail, 3, 99));
        Assert.Equal([1, 2, 99, 3], tail.ToArray());
        Assert.Equal(3, tail.Tail!.Value);

        var missing = TestHelpers.List(1, 2);
        Assert.False(LinkedListAlgorithms.InsertBeforeFirst(missing, 77, 99));
        Assert.Equal([1, 2], missing.ToArray());

        var empty = TestHelpers.List<int>();
        Assert.False(LinkedListAlgorithms.InsertBeforeFirst(empty, 1, 2));
        Assert.Empty(empty.ToArray());
    }

    [Fact]
    public void Concatenate_AppendsIndependentCopies()
    {
        var left = TestHelpers.List(1, 2);
        var right = TestHelpers.List(3, 4);

        LinkedListAlgorithms.Concatenate(left, right);

        Assert.Equal([1, 2, 3, 4], left.ToArray());
        Assert.Equal([3, 4], right.ToArray());
        Assert.Equal(4, left.Tail!.Value);
        Assert.Equal(4, right.Tail!.Value);

        left.NodeAt(2).Value = 99;
        Assert.Equal(3, right.Head!.Value);
    }

    [Fact]
    public void Concatenate_HandlesEmptyAndSelf()
    {
        var empty = TestHelpers.List<int>();
        var right = TestHelpers.List(1, 2);
        LinkedListAlgorithms.Concatenate(empty, right);
        Assert.Equal([1, 2], empty.ToArray());
        Assert.Equal([1, 2], right.ToArray());

        var left = TestHelpers.List(1, 2);
        LinkedListAlgorithms.Concatenate(left, TestHelpers.List<int>());
        Assert.Equal([1, 2], left.ToArray());

        var self = TestHelpers.List(1, 2);
        LinkedListAlgorithms.Concatenate(self, self);
        Assert.Equal([1, 2, 1, 2], self.ToArray());
        Assert.Equal(4, self.Count);
        Assert.False(self.HasCycle());
    }

    [Fact]
    public void SplitAtFirst_SplitsAtMiddle()
    {
        var source = TestHelpers.List(1, 2, 3, 4, 5);
        var (first, second) = LinkedListAlgorithms.SplitAtFirst(source, 3);

        Assert.Same(source, first);
        Assert.Equal([1, 2], first.ToArray());
        Assert.Equal([3, 4, 5], second.ToArray());
        Assert.Equal(2, first.Count);
        Assert.Equal(3, second.Count);
        Assert.Equal(2, first.Tail!.Value);
        Assert.Equal(5, second.Tail!.Value);
        Assert.False(first.HasCycle());
        Assert.False(second.HasCycle());
    }

    [Fact]
    public void SplitAtFirst_HandlesHeadTailAbsentAndEmpty()
    {
        var headSource = TestHelpers.List(1, 2, 3);
        var (headFirst, headSecond) = LinkedListAlgorithms.SplitAtFirst(headSource, 1);
        Assert.Empty(headFirst.ToArray());
        Assert.Equal([1, 2, 3], headSecond.ToArray());

        var tailSource = TestHelpers.List(1, 2, 3);
        var (tailFirst, tailSecond) = LinkedListAlgorithms.SplitAtFirst(tailSource, 3);
        Assert.Equal([1, 2], tailFirst.ToArray());
        Assert.Equal([3], tailSecond.ToArray());

        var absentSource = TestHelpers.List(1, 2, 3);
        var (absentFirst, absentSecond) = LinkedListAlgorithms.SplitAtFirst(absentSource, 77);
        Assert.Same(absentSource, absentFirst);
        Assert.Equal([1, 2, 3], absentFirst.ToArray());
        Assert.Empty(absentSecond.ToArray());

        var emptySource = TestHelpers.List<int>();
        var (emptyFirst, emptySecond) = LinkedListAlgorithms.SplitAtFirst(emptySource, 1);
        Assert.Empty(emptyFirst.ToArray());
        Assert.Empty(emptySecond.ToArray());
    }

    [Fact]
    public void SplitAtFirst_UsesFirstOccurrence()
    {
        var source = TestHelpers.List(1, 2, 1, 3);
        var (first, second) = LinkedListAlgorithms.SplitAtFirst(source, 1);

        Assert.Empty(first.ToArray());
        Assert.Equal([1, 2, 1, 3], second.ToArray());
    }

    [Fact]
    public void Duplicate_AppendsIndependentCopy()
    {
        var list = TestHelpers.List(1, 2, 3);
        LinkedListAlgorithms.Duplicate(list);

        Assert.Equal([1, 2, 3, 1, 2, 3], list.ToArray());
        Assert.Equal(6, list.Count);
        Assert.Equal(3, list.Tail!.Value);
        Assert.False(list.HasCycle());

        list.NodeAt(0).Value = 99;
        Assert.Equal(1, list.NodeAt(3).Value);
    }

    [Fact]
    public void Duplicate_HandlesEmptyAndSingle()
    {
        var empty = TestHelpers.List<int>();
        LinkedListAlgorithms.Duplicate(empty);
        Assert.Empty(empty.ToArray());

        var single = TestHelpers.List(5);
        LinkedListAlgorithms.Duplicate(single);
        Assert.Equal([5, 5], single.ToArray());
        Assert.Equal(2, single.Count);
    }

    [Fact]
    public void SwapAt_SwapsNodesIncludingHeadAndTail()
    {
        var list = TestHelpers.List(1, 2, 3, 4);
        LinkedListAlgorithms.SwapAt(list, 0, 3);
        Assert.Equal([4, 2, 3, 1], list.ToArray());
        Assert.Equal(4, list.Head!.Value);
        Assert.Equal(1, list.Tail!.Value);

        var adjacent = TestHelpers.List(1, 2, 3, 4);
        LinkedListAlgorithms.SwapAt(adjacent, 0, 1);
        Assert.Equal([2, 1, 3, 4], adjacent.ToArray());

        var middle = TestHelpers.List(1, 2, 3, 4);
        LinkedListAlgorithms.SwapAt(middle, 1, 2);
        Assert.Equal([1, 3, 2, 4], middle.ToArray());

        var reversedIndices = TestHelpers.List(1, 2, 3, 4);
        LinkedListAlgorithms.SwapAt(reversedIndices, 2, 0);
        Assert.Equal([3, 2, 1, 4], reversedIndices.ToArray());
    }

    [Fact]
    public void SwapAt_SameIndexAndTwoElements()
    {
        var same = TestHelpers.List(1, 2, 3);
        LinkedListAlgorithms.SwapAt(same, 1, 1);
        Assert.Equal([1, 2, 3], same.ToArray());

        var two = TestHelpers.List(1, 2);
        LinkedListAlgorithms.SwapAt(two, 0, 1);
        Assert.Equal([2, 1], two.ToArray());
        Assert.Equal(2, two.Head!.Value);
        Assert.Equal(1, two.Tail!.Value);
    }

    [Fact]
    public void SwapAt_InvalidIndexesThrow()
    {
        var list = TestHelpers.List(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => LinkedListAlgorithms.SwapAt(list, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinkedListAlgorithms.SwapAt(list, 0, 3));

        var empty = TestHelpers.List<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => LinkedListAlgorithms.SwapAt(empty, 0, 1));
    }
}
