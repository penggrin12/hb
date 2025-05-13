using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HighBasic;

public enum ExpressionType
{
    VARIABLE_VALUE,
    CONSTANT,
}

public struct Expression
{
    public ExpressionType Type { get; set; }
    public string Source { get; set; }

    public string Value { get; set; }
}

public enum StatementType
{
    VARIABLE_ASSIGNMENT,
    OPERATOR,
    FUNCTION_CALL,
}

public struct Statement
{
    public StatementType Type { get; set; }
    public string Source { get; set; }

    public Expression[] Expressions { get; set; }
}

public static partial class Lexer
{
    private static readonly char[] STATEMENT_TERMINATORS = [ '\n', ';' ];

    public static Expression[] ParseManyExpressions(string[] tokens)
    {
        var expressions = new List<Expression>(tokens.Length);

        foreach (var raw in tokens)
        {
            int commentIdx = raw.IndexOf(':');
            string tok = raw;
            if (commentIdx >= 0)
            {
                // if there's content before the colon, parse that as one last token
                tok = raw[..commentIdx];
                if (string.IsNullOrEmpty(tok))
                    break;

                AddExpression(tok, expressions);
                break;
            }

            AddExpression(tok, expressions);
        }

        return [.. expressions];
    }

    private static void AddExpression(string tok, List<Expression> expressions)
    {
        // string literal?
        if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"' && IsUnescapedQuote(tok, tok.Length - 1))
        {
            // unquote & unescape
            var inner = tok[1..^1]
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
            expressions.Add(new Expression
            {
                Type   = ExpressionType.CONSTANT,
                Source = tok,
                Value  = inner
            });
        }
        else
        {
            // Delegate to your normal parser
            var expr = ParseExpression(tok)
                ?? throw new Exception($"unable to parse: '{tok}'");
            expressions.Add(expr);
        }
    }

    private static bool IsUnescapedQuote(string s, int idx)
    {
        // Count backslashes immediately before s[idx]; odd => escaped
        int count = 0;
        for (int i = idx - 1; i >= 0 && s[i] == '\\'; i--)
            count++;
        return (count % 2) == 0;
    }

    public static Expression? ParseExpression(string source)
    {
        // should this be handled outside..?
        if (source.Length == 0)
            return null;

        return source[0] switch
        {
            // '@' => new() { Type = ExpressionType.VARIABLE_POINTER, Source = source, Value = source[1..] },
            '$' => new() { Type = ExpressionType.VARIABLE_VALUE, Source = source, Value = source[1..] },
            '\\' => new() { Type = ExpressionType.CONSTANT, Source = source, Value = source[1..] }, // to escape @ and $
            _ => new() { Type = ExpressionType.CONSTANT, Source = source, Value = source },
        };
    }

    [GeneratedRegex("\"(?:\\\\.|[^\"])*\"|\\S+")]
    private static partial Regex LineTokenizerRegex();

    private static string[] TokenizeLine(string line)
    {
        var matches = LineTokenizerRegex().Matches(line);
        return matches
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();
    }

    public static Statement ParseLine(string line)
    {
        string[] linePieces = TokenizeLine(line);
        string statementName = linePieces[0];

        return statementName switch
        {
            "var"      => new Statement { Type = StatementType.VARIABLE_ASSIGNMENT, Source = line, Expressions = ParseManyExpressions(linePieces[1..]) },
            "operator" => new Statement { Type = StatementType.OPERATOR,            Source = line, Expressions = ParseManyExpressions(linePieces[1..]) },
            _          => new Statement { Type = StatementType.FUNCTION_CALL,       Source = line, Expressions = ParseManyExpressions(linePieces)      },
        };
    }

    public static IEnumerable<Statement> ParseString(string source)
    {
        var result = new List<Statement>();

        foreach (string line in source.Split(STATEMENT_TERMINATORS, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith(':')) continue;
            result.Add(ParseLine(line));
        }

        return result;
    }
}
