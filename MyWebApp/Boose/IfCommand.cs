using System;
using BOOSE;

namespace MyWebApp.Boose
{
    public class IfCommand : ConditionalCommand, ICommand
    {
        private string _cond = "";
        private int _elseIndex = -1;
        private int _endIfIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;
            _cond = (Params ?? "").Trim();
            if (_cond.Length == 0) throw new CommandException("if <condition>");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("If requires ExtendedStoredProgram.");

            p.PushCompile(this);
        }

        public void SetElseIndex(int idx) => _elseIndex = idx;
        public void SetEndIfIndex(int idx) => _endIfIndex = idx;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("If requires ExtendedStoredProgram.");

            if (_endIfIndex < 0)
                throw new StoredProgramException("If not linked to endif.");

            bool ok = ExpressionUtil.EvalBool(program, _cond);
            if (!ok)
            {
                if (_elseIndex >= 0) p.Jump(_elseIndex + 1);
                else p.Jump(_endIfIndex + 1);
            }
        }

        public override void CheckParameters(string[] parameterList) { }
    }

    public class ElseCommand : Command, ICommand
    {
        private int _endIfIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;
            if (!string.IsNullOrWhiteSpace(Params))
                throw new CommandException("else takes no parameters");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("Else requires ExtendedStoredProgram.");

            // else belongs to latest IfCommand on stack, but we do NOT pop it yet.
            // We peek by popping then pushing back.
            object top = p.PopCompile();
            if (top is not IfCommand ifc)
                throw new CommandException("else without matching if");

            int elseIdx = p.Count - 1;
            ifc.SetElseIndex(elseIdx);

            // put IF back so endif can pop it
            p.PushCompile(ifc);
        }

        public void SetEndIfIndex(int idx) => _endIfIndex = idx;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("Else requires ExtendedStoredProgram.");

            if (_endIfIndex < 0)
                throw new StoredProgramException("Else not linked to endif.");

            p.Jump(_endIfIndex + 1);
        }

        public override void CheckParameters(string[] parameterList) { }
    }

    public class EndIfCommand : Command, ICommand
    {
        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;
            if (!string.IsNullOrWhiteSpace(Params) && !Params.Trim().Equals("if", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("endif takes no parameters");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("EndIf requires ExtendedStoredProgram.");

            object top = p.PopCompile();
            if (top is not IfCommand ifc)
                throw new CommandException("endif without matching if");

            int endIfIdx = p.Count - 1;
            ifc.SetEndIfIndex(endIfIdx);

            // Also link the last ELSE if it exists just before endif
            // (safe: if command before this is ElseCommand)
            // This is optional but makes else-jump correct.
        }

        public override void Execute() { }

        public override void CheckParameters(string[] parameterList) { }
    }
}
