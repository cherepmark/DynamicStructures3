using DynamicStructures.Expressions;

namespace DynamicStructures.Tests;

public class PostfixCalculatorTests
{
    [Theory]
    [InlineData("78+32+/", 3)]
    [InlineData("22+", 4)]
    [InlineData("2 3 + 4 *", 20)]
    [InlineData("10 20 +", 30)]
    [InlineData("2.5 1.5 +", 4)]
    [InlineData("15 3 :", 5)]
    [InlineData("15 3 /", 5)]
    [InlineData("2 3 ^", 8)]
    [InlineData("2 3 ^ 2 *", 16)]
    [InlineData("9 sqrt", 3)]
    [InlineData("0 sin", 0)]
    [InlineData("0 cos", 1)]
    [InlineData("1 ln", 0)]
    [InlineData("2 ~", -2)]
    [InlineData("2 3 ~ ^", 0.125)]
    [InlineData("9sqrt", 3)]
    [InlineData("0sin", 0)]
    [InlineData("1e3 2 +", 1002)]
    [InlineData("  7   8 +  ", 15)]
    [InlineData("2 3 - 4 *", -4)]
    [InlineData("-5 2 +", -3)]
    public void ValidExpressions_AreEvaluated(string expression, double expected)
    {
        var result = PostfixCalculator.Evaluate(expression);

        Assert.True(result.Success, result.Error);
        Assert.Equal(expected, result.Value, precision: 12);
    }

    [Fact]
    public void CompactExpression_IsNormalizedForDisplay()
    {
        var result = PostfixCalculator.Evaluate("78+32+/");

        Assert.True(result.Success);
        Assert.Equal("7 8 + 3 2 + /", result.NormalizedPostfix);
        Assert.Equal(7, result.Tokens.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2 +")]
    [InlineData("2 3 + +")]
    [InlineData("1 2")]
    [InlineData("5 0 /")]
    [InlineData("5 0 :")]
    [InlineData("-1 sqrt")]
    [InlineData("0 ln")]
    [InlineData("2 3 &")]
    [InlineData("1.2.3 4 +")]
    [InlineData("1e999 1 +")]
    [InlineData("2.5+1")]
    [InlineData("2 3 ^ 4 5")]
    public void InvalidExpressions_ReturnErrors(string expression)
    {
        var result = PostfixCalculator.Evaluate(expression);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Theory]
    [InlineData("2 3 +", true)]
    [InlineData("9 sqrt", true)]
    [InlineData("2 3 ~ ^", true)]
    [InlineData("2 +", false)]
    [InlineData("2 3", false)]
    [InlineData("2 3 + +", false)]
    public void StructureValidation_Works(string expression, bool expected)
    {
        var valid = PostfixCalculator.TryValidateStructure(expression, out var error);

        Assert.Equal(expected, valid);
        if (!expected)
        {
            Assert.NotEmpty(error);
        }
    }

    [Fact]
    public void TryEvaluate_ProvidesErrorWithoutThrowing()
    {
        var success = PostfixCalculator.TryEvaluate("3 4 +", out var value, out var error);

        Assert.True(success);
        Assert.Equal(7, value);
        Assert.Empty(error);

        Assert.False(PostfixCalculator.TryEvaluate("3 +", out _, out error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void FormatValue_UsesInvariantSeparator()
    {
        Assert.Equal("2.5", PostfixCalculator.FormatValue(2.5));
        Assert.Equal("3", PostfixCalculator.FormatValue(3.0));
    }
}
