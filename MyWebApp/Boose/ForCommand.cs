using System;
using BOOSE;

namespace MyWebApp.Boose
{
    /// <summary>
    /// Implements: for var = fromExpr to toExpr step stepExpr
    /// Linked to EndForCommand during Compile() using ExtendedStoredProgram compile stack.
    /// </summary>
    public class ForCommand : Command, ICommand
    {
        private string _varName = "";
        private string _fromExpr = "";
        private string _toExpr = "";
        private string _stepExpr = "";

        private double _to;
        private double _step;
        private bool _initialised;

        private int _forIndex = -1;
        private int _endForIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            var parts = (Params ?? "").Replace(",", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // for count = 1 to 10 step 2
            if (parts.Length < 7 ||
                parts[1] != "=" ||
                !parts[3].Equals("to", StringComparison.OrdinalIgnoreCase) ||
                !parts[5].Equals("step", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("for <var> = <from> to <to> step <step>");

            _varName = parts[0];
            _fromExpr = parts[2];
            _toExpr = parts[4];
            _stepExpr = parts[6];
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("For requires ExtendedStoredProgram.");

            _forIndex = p.Count - 1; // already added before compile
            p.PushCompile(this);
        }

        public void SetEndForIndex(int idx) => _endForIndex = idx;
        public int GetForIndex() => _forIndex;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("For requires ExtendedStoredProgram.");

            if (_endForIndex < 0)
                throw new StoredProgramException("For not linked to endfor.");

            // evaluate bounds each time (matches typical BOOSE behaviour)
            _to = ExpressionUtil.EvalDouble(program, _toExpr);
            _step = ExpressionUtil.EvalDouble(program, _stepExpr);

            if (Math.Abs(_step) < double.Epsilon)
                throw new CommandException("for step cannot be 0");

            if (!_initialised)
            {
                _initialised = true;

                double startValue = ExpressionUtil.EvalDouble(program, _fromExpr);

                if (!program.VariableExists(_varName))
                {
                    var v = new IntCommand();
                    v.Set(program, $"{_varName} = {startValue}");
                    v.Compile();
                }
                else
                {
                    var existing = program.GetVariable(_varName);

                    if (existing is IntCommand i)
                        i.Value = (int)Math.Round(startValue);
                    else if (existing is RealCommand r)
                        r.RealValue = startValue;
                    else if (existing is BooleanCommand b)
                        b.BoolValue = Math.Abs(startValue) > double.Epsilon;
                    else
                        throw new CommandException("for loop variable must be numeric");
                }
            }


            double current = ExpressionUtil.EvalDouble(program, _varName);

            bool shouldRun = _step > 0 ? current <= _to : current >= _to;
            if (!shouldRun)
                p.Jump(_endForIndex + 1); // jump after endfor
        }

        public override void CheckParameters(string[] parameterList) { }
    }
    public class EndForCommand : Command, ICommand
    {
        private ForCommand _forCmd;
        private int _forIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Trim();
            if (p.Length != 0 && !p.Equals("for", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("endfor takes no parameters");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("EndFor requires ExtendedStoredProgram.");

            object top = p.PopCompile();
            if (top is not ForCommand f)
                throw new CommandException("endfor without matching for");

            _forCmd = f;

            int endIdx = p.Count - 1;
            f.SetEndForIndex(endIdx);

            _forIndex = f.GetForIndex();
        }

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("EndFor requires ExtendedStoredProgram.");

            if (_forCmd == null || _forIndex < 0)
                throw new StoredProgramException("EndFor not linked properly.");

            // increment loop variable
            double step = ExpressionUtil.EvalDouble(program, GetPrivate(_forCmd, "_stepExpr"));
            string varName = GetPrivate(_forCmd, "_varName");

            double current = ExpressionUtil.EvalDouble(program, varName);
            double next = current + step;

            // assign back
            var v = program.GetVariable(varName);
            if (v is RealCommand r) r.RealValue = next;
            else if (v is IntCommand i) i.Value = (int)Math.Round(next);
            else if (v is BooleanCommand b) b.BoolValue = Math.Abs(next) > double.Epsilon;
            else throw new CommandException("for loop variable must be int/real/boolean");

            // jump back to for (which will check bounds)
            p.Jump(_forIndex);
        }

        public override void CheckParameters(string[] parameterList) { }

        // tiny helper to avoid adding extra framework: read private fields safely
        private static string GetPrivate(ForCommand cmd, string field)
        {
            var f = typeof(ForCommand).GetField(field,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            return (string)(f?.GetValue(cmd) ?? "");
        }
    }
}
