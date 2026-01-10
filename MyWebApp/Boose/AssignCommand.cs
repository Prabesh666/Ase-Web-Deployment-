using System;
using BOOSE;

namespace MyWebApp.Boose
{
    /// <summary>
    /// Handles: assign x = expr
    /// Parser rewrites "x = expr" into "assign x = expr"
    /// </summary>
    public class AssignCommand : Command, ICommand
    {
        private string _name = "";
        private string _expr = "";

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            // Params: "x = 10"  (because parser prepends assign)
            string p = (Params ?? "").Trim();
            int eq = p.IndexOf('=');
            if (eq < 0) throw new CommandException("assign <var> = <expr>");

            _name = p.Substring(0, eq).Trim();
            _expr = p.Substring(eq + 1).Trim();

            if (_name.Length == 0) throw new CommandException("assign <var> = <expr>");
            if (_expr.Length == 0) _expr = "0";
        }

        public override void Compile()
        {
            if (!program.VariableExists(_name))
                throw new CommandException("Attempt to retrieve non-existant variable.");
        }

        public override void Execute()
        {
            var v = program.GetVariable(_name);
            if (v == null)
                throw new CommandException("Attempt to retrieve non-existant variable.");

            if (v is RealCommand r)
            {
                r.RealValue = ExpressionUtil.EvalDouble(program, _expr);
                return;
            }

            if (v is BooleanCommand b)
            {
                b.BoolValue = ExpressionUtil.EvalBool(program, _expr);
                return;
            }

            if (v is IntCommand i)
            {
                i.Value = ExpressionUtil.EvalInt(program, _expr);
                return;
            }

            throw new CommandException("Cannot assign to this type: " + _name);
        }

        public override void CheckParameters(string[] parameterList) { }
    }
}
