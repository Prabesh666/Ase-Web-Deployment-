using BOOSE;

namespace MyWebApp.Boose
{
    public class CircleCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string radiusExpr = "";

        public CircleCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;
            radiusExpr = (param ?? "").Trim();

            if (radiusExpr.Length == 0)
                throw new CommandException("circle <radius>");
        }

        public override void Execute()
        {
            int r = ExpressionUtil.EvalInt(program, radiusExpr);
            if (r <= 0)
                throw new CommandException("Invalid circle radius");

            canvas.Circle(r, false);
        }


        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
