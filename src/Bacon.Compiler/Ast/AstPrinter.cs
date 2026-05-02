using System.Text;

namespace Bacon.Compiler.Ast;

public static class AstPrinter
{
    public static string Print(AstNode node)
    {
        var sb = new StringBuilder();
        PrintNode(node, sb, 0);
        return sb.ToString();
    }

    private static void PrintNode(AstNode node, StringBuilder sb, int indent)
    {
        switch (node)
        {
            // Program
            case Program p:
                WriteLine(sb, indent, "Program");
                foreach (var decl in p.Declarations)
                    PrintNode(decl, sb, indent + 1);
                break;

            // Declarations
            case ImportDeclaration imp:
                var alias = imp.Alias != null ? $" as {imp.Alias}" : "";
                WriteLine(sb, indent, $"Import \"{imp.Path}\"{alias}");
                break;

            case BesetningDeclaration bes:
                WriteLine(sb, indent, $"Besetning \"{bes.Name}\"");
                foreach (var field in bes.Fields)
                    PrintNode(field, sb, indent + 1);
                break;

            case FieldDeclaration field:
                var mutability = field.IsImmutable ? "fast" : "open";
                WriteLine(sb, indent, $"Field \"{field.Name}\" : {field.TypeName} ({mutability})");
                break;

            case ProcessDeclaration proc:
                WriteLine(sb, indent, $"Process \"{proc.Name}\"");
                foreach (var param in proc.Parameters)
                    PrintNode(param, sb, indent + 1);
                WriteLine(sb, indent + 1, "Body");
                foreach (var stmt in proc.Body)
                    PrintNode(stmt, sb, indent + 2);
                break;

            case ParameterDeclaration param:
                var type = param.TypeName != null ? $" : {param.TypeName}" : "";
                WriteLine(sb, indent, $"Parameter \"{param.Name}\"{type}");
                break;

            case RouteDeclaration route:
                WriteLine(sb, indent, $"Route {route.HttpMethod} \"{route.Path}\"");
                if (route.Body != null)
                    PrintNode(route.Body, sb, indent + 1);
                WriteLine(sb, indent + 1, "Statements");
                foreach (var stmt in route.Statements)
                    PrintNode(stmt, sb, indent + 2);
                break;

            case BodyBinding binding:
                WriteLine(sb, indent, $"BodyBinding \"{binding.Name}\" : {binding.TypeName}");
                break;

            // Statements
            case VariableDeclarationStatement vd:
                var mutWord = vd.IsImmutable ? "fast" : "open";
                WriteLine(sb, indent, $"VariableDeclaration \"{vd.Name}\" ({mutWord})");
                PrintNode(vd.Value, sb, indent + 1);
                break;

            case AssignmentStatement assign:
                WriteLine(sb, indent, "Assignment");
                PrintNode(assign.Target, sb, indent + 1);
                PrintNode(assign.Value, sb, indent + 1);
                break;

            case ExpressionStatement exprStmt:
                WriteLine(sb, indent, "ExpressionStatement");
                PrintNode(exprStmt.Expression, sb, indent + 1);
                break;

            case IfStatement ifStmt:
                WriteLine(sb, indent, "If");
                WriteLine(sb, indent + 1, "Condition");
                PrintNode(ifStmt.Condition, sb, indent + 2);
                WriteLine(sb, indent + 1, "Then");
                foreach (var s in ifStmt.ThenBranch)
                    PrintNode(s, sb, indent + 2);
                if (ifStmt.ElseBranch != null)
                {
                    WriteLine(sb, indent + 1, "Else");
                    foreach (var s in ifStmt.ElseBranch)
                        PrintNode(s, sb, indent + 2);
                }
                break;

            case ForEachStatement forEach:
                WriteLine(sb, indent, $"ForEach \"{forEach.Variable}\"");
                WriteLine(sb, indent + 1, "Iterable");
                PrintNode(forEach.Iterable, sb, indent + 2);
                WriteLine(sb, indent + 1, "Body");
                foreach (var s in forEach.Body)
                    PrintNode(s, sb, indent + 2);
                break;

            case WhileStatement whileStmt:
                WriteLine(sb, indent, "While");
                WriteLine(sb, indent + 1, "Condition");
                PrintNode(whileStmt.Condition, sb, indent + 2);
                WriteLine(sb, indent + 1, "Body");
                foreach (var s in whileStmt.Body)
                    PrintNode(s, sb, indent + 2);
                break;

            case ReturnStatement ret:
                if (ret.Value != null)
                {
                    WriteLine(sb, indent, "Return");
                    PrintNode(ret.Value, sb, indent + 1);
                }
                else
                {
                    WriteLine(sb, indent, "Return (no value)");
                }
                break;

            case ThrowStatement thr:
                WriteLine(sb, indent, "Throw");
                PrintNode(thr.Value, sb, indent + 1);
                break;

            // Expressions - literals
            case IntegerLiteralExpression intLit:
                WriteLine(sb, indent, $"IntegerLiteral ({intLit.Value})");
                break;

            case DecimalLiteralExpression decLit:
                WriteLine(sb, indent, $"DecimalLiteral ({decLit.Value})");
                break;

            case StringLiteralExpression strLit:
                WriteLine(sb, indent, $"StringLiteral (\"{strLit.Value}\")");
                break;

            case BooleanLiteralExpression boolLit:
                WriteLine(sb, indent, $"BooleanLiteral ({boolLit.Value})");
                break;

            case NothingLiteralExpression:
                WriteLine(sb, indent, "NothingLiteral");
                break;

            case VariableExpression varExpr:
                WriteLine(sb, indent, $"Variable \"{varExpr.Name}\"");
                break;

            case BinaryExpression bin:
                WriteLine(sb, indent, $"Binary ({bin.Op})");
                PrintNode(bin.Left, sb, indent + 1);
                PrintNode(bin.Right, sb, indent + 1);
                break;

            case UnaryExpression un:
                WriteLine(sb, indent, $"Unary ({un.Op})");
                PrintNode(un.Operand, sb, indent + 1);
                break;

            case CallExpression call:
                WriteLine(sb, indent, "Call");
                WriteLine(sb, indent + 1, "Callee");
                PrintNode(call.Callee, sb, indent + 2);
                if (call.Arguments.Count > 0)
                {
                    WriteLine(sb, indent + 1, "Arguments");
                    foreach (var arg in call.Arguments)
                        PrintNode(arg, sb, indent + 2);
                }
                break;

            case FieldAccessExpression fieldAccess:
                WriteLine(sb, indent, $"FieldAccess \"{fieldAccess.FieldName}\"");
                PrintNode(fieldAccess.Target, sb, indent + 1);
                break;

            case ListExpression list:
                WriteLine(sb, indent, "List");
                foreach (var elem in list.Elements)
                    PrintNode(elem, sb, indent + 1);
                break;

            default:
                WriteLine(sb, indent, $"<unknown node: {node.GetType().Name}>");
                break;
        }
    }

    private static void WriteLine(StringBuilder sb, int indent, string text)
    {
        sb.Append(new string(' ', indent * 2));
        sb.AppendLine(text);
    }
}