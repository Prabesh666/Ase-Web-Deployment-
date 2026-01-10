using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BOOSE;
using MyWebApp.Boose;

namespace MyWebApp.Services;

public sealed class BooseRunner
{
    public BooseRunResult Run(string? programText, string? singleCommand)
    {
        var canvas = new canvasApp();
        var factory = new AppCommandFactory(canvas);
        var program = new ExtendedStoredProgram(canvas);
        var parser = new ExtendedParser(factory, program);

        string programInput = (programText ?? string.Empty).Trim();
        string oneLine = (singleCommand ?? string.Empty).Trim();

        try
        {
            canvas.Clear();
            program.Clear();

            if (!string.IsNullOrWhiteSpace(oneLine))
            {
                ICommand command = parser.ParseCommand(oneLine);
                if (command == null)
                {
                    return BooseRunResult.Fail("No command produced.");
                }

                command.Execute();
                return BooseRunResult.Success(ToPngBytes(canvas), "Command executed successfully.");
            }

            if (string.IsNullOrWhiteSpace(programInput))
            {
                return BooseRunResult.Fail("Program text cannot be empty.");
            }

            parser.ParseProgram(programInput);
            program.Run();

            return BooseRunResult.Success(ToPngBytes(canvas), "Program executed successfully.");
        }
        catch (Exception ex)
        {
            return BooseRunResult.Fail(ex.Message);
        }
    }

    private static byte[] ToPngBytes(canvasApp canvas)
    {
        var bmp = (Bitmap)canvas.getBitmap();
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}

public sealed record BooseRunResult(bool Ok, string Message, byte[]? ImageBytes)
{
    public static BooseRunResult Success(byte[] imageBytes, string message) => new(true, message, imageBytes);
    public static BooseRunResult Fail(string message) => new(false, message, null);

    public string? ImageBase64 => ImageBytes is null ? null : Convert.ToBase64String(ImageBytes);
}
