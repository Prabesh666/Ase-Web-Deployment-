using System;
using System.Collections.Generic;
using BOOSE;

namespace MyWebApp.Boose
{
    public class ExtendedStoredProgram : StoredProgram
    {
        private readonly List<ICommand> _commands = new();
        private int _pc;
        private readonly Dictionary<string, int> _methodStart = new(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<int> _returnStack = new();
        private readonly Stack<object> _compileStack = new();
        private readonly Dictionary<string, Evaluation> _variables = new();

        public ExtendedStoredProgram(ICanvas canvas) : base(canvas) { }

        public int PC => _pc;
        public int Count => _commands.Count;

        // Method to add a command
        public new void Add(ICommand cmd)
        {
            if (cmd == null) return;
            _commands.Add(cmd);
            base.Add(cmd); // Keep base program list in sync (safe)
        }

        public new void Clear()
        {
            _commands.Clear();
            _pc = 0;
            _methodStart.Clear();
            _returnStack.Clear();
            _compileStack.Clear();
            _variables.Clear(); // Clear all variables
            base.Clear();
        }

        public void Jump(int target)
        {
            if (target < 0) target = 0;
            if (target > _commands.Count) target = _commands.Count;
            _pc = target;
        }

        public void PushReturn(int address) => _returnStack.Push(address);

        public int PopReturn()
        {
            if (_returnStack.Count == 0)
                throw new StoredProgramException("Call stack empty (endmethod without call).");
            return _returnStack.Pop();
        }

        public void PushCompile(object o) => _compileStack.Push(o);

        public object PopCompile()
        {
            if (_compileStack.Count == 0)
                throw new StoredProgramException("Compile stack empty (missing opening command).");
            return _compileStack.Pop();
        }

        public bool CompileStackAny => _compileStack.Count > 0;

        // ---- method registry ----
        public void RegisterMethod(string methodName, int startIndex)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new StoredProgramException("Invalid method name.");
            _methodStart[methodName.Trim()] = startIndex;
        }

        public bool MethodExists(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName)) return false;
            return _methodStart.ContainsKey(methodName.Trim());
        }

        public int GetMethodStartIndex(string methodName)
        {
            if (!MethodExists(methodName))
                throw new StoredProgramException("Method does not exist: " + methodName);
            return _methodStart[methodName.Trim()];
        }

        // ---- Variable Access Methods ----
        public bool VariableExists(string name) => _variables.ContainsKey(name);

        public void SetVariable(string name, object value)
        {
            if (_variables.ContainsKey(name))
                _variables[name] = (Evaluation)value;
            else
                _variables.Add(name, (Evaluation)value);
        }

        public Evaluation GetVariable(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            return null;
        }

        // ---- Retrieve Command by Index ----
        public ICommand GetCommandAt(int index)
        {
            if (index < 0 || index >= _commands.Count)
                throw new StoredProgramException("Invalid command index.");
            return _commands[index];
        }

        public override void Run()
        {
            _pc = 0;

            while (_pc < _commands.Count)
            {
                ICommand cmd = _commands[_pc];
                _pc++; // advance first (important for call return address)
                cmd.Execute();
            }
        }
    }
}
