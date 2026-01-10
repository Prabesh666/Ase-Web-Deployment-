using BOOSE;

namespace MyWebApp.Boose
{
    public class TriangleCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string sizeExpr = "";

        public TriangleCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;
            sizeExpr = (param ?? "").Trim();

            if (sizeExpr.Length == 0)
                throw new CommandException("tri <size>");
        }

        public override void Execute()
        {
            int size = ExpressionUtil.EvalInt(program, sizeExpr);
            if (size <= 0)
                throw new CommandException("Invalid triangle size");

            canvas.Tri(size, size); // width & height
        }


        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
