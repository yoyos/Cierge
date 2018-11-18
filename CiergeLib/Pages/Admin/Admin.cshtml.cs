using CiergeLib.Data;
using CiergeLib.Models;
using CiergeLib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CiergeLib.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminModel : PageModelWithHelpers
    {
        public AdminModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        public int UserCount { get; set; }

        [Display(Name = "Search users...")]
        public string SearchTerm { get; set; }

        public IList<ApplicationUser> Users { get; set; }

        public void OnGet()
        {
            Users = _context.Users.Take(10).OrderBy(u => u.DateCreated).ToList();
            UserCount = _userManager.Users.Count();
        }

        public void OnPost()
        {
            var searchTerm = _userManager.NormalizeKey(SearchTerm);

            Users = _context.Users.Where(u => u.NormalizedUserName.Contains(searchTerm) ||
                                                   u.NormalizedEmail.Contains(searchTerm) ||
                                                   u.FullName.Contains(searchTerm) ||
                                                   u.Id.Contains(searchTerm))
                                        .ToList();
            UserCount = Users.Count();
        }

        // TODO
        /*
        [HttpGet]
        public async Task<IActionResult> Lockout(string userName, int minutes = 120)
        {
            var user = await _userManager.FindByNameAsync(userName);

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(minutes)));

            return _notice.Success(this, $"User {user.UserName} locked out for {minutes} minutes.");
        }

        [HttpGet]
        public async Task<IActionResult> Impersonate(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, false);

            return _notice.Success(this, $"You are now logged-in as {user.UserName}.", "Don't forget to log out later.");
        }
        */
    }
}