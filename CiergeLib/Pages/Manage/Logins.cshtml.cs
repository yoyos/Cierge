using System;
using System.Collections.Generic;
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
    public class LoginsModel : PageModelWithHelpers
    {
        public LoginsModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        public IList<UserLoginInfo> Logins { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return _notice.Error(this);
            }

            Logins = await _userManager.GetLoginsAsync(user);
            return Page();
        }
        
        [BindProperty]
        public RemoveLoginModel RemoveLoginModel { get; set; }

        public async Task<IActionResult> OnPostRemoveLoginAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                _notice.AddErrors(ModelState);
                return Page();
            }

            // If user is removing last/only login or
            // if user is trying to remove thier only confirmed email,
            // then delete account.
            // This prevents users from ever "unconfirming" themselves.
            // Why: there should always be atleast 1 other confirmed
            // email to fallback to as the confirmed email when deleting the other
            // This is also checked for in the view.
            var userLogins = await _userManager.GetLoginsAsync(user);
            if (userLogins.Count == 1 ||
               (userLogins.Where(l => l.LoginProvider == "Email").Count() == 1 && RemoveLoginModel.LoginProvider == "Email"))
            {
                //await _events.AddEvent(AuthEventType.Delete,
                //    JsonConvert.SerializeObject(model), user);

                var deleteResult = await _userManager.DeleteAsync(user);
                if (deleteResult.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    return _notice.Success(this, "Your account has been successfully deleted."); // TODO: keep account for n days feature
                }
                else
                {
                    _notice.AddErrors(ModelState);
                    return Page();
                }
            }

            // If user trying to remove their primary email then find another one.
            if (RemoveLoginModel.LoginProvider == "Email" && (user.NormalizedEmail == _userManager.NormalizeKey(RemoveLoginModel.ProviderKey)))
            {
                var fallbackPrimaryEmailLogin = userLogins.FirstOrDefault(l => l.LoginProvider == "Email" && l.ProviderKey != RemoveLoginModel.ProviderKey);

                if (fallbackPrimaryEmailLogin == null) // This should never happen thanks to the check above.
                {
                    _notice.AddErrors(ModelState);
                    return Page();
                }

                user.Email = fallbackPrimaryEmailLogin.ProviderKey;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _notice.AddErrors(ModelState);
                    return Page();
                }
            }

            await _events.AddEvent(AuthEventType.RemoveLogin,
                JsonConvert.SerializeObject(RemoveLoginModel), user);

            var removeLoginResult = await _userManager.RemoveLoginAsync(user, RemoveLoginModel.LoginProvider, RemoveLoginModel.ProviderKey);
            if (!removeLoginResult.Succeeded)
            {
                _notice.AddErrors(ModelState);
                return Page();
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            return _notice.Success(this, "Login successfully removed.");
        }
    }
    
}