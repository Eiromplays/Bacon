using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;
using Shouldly;

namespace Bacon.Compiler.Tests;


public partial class ParserTests
{
    [Fact]
    public void Parse_Integer_ProducesIntegerLiteralExpression()
    {
        var tokens = Lexer.Tokenize("42");
        var expr = Parser.ParseExpressionOnly(tokens);

        var literal = expr.ShouldBeOfType<IntegerLiteralExpression>();
        literal.Value.ShouldBe(42L);
    }

    [Fact]
    public void Parse_Decimal_ProducesDecimalLiteralExpression()
    {
        var tokens = Lexer.Tokenize("3.14");
        var expr = Parser.ParseExpressionOnly(tokens);

        var literal = expr.ShouldBeOfType<DecimalLiteralExpression>();
        literal.Value.ShouldBe(3.14);
    }

    [Fact]
    public void Parse_String_ProducesStringLiteralExpression()
    {
        var tokens = Lexer.Tokenize("\"hello\"");
        var expr = Parser.ParseExpressionOnly(tokens);

        var literal = expr.ShouldBeOfType<StringLiteralExpression>();
        literal.Value.ShouldBe("hello");
    }

    [Fact]
    public void Parse_True_ProducesBooleanLiteralWithTrueValue()
    {
        var tokens = Lexer.Tokenize("sant");
        var expr = Parser.ParseExpressionOnly(tokens);

        var literal = expr.ShouldBeOfType<BooleanLiteralExpression>();
        literal.Value.ShouldBeTrue();
    }

    [Fact]
    public void Parse_False_ProducesBooleanLiteralWithFalseValue()
    {
        var tokens = Lexer.Tokenize("usant");
        var expr = Parser.ParseExpressionOnly(tokens);

        var literal = expr.ShouldBeOfType<BooleanLiteralExpression>();
        literal.Value.ShouldBeFalse();
    }

    [Fact]
    public void Parse_Nothing_ProducesNothingLiteralExpression()
    {
        var tokens = Lexer.Tokenize("ingenting");
        var expr = Parser.ParseExpressionOnly(tokens);

        expr.ShouldBeOfType<NothingLiteralExpression>();
    }

    [Fact]
    public void Parse_Identifier_ProducesVariableExpression()
    {
        var tokens = Lexer.Tokenize("myVar");
        var expr = Parser.ParseExpressionOnly(tokens);

        var variable = expr.ShouldBeOfType<VariableExpression>();
        variable.Name.ShouldBe("myVar");
    }

    [Fact]
    public void Parse_SimpleAddition_ProducesBinaryExpression()
    {
        var tokens = Lexer.Tokenize("5 + 3");
        var expr = Parser.ParseExpressionOnly(tokens);

        var binary = expr.ShouldBeOfType<BinaryExpression>();
        binary.Op.ShouldBe(BinaryOperator.Plus);
        binary.Left.ShouldBeOfType<IntegerLiteralExpression>();
        binary.Right.ShouldBeOfType<IntegerLiteralExpression>();
    }

    [Fact]
    public void Parse_PrecedenceRespected_MultiplicationBindsTighter()
    {
        var tokens = Lexer.Tokenize("5 + 3 * 2");
        var expr = Parser.ParseExpressionOnly(tokens);

        var outer = expr.ShouldBeOfType<BinaryExpression>();
        outer.Op.ShouldBe(BinaryOperator.Plus);
        outer.Left.ShouldBeOfType<IntegerLiteralExpression>();

        var inner = outer.Right.ShouldBeOfType<BinaryExpression>();
        inner.Op.ShouldBe(BinaryOperator.Star);
    }

    [Fact]
    public void Parse_Parens_OverridePrecedence()
    {
        var tokens = Lexer.Tokenize("(5 + 3) * 2");
        var expr = Parser.ParseExpressionOnly(tokens);

        var outer = expr.ShouldBeOfType<BinaryExpression>();
        outer.Op.ShouldBe(BinaryOperator.Star);

        var inner = outer.Left.ShouldBeOfType<BinaryExpression>();
        inner.Op.ShouldBe(BinaryOperator.Plus);
    }

