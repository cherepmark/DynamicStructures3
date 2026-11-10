using DynamicStructures.Applications;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class ApplicationTests
{
    [Fact]
    public void Playlist_AddMoveRemove()
    {
        var playlist = new Playlist("Test");
        playlist.Add(new Song("A", "Artist", 60));
        playlist.Add(new Song("B", "Artist", 120));
        playlist.Add(new Song("C", "Artist", 30));

        Assert.Equal(["A", "B", "C"], playlist.Titles());
        Assert.Equal(TimeSpan.FromSeconds(210), playlist.TotalDuration);

        Assert.True(playlist.Move(0, 2));
        Assert.Equal(["B", "C", "A"], playlist.Titles());

        Assert.Equal(0, playlist.IndexOf("B"));
        Assert.False(playlist.Remove("missing"));
        Assert.True(playlist.Remove("C"));
        Assert.Equal(["B", "A"], playlist.Titles());
    }

    [Fact]
    public void Playlist_ValidatesInput()
    {
        var playlist = new Playlist();
        Assert.Throws<ArgumentException>(() => playlist.Add(new Song("", "Artist", 10)));
        Assert.Throws<ArgumentOutOfRangeException>(() => playlist.Add(new Song("A", "Artist", -1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => playlist.Move(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => playlist[0]);
    }

    [Fact]
    public void EditorHistory_UndoUsesStackOrder()
    {
        var history = new EditorHistory();
        history.Apply("A");
        history.Apply("B");
        history.Apply("C");

        Assert.Equal("C", history.Current);
        Assert.Equal(3, history.UndoDepth);

        Assert.True(history.Undo());
        Assert.Equal("B", history.Current);
        Assert.True(history.Undo());
        Assert.Equal("A", history.Current);
        Assert.True(history.Undo());
        Assert.Equal("", history.Current);
        Assert.False(history.Undo());
        Assert.Equal(0, history.UndoDepth);
    }

    [Fact]
    public void EditorHistory_SupportsSnapshotsAndClear()
    {
        var history = new EditorHistory();
        history.Apply("one");
        history.Apply("two");

        Assert.Equal(["one", ""], history.SnapshotsTopFirst());

        history.Clear();
        Assert.Equal("", history.Current);
        Assert.Equal(0, history.UndoDepth);
    }

    [Fact]
    public void PrintQueue_ProcessesFifo()
    {
        var printer = new PrintQueueSimulation();
        printer.Submit(new PrintJob("one", 1));
        printer.Submit(new PrintJob("two", 2));
        printer.Submit(new PrintJob("three", 3));

        Assert.Equal(["one", "two", "three"], printer.Pending().Select(job => job.Document).ToArray());

        Assert.True(printer.TryProcessNext(out var first));
        Assert.Equal("one", first.Document);
        Assert.Equal(2, printer.PendingCount);

        printer.ProcessAll();
        Assert.Equal(
            ["one", "two", "three"],
            printer.Processed.Select(job => job.Document).ToArray());
        Assert.True(printer.IsEmpty);
        Assert.False(printer.TryProcessNext(out _));
    }

    [Fact]
    public void PrintQueue_CanUseStandardQueueAndValidates()
    {
        var printer = new PrintQueueSimulation(new StandardQueue<PrintJob>());
        Assert.Equal("StandardQueue", printer.ImplementationName);

        Assert.Throws<ArgumentException>(() => printer.Submit(new PrintJob("", 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => printer.Submit(new PrintJob("x", 0)));

        printer.Submit(new PrintJob("x", 1));
        Assert.Equal(1, printer.PendingCount);
    }

    [Fact]
    public void ExpressionTree_BuildsAndTraverses()
    {
        var tree = ExpressionTree.FromPostfix("2 3 + 4 *");

        Assert.Equal("((2 + 3) * 4)", tree.Inorder());
        Assert.Equal("* + 2 3 4", tree.Preorder());
        Assert.Equal("2 3 + 4 *", tree.Postorder());
        Assert.Equal(20, tree.Evaluate());
        Assert.Equal("2 3 + 4 *", tree.SourcePostfix);
    }

    [Fact]
    public void ExpressionTree_HandlesFunctionsAndUnaryMinus()
    {
        var tree = ExpressionTree.FromInfix("sin(0)+sqrt(9)");
        Assert.Equal(3, tree.Evaluate());
        Assert.Contains("sin", tree.Inorder());
        Assert.Contains("sqrt", tree.Inorder());

        var unary = ExpressionTree.FromInfix("-2^2");
        Assert.Equal(-4, unary.Evaluate());
        Assert.Equal("2 2 ^ ~", unary.Postorder());

        var compact = ExpressionTree.FromPostfix("78+32+/");
        Assert.Equal(3, compact.Evaluate());
        Assert.Equal("7 8 + 3 2 + /", compact.SourcePostfix);
    }

    [Fact]
    public void ExpressionTree_RejectsInvalidExpressions()
    {
        Assert.Throws<ExpressionTreeException>(() => ExpressionTree.FromPostfix("2 +"));
        Assert.Throws<ExpressionTreeException>(() => ExpressionTree.FromPostfix("1 2"));
        Assert.Throws<ExpressionTreeException>(() => ExpressionTree.FromPostfix(""));
        Assert.Throws<ExpressionTreeException>(() => ExpressionTree.FromInfix("(2+3"));

        var domain = ExpressionTree.FromPostfix("0 ln");
        Assert.Throws<ExpressionTreeException>(() => domain.Evaluate());
    }

    [Fact]
    public void ApplicationDemos_RunAllSections()
    {
        var writer = new StringWriter();
        ApplicationDemos.RunAll(writer);
        var text = writer.ToString();

        Assert.Contains("1. Список", text);
        Assert.Contains("2. Стек", text);
        Assert.Contains("3. Очередь", text);
        Assert.Contains("4. Дерево", text);
        Assert.Contains("Значение: 20", text);
    }
}
