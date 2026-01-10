using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models;

public sealed class RunViewModel
{
    [Display(Name = "Program Window")]
    public string ProgramText { get; set; } = "";

    [Display(Name = "Single Command")]
    public string SingleCommand { get; set; } = "";

    public string StatusMessage { get; set; } = "";
    public bool IsError { get; set; }

    // Base64 PNG (without data URI prefix)
    public string? OutputImageBase64 { get; set; }
}
