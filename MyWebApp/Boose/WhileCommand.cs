using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class WhileCommand : ConditionalCommand, ICommand
    {
        private string _cond = "";
        private int _whileIndex = -1;
        private int _endWhileIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;
            _cond = (Params ?? "").Trim();
            if (_cond.Length == 0) throw new CommandException("while <condition>");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("While requires ExtendedStoredProgram.");

            _whileIndex = p.Count - 1;       // we were added before compile
            p.PushCompile(this);
        }

        public void SetEndWhileIndex(int index) => _endWhileIndex = index;
        public int GetWhileIndex() => _whileIndex;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("While requires ExtendedStoredProgram.");

            if (_endWhileIndex < 0)
                throw new StoredProgramException("While not linked to endwhile.");

            bool ok = ExpressionUtil.EvalBool(program, _cond);
            if (!ok)
                p.Jump(_endWhileIndex + 1); // jump after endwhile
        }

        public override void CheckParameters(string[] parameterList) { }
    }
    public class EndWhileCommand : Command, ICommand
    {
        private int _whileIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Trim();
            if (p.Length != 0 && !p.Equals("while", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("endwhile takes no parameters");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("EndWhile requires ExtendedStoredProgram.");

            object top = p.PopCompile();
            if (top is not WhileCommand w)
                throw new CommandException("endwhile without matching while");

            int endIdx = p.Count - 1;
            w.SetEndWhileIndex(endIdx);
            _whileIndex = w.GetWhileIndex();
        }

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("EndWhile requires ExtendedStoredProgram.");

            if (_whileIndex < 0)
                throw new StoredProgramException("EndWhile not linked properly.");

            p.Jump(_whileIndex); // jump back to while for re-check
        }

        public override void CheckParameters(string[] parameterList) { }
    }
}
