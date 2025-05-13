using System;
using System.Collections.Generic;
using System.Linq;

namespace HighBasic;

public class HBDictionary : Dictionary<dynamic, dynamic>
{
    public static HBDictionary FromList(List<dynamic> list)
    {
        var dict = new HBDictionary();
        for (int i = 0; i < list.Count; i++)
            dict.Add(i, list[i]);
        return dict;
    }

    public static HBDictionary FromArray(dynamic[] array)
    {
        var dict = new HBDictionary();
        for (int i = 0; i < array.Length; i++)
            dict.Add(i, array[i]);
        return dict;
    }

    public override string ToString()
    {
        return $"{{ {string.Join(", ", this.Select(x => $"{x.Key}: {x.Value}"))} }}";
    }

    public List<dynamic> AsList() => [.. Values];
}

public struct HBFunction
{
    public string Name { get; set; }
    public Statement[] Statements { get; set; }
}

public struct NativeHBFunction
{
    public string Name { get; set; }
    public Action<Expression[]> Method { get; set; }
}

public class Interpreter
{
    public Dictionary<string, dynamic> variables = [];

    public dynamic ResolveExpression(Expression expr)
    {
        return expr.Type switch
        {
            // ExpressionType.VARIABLE_POINTER => variables.TryGetValue(expr.Value, out dynamic? value) ? value : throw new Exception($"'{expr.Value}' is not a variable"), // check if it is actually a variable. if not - throw
            ExpressionType.VARIABLE_VALUE => variables[expr.Value],
            _ => expr.Value,
        };
    }

    public void InterpretStatements(IEnumerable<Statement> statements)
    {
        // debug print
        // TODO: make this prettier and move it somewhere else
        foreach (var statement in statements)
        {
            Console.WriteLine($"{statement.Source}: {statement.Type}");
            foreach (var expression in statement.Expressions)
            {
                Console.WriteLine($"|  {expression.Source}: {expression.Type} ({expression.Value})");
            }
        }

        Console.WriteLine();

        foreach (var statement in statements)
        {
            switch (statement.Type)
            {
                case StatementType.VARIABLE_ASSIGNMENT:
                    if (statement.Expressions.Length < 2)
                        throw new Exception($"too few expressions in variable assignment ('{statement.Source}') (expected 2)");
                    if (statement.Expressions.Length > 2)
                        throw new Exception($"too many expressions in variable assignment ('{statement.Source}') (expected 2)");

                    variables[ResolveExpression(statement.Expressions[0])] = ResolveExpression(statement.Expressions[1]);
                    break;
                case StatementType.OPERATOR:
                    throw new NotImplementedException("operators not yet implemented");
                case StatementType.FUNCTION_CALL:
                    string functionName = ResolveExpression(statement.Expressions[0]);
                    Expression[] arguments = statement.Expressions[1..].ToArray();

                    if (variables.TryGetValue(functionName, out dynamic? possiblyFunction))
                    {
                        switch (possiblyFunction)
                        {
                            case NativeHBFunction nativeFunction:
                                nativeFunction.Method(arguments);
                                break;
                            case HBFunction userFunction:
                                InterpretStatements(userFunction.Statements);
                                break;
                            default:
                                throw new Exception($"'{functionName}' is not a function");
                        }
                    }
                    else
                    {
                        throw new Exception($"'{functionName}' is not a function (variable not found)");
                    }

                    break;
                default:
                    throw new NotImplementedException($"unknown statement type: {statement.Type}");
            }
        }
    }
}
