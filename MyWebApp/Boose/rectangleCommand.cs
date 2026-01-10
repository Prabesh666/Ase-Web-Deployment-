using BOOSE;

namespace MyWebApp.Boose
{
    public class RectangleCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string widthExpr = "";
        private string heightExpr = "";
        private bool filled;

        public RectangleCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;

            var parts = (param ?? "").Replace(",", " ")
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || parts.Length > 3)
                throw new CommandException("rect <width> <height> [true|false]");

            widthExpr = parts[0];
            heightExpr = parts[1];
            filled = parts.Length == 3 && bool.Parse(parts[2]);
        }

        public override void Execute()
        {
            int w = ExpressionUtil.EvalInt(program, widthExpr);
            int h = ExpressionUtil.EvalInt(program, heightExpr);

            if (w <= 0 || h <= 0)
                throw new CommandException("Invalid rectangle dimensions");

            canvas.Rect(w, h, filled);
        }

        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
