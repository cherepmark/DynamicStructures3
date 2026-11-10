using System.Globalization;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Expressions;

public sealed class ConversionResult
{
    private ConversionResult(bool success, string? postfix, string? error)
    {
        Success = success;
        Postfix = postfix;
        Error = error;
    }

    public bool Success { get; }

    public string? Postfix { get; }

    public string? Error { get; }

    internal static ConversionResult Ok(string postfix) => new(true, postfix, null);

    internal static ConversionResult Fail(string error) => new(false, null, error);
}

internal enum InfixTokenKind
{
    Number,
    BinaryOperator,
    UnaryOperator,
    Function,
    LeftParenthesis,
    RightParenthesis
}

internal readonly record struct InfixToken(string Text, InfixTokenKind Kind, int Position);

/// <summary>
/// Перевод инфиксной записи в постфиксную алгоритмом сортировочной станции.
/// Поддерживает +, -, *, /, :, ^, скобки, функции sin/cos/ln/sqrt и унарный минус.
/// </summary>
public static class ExpressionConverter
{
    private static readonly string[] FunctionNames = ["sqrt", "sin", "cos", "ln"];

    public static ConversionResult Convert(string infix)
    {
        if (string.IsNullOrWhiteSpace(infix))
        {
            return ConversionResult.Fail("Инфиксное выражение пусто.");
        }

        if (!TryTokenize(infix, out var tokens, out var tokenError))
        {
            return ConversionResult.Fail(tokenError);
        }

        var output = new List<string>();
        var operators = new LinkedStack<InfixToken>();
        var expectOperand = true;

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case InfixTokenKind.Number:
                    if (!expectOperand)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: перед числом '{token.Text}' пропущен оператор.");
                    }

                    output.Add(token.Text);
                    expectOperand = false;
                    break;

