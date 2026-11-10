using DynamicStructures.Expressions;

namespace DynamicStructures.Tests;

public class ExpressionConverterTests
{
    [Theory]
    [InlineData("2+2", "2 2 +")]
    [InlineData("(2+3)*4", "2 3 + 4 *")]
    [InlineData("2+3*4", "2 3 4 * +")]
    [InlineData("2*3+4", "2 3 * 4 +")]
    [InlineData("2^3^2", "2 3 2 ^ ^")]
    [InlineData("8-3-2", "8 3 - 2 -")]
    [InlineData("8/4/2", "8 4 / 2 /")]
    [InlineData("8:4:2", "8 4 : 2 :")]
    [InlineData("sin(0)", "0 sin")]
    [InlineData("sqrt(9)", "9 sqrt")]
    [InlineData("ln(1)+sqrt(9)", "1 ln 9 sqrt +")]
    [InlineData("((2+3)*4)^2", "2 3 + 4 * 2 ^")]
    [InlineData("sin(2+3)", "2 3 + sin")]
    [InlineData("2 * (3 + 4)", "2 3 4 + *")]
    [InlineData("SIN(0)", "0 sin")]
    [InlineData("sqrt 9", "9 sqrt")]
    [InlineData("sin cos 0", "0 cos sin")]
    [InlineData("2 + 3 * 4", "2 3 4 * +")]
    [InlineData(".5+1", ".5 1 +")]
    [InlineData("1e3+2", "1e3 2 +")]
    [InlineData("2++3", "2 3 +")]
    public void ConvertsInfixToPostfix(string infix, string expected)
    {
        var result = ExpressionConverter.Convert(infix);

        Assert.True(result.Success, result.Error);
        Assert.Equal(expected, result.Postfix);
    }

    [Theory]
    [InlineData("-2^2", "2 2 ^ ~", -4)]
    [InlineData("(-2)^2", "2 ~ 2 ^", 4)]
    [InlineData("2*-3", "2 3 ~ *", -6)]
    [InlineData("2+-3", "2 3 ~ +", -1)]
    [InlineData("2--3", "2 3 ~ -", 5)]
    [InlineData("2^-3", "2 3 ~ ^", 0.125)]
    [InlineData("-(2+3)", "2 3 + ~", -5)]
    [InlineData("2 ^ 3 ^ 2", "2 3 2 ^ ^", 512)]
    [InlineData("sin(0)+sqrt(9)", "0 sin 9 sqrt +", 3)]
    [InlineData("ln(1)+sqrt(9)", "1 ln 9 sqrt +", 3)]
    [InlineData("2+3*4", "2 3 4 * +", 14)]
    [InlineData("(1+2)*(3+4)/7", "1 2 + 3 4 + * 7 /", 3)]
    public void ConvertedExpression_EvaluatesCorrectly(string infix, string expectedPostfix, double expectedValue)
    {
        var conversion = ExpressionConverter.Convert(infix);
        Assert.True(conversion.Success, conversion.Error);
        Assert.Equal(expectedPostfix, conversion.Postfix);

        var evaluation = PostfixCalculator.Evaluate(conversion.Postfix!);
        Assert.True(evaluation.Success, evaluation.Error);
        Assert.Equal(expectedValue, evaluation.Value, precision: 12);
    }

    [Fact]
    public void ConversionDoesNotEvaluateDomainRestrictions()
    {
        var conversion = ExpressionConverter.Convert("sqrt(-1)");
        Assert.True(conversion.Success);
        Assert.Equal("1 ~ sqrt", conversion.Postfix);

        var evaluation = PostfixCalculator.Evaluate(conversion.Postfix!);
        Assert.False(evaluation.Success);
        Assert.Contains("sqrt", evaluation.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("(2+3")]
    [InlineData("2+3)")]
    [InlineData("2+")]
    [InlineData("*2")]
    [InlineData("2 3")]
    [InlineData("sin()")]
    [InlineData("()")]
    [InlineData("sin")]
    [InlineData("foo(2)")]
    [InlineData("2%3")]
    [InlineData("2(3+4)")]
    [InlineData("2+*3")]
    [InlineData("2//3")]
    [InlineData("(2+3))")]
    [InlineData("-")]
    [InlineData("2^")]
    [InlineData("2..3")]
    [InlineData("1e+ 2 +")]
    [InlineData("{2}")]
    public void InvalidInfix_IsRejected(string infix)
    {
        var result = ExpressionConverter.Convert(infix);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.Null(result.Postfix);
    }

    [Fact]
    public void TryConvert_ProvidesOutputs()
    {
        Assert.True(ExpressionConverter.TryConvert("2+2", out var postfix, out var error));
        Assert.Equal("2 2 +", postfix);
        Assert.Empty(error);

        Assert.False(ExpressionConverter.TryConvert("2+", out _, out error));
        Assert.NotEmpty(error);
    }
}
