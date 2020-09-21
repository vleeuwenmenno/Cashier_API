using Cashier_API;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace RazorPagesIntro.Pages
{
    public class InvoicePage : PageModel
    {
        // public string Message { get; private set; } = "PageModel in C#";

        public void OnGet()
        {
            // Message += Program.options.GetJson();
        }
    }
}