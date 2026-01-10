using BOOSE;

namespace MyWebApp.Boose
{
    public class DrawToCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string xExpr = "";
        private string yExpr = "";

        public DrawToCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;

            var parts = (param ?? "").Replace(",", " ")
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new CommandException("drawto <x> <y>");

            xExpr = parts[0];
            yExpr = parts[1];
        }

        public override void Execute()
        {
            int x = ExpressionUtil.EvalInt(program, xExpr);
            int y = ExpressionUtil.EvalInt(program, yExpr);
            canvas.DrawTo(x, y);
        }

        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
