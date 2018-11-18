using CiergeLib.Data;
using CiergeLib.Models.HomeViewModels;
using CiergeLib.Pages.Home;
using CiergeLib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace CiergeLib.Models
{
    public partial class PageModelWithHelpers : PageModel
    {

        protected readonly EventsService _events;
        protected readonly NoticeService _notice;
        protected readonly IConfiguration _configuration;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly ApplicationDbContext _context;
        protected readonly IEmailSender _emailSender;
        protected readonly ILogger _logger;

        public PageModelWithHelpers(
            EventsService events,
            NoticeService notice,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<PageModelWithHelpers> logger)
        {
            _events = events;
            _notice = notice;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;


        }

        #region Helpers

        public IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToPage("Notice", "Home", new NoticeModel
                {
                    NoticeType = NoticeType.Success,
                    Title = " ",
                    Description = " ",
                    ShowBackButton = false
                });
            }
        }

        public int MaxLoginsAllowed
        {
            get
            {
                return Int32.Parse(_configuration["Cierge:Logins:MaxLoginsAllowed"] ?? "5");
            }
        }

        public bool DidReachMaxLoginsAllowed(ApplicationUser user)
        {
            // Check to see if user reached max logins

            var userLoginCount = _context.UserLogins.Count(l => l.UserId == user.Id);
            return userLoginCount >= MaxLoginsAllowed;
        }

        public string GenerateUserName(string name)
        {
            name = name.Split('@')[0] ?? "";
            Regex rgx = new Regex("[^a-zA-Z0-9_-]");
            return rgx.Replace(name, "").ToLower();
        }

        #endregion
    }
}
