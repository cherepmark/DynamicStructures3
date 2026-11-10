using System.Text;

namespace DynamicStructures.Trees;

public sealed class BinaryTreeNode
{
    public BinaryTreeNode(char value)
    {
        Value = value;
    }

    public char Value { get; }

    public BinaryTreeNode? Left { get; internal set; }

    public BinaryTreeNode? Right { get; internal set; }
}

/// <summary>
/// Восстанавливает бинарное дерево из префиксной записи, в которой '*' означает
/// пустую связь. Узел читается, затем рекурсивно строятся левое и правое поддеревья.
/// </summary>
public static class TreeBuilder
{
    public const string DefaultPrefix = "ABD*G***CE**FH**J**";

    public static BinaryTreeNode? Build(string prefix)
    {
        if (!TryBuild(prefix, out var root, out var error))
        {
            throw new FormatException(error);
        }

        return root;
    }

    public static bool TryBuild(string prefix, out BinaryTreeNode? root, out string error)
    {
        root = null;
        error = string.Empty;

        if (prefix is null)
        {
            error = "Строка префиксной записи не может быть null.";
            return false;
        }

        var text = RemoveWhitespace(prefix);
        if (text.Length == 0)
        {
            error = "Префиксная запись пуста.";
            return false;
        }

        try
        {
            var index = 0;
            root = ParseNode(text, ref index);
            if (index != text.Length)
            {
                throw new FormatException(
                    $"Лишние символы после построения дерева: позиция {index + 1}.");
            }

            return true;
        }
        catch (FormatException exception)
        {
            root = null;
            error = exception.Message;
            return false;
        }
    }

    public static int CountNodes(BinaryTreeNode? root) =>
        root is null ? 0 : 1 + CountNodes(root.Left) + CountNodes(root.Right);

    public static int CountLeaves(BinaryTreeNode? root)
    {
        if (root is null)
        {
            return 0;
        }

        if (root.Left is null && root.Right is null)
        {
            return 1;
        }

        return CountLeaves(root.Left) + CountLeaves(root.Right);
    }

    public static int Height(BinaryTreeNode? root) =>
        root is null
            ? 0
            : 1 + Math.Max(Height(root.Left), Height(root.Right));

    public static string Preorder(BinaryTreeNode? root)
    {
        var builder = new StringBuilder();
        AppendPreorder(builder, root);
        return builder.ToString().TrimEnd();
    }

    public static string Inorder(BinaryTreeNode? root)
    {
        var builder = new StringBuilder();
        AppendInorder(builder, root);
        return builder.ToString().TrimEnd();
    }

    public static string Postorder(BinaryTreeNode? root)
    {
        var builder = new StringBuilder();
        AppendPostorder(builder, root);
        return builder.ToString().TrimEnd();
    }

    public static string SerializePrefix(BinaryTreeNode? root)
    {
        var builder = new StringBuilder();
        AppendPrefix(builder, root);
        return builder.ToString();
    }

    public static string ToAscii(BinaryTreeNode? root)
    {
        if (root is null)
        {
            return "(пусто)";
        }

        var builder = new StringBuilder();
        builder.AppendLine(root.Value.ToString());
        AppendAsciiChildren(builder, root, string.Empty);
        return builder.ToString().TrimEnd();
    }

    private static BinaryTreeNode? ParseNode(string text, ref int index)
    {
        if (index >= text.Length)
        {
            throw new FormatException(
                $"Неожиданный конец строки: в позиции {index + 1} ожидался узел или '*'.");
        }

        var position = index;
        var value = text[index++];

        if (value == '*')
        {
            return null;
        }

        if (!char.IsLetterOrDigit(value))
        {
            throw new FormatException(
                $"Недопустимый символ '{value}' в позиции {position + 1}: ожидалась буква, цифра или '*'.");
        }

        var node = new BinaryTreeNode(value);
        node.Left = ParseNode(text, ref index);
        node.Right = ParseNode(text, ref index);
        return node;
    }

    private static string RemoveWhitespace(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var symbol in text)
        {
            if (!char.IsWhiteSpace(symbol))
            {
                builder.Append(symbol);
            }
        }

        return builder.ToString();
    }

    private static void AppendPreorder(StringBuilder builder, BinaryTreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        builder.Append(node.Value).Append(' ');
        AppendPreorder(builder, node.Left);
        AppendPreorder(builder, node.Right);
    }

    private static void AppendInorder(StringBuilder builder, BinaryTreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        AppendInorder(builder, node.Left);
        builder.Append(node.Value).Append(' ');
        AppendInorder(builder, node.Right);
    }

    private static void AppendPostorder(StringBuilder builder, BinaryTreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        AppendPostorder(builder, node.Left);
        AppendPostorder(builder, node.Right);
        builder.Append(node.Value).Append(' ');
    }

    private static void AppendPrefix(StringBuilder builder, BinaryTreeNode? node)
    {
        if (node is null)
        {
            builder.Append('*');
            return;
        }

        builder.Append(node.Value);
        AppendPrefix(builder, node.Left);
        AppendPrefix(builder, node.Right);
    }

    private static void AppendAsciiChildren(StringBuilder builder, BinaryTreeNode node, string prefix)
    {
        builder.Append(prefix).Append("├─ L: ").AppendLine(node.Left?.Value.ToString() ?? "*");
        if (node.Left is not null)
        {
            AppendAsciiChildren(builder, node.Left, prefix + "│  ");
        }

        builder.Append(prefix).Append("└─ R: ").AppendLine(node.Right?.Value.ToString() ?? "*");
        if (node.Right is not null)
        {
            AppendAsciiChildren(builder, node.Right, prefix + "   ");
        }
    }
}
