using System;
using System.Linq;

namespace HighBasic;

// now that Lexer is static, is Runtime even necessary?
public class Runtime(Interpreter? interpreter = null)
{
    public Interpreter Interpreter { get; } = interpreter ?? new Interpreter();

    public bool Debug { get; set; } = false;

    public Runtime InsertStandardLibrary()
    {
        Interpreter.variables["noop"] = new NativeHBFunction()
        {
            Name = "noop",
            Method = (Expression[] _) => {}
        };

        Interpreter.variables["println"] = new NativeHBFunction()
        {
            Name = "println",
            Method = (Expression[] args) => Console.WriteLine(string.Join(" ", args.Select(x => Interpreter.ResolveExpression(x).ToString())))
        };

        Interpreter.variables["print"] = new NativeHBFunction()
        {
            Name = "print",
            Method = (Expression[] args) => Console.Write(string.Join(" ", args.Select(x => Interpreter.ResolveExpression(x).ToString())))
        };

        Interpreter.variables["vars"] = new NativeHBFunction()
        {
            Name = "vars",
            Method = (Expression[] args) =>
            {
                if (args.Length != 1)
                    throw new Exception("`vars` takes exactly one argument");

                Interpreter.variables[Interpreter.ResolveExpression(args[0])] = HBDictionary.FromArray(Interpreter.variables.Keys.ToArray());
            }
        };

        // return self for chaining
        return this;
    }

    public void DoString(string code)
    {
        var statements = Lexer.ParseString(code);

        if (Debug)
        {
            foreach (var statement in statements)
            {
                Console.WriteLine($"{statement.Source}: {statement.Type}");
                foreach (var expression in statement.Expressions)
                {
                    Console.WriteLine($"|  {expression.Source}: {expression.Type} ({expression.Value})");
                }
            }
            Console.WriteLine();
        }

        Interpreter.InterpretStatements(statements);
    }
}
