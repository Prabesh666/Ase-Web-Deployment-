using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class PokeCommand : Command, ICommand
    {
        private string _arrayName = "";
        private string _indexExpr = "";
        private string _valueExpr = "";

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            var parts = (Params ?? "").Replace(",", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // poke arr 5 = 99
            int eq = System.Array.IndexOf(parts, "=");
            if (parts.Length < 4 || eq != 2)
                throw new CommandException("poke <array> <index> = <value>");

            _arrayName = parts[0];
            _indexExpr = parts[1];
            _valueExpr = string.Join(" ", parts, 3, parts.Length - 3);
        }

        public override void Compile()
        {
            if (!program.VariableExists(_arrayName))
                throw new CommandException("Array does not exist: " + _arrayName);

            if (program.GetVariable(_arrayName) is not ArrayCommand)
                throw new CommandException("Not an array: " + _arrayName);
        }

        public override void Execute()
        {
            var arr = program.GetVariable(_arrayName) as ArrayCommand;
            if (arr == null) throw new CommandException("Array does not exist: " + _arrayName);

            int index = ExpressionUtil.EvalInt(program, _indexExpr);

            if (arr.IsIntArray())
                arr.SetIntArray(ExpressionUtil.EvalInt(program, _valueExpr), index);
            else
                arr.SetRealArray(ExpressionUtil.EvalDouble(program, _valueExpr), index);
        }

        public override void CheckParameters(string[] parameterList) { }
    }
}
