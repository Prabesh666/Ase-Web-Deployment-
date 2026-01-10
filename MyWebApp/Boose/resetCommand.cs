using BOOSE;

namespace MyWebApp.Boose
{
    /// <summary>
    /// Represents the RESET command.
    /// Clears the stored program and resets the drawing canvas.
    /// Syntax: <c>reset</c>
    /// </summary>
    public class resetCommand : ICommand
    {
        private readonly ICanvas canvas;
        private StoredProgram program;

        /// <summary>
        /// Creates a new instance of the resetCommand.
        /// </summary>
        /// <param name="canvas">The drawing canvas to reset.</param>
        public resetCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Sets the program reference and validates that no parameters are provided.
        /// </summary>
        /// <param name="Program">The stored program.</param>
        /// <param name="Params">Parameters passed to the command (should be empty).</param>
        /// <exception cref="CommandException">Thrown if parameters are present.</exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program;

            if (!string.IsNullOrWhiteSpace(Params))
                throw new CommandException("reset takes no parameters");
        }

        /// <summary>
        /// Ensures no parameters were supplied.
        /// </summary>
        /// <param name="p">The parameter array.</param>
        /// <exception cref="CommandException">Thrown if parameters exist.</exception>
        public void CheckParameters(string[] p)
        {
            if (p.Length != 0)
                throw new CommandException("reset takes no parameters");
        }

        /// <summary>
        /// No compilation required for reset.
        /// </summary>
        public void Compile()
        {
        }

        /// <summary>
        /// Executes the reset operation by clearing the program 
        /// and resetting the canvas.
        /// </summary>
        public void Execute()
        {
            program.ResetProgram();
            //canvas.Reset();
        }
    }
}