    [Fact]
    public void Parse_Comparison_ProducesBinaryExpression()
    {
        var tokens = Lexer.Tokenize("x større enn 5");
        var expr = Parser.ParseExpressionOnly(tokens);

        var binary = expr.ShouldBeOfType<BinaryExpression>();
        binary.Op.ShouldBe(BinaryOperator.Greater);
    }

    [Fact]
    public void Parse_NotEqual_ProducesBinaryExpression()
    {
        var tokens = Lexer.Tokenize("x er ikke 5");
        var expr = Parser.ParseExpressionOnly(tokens);

        var binary = expr.ShouldBeOfType<BinaryExpression>();
        binary.Op.ShouldBe(BinaryOperator.NotEqual);
    }

    [Fact]
    public void Parse_LogicalAnd_ProducesBinaryExpression()
    {
        var tokens = Lexer.Tokenize("x og y");
        var expr = Parser.ParseExpressionOnly(tokens);

        var binary = expr.ShouldBeOfType<BinaryExpression>();
        binary.Op.ShouldBe(BinaryOperator.And);
    }

    [Fact]
    public void Parse_UnaryNegation_ProducesUnaryExpression()
    {
        var tokens = Lexer.Tokenize("-5");
        var expr = Parser.ParseExpressionOnly(tokens);

        var unary = expr.ShouldBeOfType<UnaryExpression>();
        unary.Op.ShouldBe(UnaryOperator.Negate);
    }

    [Fact]
    public void Parse_UnaryNot_ProducesUnaryExpression()
    {
        var tokens = Lexer.Tokenize("ikke x");
        var expr = Parser.ParseExpressionOnly(tokens);

        var unary = expr.ShouldBeOfType<UnaryExpression>();
        unary.Op.ShouldBe(UnaryOperator.Not);
    }

    [Fact]
    public void Parse_FunctionCall_ProducesCallExpression()
    {
        var tokens = Lexer.Tokenize("foo(1, 2)");
        var expr = Parser.ParseExpressionOnly(tokens);

        var call = expr.ShouldBeOfType<CallExpression>();
        call.Arguments.Count.ShouldBe(2);
        call.Callee.ShouldBeOfType<VariableExpression>();
    }

    [Fact]
    public void Parse_FunctionCallNoArgs_ProducesCallExpressionWithEmptyArgs()
    {
        var tokens = Lexer.Tokenize("foo()");
        var expr = Parser.ParseExpressionOnly(tokens);

        var call = expr.ShouldBeOfType<CallExpression>();
        call.Arguments.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_FieldAccess_ProducesFieldAccessExpression()
    {
        var tokens = Lexer.Tokenize("bil.modell");
        var expr = Parser.ParseExpressionOnly(tokens);

        var access = expr.ShouldBeOfType<FieldAccessExpression>();
        access.FieldName.ShouldBe("modell");
        access.Target.ShouldBeOfType<VariableExpression>();
    }

    [Fact]
    public void Parse_ChainedAccess_ProducesNestedExpression()
    {
        var tokens = Lexer.Tokenize("bil.modell.toUpper()");
        var expr = Parser.ParseExpressionOnly(tokens);

        var call = expr.ShouldBeOfType<CallExpression>();
        var fieldOnField = call.Callee.ShouldBeOfType<FieldAccessExpression>();
        fieldOnField.FieldName.ShouldBe("toUpper");
        fieldOnField.Target.ShouldBeOfType<FieldAccessExpression>();
    }

    [Fact]
    public void Parse_EmptyList_ProducesListExpressionWithNoElements()
    {
        var tokens = Lexer.Tokenize("[]");
        var expr = Parser.ParseExpressionOnly(tokens);

        var list = expr.ShouldBeOfType<ListExpression>();
        list.Elements.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_ListWithElements_CapturesElements()
    {
        var tokens = Lexer.Tokenize("[1, 2, 3]");
        var expr = Parser.ParseExpressionOnly(tokens);

        var list = expr.ShouldBeOfType<ListExpression>();
        list.Elements.Count.ShouldBe(3);
    }
}