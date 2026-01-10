using BOOSE;

namespace MyWebApp.Boose
{
    public class WriteCommand : Command, ICommand
    {
        private readonly ICanvas canvas;
        private string expr = "";

        public WriteCommand(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public override void Set(StoredProgram program, string param)
        {
            this.program = program;
            expr = (param ?? "").Trim();

            if (expr.Length == 0)
                throw new CommandException("write <expression>");
        }

        public override void Execute()
        {
            // allow expressions like "£" + y
            string text;
            try
            {
                double val = ExpressionUtil.EvalDouble(program, expr);
                text = val.ToString();
            }
            catch
            {
                text = expr.Trim('"');
            }

            canvas.WriteText(text);
        }

        public override void Compile() { }
        public override void CheckParameters(string[] p) { }
    }
}
