using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Services;

namespace MyWebApp.Controllers;

public class HomeController : Controller
{
    private readonly BooseRunner _runner;

    public HomeController(BooseRunner runner)
    {
        _runner = runner;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new RunViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(RunViewModel model)
    {
        var result = _runner.Run(model.ProgramText, model.SingleCommand);

        model.IsError = !result.Ok;
        model.StatusMessage = result.Message;

        if (result.Ok)
        {
            model.OutputImageBase64 = result.ImageBase64;
        }

        return View(model);
    }
}
