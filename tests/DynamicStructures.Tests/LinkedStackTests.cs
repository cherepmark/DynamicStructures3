using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class LinkedStackTests
{
    [Fact]
    public void PushTopPop_RespectLifo()
    {
        var stack = new LinkedStack<string>();
        stack.Push("56");
        stack.Push("cat");
        stack.Push("hello");

        Assert.Equal(3, stack.Count);
        Assert.False(stack.IsEmpty);
        Assert.Equal("hello", stack.Top());
        Assert.Equal("hello", stack.Pop());
        Assert.Equal("cat", stack.Top());
        Assert.Equal("cat", stack.Pop());
        Assert.Equal("56", stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void EmptyStack_ReturnsFriendlyTryResults()
    {
        var stack = new LinkedStack<int>();
        Assert.False(stack.TryPop(out var popped));
        Assert.Equal(0, popped);
        Assert.False(stack.TryTop(out var top));
        Assert.Equal(0, top);
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Top());
    }

    [Fact]
    public void OneElement_Works()
    {
        var stack = new LinkedStack<int>();
        stack.Push(-7);
        Assert.Equal(-7, stack.Top());
        Assert.Equal(-7, stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void Snapshot_IsTopFirst()
    {
        var stack = new LinkedStack<string>();
        stack.Push("bottom");
        stack.Push("middle");
        stack.Push("top");

        Assert.Equal(["top", "middle", "bottom"], stack.ToArrayTopFirst());

        var writer = new StringWriter();
        stack.Print(writer);
        Assert.Equal("top middle bottom", writer.ToString());
    }

    [Fact]
    public void Clear_EmptiesStack()
    {
        var stack = new LinkedStack<int>();
        stack.Push(1);
        stack.Push(2);
        stack.Clear();

        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void SupportsNumbersAndWords()
    {
        var stack = new LinkedStack<string>();
        stack.Push("56");
        stack.Push("cat");

        Assert.Equal("cat", stack.Pop());
        Assert.Equal("56", stack.Pop());
    }
}
