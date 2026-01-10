using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class ArrayCommand : Evaluation, ICommand
    {
        private string _type = "";
        private string _name = "";
        private string _sizeExpr = "0";

        private int[] _ints;
        private double[] _reals;
        private int _size;

        public int Size => _size;

        public bool IsIntArray() => _type == "int";
        public bool IsRealArray() => _type == "real";

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            var parts = (Params ?? "").Replace(",", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
                throw new CommandException("array <int|real> <name> <size>");

            _type = parts[0].Trim().ToLowerInvariant();
            _name = parts[1].Trim();
            _sizeExpr = parts[2].Trim();

            if (_type != "int" && _type != "real")
                throw new CommandException("array supports only int or real");

            varName = _name;
        }

        public override void Compile()
        {
            _size = ExpressionUtil.EvalInt(program, _sizeExpr);
            if (_size <= 0)
                throw new CommandException("array size must be > 0");

            if (_type == "int") _ints = new int[_size];
            else _reals = new double[_size];

            if (!program.VariableExists(varName))
                program.AddVariable(this);
        }

        public override void Execute()
        {
            // no runtime action required for array declaration in docs approach
        }

        public override void CheckParameters(string[] parameterList) { }

        public int GetIntArray(int index)
        {
            if (_ints == null) throw new CommandException("Not an int array: " + _name);
            CheckIndex(index);
            return _ints[index];
        }

        public double GetRealArray(int index)
        {
            if (_reals == null) throw new CommandException("Not a real array: " + _name);
            CheckIndex(index);
            return _reals[index];
        }

        public void SetIntArray(int val, int index)
        {
            if (_ints == null) throw new CommandException("Not an int array: " + _name);
            CheckIndex(index);
            _ints[index] = val;
        }

        public void SetRealArray(double val, int index)
        {
            if (_reals == null) throw new CommandException("Not a real array: " + _name);
            CheckIndex(index);
            _reals[index] = val;
        }

        private void CheckIndex(int index)
        {
            if (index < 0 || index >= _size)
                throw new CommandException($"Array index out of range: {index} (0..{_size - 1})");
        }
    }
}
