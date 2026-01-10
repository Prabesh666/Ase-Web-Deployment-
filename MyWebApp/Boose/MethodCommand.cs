using System;
using System.Collections.Generic;
using BOOSE;

namespace MyWebApp.Boose
{
    public class MethodCommand : CompoundCommand, ICommand
    {
        private string _returnType = "int";
        private string _name = "";
        private readonly List<(string type, string name)> _params = new();

        private int _methodIndex = -1;
        private int _endMethodIndex = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Replace(",", " ").Trim();
            var parts = p.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 1)
                throw new CommandException("method [type] name [type param ...]");

            int start = 0;

            string first = parts[0].ToLowerInvariant();
            if (first == "int" || first == "real" || first == "boolean" || first == "bool")
            {
                _returnType = NormalizeType(first);
                start = 1;
            }
            else
            {
                _returnType = "int";  // Default return type
                start = 0;
            }

            if (start >= parts.Length)
                throw new CommandException("method missing name");

            _name = parts[start];
            _params.Clear();

            int i = start + 1;
            while (i < parts.Length)
            {
                if (i + 1 >= parts.Length)
                    throw new CommandException("method parameters must be <type> <name>");

                string t = NormalizeType(parts[i]);
                string n = parts[i + 1];

                if (t != "int" && t != "real" && t != "boolean")
                    throw new CommandException("method parameter type must be int/real/boolean");

                _params.Add((t, n));
                i += 2;
            }
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("Method requires ExtendedStoredProgram.");

            _methodIndex = p.Count - 1;
            p.RegisterMethod(_name, _methodIndex);

            // Create variable for the method name (return value)
            if (!p.VariableExists(_name))
            {
                ICommand retCmd = CreateVariable(_returnType, _name);
                if (retCmd != null)
                {
                    retCmd.Compile();
                }
            }

            // Create variables for parameters
            foreach (var param in _params)
            {
                if (!p.VariableExists(param.name))
                {
                    ICommand paramCmd = CreateVariable(param.type, param.name);
                    if (paramCmd != null)
                    {
                        paramCmd.Compile();
                    }
                }
                else
                {
                    // Optional: Verify type matches existing variable?
                    // For now, assume global reuse is valid and intended.
                    // We do nothing, as the variable is already valid.
                }
            }

            p.PushCompile(this);
        }

        private ICommand CreateVariable(string type, string name)
        {
            ICommand cmd = null;
            switch (type)
            {
                case "int": cmd = new IntCommand(); break;
                case "real": cmd = new RealCommand(); break;
                case "boolean": cmd = new BooleanCommand(); break;
            }
            if (cmd != null)
            {
                cmd.Set(program, name);
            }
            return cmd;
        }

        public void SetEndMethodIndex(int idx) => _endMethodIndex = idx;
        public int GetMethodStartIndex() => _methodIndex;
        public string GetMethodName() => _name;
        public string GetReturnType() => _returnType;
        public List<(string type, string name)> GetParameters() => _params;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("Method requires ExtendedStoredProgram.");

            if (_endMethodIndex < 0)
                throw new StoredProgramException("Method not linked to endmethod: " + _name);

            p.Jump(_endMethodIndex + 1);
        }

        public override void CheckParameters(string[] parameterList) { }

        private static string NormalizeType(string t)
        {
            t = (t ?? "").Trim().ToLowerInvariant();
            if (t == "bool") return "boolean";
            return t;
        }
    }

    public class EndMethodCommand : Command, ICommand
    {
        private int _returnAddress = -1;

        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Trim();
            if (p.Length != 0 && !p.Equals("method", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("endmethod takes no parameters");
        }

        public override void Compile()
        {
            if (program is not ExtendedStoredProgram p)
                throw new CommandException("EndMethod requires ExtendedStoredProgram.");

            object top = p.PopCompile();
            if (top is not MethodCommand m)
                throw new CommandException("endmethod without matching method");

            int endIdx = p.Count - 1;
            m.SetEndMethodIndex(endIdx);
        }

        public void SetReturnAddress(int address) => _returnAddress = address;

        public override void Execute()
        {
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("EndMethod requires ExtendedStoredProgram.");

            int ret = p.PopReturn();
            p.Jump(ret);
        }

        public override void CheckParameters(string[] parameterList) { }
    }
}