                case InfixTokenKind.Function:
                    if (!expectOperand)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: перед функцией '{token.Text}' пропущен оператор.");
                    }

                    operators.Push(token);
                    expectOperand = true;
                    break;

                case InfixTokenKind.UnaryOperator:
                    if (!expectOperand)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: унарный оператор '{token.Text}' стоит после операнда.");
                    }

                    operators.Push(token);
                    expectOperand = true;
                    break;

                case InfixTokenKind.LeftParenthesis:
                    if (!expectOperand)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: перед '(' пропущен оператор (неявное умножение не поддерживается).");
                    }

                    operators.Push(token);
                    expectOperand = true;
                    break;

                case InfixTokenKind.RightParenthesis:
                    if (expectOperand)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: пустые скобки или отсутствует операнд перед ')'.");
                    }

                    var foundLeftParenthesis = false;
                    while (operators.Count > 0)
                    {
                        var top = operators.Pop();
                        if (top.Kind == InfixTokenKind.LeftParenthesis)
                        {
                            foundLeftParenthesis = true;
                            break;
                        }

                        output.Add(top.Text);
                    }

                    if (!foundLeftParenthesis)
                    {
                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: несбалансированная закрывающая скобка.");
                    }

                    if (operators.Count > 0 && operators.Top().Kind == InfixTokenKind.Function)
                    {
                        output.Add(operators.Pop().Text);
                    }

                    expectOperand = false;
                    break;

                case InfixTokenKind.BinaryOperator:
                    if (expectOperand)
                    {
                        if (token.Text == "-")
                        {
                            operators.Push(new InfixToken("~", InfixTokenKind.UnaryOperator, token.Position));
                            expectOperand = true;
                            break;
                        }

                        if (token.Text == "+")
                        {
                            expectOperand = true;
                            break;
                        }

                        return ConversionResult.Fail(
                            $"Позиция {token.Position + 1}: оператор '{token.Text}' без левого операнда.");
                    }

                    while (operators.Count > 0)
                    {
                        var top = operators.Top();
                        if (top.Kind == InfixTokenKind.LeftParenthesis)
                        {
                            break;
                        }

                        if (top.Kind == InfixTokenKind.Function ||
                            ShouldPop(top, token))
                        {
                            output.Add(operators.Pop().Text);
                            continue;
                        }

                        break;
                    }

                    operators.Push(token);
                    expectOperand = true;
                    break;
            }
        }

        if (expectOperand)
        {
            return ConversionResult.Fail("Выражение заканчивается оператором, функцией или '(' без операнда.");
        }

        while (operators.Count > 0)
        {
            var top = operators.Pop();
            if (top.Kind == InfixTokenKind.LeftParenthesis)
            {
                return ConversionResult.Fail("Несбалансированная открывающая скобка.");
            }

            output.Add(top.Text);
        }

        var postfix = string.Join(" ", output);
        if (!PostfixCalculator.TryValidateStructure(postfix, out var structureError))
        {
            return ConversionResult.Fail("Некорректное выражение: " + structureError);
        }

        return ConversionResult.Ok(postfix);
    }

    public static bool TryConvert(string infix, out string postfix, out string error)
    {
        var result = Convert(infix);
        postfix = result.Postfix ?? string.Empty;
        error = result.Error ?? string.Empty;
        return result.Success;
    }

    private static bool ShouldPop(InfixToken top, InfixToken current)
    {
        var topPrecedence = Precedence(top);
        var currentPrecedence = Precedence(current);

        if (topPrecedence > currentPrecedence)
        {
            return true;
        }

        if (topPrecedence < currentPrecedence)
        {
            return false;
        }

        return current.Kind != InfixTokenKind.UnaryOperator && current.Text != "^";
    }

    private static int Precedence(InfixToken token)
    {
        if (token.Kind == InfixTokenKind.Function)
        {
            return 5;
        }

        if (token.Kind == InfixTokenKind.UnaryOperator)
        {
            return 3;
        }

        return token.Text switch
        {
            "+" or "-" => 1,
            "*" or "/" or ":" => 2,
            "^" => 4,
            _ => 0
        };
    }

    private static bool TryTokenize(
        string infix,
        out List<InfixToken> tokens,
        out string error)
    {
        tokens = new List<InfixToken>();
        error = string.Empty;
        var index = 0;

        while (index < infix.Length)
        {
            var current = infix[index];

            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (char.IsDigit(current) || (current == '.' && index + 1 < infix.Length && char.IsDigit(infix[index + 1])))
            {
                if (!TryReadNumber(infix, ref index, out var numberText, out error))
                {
                    return false;
                }

                tokens.Add(new InfixToken(numberText, InfixTokenKind.Number, index - numberText.Length));
                continue;
            }

            if (char.IsLetter(current))
            {
                var start = index;
                while (index < infix.Length && char.IsLetter(infix[index]))
                {
                    index++;
                }

                var word = infix[start..index];
                var function = FunctionNames.FirstOrDefault(
                    name => name.Equals(word, StringComparison.OrdinalIgnoreCase));
                if (function is null)
                {
                    error = $"Позиция {start + 1}: неизвестная функция или слово '{word}'.";
                    return false;
                }

                tokens.Add(new InfixToken(function, InfixTokenKind.Function, start));
                continue;
            }

            switch (current)
            {
                case '(':
                    tokens.Add(new InfixToken("(", InfixTokenKind.LeftParenthesis, index));
                    index++;
                    continue;
                case ')':
                    tokens.Add(new InfixToken(")", InfixTokenKind.RightParenthesis, index));
                    index++;
                    continue;
                case '+':
                case '-':
                case '*':
                case '/':
                case ':':
                case '^':
                    tokens.Add(new InfixToken(
                        current.ToString(),
                        InfixTokenKind.BinaryOperator,
                        index));
                    index++;
                    continue;
                case '~':
                    tokens.Add(new InfixToken("~", InfixTokenKind.UnaryOperator, index));
                    index++;
                    continue;
                default:
                    error = $"Позиция {index + 1}: неизвестный символ '{current}'.";
                    return false;
            }
        }

        return true;
    }

    private static bool TryReadNumber(
        string text,
        ref int index,
        out string numberText,
        out string error)
    {
        error = string.Empty;
        var start = index;
        var hasDigits = false;

        while (index < text.Length && char.IsDigit(text[index]))
        {
            index++;
            hasDigits = true;
        }

        if (index < text.Length && text[index] == '.')
        {
            index++;
            while (index < text.Length && char.IsDigit(text[index]))
            {
                index++;
                hasDigits = true;
            }
        }

        if (!hasDigits)
        {
            numberText = string.Empty;
            error = $"Позиция {start + 1}: некорректная запись числа.";
            return false;
        }

        if (index < text.Length && (text[index] == 'e' || text[index] == 'E'))
        {
            index++;
            if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            {
                index++;
            }

            var exponentStart = index;
            while (index < text.Length && char.IsDigit(text[index]))
            {
                index++;
            }

            if (index == exponentStart)
            {
                numberText = string.Empty;
                error = $"Позиция {start + 1}: в экспоненте числа отсутствуют цифры.";
                return false;
            }
        }

        numberText = text[start..index];
        if (!double.TryParse(
                numberText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value) ||
            !double.IsFinite(value))
        {
            error = $"Позиция {start + 1}: некорректное число '{numberText}'.";
            return false;
        }

        return true;
    }
}
