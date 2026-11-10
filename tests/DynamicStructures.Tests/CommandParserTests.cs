using DynamicStructures.Core;

namespace DynamicStructures.Tests;

public class CommandParserTests
{
    [Fact]
    public void ParsesRequiredExample()
    {
        var file = CommandParser.Parse("3 4 1,56 1,7 1,cat 2 5 4");

        Assert.False(file.HasIssues);
        Assert.Equal(8, file.Commands.Count);
        Assert.Equal(
            [
                CommandKind.Peek,
                CommandKind.IsEmpty,
                CommandKind.Push,
                CommandKind.Push,
                CommandKind.Push,
                CommandKind.Pop,
                CommandKind.Print,
                CommandKind.IsEmpty
            ],
            file.Commands.Select(command => command.Kind).ToArray());

        Assert.Equal(["56", "7", "cat"], file.Commands
            .Where(command => command.Kind == CommandKind.Push)
            .Select(command => command.Value!)
            .ToArray());
    }

    [Fact]
    public void ValueMayContainComma()
    {
        var file = CommandParser.Parse("1,a,b 5");
        Assert.Empty(file.Issues);
        Assert.Equal("a,b", file.Commands[0].Value);
    }

    [Fact]
    public void EmptyText_HasNoCommands()
    {
        var file = CommandParser.Parse("  \r\n\t ");
        Assert.Empty(file.Commands);
        Assert.Empty(file.Issues);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1,")]
    [InlineData("7")]
    [InlineData("abc")]
    [InlineData("1 56")]
    public void InvalidToken_IsReportedWithoutThrowing(string text)
    {
        var file = CommandParser.Parse(text);
        Assert.NotEmpty(file.Issues);
        Assert.Empty(file.Commands);
    }

    [Fact]
    public void ReportsLineNumbers()
    {
        var file = CommandParser.Parse("1,ok\n2\nbad", "unit");
        var issue = Assert.Single(file.Issues);
        Assert.Equal(3, issue.Line);
        Assert.Equal("bad", issue.Token);
        Assert.Equal("unit", file.Source);
    }

    [Fact]
    public void Composition_CountsOperations()
    {
        var file = CommandParser.Parse("1,a 1,b 3 2 4 5");
        var description = CommandComposition.Describe(file.Commands);

        Assert.Contains("push/enqueue=2", description);
        Assert.Contains("pop/dequeue=1", description);
        Assert.Contains("peek/top=1", description);
        Assert.Contains("isEmpty=1", description);
        Assert.Contains("print=1", description);
        Assert.Contains("total=6", description);
    }
}
