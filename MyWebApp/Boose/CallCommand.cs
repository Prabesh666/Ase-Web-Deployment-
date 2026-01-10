using System;
using System.Collections.Generic;
using BOOSE;

namespace MyWebApp.Boose
{
    public class CallCommand : Command, ICommand
    {
        private string _method = "";
        private readonly List<string> _args = new();

        // Set the method name and arguments from the params string
        public new void Set(StoredProgram Program, string Params)
        {
            program = Program;

            string p = (Params ?? "").Replace(",", " ").Trim();
            var parts = p.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Check if there is at least one argument for the method name
            if (parts.Length < 1)
                throw new CommandException("call <methodName> [args...]");

            // Set the method name
            _method = parts[0];
            _args.Clear();

            // Add arguments to the args list
            for (int i = 1; i < parts.Length; i++)
                _args.Add(parts[i]);
        }

        // Compile method (no additional logic needed for CallCommand)
        public override void Compile() { }

        // Check the parameters (no additional validation needed here)
        public override void CheckParameters(string[] parameterList) { }

        // Execute the command: find the method, set arguments, and jump to the method
        public override void Execute()
        {
            // Ensure the program is of type ExtendedStoredProgram (which allows method handling)
            if (program is not ExtendedStoredProgram p)
                throw new StoredProgramException("Call requires ExtendedStoredProgram.");

            // Ensure the method exists
            if (!p.MethodExists(_method))
                throw new CommandException("Method does not exist: " + _method);

            int methodIndex = p.GetMethodStartIndex(_method);

            // Retrieve the MethodCommand from the list of commands
            MethodCommand method = p.GetCommandAt(methodIndex) as MethodCommand;

            if (method == null)
                throw new CommandException("Failed to retrieve MethodCommand: " + _method);

            // Set method arguments (parameters)
            for (int i = 0; i < _args.Count; i++)
            {
                string expr = _args[i];
                string paramType = method.GetParameters()[i].type;

                // Initialize the method parameters as variables
                if (paramType == "int")
                {
                    var value = ExpressionUtil.EvalInt(program, expr);
                    p.SetVariable(method.GetParameters()[i].name, value);  // Initialize method parameter in program's variable list
                }
                else if (paramType == "real")
                {
                    var value = ExpressionUtil.EvalDouble(program, expr);
                    p.SetVariable(method.GetParameters()[i].name, value);  // Initialize method parameter in program's variable list
                }
                else if (paramType == "boolean")
                {
                    var value = ExpressionUtil.EvalBool(program, expr);
                    p.SetVariable(method.GetParameters()[i].name, value);  // Initialize method parameter in program's variable list
                }
            }

            // Jump to method's body (methodIndex + 1 to skip the MethodCommand itself)
            p.PushReturn(p.PC); // Save return address
            p.Jump(methodIndex + 1); // Jump to the method body
        }
    }
}
