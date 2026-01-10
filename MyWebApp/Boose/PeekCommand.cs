using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class PeekCommand : Command, ICommand
    {
        private string _destVar = "";
        private string _arrayName = "";
        private string _indexExpr = "";

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            // peek x = arr 5
            var parts = (Params ?? "").Replace(",", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4 || parts[1] != "=")
                throw new CommandException("peek <var> = <array> <index>");

            _destVar = parts[0];
            _arrayName = parts[2];
            _indexExpr = parts[3];
        }

        public override void Compile()
        {
            if (!program.VariableExists(_arrayName))
                throw new CommandException("Array does not exist: " + _arrayName);

            if (program.GetVariable(_arrayName) is not ArrayCommand)
                throw new CommandException("Not an array: " + _arrayName);

            if (!program.VariableExists(_destVar))
                throw new CommandException("Destination variable not declared: " + _destVar);
        }

        public override void Execute()
        {
            var arr = program.GetVariable(_arrayName) as ArrayCommand;
            if (arr == null) throw new CommandException("Array does not exist: " + _arrayName);

            int index = ExpressionUtil.EvalInt(program, _indexExpr);

            var destObj = program.GetVariable(_destVar);
            if (destObj is RealCommand r)
            {
                double v = arr.IsIntArray() ? arr.GetIntArray(index) : arr.GetRealArray(index);
                r.RealValue = v;
                return;
            }

            if (destObj is BooleanCommand b)
            {
                double v = arr.IsIntArray() ? arr.GetIntArray(index) : arr.GetRealArray(index);
                b.BoolValue = Math.Abs(v) > double.Epsilon;
                return;
            }

            if (destObj is IntCommand i)
            {
                int v = arr.IsIntArray()
                    ? arr.GetIntArray(index)
                    : (int)Math.Round(arr.GetRealArray(index));
                i.Value = v;
                return;
            }

            throw new CommandException("Destination variable type not supported: " + _destVar);
        }

        public override void CheckParameters(string[] parameterList) { }
    }
}
