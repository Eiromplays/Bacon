using Bacon.Compiler.Ast;
using Bacon.Compiler.Evaluation;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;
using Shouldly;

namespace Bacon.Compiler.Tests;

public class EvaluatorTests
{
    private static BaconValue Eval(string source)
    {
        var tokens = Lexer.Tokenize(source);
        var expr = Parser.ParseExpressionOnly(tokens);
        var evaluator = new Evaluator();
        return evaluator.EvaluateExpression(expr);
    }

    [Fact]
    public void Evaluate_IntegerLiteral_ProducesBaconInteger()
    {
        var result = Eval("42");
        var integer = result.ShouldBeOfType<BaconInteger>();
        integer.Value.ShouldBe(42L);
    }

    [Fact]
    public void Evaluate_StringLiteral_ProducesBaconString()
    {
        var result = Eval("\"hello\"");
        var str = result.ShouldBeOfType<BaconString>();
        str.Value.ShouldBe("hello");
    }

    [Fact]
    public void Evaluate_TrueLiteral_ProducesBaconBoolean()
    {
        var result = Eval("sant");
        var b = result.ShouldBeOfType<BaconBoolean>();
        b.Value.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_NothingLiteral_ProducesBaconNothing()
    {
        var result = Eval("ingenting");
        result.ShouldBeOfType<BaconNothing>();
    }

    [Fact]
    public void Evaluate_IntegerAddition_ProducesIntegerSum()
    {
        var result = Eval("5 + 3");
        var i = result.ShouldBeOfType<BaconInteger>();
        i.Value.ShouldBe(8L);
    }

    [Fact]
    public void Evaluate_PrecedenceRespected()
    {
        var result = Eval("5 + 3 * 2");
        var i = result.ShouldBeOfType<BaconInteger>();
        i.Value.ShouldBe(11L);
    }

    [Fact]
    public void Evaluate_ParensOverridePrecedence()
    {
        var result = Eval("(5 + 3) * 2");
        var i = result.ShouldBeOfType<BaconInteger>();
        i.Value.ShouldBe(16L);
    }

    [Fact]
    public void Evaluate_IntegerPlusDecimal_ProducesDecimal()
    {
        var result = Eval("5 + 3.5");
        var d = result.ShouldBeOfType<BaconDecimal>();
        d.Value.ShouldBe(8.5);
    }

    [Fact]
    public void Evaluate_StringConcatenation_Works()
    {
        var result = Eval("\"hello \" + \"world\"");
        var s = result.ShouldBeOfType<BaconString>();
        s.Value.ShouldBe("hello world");
    }

    [Fact]
    public void Evaluate_AddIntegerAndString_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Eval("5 + \"hello\""));
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Eval("5 / 0"));
    }

    [Fact]
    public void Evaluate_GreaterThan_ProducesBoolean()
    {
        var result = Eval("5 større enn 3");
        var b = result.ShouldBeOfType<BaconBoolean>();
        b.Value.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_LogicalAnd_ProducesBoolean()
    {
        var result = Eval("sant og usant");
        var b = result.ShouldBeOfType<BaconBoolean>();
        b.Value.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_NegationOfInteger_Works()
    {
        var result = Eval("-5");
        var i = result.ShouldBeOfType<BaconInteger>();
        i.Value.ShouldBe(-5L);
    }

    [Fact]
    public void Evaluate_NotOfBoolean_Works()
    {
        var result = Eval("ikke sant");
        var b = result.ShouldBeOfType<BaconBoolean>();
        b.Value.ShouldBeFalse();
    }


    private static (Evaluator evaluator, BaconValue? lastValue) EvalProgram(string source)
    {
        var tokens = Lexer.Tokenize(source);

        // Parse as statements ("programmer" at top level is statements for now)
        // We simulate by parsing a "program" and evaluating statements directly
        var evaluator = new Evaluator();

        // Workaround: wrap source in a process to get statements
        var wrapped = $"prosess test() {{ {source} }}";
        var wrappedTokens = Lexer.Tokenize(wrapped);
        var program = Parser.Parse(wrappedTokens);
        var processDecl = (Bacon.Compiler.Ast.ProcessDeclaration)program.Declarations[0];

        evaluator.EvaluateStatements(processDecl.Body);
        return (evaluator, null);
    }

    [Fact]
    public void Evaluate_VariableDeclaration_BindsValue()
    {
        var (eval, _) = EvalProgram("fast x er 5");

        // Verify x is bound by using EvaluateExpression
        var result = eval.EvaluateExpression(new VariableExpression("x", 1));
        var i = result.ShouldBeOfType<BaconInteger>();
        i.Value.ShouldBe(5L);
    }

    [Fact]
    public void Evaluate_VariableUsedInExpression_RetrievesValue()
    {
        var (eval, _) = EvalProgram("fast x er 5\nfast y er x + 3");

        var result = eval.EvaluateExpression(new VariableExpression("y", 1));
        result.ShouldBeOfType<BaconInteger>().Value.ShouldBe(8L);
    }

    [Fact]
    public void Evaluate_Assignment_UpdatesValue()
    {
        var (eval, _) = EvalProgram("åpen x er 5\nx er 10");

        var result = eval.EvaluateExpression(new VariableExpression("x", 1));
        result.ShouldBeOfType<BaconInteger>().Value.ShouldBe(10L);
    }

    [Fact]
    public void Evaluate_AssignToImmutable_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => EvalProgram("fast x er 5\nx er 10"));
    }

    [Fact]
    public void Evaluate_If_TakesThenBranch()
    {
        var (eval, _) = EvalProgram("åpen result er 0\nhvis sant { result er 1 }");

        var r = eval.EvaluateExpression(new VariableExpression("result", 1));
        r.ShouldBeOfType<BaconInteger>().Value.ShouldBe(1L);
    }

    [Fact]
    public void Evaluate_If_TakesElseBranch()
    {
        var (eval, _) = EvalProgram("""
            åpen result er 0
            hvis usant {
                result er 1
            } ellers {
                result er 2
            }
            """);

        var r = eval.EvaluateExpression(new VariableExpression("result", 1));
        r.ShouldBeOfType<BaconInteger>().Value.ShouldBe(2L);
    }

    [Fact]
    public void Evaluate_While_LoopsCorrectNumberOfTimes()
    {
        var (eval, _) = EvalProgram("""
            åpen i er 0
            så lenge i mindre enn 5 {
                i er i + 1
            }
            """);

        var r = eval.EvaluateExpression(new VariableExpression("i", 1));
        r.ShouldBeOfType<BaconInteger>().Value.ShouldBe(5L);
    }

    [Fact]
    public void Evaluate_ForEach_IteratesAllElements()
    {
        var (eval, _) = EvalProgram("""
            åpen sum er 0
            for hver x i [1, 2, 3, 4, 5] {
                sum er sum + x
            }
            """);

        var r = eval.EvaluateExpression(new VariableExpression("sum", 1));
        r.ShouldBeOfType<BaconInteger>().Value.ShouldBe(15L);
    }

    [Fact]
    public void Evaluate_Throw_RaisesRuntimeException()
    {
        Should.Throw<RuntimeException>(() => EvalProgram("kast \"feil\""));
    }

    private static BaconValue Run(string source)
    {
        var tokens = Lexer.Tokenize(source);
        var program = Parser.Parse(tokens);
        return Evaluator.Evaluate(program);
    }

    [Fact]
    public void Evaluate_HovedprogramReturnsValue_ReturnsThatValue()
    {
        var result = Run("""
            prosess hovedprogram() {
                leverer 42
            }
            """);

        result.ShouldBeOfType<BaconInteger>().Value.ShouldBe(42L);
    }

    [Fact]
    public void Evaluate_ProcessCall_PassesArgumentsCorrectly()
    {
        var result = Run("""
            prosess summer(a, b) {
                leverer a + b
            }
            prosess hovedprogram() {
                leverer summer(5, 3)
            }
            """);

        result.ShouldBeOfType<BaconInteger>().Value.ShouldBe(8L);
    }

    [Fact]
    public void Evaluate_RecursiveProcess_Works()
    {
        var result = Run("""
            prosess fakultet(n) {
                hvis n mindre eller lik 1 {
                    leverer 1
                }
                leverer n * fakultet(n - 1)
            }
            prosess hovedprogram() {
                leverer fakultet(5)
            }
            """);

        result.ShouldBeOfType<BaconInteger>().Value.ShouldBe(120L);
    }

    [Fact]
    public void Evaluate_ProcessLocalScope_DoesNotLeakOutside()
    {
        // x in hjelp() should not be accessible in hovedprogram
        Should.Throw<RuntimeException>(() => Run("""
            prosess hjelp() {
                fast x er 5
            }
            prosess hovedprogram() {
                hjelp()
                leverer x
            }
            """));
    }

    [Fact]
    public void Evaluate_NoHovedprogram_ReturnsNothing()
    {
        var result = Run("""
            prosess foo() {
                leverer 5
            }
            """);

        result.ShouldBeOfType<BaconNothing>();
    }

    [Fact]
    public void Evaluate_ProcessWrongArgCount_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
            prosess summer(a, b) {
                leverer a + b
            }
            prosess hovedprogram() {
                leverer summer(5)
            }
            """));
    }

    [Fact]
    public void Evaluate_BesetningInstantiation_CreatesInstance()
    {
        var result = Run("""
            besetning Bil {
                fast id : tekst
                åpen modell : tekst
            }
            prosess hovedprogram() {
                fast b er Bil("1", "Volvo")
                leverer b
            }
            """);

        var instance = result.ShouldBeOfType<BaconBesetningInstance>();
        instance.TypeName.ShouldBe("Bil");
        instance.Fields["id"].ShouldBeOfType<BaconString>().Value.ShouldBe("1");
        instance.Fields["modell"].ShouldBeOfType<BaconString>().Value.ShouldBe("Volvo");
    }

    [Fact]
    public void Evaluate_FieldAccess_ReadsField()
    {
        var result = Run("""
            besetning Bil {
                fast id : tekst
            }
            prosess hovedprogram() {
                fast b er Bil("ABC123")
                leverer b.id
            }
            """);

        result.ShouldBeOfType<BaconString>().Value.ShouldBe("ABC123");
    }

    [Fact]
    public void Evaluate_FieldAssignment_UpdatesField()
    {
        var result = Run("""
            besetning Bil {
                åpen modell : tekst
            }
            prosess hovedprogram() {
                fast b er Bil("Volvo")
                b.modell er "Tesla"
                leverer b.modell
            }
            """);

        result.ShouldBeOfType<BaconString>().Value.ShouldBe("Tesla");
    }

    [Fact]
    public void Evaluate_WrongArgCount_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
            besetning Bil {
                fast id : tekst
                fast modell : tekst
            }
            prosess hovedprogram() {
                leverer Bil("1")
            }
            """));
    }

    [Fact]
    public void Evaluate_UnknownField_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
            besetning Bil {
                fast id : tekst
            }
            prosess hovedprogram() {
                fast b er Bil("1")
                leverer b.modell
            }
            """));
    }

    [Fact]
    public void Evaluate_FieldAccessOnNonBesetning_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
            prosess hovedprogram() {
                fast x er 5
                leverer x.modell
            }
            """));
    }

    [Fact]
    public void Evaluate_ReassignFastField_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
                                                 besetning Bil {
                                                     fast id : tekst
                                                     åpen modell : tekst
                                                 }
                                                 prosess hovedprogram() {
                                                     fast b er Bil("1", "Volvo")
                                                     b.id er "2"
                                                 }
                                                 """));
    }

    [Fact]
    public void Evaluate_ReassignOpenField_Works()
    {
        var result = Run("""
                         besetning Bil {
                             fast id : tekst
                             åpen modell : tekst
                         }
                         prosess hovedprogram() {
                             fast b er Bil("1", "Volvo")
                             b.modell er "Tesla"
                             leverer b.modell
                         }
                         """);

        result.ShouldBeOfType<BaconString>().Value.ShouldBe("Tesla");
    }

    [Fact]
    public void Evaluate_AssignNonexistentField_ThrowsRuntimeException()
    {
        Should.Throw<RuntimeException>(() => Run("""
                                                 besetning Bil {
                                                     fast id : tekst
                                                 }
                                                 prosess hovedprogram() {
                                                     fast b er Bil("1")
                                                     b.fart er 100
                                                 }
                                                 """));
    }
}