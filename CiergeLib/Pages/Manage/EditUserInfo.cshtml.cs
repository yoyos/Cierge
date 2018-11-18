using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CiergeLib.Data;
using CiergeLib.Models;
using CiergeLib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CiergeLib.Pages.Manage
{
    [Authorize]
    public class EditUserInfoModel : PageModelWithAdditionalUserInfo
    {
        public EditUserInfoModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        [Required]
        [JsonIgnore]
        public IList<UserLoginInfo> Logins { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Main email")]
        [JsonIgnore]
        public string Email { get; set; }

        [JsonIgnore]
        public bool EmailConfirmed { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return _notice.Error(this);
            }

            UserName = user.UserName;
            Logins = await _userManager.GetLoginsAsync(user);
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            FullName = user.FullName;

            FavColor = user.FavColor; // !! ADDING FIELDS: If you want users to be able to edit field

            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                _notice.AddErrors(ModelState);
                return Page();
            }

            var emailToMakePrimary = _userManager.NormalizeKey(Email);
            var userLogins = await _userManager.GetLoginsAsync(user);

            Logins = userLogins; // Since model binding doesn't work with IList

            if (!ModelState.IsValid)
                return Page();

            user.UserName = UserName;
            user.FullName = FullName;

            user.FavColor = FavColor; // !! ADDING FIELDS: If you want users to be able to edit field

            // If the user's email is confirmed (ie. local login) and they provided a different email that exists, set it to the primary
            if (user.EmailConfirmed &&
                user.NormalizedEmail != emailToMakePrimary &&
                userLogins.Any(l => l.LoginProvider == "Email" && l.ProviderKey == emailToMakePrimary))
            {
                user.Email = emailToMakePrimary;
            }

            // Update sumbitted user info, including changing email if required
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                _notice.AddErrors(ModelState, updateResult);
                return Page();
            }

            await _events.AddEvent(AuthEventType.EditUserInfo,
                JsonConvert.SerializeObject(this), user);

            await _signInManager.RefreshSignInAsync(user);
            return _notice.Success(this, "Your profile has been updated.");

        }
    }
}