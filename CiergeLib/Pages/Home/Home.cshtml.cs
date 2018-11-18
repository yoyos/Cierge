using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CiergeLib.Data;
using CiergeLib.Models;
using CiergeLib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CiergeLib.Pages.Home
{
    public class HomeModel : PageModelWithHelpers
    {
        public HomeModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        public IActionResult OnGet()
        {
            if (!_signInManager.IsSignedIn(User))
                return RedirectToPage("/Account/Login");
            else
                return Page();
        }

    }
}