using BOOSE;

namespace MyWebApp.Boose
{
    public class PenCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string rExpr = "", gExpr = "", bExpr = "";

        public PenCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;

            var parts = (param ?? "").Replace(",", " ")
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
                throw new CommandException("pen <r> <g> <b>");

            rExpr = parts[0];
            gExpr = parts[1];
            bExpr = parts[2];
        }

        public override void Execute()
        {
            int r = ExpressionUtil.EvalInt(program, rExpr);
            int g = ExpressionUtil.EvalInt(program, gExpr);
            int b = ExpressionUtil.EvalInt(program, bExpr);

            canvas.SetColour(r, g, b);
        }

        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
