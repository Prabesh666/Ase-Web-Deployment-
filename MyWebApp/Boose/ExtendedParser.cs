using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class ExtendedParser : Parser
    {
        private readonly CommandFactory _factory;
        private readonly ExtendedStoredProgram _program;

        public ExtendedParser(CommandFactory factory, ExtendedStoredProgram program)
            : base(factory, program)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _program = program ?? throw new ArgumentNullException(nameof(program));
        }

        public override void ParseProgram(string programText)
        {
            programText ??= "";
            programText = programText.Replace("\r\n", "\n").Replace("\r", "\n");

            _program.Clear();
            _program.SetSyntaxStatus(true);

            string errors = "";
            string[] lines = programText.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = (lines[i] ?? "").Trim();
                if (line.Length == 0) continue;

                try
                {
                    ICommand cmd = ParseCommand(line);
                    if (cmd == null) continue;

                    // Add first so Compile() can know correct index (like the docs approach)
                    _program.Add(cmd);
                    cmd.Compile();
                }
                catch (BOOSEException ex)
                {
                    _program.SetSyntaxStatus(false);
                    errors += $"{ex.Message} on line {i + 1}\n";
                }
                catch (Exception ex)
                {
                    _program.SetSyntaxStatus(false);
                    errors += $"{ex.Message} on line {i + 1}\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(errors))
                throw new ParserException(errors.Trim());
        }

        public override ICommand ParseCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            line = line.Trim();
            if (line.StartsWith("*") || line.StartsWith("//")) return null;

            line = line.Replace("\t", " ");
            while (line.Contains("  ")) line = line.Replace("  ", " ");

            // Normalize "end X"
            line = NormalizeEnd(line);

            // Assignment rewrite: "x = expr"  (only if x is not a known keyword)
            // BOOSE parser treats first token as command name, so we rewrite to "assign ..."
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && parts[1] == "=")
            {
                string first = parts[0].ToLowerInvariant();
                if (first != "int" && first != "real" && first != "boolean" && first != "bool" &&
                    first != "array" && first != "poke" && first != "peek" &&
                    first != "if" && first != "while" && first != "for" &&
                    first != "method" && first != "call")
                {
                    line = "assign " + line;
                }
            }

            var p2 = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p2.Length == 0) return null;

            string cmdName = p2[0];
            string paramString = (p2.Length > 1) ? string.Join(" ", p2, 1, p2.Length - 1) : "";

            ICommand cmd = _factory.MakeCommand(cmdName);
            cmd.Set(_program, paramString);
            return cmd;
        }

        private static string NormalizeEnd(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[0].Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                string second = parts[1].ToLowerInvariant();
                if (second == "if") return "endif";
                if (second == "while") return "endwhile";
                if (second == "for") return "endfor";
                if (second == "method") return "endmethod";
            }
            return line;
        }
    }
}
