using System.Globalization;

namespace DynamicStructures.Expressions;

internal enum PostfixTokenKind
{
    Number,
    BinaryOperator,
    UnaryOperator,
    Function
}

internal readonly record struct PostfixToken(string Text, PostfixTokenKind Kind, double Number = 0);

internal static class PostfixTokenizer
{
    private static readonly string[] Functions = ["sqrt", "sin", "cos", "ln"];
    private static readonly string[] FunctionDisplayNames = ["sqrt", "sin", "cos", "ln"];

    public static bool TryTokenize(
        string expression,
        out List<PostfixToken> tokens,
        out string error)
    {
        tokens = new List<PostfixToken>();
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(expression))
        {
            error = "Выражение пусто.";
            return false;
        }

        var trimmed = expression.Trim();
        var whitespaceMode = trimmed.Any(char.IsWhiteSpace);

        if (whitespaceMode)
        {
            return TryTokenizeWhitespace(trimmed, tokens, out error);
        }

        return TryTokenizeCompact(trimmed, tokens, out error);
    }

    private static bool TryTokenizeWhitespace(
        string expression,
        List<PostfixToken> tokens,
        out string error)
    {
        error = string.Empty;
        var parts = expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (TryReadOperator(part, out var operatorToken))
            {
                tokens.Add(operatorToken);
                continue;
            }

            if (TryReadFunction(part, out var functionToken))
            {
                tokens.Add(functionToken);
                continue;
            }

            if (LooksLikeNumber(part))
            {
                if (!double.TryParse(
                        part,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var number) ||
                    !double.IsFinite(number))
                {
                    error = $"Некорректное число '{part}'. Используйте точку как десятичный разделитель.";
                    return false;
                }

                tokens.Add(new PostfixToken(part, PostfixTokenKind.Number, number));
                continue;
            }

            error = $"Неизвестный токен '{part}'.";
            return false;
        }

        return true;
    }

    private static bool TryTokenizeCompact(
        string expression,
        List<PostfixToken> tokens,
        out string error)
    {
        error = string.Empty;
        var index = 0;

        while (index < expression.Length)
        {
            var current = expression[index];

            if (current is >= '0' and <= '9')
            {
                tokens.Add(new PostfixToken(current.ToString(), PostfixTokenKind.Number, current - '0'));
                index++;
                continue;
            }

            if (current == '.')
            {
                error = "В компактной записи дробные числа не поддерживаются. " +
                        "Разделите числа пробелами, например: 2.5 1.5 +.";
                return false;
            }

            if (char.IsLetter(current))
            {
                var matched = false;
                for (var i = 0; i < Functions.Length; i++)
                {
                    var name = Functions[i];
                    if (index + name.Length <= expression.Length &&
                        expression.AsSpan(index, name.Length).Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        tokens.Add(new PostfixToken(FunctionDisplayNames[i], PostfixTokenKind.Function));
                        index += name.Length;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    error = $"Неизвестная функция или слово, начинающееся с '{current}'.";
                    return false;
                }

                continue;
            }

            if (TryReadOperator(current.ToString(), out var operatorToken))
            {
                tokens.Add(operatorToken);
                index++;
                continue;
            }

            error = $"Неизвестный символ '{current}'.";
            return false;
        }

        return true;
    }

    private static bool LooksLikeNumber(string token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        var first = token[0];
        if (char.IsDigit(first) || first == '.')
        {
            return true;
        }

        return token.Length > 1 && (first == '+' || first == '-');
    }

    private static bool TryReadOperator(string token, out PostfixToken result)
    {
        switch (token)
        {
            case "+":
            case "-":
            case "*":
            case "/":
            case ":":
            case "^":
                result = new PostfixToken(token, PostfixTokenKind.BinaryOperator);
                return true;
            case "~":
                result = new PostfixToken(token, PostfixTokenKind.UnaryOperator);
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static bool TryReadFunction(string token, out PostfixToken result)
    {
        for (var i = 0; i < Functions.Length; i++)
        {
            if (token.Equals(Functions[i], StringComparison.OrdinalIgnoreCase))
            {
                result = new PostfixToken(FunctionDisplayNames[i], PostfixTokenKind.Function);
                return true;
            }
        }

        result = default;
        return false;
    }
}
