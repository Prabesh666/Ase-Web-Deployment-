using BOOSE;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

internal static class ExpressionUtil
{
    private static readonly Regex Ident = new(@"\b[A-Za-z_]\w*\b", RegexOptions.Compiled);

    public static int EvalInt(StoredProgram program, string expr)
        => (int)Math.Round(EvalDouble(program, expr));

    public static double EvalDouble(StoredProgram program, string expr)
    {
        if (program == null) throw new ArgumentNullException(nameof(program));

        expr = (expr ?? "").Trim();
        if (expr.Length == 0) return 0;

        if (double.TryParse(expr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lit))
            return lit;

        string replaced = Ident.Replace(expr, m =>
        {
            string name = m.Value;

            if (name.Equals("true", StringComparison.OrdinalIgnoreCase)) return "1";
            if (name.Equals("false", StringComparison.OrdinalIgnoreCase)) return "0";

            if (!program.VariableExists(name))
                return name;

            var v = program.GetVariable(name);
            if (v == null) return "0";

            // Our variables override ToString properly (real/bool/int)
            string s = v.ToString()?.Trim() ?? "0";

            if (s.Equals("true", StringComparison.OrdinalIgnoreCase)) return "1";
            if (s.Equals("false", StringComparison.OrdinalIgnoreCase)) return "0";

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var dv))
                return dv.ToString(CultureInfo.InvariantCulture);

            return "0";
        });

        try
        {
            object result = new DataTable().Compute(replaced, "");
            if (result == null) throw new CommandException("Invalid expression: " + expr);

            if (!double.TryParse(Convert.ToString(result, CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                throw new CommandException("Invalid expression: " + expr);

            return d;
        }
        catch
        {
            throw new CommandException("Invalid expression: " + expr);
        }
    }

    public static bool EvalBool(StoredProgram program, string expr)
    {
        if (program == null) throw new ArgumentNullException(nameof(program));
        expr = (expr ?? "").Trim();
        if (expr.Length == 0) return false;

        // handle parentheses by recursion (simple but effective)
        while (expr.StartsWith("(") && expr.EndsWith(")"))
        {
            // only strip if parentheses actually wrap whole expression
            int depth = 0;
            bool wraps = true;
            for (int i = 0; i < expr.Length; i++)
            {
                if (expr[i] == '(') depth++;
                else if (expr[i] == ')') depth--;
                if (depth == 0 && i < expr.Length - 1) { wraps = false; break; }
            }
            if (!wraps) break;
            expr = expr.Substring(1, expr.Length - 2).Trim();
        }

        // NOT
        if (expr.StartsWith("!"))
            return !EvalBool(program, expr.Substring(1));

        // OR (lowest precedence)
        int orPos = FindTopLevel(expr, "||");
        if (orPos >= 0)
            return EvalBool(program, expr.Substring(0, orPos)) || EvalBool(program, expr.Substring(orPos + 2));

        // AND
        int andPos = FindTopLevel(expr, "&&");
        if (andPos >= 0)
            return EvalBool(program, expr.Substring(0, andPos)) && EvalBool(program, expr.Substring(andPos + 2));

        // Comparisons
        foreach (var op in new[] { ">=", "<=", "==", "!=", ">", "<" })
        {
            int pos = FindTopLevel(expr, op);
            if (pos >= 0)
            {
                string left = expr.Substring(0, pos).Trim();
                string right = expr.Substring(pos + op.Length).Trim();

                double a = EvalDouble(program, left);
                double b = EvalDouble(program, right);

                return op switch
                {
                    ">=" => a >= b,
                    "<=" => a <= b,
                    "==" => Math.Abs(a - b) < 1e-9,
                    "!=" => Math.Abs(a - b) >= 1e-9,
                    ">" => a > b,
                    "<" => a < b,
                    _ => false
                };
            }
        }

        // literals
        if (expr.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (expr.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;

        // numeric truthiness
        return Math.Abs(EvalDouble(program, expr)) > double.Epsilon;
    }

    private static int FindTopLevel(string s, string op)
    {
        int depth = 0;
        for (int i = 0; i <= s.Length - op.Length; i++)
        {
            char c = s[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;

            if (depth == 0 && s.AsSpan(i, op.Length).SequenceEqual(op))
                return i;
        }
        return -1;
    }
}

