using DynamicStructures.Core;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class CommandExecutorTests
{
    [Fact]
    public void StackCommands_ProduceExpectedOutcomes()
    {
        var commands = CommandParser.Parse("4 1,A 1,B 3 2 5 4").Commands;
        var stack = new LinkedStack<string>();
        var observer = new CollectingOperationObserver();

        CommandExecutor.ExecuteStack(stack, commands, observer);

        Assert.Equal(7, observer.Outcomes.Count);
        Assert.Equal(OperationStatus.Ok, observer.Outcomes[0].Status);
        Assert.True(observer.Outcomes[0].BooleanValue);
        Assert.Equal("A", observer.Outcomes[1].Value);
        Assert.Equal("B", observer.Outcomes[2].Value);
        Assert.Equal("B", observer.Outcomes[3].Value);
        Assert.Equal("B", observer.Outcomes[4].Value);
        Assert.Equal(["A"], observer.Outcomes[5].Items);
        Assert.False(observer.Outcomes[6].BooleanValue);
        Assert.Equal("A", stack.Top());
    }

    [Fact]
    public void StackEmptyPopAndPeek_AreReportedAsEmpty()
    {
        var commands = CommandParser.Parse("2 3").Commands;
        var observer = new CollectingOperationObserver();

        CommandExecutor.ExecuteStack(new LinkedStack<string>(), commands, observer);

        Assert.All(observer.Outcomes, outcome => Assert.Equal(OperationStatus.Empty, outcome.Status));
    }

    [Fact]
    public void QueueCommands_RespectFifo()
    {
        var commands = CommandParser.Parse("1,first 1,second 2 3 5").Commands;
        var queue = new ListBasedQueue<string>();
        var observer = new CollectingOperationObserver();

        CommandExecutor.ExecuteQueue(queue, commands, observer);

        Assert.Equal("first", observer.Outcomes[2].Value);
        Assert.Equal("second", observer.Outcomes[3].Value);
        Assert.Equal(["second"], observer.Outcomes[4].Items);
        Assert.Equal("second", queue.Snapshot()[0]);
    }

    [Fact]
    public void QueueEmptyDequeueAndPeek_AreReportedAsEmpty()
    {
        var commands = CommandParser.Parse("2 3").Commands;
        var observer = new CollectingOperationObserver();

        CommandExecutor.ExecuteQueue(new StandardQueue<string>(), commands, observer);

        Assert.All(observer.Outcomes, outcome => Assert.Equal(OperationStatus.Empty, outcome.Status));
    }

    [Fact]
    public void ExecuteWithoutObserver_StillChangesStructure()
    {
        var commands = CommandParser.Parse("1,a 1,b 2").Commands;
        var stack = new LinkedStack<string>();

        CommandExecutor.ExecuteStack(stack, commands, observer: null);

        Assert.Equal(1, stack.Count);
        Assert.Equal("a", stack.Top());
    }

    [Fact]
    public void ConsoleObserver_FormatsFriendlyMessages()
    {
        var outcome = new OperationOutcome(
            1,
            "3",
            CommandKind.Peek,
            OperationStatus.Empty,
            Count: 0);
        var text = ConsoleOperationObserver.Format(outcome, "стек", isStack: true);
        Assert.Contains("стек пуст", text);

        var printOutcome = new OperationOutcome(
            2,
            "5",
            CommandKind.Print,
            OperationStatus.Ok,
            Count: 2,
            Items: new[] { "b", "a" });
        var printText = ConsoleOperationObserver.Format(printOutcome, "стек", isStack: true);
        Assert.Contains("b a", printText);
    }
}
