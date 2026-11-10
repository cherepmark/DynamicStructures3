using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class QueueTests
{
    public static IEnumerable<object[]> Implementations()
    {
        yield return ["list"];
        yield return ["standard"];
    }

    private static IQueue<string> Create(string kind) => kind switch
    {
        "list" => new ListBasedQueue<string>(),
        "standard" => new StandardQueue<string>(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    [Theory]
    [MemberData(nameof(Implementations))]
    public void EnqueueDequeuePeek_RespectFifo(string kind)
    {
        var queue = Create(kind);
        queue.Enqueue("first");
        queue.Enqueue("second");
        queue.Enqueue("cat");

        Assert.Equal(3, queue.Count);
        Assert.False(queue.IsEmpty);
        Assert.True(queue.TryPeek(out var peeked));
        Assert.Equal("first", peeked);

        Assert.True(queue.TryDequeue(out var first));
        Assert.Equal("first", first);
        Assert.True(queue.TryPeek(out var second));
        Assert.Equal("second", second);
        Assert.True(queue.TryDequeue(out var secondRemoved));
        Assert.Equal("second", secondRemoved);
        Assert.True(queue.TryDequeue(out var third));
        Assert.Equal("cat", third);
        Assert.True(queue.IsEmpty);
    }

    [Theory]
    [MemberData(nameof(Implementations))]
    public void EmptyQueue_ReturnsFalse(string kind)
    {
        var queue = Create(kind);
        Assert.False(queue.TryDequeue(out var item));
        Assert.Null(item);
        Assert.False(queue.TryPeek(out var peeked));
        Assert.Null(peeked);
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
    }

    [Theory]
    [MemberData(nameof(Implementations))]
    public void OneElement_Works(string kind)
    {
        var queue = Create(kind);
        queue.Enqueue("only");
        Assert.Equal("only", queue.Snapshot()[0]);
        Assert.True(queue.TryPeek(out var peeked));
        Assert.Equal("only", peeked);
        Assert.True(queue.TryDequeue(out var removed));
        Assert.Equal("only", removed);
        Assert.True(queue.IsEmpty);
    }

    [Theory]
    [MemberData(nameof(Implementations))]
    public void Snapshot_IsFrontFirst(string kind)
    {
        var queue = Create(kind);
        queue.Enqueue("a");
        queue.Enqueue("b");
        queue.Enqueue("c");

        Assert.Equal(["a", "b", "c"], queue.Snapshot());
    }

    [Theory]
    [MemberData(nameof(Implementations))]
    public void ImplementationName_IsSet(string kind)
    {
        var queue = Create(kind);
        Assert.False(string.IsNullOrWhiteSpace(queue.ImplementationName));
        Assert.Equal(kind == "list" ? "ListBasedQueue" : "StandardQueue", queue.ImplementationName);
    }
}
