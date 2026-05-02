using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;
using Shouldly;

namespace Bacon.Compiler.Tests;

public partial class ParserTests
{
    [Fact]
    public void Parse_FastDeclaration_ProducesVariableDeclaration()
    {
        var tokens = Lexer.Tokenize("fast x er 5");
        var stmt = Parser.ParseStatementOnly(tokens);

        var varDecl = stmt.ShouldBeOfType<VariableDeclarationStatement>();
        varDecl.Name.ShouldBe("x");
        varDecl.IsImmutable.ShouldBeTrue();
        varDecl.Value.ShouldBeOfType<IntegerLiteralExpression>();
    }

    [Fact]
    public void Parse_OpenDeclaration_HasIsImmutableFalse()
    {
        var tokens = Lexer.Tokenize("åpen y er 10");
        var stmt = Parser.ParseStatementOnly(tokens);

        var varDecl = stmt.ShouldBeOfType<VariableDeclarationStatement>();
        varDecl.IsImmutable.ShouldBeFalse();
    }

    [Fact]
    public void Parse_Assignment_ProducesAssignmentStatement()
    {
        var tokens = Lexer.Tokenize("x er 10");
        var stmt = Parser.ParseStatementOnly(tokens);

        var assign = stmt.ShouldBeOfType<AssignmentStatement>();
        assign.Target.ShouldBeOfType<VariableExpression>();
        assign.Value.ShouldBeOfType<IntegerLiteralExpression>();
    }

    [Fact]
    public void Parse_FieldAssignment_ProducesAssignmentStatement()
    {
        var tokens = Lexer.Tokenize("bil.modell er \"Volvo\"");
        var stmt = Parser.ParseStatementOnly(tokens);

        var assign = stmt.ShouldBeOfType<AssignmentStatement>();
        assign.Target.ShouldBeOfType<FieldAccessExpression>();
    }

    [Fact]
    public void Parse_FunctionCallStatement_ProducesExpressionStatement()
    {
        var tokens = Lexer.Tokenize("foo(1, 2)");
        var stmt = Parser.ParseStatementOnly(tokens);

        var exprStmt = stmt.ShouldBeOfType<ExpressionStatement>();
        exprStmt.Expression.ShouldBeOfType<CallExpression>();
    }

    [Fact]
    public void Parse_IfStatement_ParsesConditionAndBlock()
    {
        var tokens = Lexer.Tokenize("hvis x { fast y er 5 }");
        var stmt = Parser.ParseStatementOnly(tokens);

        var ifStmt = stmt.ShouldBeOfType<IfStatement>();
        ifStmt.Condition.ShouldBeOfType<VariableExpression>();
        ifStmt.ThenBranch.Count.ShouldBe(1);
        ifStmt.ElseBranch.ShouldBeNull();
    }

    [Fact]
    public void Parse_IfElse_ParsesBothBranches()
    {
        var tokens = Lexer.Tokenize("hvis x { } ellers { }");
        var stmt = Parser.ParseStatementOnly(tokens);

        var ifStmt = stmt.ShouldBeOfType<IfStatement>();
        ifStmt.ElseBranch.ShouldNotBeNull();
    }

    [Fact]
    public void Parse_ElseIf_ProducesNestedIf()
    {
        var tokens = Lexer.Tokenize("hvis x { } ellers hvis y { }");
        var stmt = Parser.ParseStatementOnly(tokens);

        var outer = stmt.ShouldBeOfType<IfStatement>();
        outer.ElseBranch.ShouldNotBeNull();
        outer.ElseBranch!.Count.ShouldBe(1);
        outer.ElseBranch[0].ShouldBeOfType<IfStatement>();
    }

    [Fact]
    public void Parse_ForEach_ParsesAllParts()
    {
        var tokens = Lexer.Tokenize("for hver b i biler { }");
        var stmt = Parser.ParseStatementOnly(tokens);

        var forEach = stmt.ShouldBeOfType<ForEachStatement>();
        forEach.Variable.ShouldBe("b");
        forEach.Iterable.ShouldBeOfType<VariableExpression>();
        forEach.Body.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_While_ParsesConditionAndBody()
    {
        var tokens = Lexer.Tokenize("så lenge x { fast y er 1 }");
        var stmt = Parser.ParseStatementOnly(tokens);

        var whileStmt = stmt.ShouldBeOfType<WhileStatement>();
        whileStmt.Condition.ShouldBeOfType<VariableExpression>();
        whileStmt.Body.Count.ShouldBe(1);
    }

    [Fact]
    public void Parse_Return_WithValue_ParsesValue()
    {
        var tokens = Lexer.Tokenize("leverer 5");
        var stmt = Parser.ParseStatementOnly(tokens);

        var ret = stmt.ShouldBeOfType<ReturnStatement>();
        ret.Value.ShouldBeOfType<IntegerLiteralExpression>();
    }

    [Fact]
    public void Parse_Return_WithoutValue_HasNullValue()
    {
        var tokens = Lexer.Tokenize("leverer");
        var stmt = Parser.ParseStatementOnly(tokens);

        var ret = stmt.ShouldBeOfType<ReturnStatement>();
        ret.Value.ShouldBeNull();
    }

    [Fact]
    public void Parse_Throw_ParsesValue()
    {
        var tokens = Lexer.Tokenize("kast \"feil\"");
        var stmt = Parser.ParseStatementOnly(tokens);

        var throwStmt = stmt.ShouldBeOfType<ThrowStatement>();
        throwStmt.Value.ShouldBeOfType<StringLiteralExpression>();
    }
}