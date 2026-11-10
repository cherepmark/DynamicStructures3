using DynamicStructures.Core;
using DynamicStructures.DataStructures;
using DynamicStructures.Expressions;
using DynamicStructures.Infrastructure;

namespace DynamicStructures.Tests;

public class FileAndInputTests
{
    [Fact]
    public void InputFile_HasRequiredExampleAndExecutes()
    {
        Assert.True(File.Exists(ProjectPaths.InputFile));

        var file = CommandParser.Load(ProjectPaths.InputFile);
        Assert.Empty(file.Issues);
        Assert.Equal(8, file.Commands.Count);

        var stackObserver = new CollectingOperationObserver();
        var stack = new LinkedStack<string>();
        CommandExecutor.ExecuteStack(stack, file.Commands, stackObserver);

        Assert.All(stackObserver.Outcomes, outcome => Assert.NotEqual(OperationStatus.Invalid, outcome.Status));
        Assert.Equal(OperationStatus.Empty, stackObserver.Outcomes[0].Status);
        Assert.Equal(2, stack.Count);
        Assert.Equal("7", stack.Top());

        foreach (var queue in new IQueue<string>[]
                 {
                     new ListBasedQueue<string>(),
                     new StandardQueue<string>()
                 })
        {
            var observer = new CollectingOperationObserver();
            CommandExecutor.ExecuteQueue(queue, file.Commands, observer);
            Assert.All(observer.Outcomes, outcome => Assert.NotEqual(OperationStatus.Invalid, outcome.Status));
            Assert.Equal(2, queue.Count);
            Assert.Equal("7", queue.Snapshot()[0]);
        }
    }

    [Fact]
    public void PostfixDataFiles_ContainExpectedValidAndInvalidExamples()
    {
        var validPath = ProjectPaths.ResolveExistingFile("data/postfix.txt");
        var valid = ExpressionFile.Load(validPath);
        Assert.NotEmpty(valid);
        foreach (var line in valid)
        {
            var result = PostfixCalculator.Evaluate(line.Expression);
            Assert.True(result.Success, $"Строка {line.LineNumber}: {result.Error}");
        }

        var invalidPath = ProjectPaths.ResolveExistingFile("data/postfix_invalid.txt");
        var invalid = ExpressionFile.Load(invalidPath);
        Assert.NotEmpty(invalid);
        foreach (var line in invalid)
        {
            var result = PostfixCalculator.Evaluate(line.Expression);
            Assert.False(result.Success, $"Строка {line.LineNumber} неожиданно вычислена.");
        }
    }

    [Fact]
    public void InfixDataFiles_ContainExpectedValidAndInvalidExamples()
    {
        var validPath = ProjectPaths.ResolveExistingFile("data/infix.txt");
        var valid = ExpressionFile.Load(validPath);
        Assert.NotEmpty(valid);
        foreach (var line in valid)
        {
            var conversion = ExpressionConverter.Convert(line.Expression);
            Assert.True(conversion.Success, $"Строка {line.LineNumber}: {conversion.Error}");
            var evaluation = PostfixCalculator.Evaluate(conversion.Postfix!);
            Assert.True(evaluation.Success, $"Строка {line.LineNumber}: {evaluation.Error}");
        }

        var invalidPath = ProjectPaths.ResolveExistingFile("data/infix_invalid.txt");
        var invalid = ExpressionFile.Load(invalidPath);
        Assert.NotEmpty(invalid);
        foreach (var line in invalid)
        {
            var conversion = ExpressionConverter.Convert(line.Expression);
            Assert.False(conversion.Success, $"Строка {line.LineNumber} неожиданно преобразована.");
        }
    }

    [Fact]
    public void IntListFiles_Parse()
    {
        var path = ProjectPaths.ResolveExistingFile("data/list_L.txt");
        var list = IntListFile.Load(path);
        Assert.Equal([3, 1, 2, 1, 5, 2, 4], list.ToArray());

        Assert.True(File.Exists(ProjectPaths.ResolveExistingFile("data/list_E.txt")));
        Assert.True(File.Exists(ProjectPaths.ResolveExistingFile("data/sorted.txt")));
        Assert.True(File.Exists(ProjectPaths.ResolveExistingFile("data/split.txt")));
    }

    [Fact]
    public void IntListFile_ParsesSeparatorsAndComments()
    {
        var list = IntListFile.Parse("1, 2; -3\n# comment\n4", "unit");
        Assert.Equal([1, 2, -3, 4], list.ToArray());
    }

    [Fact]
    public void IntListFile_RejectsNonInteger()
    {
        var exception = Assert.Throws<FormatException>(() => IntListFile.Parse("1 x 2", "unit"));
        Assert.Contains("строка 1", exception.Message);
        Assert.Contains("unit", exception.Message);
    }

    [Fact]
    public void ResolveExistingFile_ThrowsHelpfulError()
    {
        var exception = Assert.Throws<FileNotFoundException>(
            () => ProjectPaths.ResolveExistingFile("definitely-missing-file-xyz.txt"));
        Assert.Contains("definitely-missing-file-xyz.txt", exception.Message);
    }

    [Fact]
    public void ExpressionFile_SkipsEmptyAndCommentLines()
    {
        var directory = TestHelpers.TempDirectory();
        try
        {
            var path = Path.Combine(directory, "expressions.txt");
            File.WriteAllText(path, "# comment\n\n2 2 +\n   \n3 3 +\n");
            var lines = ExpressionFile.Load(path);

            Assert.Equal(2, lines.Count);
            Assert.Equal(3, lines[0].LineNumber);
            Assert.Equal("2 2 +", lines[0].Expression);
            Assert.Equal(5, lines[1].LineNumber);
        }
        finally
        {
            TestHelpers.DeleteDirectory(directory);
        }
    }
}
