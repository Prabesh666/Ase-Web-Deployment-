using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class BooleanCommand : Evaluation, ICommand
    {
        private string _expr = "false";

        public bool BoolValue { get; set; }

        public override int Value => BoolValue ? 1 : 0;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Trim();
            if (p.Length == 0) throw new CommandException("boolean <name> [= <expr>]");

            int eq = p.IndexOf('=');
            if (eq < 0)
            {
                varName = p.Trim();
                _expr = "false";
            }
            else
            {
                varName = p.Substring(0, eq).Trim();
                _expr = p.Substring(eq + 1).Trim();
                if (_expr.Length == 0) _expr = "false";
            }
        }

        public override void Compile()
        {
            if (!program.VariableExists(varName))
                program.AddVariable(this);

            Execute();
        }

        public override void Execute()
        {
            BoolValue = ExpressionUtil.EvalBool(program, _expr);
        }

        public override void CheckParameters(string[] p) { }

        public override string ToString() => BoolValue ? "true" : "false";
    }
}
