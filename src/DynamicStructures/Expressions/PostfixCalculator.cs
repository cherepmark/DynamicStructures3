using System.Globalization;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Expressions;

public sealed class PostfixEvaluationResult
{
    private PostfixEvaluationResult(
        bool success,
        double value,
        string normalizedPostfix,
        IReadOnlyList<string> tokens,
        string? error)
    {
        Success = success;
        Value = value;
        NormalizedPostfix = normalizedPostfix;
        Tokens = tokens;
        Error = error;
    }

    public bool Success { get; }

    public double Value { get; }

    public string NormalizedPostfix { get; }

    public IReadOnlyList<string> Tokens { get; }

    public string? Error { get; }

    internal static PostfixEvaluationResult Ok(double value, string normalized, IReadOnlyList<string> tokens) =>
        new(true, value, normalized, tokens, null);

    internal static PostfixEvaluationResult Fail(string error, string normalized = "", IReadOnlyList<string>? tokens = null) =>
        new(false, 0, normalized, tokens ?? Array.Empty<string>(), error);
}

/// <summary>Вычисление постфиксных выражений с помощью собственного стека.</summary>
public static class PostfixCalculator
{
    public static PostfixEvaluationResult Evaluate(string expression)
    {
        if (!PostfixTokenizer.TryTokenize(expression, out var tokens, out var tokenError))
        {
            return PostfixEvaluationResult.Fail(tokenError);
        }

        var normalized = string.Join(" ", tokens.Select(token => token.Text));
        var displayTokens = tokens.Select(token => token.Text).ToArray();
        var stack = new LinkedStack<double>();

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == PostfixTokenKind.Number)
            {
                stack.Push(token.Number);
                continue;
            }

            if (token.Kind == PostfixTokenKind.Function || token.Kind == PostfixTokenKind.UnaryOperator)
            {
                if (!stack.TryPop(out var argument))
                {
                    return PostfixEvaluationResult.Fail(
                        $"Токен {index + 1} '{token.Text}': недостаточно операндов.",
                        normalized,
                        displayTokens);
                }

                if (!TryApplyUnary(token, argument, out var unaryResult, out var unaryError))
                {
                    return PostfixEvaluationResult.Fail(
                        $"Токен {index + 1} '{token.Text}': {unaryError}",
                        normalized,
                        displayTokens);
                }

                stack.Push(unaryResult);
                continue;
            }

            if (!stack.TryPop(out var right))
            {
                return PostfixEvaluationResult.Fail(
                    $"Токен {index + 1} '{token.Text}': недостаточно операндов.",
                    normalized,
                    displayTokens);
            }

            if (!stack.TryPop(out var left))
            {
                return PostfixEvaluationResult.Fail(
                    $"Токен {index + 1} '{token.Text}': недостаточно операндов.",
                    normalized,
                    displayTokens);
            }

            if (!TryApplyBinary(token.Text, left, right, out var binaryResult, out var binaryError))
            {
                return PostfixEvaluationResult.Fail(
                    $"Токен {index + 1} '{token.Text}': {binaryError}",
                    normalized,
                    displayTokens);
            }

            stack.Push(binaryResult);
        }

        if (stack.Count == 0)
        {
            return PostfixEvaluationResult.Fail("Выражение не содержит результата.", normalized, displayTokens);
        }

        if (stack.Count > 1)
        {
            return PostfixEvaluationResult.Fail(
                $"После вычисления осталось {stack.Count} значений вместо одного. Проверьте количество операций.",
                normalized,
                displayTokens);
        }

        stack.TryPop(out var value);
        return PostfixEvaluationResult.Ok(value, normalized, displayTokens);
    }

    public static bool TryEvaluate(string expression, out double value, out string error)
    {
        var result = Evaluate(expression);
        value = result.Value;
        error = result.Error ?? string.Empty;
        return result.Success;
    }

    /// <summary>Проверяет только структуру (число операндов), не вычисляя значения.</summary>
    public static bool TryValidateStructure(string expression, out string error)
    {
        if (!PostfixTokenizer.TryTokenize(expression, out var tokens, out var tokenError))
        {
            error = tokenError;
            return false;
        }

        var operands = 0;
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            switch (token.Kind)
            {
                case PostfixTokenKind.Number:
                    operands++;
                    break;
                case PostfixTokenKind.Function:
                case PostfixTokenKind.UnaryOperator:
                    if (operands < 1)
                    {
                        error = $"Токен {index + 1} '{token.Text}': недостаточно операндов.";
                        return false;
                    }

                    break;
                case PostfixTokenKind.BinaryOperator:
                    if (operands < 2)
                    {
                        error = $"Токен {index + 1} '{token.Text}': недостаточно операндов.";
                        return false;
                    }

                    operands--;
                    break;
            }
        }

        if (operands != 1)
        {
            error = operands == 0
                ? "Выражение не содержит результата."
                : $"После вычисления осталось бы {operands} значений вместо одного.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryApplyBinary(
        string operation,
        double left,
        double right,
        out double result,
        out string error)
    {
        error = string.Empty;
        switch (operation)
        {
            case "+":
                result = left + right;
                break;
            case "-":
                result = left - right;
                break;
            case "*":
                result = left * right;
                break;
            case "/":
            case ":":
                if (right == 0.0)
                {
                    result = 0;
                    error = "деление на ноль.";
                    return false;
                }

                result = left / right;
                break;
            case "^":
                result = Math.Pow(left, right);
                break;
            default:
                result = 0;
                error = "неизвестная операция.";
                return false;
        }

        if (!double.IsFinite(result))
        {
            error = "результат не является конечным числом (переполнение).";
            return false;
        }

        return true;
    }

    private static bool TryApplyUnary(
        PostfixToken token,
        double argument,
        out double result,
        out string error)
    {
        error = string.Empty;
        switch (token.Kind == PostfixTokenKind.UnaryOperator ? "~" : token.Text)
        {
            case "~":
                result = -argument;
                break;
            case "sqrt":
                if (argument < 0)
                {
                    result = 0;
                    error = "sqrt: аргумент не может быть отрицательным.";
                    return false;
                }

                result = Math.Sqrt(argument);
                break;
            case "ln":
                if (argument <= 0)
                {
                    result = 0;
                    error = "ln: аргумент должен быть больше нуля.";
                    return false;
                }

                result = Math.Log(argument);
                break;
            case "sin":
                result = Math.Sin(argument);
                break;
            case "cos":
                result = Math.Cos(argument);
                break;
            default:
                result = 0;
                error = "неизвестная функция.";
                return false;
        }

        if (!double.IsFinite(result))
        {
            error = "результат не является конечным числом.";
            return false;
        }

        return true;
    }

    public static string FormatValue(double value) => value.ToString("G15", CultureInfo.InvariantCulture);
}
