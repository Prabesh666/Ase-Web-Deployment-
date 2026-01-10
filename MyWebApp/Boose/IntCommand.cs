using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class IntCommand : Evaluation, ICommand
    {
        private string _expr = "0";

        public override int Value { get; set; }

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Trim();
            if (p.Length == 0) throw new CommandException("int <name> [= <expr>]");

            int eq = p.IndexOf('=');
            if (eq < 0)
            {
                varName = p.Trim();
                _expr = "0";
            }
            else
            {
                varName = p.Substring(0, eq).Trim();
                _expr = p.Substring(eq + 1).Trim();
                if (_expr.Length == 0) _expr = "0";
            }
        }

        public override void Compile()
        {
            if (!program.VariableExists(varName))
                program.AddVariable(this);

            // initializer allowed at compile-time like docs
            Execute();
        }

        public override void Execute()
        {
            Value = ExpressionUtil.EvalInt(program, _expr);
        }

        public override void CheckParameters(string[] parameterList) { }

        public override string ToString() => Value.ToString();
    }
}
