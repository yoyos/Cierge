using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CiergeLib.Data;
using CiergeLib.Models;
using CiergeLib.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CiergeLib.Pages.Account
{
    public class ExternalLoginModel : PageModelWithHelpers
    {
        public ExternalLoginModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }
        
        [Authorize]
        public async Task<IActionResult> OnPostAsync(string provider, string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); // Clear the existing external cookie to ensure a clean login process

            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("Account/ExternalLogin", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        // Callback
        public void OnGet(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
                return RedirectToAction(nameof(Login));

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return RedirectToAction(nameof(Login));

            var userWithExternalLogin = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var userCurrentlySignedIn = await _userManager.GetUserAsync(User);


            var emailFromExternalLoginProvider = _userManager.NormalizeKey(info.Principal.FindFirstValue(ClaimTypes.Email));
            var nameFromExternalLoginProvider = _userManager.NormalizeKey(info.Principal.FindFirstValue(ClaimTypes.Name));

            if (userCurrentlySignedIn == null) // No locally signed-in user (trying to register or login)
            {
                if (userWithExternalLogin != null) // User exists and attempting to login
                {
                    var externalLoginResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

                    if (externalLoginResult.Succeeded) // Success logging in
                    {
                        await _events.AddEvent(AuthEventType.Login, JsonConvert.SerializeObject(new
                        {
                            info.LoginProvider,
                            info.ProviderKey
                        }), userWithExternalLogin);

                        return RedirectToLocal(returnUrl);
                    }

                    if (externalLoginResult.IsLockedOut || externalLoginResult.IsNotAllowed)
                        return RedirectToAction(nameof(Lockout));
                }

                // TODO: what if `await _userManager.FindByLoginAsync(ANY, info.ProviderKey)` exists?
                //       ie. email of external login is already associated with full account
                //       currently, to avoid leaking profile existence, this is ignored.

                // The user does not have an account, is attempting to register
                return View(nameof(Register), new RegisterViewModel
                {
                    Email = emailFromExternalLoginProvider,
                    UserName = GenerateUserName(nameFromExternalLoginProvider),
                    ExternalLoginProviderDisplayName = info.ProviderDisplayName,
                    ReturnUrl = returnUrl
                });

            }
            else // A user is currently locally signed-in (trying to add external login)
            {

                if (userWithExternalLogin != null) // External login already in use
                {
                    if (userWithExternalLogin.Id == userCurrentlySignedIn.Id) // External login is already in user's account
                    {
                        _notice.AddErrors(ModelState, "This external login is already in your account.");
                        return View(nameof(Login));
                    }
                    else
                    {
                        _notice.AddErrors(ModelState, "This external login is in another user's account. Try loggin out then back in with that instead.");
                        return View(nameof(Login));
                    }
                }

                // Check to see if user reached max logins
                if (DidReachMaxLoginsAllowed(userCurrentlySignedIn))
                {
                    return View(nameof(Login), new LoginViewModel
                    {
                        MaxLoginsAllowed = MaxLoginsAllowed,
                        DidReachMaxLoginsAllowed = true
                    });
                }

                // If email is not confirmed then update their unconfirmed email
                if (!String.IsNullOrWhiteSpace(emailFromExternalLoginProvider) &&
                    userCurrentlySignedIn.EmailConfirmed == false)
                {
                    userCurrentlySignedIn.Email = emailFromExternalLoginProvider;
                    userCurrentlySignedIn.EmailConfirmed = false;
                }

                var addLoginResult = await _userManager.AddLoginAsync(userCurrentlySignedIn, info);
                if (addLoginResult.Succeeded)
                {
                    var updateResult = await _userManager.UpdateAsync(userCurrentlySignedIn);
                    if (updateResult.Succeeded)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); // Clear the existing external cookie to ensure a clean login process

                        await _events.AddEvent(AuthEventType.AddLogin, JsonConvert.SerializeObject(new
                        {
                            info.LoginProvider,
                            info.ProviderKey
                        }), userCurrentlySignedIn);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        _notice.AddErrors(ModelState, updateResult);
                    }
                }
                else
                {
                    _notice.AddErrors(ModelState, addLoginResult);
                }

                return View(nameof(Login));

            }
        }

    }
}