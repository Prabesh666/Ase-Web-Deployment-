using BOOSE;

namespace MyWebApp.Boose
{
    public class AppCommandFactory : CommandFactory
    {
        private readonly ICanvas canvas;
        public AppCommandFactory(ICanvas canvas) => this.canvas = canvas;

        public override ICommand MakeCommand(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                throw new CommandException("Empty command");

            string c = rawName.Trim().ToLowerInvariant();

            switch (c)
            {
                // drawing
                case "moveto": return new MoveToCommand(canvas);
                case "drawto": return new DrawToCommand(canvas);
                case "circle": return new CircleCommand(canvas);
                case "rect": return new RectangleCommand(canvas);
                case "tri": return new TriangleCommand(canvas);

                case "pen": return new PenCommand(canvas);
                case "clear": return new ClearCommand(canvas);
                case "reset": return new resetCommand(canvas);
                
                case "text":
                case "write": return new WriteCommand(canvas);

                // assignment
                case "assign": return new AssignCommand();

                // variables
                case "int": return new IntCommand();
                case "real": return new RealCommand();
                case "bool":
                case "boolean": return new BooleanCommand();

                // arrays
                case "array": return new ArrayCommand();
                case "poke": return new PokeCommand();
                case "peek": return new PeekCommand();

                // flow
                case "if": return new IfCommand();
                case "else": return new ElseCommand();
                case "endif": return new EndIfCommand();

                case "while": return new WhileCommand();
                case "endwhile": return new EndWhileCommand();
                case "for": return new ForCommand();
                case "endfor": return new EndForCommand();

                // methods
                case "method": return new MethodCommand();
                case "endmethod": return new EndMethodCommand();
                case "call": return new CallCommand();
            }

            return base.MakeCommand(rawName);
        }
    }
}
