using System.Globalization;
using DynamicStructures.DataStructures;
using DynamicStructures.Expressions;

namespace DynamicStructures.Applications;

public sealed class ExpressionTreeException : Exception
{
    public ExpressionTreeException(string message) : base(message)
    {
    }
}

/// <summary>
/// Дерево арифметического выражения: листья — числа, внутренние узлы — операции
/// и функции. Дерево строится из постфиксной записи и обходится рекурсивно.
/// </summary>
public sealed class ExpressionTree
{
    public sealed class Node
    {
        private Node()
        {
        }

        public string Text { get; private init; } = string.Empty;

        public double Number { get; private init; }

        public bool IsNumber { get; private init; }

        public Node? Left { get; private init; }

        public Node? Right { get; private init; }

        public static Node NumberNode(double value, string text) =>
            new() { IsNumber = true, Number = value, Text = text };

        public static Node OperatorNode(string operation, Node left, Node? right = null) =>
            new() { Text = operation, Left = left, Right = right };
    }

    private ExpressionTree(Node root, string sourcePostfix)
    {
        Root = root;
        SourcePostfix = sourcePostfix;
    }

    public Node Root { get; }

    public string SourcePostfix { get; }

    public static ExpressionTree FromPostfix(string postfix)
    {
        if (!PostfixTokenizer.TryTokenize(postfix, out var tokens, out var error))
        {
            throw new ExpressionTreeException(error);
        }

        var stack = new LinkedStack<Node>();
        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case PostfixTokenKind.Number:
                    stack.Push(Node.NumberNode(token.Number, token.Text));
                    break;
                case PostfixTokenKind.BinaryOperator:
                    if (!stack.TryPop(out var right) || !stack.TryPop(out var left))
                    {
                        throw new ExpressionTreeException(
                            $"Операция '{token.Text}' требует двух операндов.");
                    }

                    stack.Push(Node.OperatorNode(token.Text, left, right));
                    break;
                case PostfixTokenKind.Function:
                case PostfixTokenKind.UnaryOperator:
                    if (!stack.TryPop(out var operand))
                    {
                        throw new ExpressionTreeException(
                            $"Функция/унарная операция '{token.Text}' требует операнда.");
                    }

                    stack.Push(Node.OperatorNode(token.Text, operand));
                    break;
            }
        }

        if (stack.Count != 1)
        {
            throw new ExpressionTreeException(
                stack.Count == 0
                    ? "Выражение не содержит результата."
                    : $"В выражении осталось {stack.Count} независимых поддеревьев.");
        }

        if (!stack.TryPop(out var root))
        {
            throw new ExpressionTreeException("Выражение не содержит результата.");
        }

        return new ExpressionTree(root, string.Join(" ", tokens.Select(token => token.Text)));
    }

    public static ExpressionTree FromInfix(string infix)
    {
        var conversion = ExpressionConverter.Convert(infix);
        if (!conversion.Success || conversion.Postfix is null)
        {
            throw new ExpressionTreeException(conversion.Error ?? "Не удалось преобразовать выражение.");
        }

        return FromPostfix(conversion.Postfix);
    }

    public double Evaluate() => Evaluate(Root);

    public string Inorder() => Inorder(Root);

    public string Preorder() => Preorder(Root);

    public string Postorder() => Postorder(Root);

    private static double Evaluate(Node node)
    {
        if (node.IsNumber)
        {
            return node.Number;
        }

        if (node.Text == "~")
        {
            return -Evaluate(node.Left!);
        }

        if (node.Text is "sqrt" or "sin" or "cos" or "ln")
        {
            var argument = Evaluate(node.Left!);
            var result = node.Text switch
            {
                "sqrt" when argument < 0 => throw new ExpressionTreeException(
                    "sqrt: аргумент не может быть отрицательным."),
                "sqrt" => Math.Sqrt(argument),
                "ln" when argument <= 0 => throw new ExpressionTreeException(
                    "ln: аргумент должен быть больше нуля."),
                "ln" => Math.Log(argument),
                "sin" => Math.Sin(argument),
                "cos" => Math.Cos(argument),
                _ => throw new ExpressionTreeException($"Неизвестная функция '{node.Text}'.")
            };

            return EnsureFinite(result, node.Text);
        }

        var left = Evaluate(node.Left!);
        var right = Evaluate(node.Right!);
        var value = node.Text switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" or ":" when right == 0 => throw new ExpressionTreeException("Деление на ноль."),
            "/" or ":" => left / right,
            "^" => Math.Pow(left, right),
            _ => throw new ExpressionTreeException($"Неизвестная операция '{node.Text}'.")
        };

        return EnsureFinite(value, node.Text);
    }

    private static double EnsureFinite(double value, string operation)
    {
        if (!double.IsFinite(value))
        {
            throw new ExpressionTreeException(
                $"Результат операции '{operation}' не является конечным числом.");
        }

        return value;
    }

    private static string Inorder(Node node)
    {
        if (node.IsNumber)
        {
            return Format(node.Number);
        }

        if (node.Right is null)
        {
            var operand = Inorder(node.Left!);
            return node.Text == "~" ? $"-({operand})" : $"{node.Text}({operand})";
        }

        return $"({Inorder(node.Left!)} {node.Text} {Inorder(node.Right)})";
    }

    private static string Preorder(Node node)
    {
        if (node.IsNumber)
        {
            return Format(node.Number);
        }

        return node.Right is null
            ? $"{node.Text} {Preorder(node.Left!)}"
            : $"{node.Text} {Preorder(node.Left!)} {Preorder(node.Right)}";
    }

    private static string Postorder(Node node)
    {
        if (node.IsNumber)
        {
            return Format(node.Number);
        }

        return node.Right is null
            ? $"{Postorder(node.Left!)} {node.Text}"
            : $"{Postorder(node.Left!)} {Postorder(node.Right)} {node.Text}";
    }

    private static string Format(double value) => value.ToString("G15", CultureInfo.InvariantCulture);
}
