using DynamicStructures.Trees;

namespace DynamicStructures.Tests;

public class TreeBuilderTests
{
    [Fact]
    public void BuildsExampleTree_WithExpectedShape()
    {
        var root = TreeBuilder.Build(TreeBuilder.DefaultPrefix);

        Assert.NotNull(root);
        Assert.Equal('A', root.Value);
        Assert.Equal('B', root.Left!.Value);
        Assert.Equal('C', root.Right!.Value);

        Assert.Equal('D', root.Left.Left!.Value);
        Assert.Null(root.Left.Left.Left);
        Assert.Equal('G', root.Left.Left.Right!.Value);
        Assert.Null(root.Left.Left.Right.Left);
        Assert.Null(root.Left.Left.Right.Right);
        Assert.Null(root.Left.Right);

        Assert.Equal('E', root.Right.Left!.Value);
        Assert.Null(root.Right.Left.Left);
        Assert.Null(root.Right.Left.Right);

        Assert.Equal('F', root.Right.Right!.Value);
        Assert.Equal('H', root.Right.Right.Left!.Value);
        Assert.Equal('J', root.Right.Right.Right!.Value);
        Assert.Null(root.Right.Right.Left.Left);
        Assert.Null(root.Right.Right.Left.Right);
        Assert.Null(root.Right.Right.Right.Left);
        Assert.Null(root.Right.Right.Right.Right);
    }

    [Fact]
    public void ExampleTree_HasExpectedStatistics()
    {
        var root = TreeBuilder.Build(TreeBuilder.DefaultPrefix);

        Assert.Equal(9, TreeBuilder.CountNodes(root));
        Assert.Equal(4, TreeBuilder.CountLeaves(root));
        Assert.Equal(4, TreeBuilder.Height(root));
    }

    [Fact]
    public void ExampleTree_HasExpectedTraversals()
    {
        var root = TreeBuilder.Build(TreeBuilder.DefaultPrefix);

        Assert.Equal("A B D G C E F H J", TreeBuilder.Preorder(root));
        Assert.Equal("D G B A E C H F J", TreeBuilder.Inorder(root));
        Assert.Equal("G D B E H J F C A", TreeBuilder.Postorder(root));
        Assert.Equal(TreeBuilder.DefaultPrefix, TreeBuilder.SerializePrefix(root));
    }

    [Fact]
    public void Whitespace_IsIgnored()
    {
        var spaced = "A B D * G * * * C E * * F H * * J * *";
        var root = TreeBuilder.Build(spaced);

        Assert.Equal(9, TreeBuilder.CountNodes(root));
        Assert.Equal(TreeBuilder.DefaultPrefix, TreeBuilder.SerializePrefix(root));
    }

    [Fact]
    public void EmptyTreeAndSingleNode_Work()
    {
        var empty = TreeBuilder.Build("*");
        Assert.Null(empty);
        Assert.Equal(0, TreeBuilder.CountNodes(empty));
        Assert.Equal(0, TreeBuilder.CountLeaves(empty));
        Assert.Equal(0, TreeBuilder.Height(empty));
        Assert.Equal(string.Empty, TreeBuilder.Preorder(empty));
        Assert.Equal("*", TreeBuilder.SerializePrefix(empty));
        Assert.Equal("(пусто)", TreeBuilder.ToAscii(empty));

        var single = TreeBuilder.Build("A**");
        Assert.NotNull(single);
        Assert.Equal('A', single.Value);
        Assert.Null(single.Left);
        Assert.Null(single.Right);
        Assert.Equal(1, TreeBuilder.CountNodes(single));
        Assert.Equal(1, TreeBuilder.CountLeaves(single));
        Assert.Equal(1, TreeBuilder.Height(single));
        Assert.Equal("A", TreeBuilder.Preorder(single));
        Assert.Equal("A**", TreeBuilder.SerializePrefix(single));
    }

    [Fact]
    public void Ascii_ShowsNodesAndEmptyLinks()
    {
        var root = TreeBuilder.Build(TreeBuilder.DefaultPrefix);
        var ascii = TreeBuilder.ToAscii(root);

        Assert.Contains("A", ascii);
        Assert.Contains("├─ L: B", ascii);
        Assert.Contains("└─ R: C", ascii);
        Assert.Contains("L: *", ascii);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    [InlineData("A*")]
    [InlineData("**")]
    [InlineData("A***X")]
    [InlineData("A*?*")]
    public void InvalidPrefix_IsRejected(string prefix)
    {
        var success = TreeBuilder.TryBuild(prefix, out var root, out var error);

        Assert.False(success);
        Assert.Null(root);
        Assert.False(string.IsNullOrWhiteSpace(error));
        Assert.Throws<FormatException>(() => TreeBuilder.Build(prefix));
    }

    [Fact]
    public void NullPrefix_IsRejected()
    {
        Assert.False(TreeBuilder.TryBuild(null!, out var root, out var error));
        Assert.Null(root);
        Assert.NotEmpty(error);
        Assert.Throws<FormatException>(() => TreeBuilder.Build(null!));
    }
}
