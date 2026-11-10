using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class SinglyLinkedListTests
{
    [Fact]
    public void AddFirstAndAddLast_PreserveOrder()
    {
        var list = new SinglyLinkedList<string>();
        list.AddLast("b");
        list.AddFirst("a");
        list.AddLast("c");

        Assert.Equal(["a", "b", "c"], list.ToArray());
        Assert.Equal(3, list.Count);
        Assert.Equal("a", list.Head!.Value);
        Assert.Equal("c", list.Tail!.Value);
        Assert.False(list.HasCycle());
    }

    [Fact]
    public void RemoveFirst_EmptyAndNonEmpty()
    {
        var empty = new SinglyLinkedList<int>();
        Assert.False(empty.RemoveFirst(out _));

        var list = TestHelpers.List(1, 2);
        Assert.True(list.RemoveFirst(out var first));
        Assert.Equal(1, first);
        Assert.True(list.RemoveFirst(out var second));
        Assert.Equal(2, second);
        Assert.True(list.IsEmpty);
        Assert.Null(list.Tail);
        Assert.False(list.RemoveFirst(out _));
    }

    [Fact]
    public void InsertAt_HeadMiddleTail()
    {
        var list = TestHelpers.List(2, 4);
        list.InsertAt(0, 1);
        list.InsertAt(2, 3);
        list.InsertAt(4, 5);

        Assert.Equal([1, 2, 3, 4, 5], list.ToArray());
        Assert.Equal(5, list.Tail!.Value);
    }

    [Fact]
    public void InsertAt_OutOfRange_Throws()
    {
        var list = TestHelpers.List(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertAt(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertAt(2, 0));
    }

    [Fact]
    public void RemoveAt_HeadMiddleTailAndMissing()
    {
        var list = TestHelpers.List(1, 2, 3);
        Assert.True(list.RemoveAt(1, out var middle));
        Assert.Equal(2, middle);
        Assert.Equal([1, 3], list.ToArray());

        Assert.True(list.RemoveAt(0, out var head));
        Assert.Equal(1, head);
        Assert.True(list.RemoveAt(0, out var tail));
        Assert.Equal(3, tail);
        Assert.True(list.IsEmpty);
        Assert.Null(list.Head);
        Assert.Null(list.Tail);

        Assert.False(list.RemoveAt(0, out _));
        Assert.False(list.RemoveAt(-1, out _));
    }

    [Fact]
    public void NodeAt_ReturnsNodesAndRejectsInvalidIndexes()
    {
        var list = TestHelpers.List(10, 20, 30);
        Assert.Equal(10, list.NodeAt(0).Value);
        Assert.Same(list.Tail, list.NodeAt(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.NodeAt(3));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.NodeAt(-1));
    }

    [Fact]
    public void Clear_ResetsList()
    {
        var list = TestHelpers.List(1, 2, 3);
        list.Clear();

        Assert.True(list.IsEmpty);
        Assert.Empty(list.ToArray());
        Assert.Null(list.Head);
        Assert.Null(list.Tail);
    }

    [Fact]
    public void Enumeration_Works()
    {
        var list = TestHelpers.List("x", "y");
        Assert.Equal(["x", "y"], list.Items().ToArray());
    }
}
