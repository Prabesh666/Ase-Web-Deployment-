using BOOSE;

namespace MyWebApp.Boose
{
    /// <summary>
    /// Command that clears the canvas to its default background color.
    /// </summary>
    public class ClearCommand : ICommand
    {
        /// <summary>
        /// Reference to the canvas where drawing operations occur.
        /// </summary>
        private readonly ICanvas canvas;

        /// <summary>
        /// Creates a new instance of the <see cref="ClearCommand"/> class.
        /// </summary>
        /// <param name="canvas">The canvas that will be cleared.</param>
        public ClearCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Sets command configuration and validates parameters.
        /// The clear command does not support parameters.
        /// </summary>
        /// <param name="Program">Stored program context (ignored).</param>
        /// <param name="Params">Parameter string (should be empty).</param>
        /// <exception cref="CommandException">Thrown when parameters are supplied.</exception>
        public void Set(StoredProgram Program, string Params)
        {
            if (!string.IsNullOrWhiteSpace(Params))
                throw new CommandException("clear takes no parameters");
        }

        /// <summary>
        /// Validates that no parameters were provided.
        /// </summary>
        /// <param name="Parameters">Array of parameters.</param>
        /// <exception cref="CommandException">Thrown when parameters exist.</exception>
        public void CheckParameters(string[] Parameters)
        {
            if (Parameters.Length != 0)
                throw new CommandException("clear has no parameters");
        }

        /// <summary>
        /// No compile-time behavior is needed for the clear command.
        /// </summary>
        public void Compile() { }

        /// <summary>
        /// Executes the clear operation on the canvas.
        /// </summary>
        public void Execute()
        {
            canvas.Clear();
        }
    }
}
