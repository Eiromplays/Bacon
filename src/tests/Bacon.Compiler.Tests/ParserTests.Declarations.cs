using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;
using Shouldly;

namespace Bacon.Compiler.Tests;

public partial class ParserTests
{
    [Fact]
    public void Parse_SimpleProcess_ProducesProcessDeclaration()
    {
        var tokens = Lexer.Tokenize("prosess hei() { leverer 5 }");
        var program = Parser.Parse(tokens);

        program.Declarations.Count.ShouldBe(1);
        var process = program.Declarations[0].ShouldBeOfType<ProcessDeclaration>();
        process.Name.ShouldBe("hei");
        process.Parameters.Count.ShouldBe(0);
        process.Body.Count.ShouldBe(1);
    }

    [Fact]
    public void Parse_ProcessWithParameters_ParsesAllParts()
    {
        var tokens = Lexer.Tokenize("prosess summer(a, b) { leverer a + b }");
        var program = Parser.Parse(tokens);

        var process = program.Declarations[0].ShouldBeOfType<ProcessDeclaration>();
        process.Parameters.Count.ShouldBe(2);
        process.Parameters[0].Name.ShouldBe("a");
        process.Parameters[1].Name.ShouldBe("b");
    }

    [Fact]
    public void Parse_ProcessWithTypedParameters_CapturesTypes()
    {
        var tokens = Lexer.Tokenize("prosess summer(a : heltall, b : heltall) { }");
        var program = Parser.Parse(tokens);

        var process = program.Declarations[0].ShouldBeOfType<ProcessDeclaration>();
        process.Parameters[0].TypeName.ShouldBe("heltall");
        process.Parameters[1].TypeName.ShouldBe("heltall");
    }

    [Fact]
    public void Parse_ProcessWithUntypedParameters_HasNullTypes()
    {
        var tokens = Lexer.Tokenize("prosess foo(a, b) { }");
        var program = Parser.Parse(tokens);

        var process = program.Declarations[0].ShouldBeOfType<ProcessDeclaration>();
        process.Parameters[0].TypeName.ShouldBeNull();
    }

    [Fact]
    public void Parse_Besetning_ParsesFields()
    {
        var tokens = Lexer.Tokenize("""
            besetning Bil {
                fast id : tekst
                åpen modell : tekst
            }
            """);
        var program = Parser.Parse(tokens);

        var besetning = program.Declarations[0].ShouldBeOfType<BesetningDeclaration>();
        besetning.Name.ShouldBe("Bil");
        besetning.Fields.Count.ShouldBe(2);
        besetning.Fields[0].Name.ShouldBe("id");
        besetning.Fields[0].IsImmutable.ShouldBeTrue();
        besetning.Fields[1].IsImmutable.ShouldBeFalse();
    }

    [Fact]
    public void Parse_BesetningField_HasCorrectTypeName()
    {
        var tokens = Lexer.Tokenize("""
            besetning Bil {
                fast id : tekst
            }
            """);
        var program = Parser.Parse(tokens);

        var besetning = program.Declarations[0].ShouldBeOfType<BesetningDeclaration>();
        besetning.Fields[0].TypeName.ShouldBe("tekst");
    }

    [Fact]
    public void Parse_BesetningWithCustomType_AcceptsIdentifierAsType()
    {
        var tokens = Lexer.Tokenize("""
            besetning Garage {
                fast bil : Bil
            }
            """);
        var program = Parser.Parse(tokens);

        var besetning = program.Declarations[0].ShouldBeOfType<BesetningDeclaration>();
        besetning.Fields[0].TypeName.ShouldBe("Bil");
    }

    [Fact]
    public void Parse_Import_ParsesPath()
    {
        var tokens = Lexer.Tokenize("import drift.tjener");
        var program = Parser.Parse(tokens);

        var import = program.Declarations[0].ShouldBeOfType<ImportDeclaration>();
        import.Path.ShouldBe("drift.tjener");
        import.Alias.ShouldBeNull();
    }

    [Fact]
    public void Parse_ImportWithAlias_CapturesAlias()
    {
        var tokens = Lexer.Tokenize("import drift.tjener som t");
        var program = Parser.Parse(tokens);

        var import = program.Declarations[0].ShouldBeOfType<ImportDeclaration>();
        import.Alias.ShouldBe("t");
    }

    [Fact]
    public void Parse_GetRoute_ParsesMethodAndPath()
    {
        var tokens = Lexer.Tokenize("""
            rute GET "/bil/{id}" {
                leverer 5
            }
            """);
        var program = Parser.Parse(tokens);

        var route = program.Declarations[0].ShouldBeOfType<RouteDeclaration>();
        route.HttpMethod.ShouldBe("GET");
        route.Path.ShouldBe("/bil/{id}");
        route.Body.ShouldBeNull();
    }

    [Fact]
    public void Parse_PostRouteWithBody_ParsesBodyBinding()
    {
        var tokens = Lexer.Tokenize("""
            rute POST "/bil" mottar data : NyBil {
            }
            """);
        var program = Parser.Parse(tokens);

        var route = program.Declarations[0].ShouldBeOfType<RouteDeclaration>();
        route.Body.ShouldNotBeNull();
        route.Body!.Name.ShouldBe("data");
        route.Body.TypeName.ShouldBe("NyBil");
    }

    [Fact]
    public void Parse_FullProgram_ParsesAllDeclarations()
    {
        var source = """
            import drift.tjener

            besetning Bil {
                fast id : tekst
            }

            prosess summer(a, b) {
                leverer a + b
            }

            rute GET "/bil" {
                leverer 5
            }
            """;

        var tokens = Lexer.Tokenize(source);
        var program = Parser.Parse(tokens);

        program.Declarations.Count.ShouldBe(4);
        program.Declarations[0].ShouldBeOfType<ImportDeclaration>();
        program.Declarations[1].ShouldBeOfType<BesetningDeclaration>();
        program.Declarations[2].ShouldBeOfType<ProcessDeclaration>();
        program.Declarations[3].ShouldBeOfType<RouteDeclaration>();
    }
}