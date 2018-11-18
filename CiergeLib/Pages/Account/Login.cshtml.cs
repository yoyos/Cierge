using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class LoginModel : PageModelWithHelpers
    {
        public LoginModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = true;

        public bool DidReachMaxLoginsAllowed { get; set; }

        public int MaxLoginsAllowed { get; set; }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.GetUserAsync(User);

            var model = new LoginViewModel();

            if (user != null)
            {
                model.DidReachMaxLoginsAllowed = DidReachMaxLoginsAllowed(user);
                model.MaxLoginsAllowed = MaxLoginsAllowed;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            AuthOperation attemptedOperation;

            ApplicationUser userToSignTokenWith;

            var email = _userManager.NormalizeKey(model.Email);

            var userWithConfirmedEmail = await _userManager.FindByLoginAsync("Email", email);
            var userCurrentlySignedIn = await _userManager.GetUserAsync(User);

            if (userCurrentlySignedIn == null) // No locally signed-in user (trying to register or login)
            {
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                if (userWithConfirmedEmail == null) // Email not associated with any other accounts (trying to register)
                {
                    userToSignTokenWith = new ApplicationUser()
                    {
                        Id = email,
                        Email = email,
                        SecurityStamp = TemporarySecurityStamp
                    };

                    attemptedOperation = AuthOperation.Registering;
                }
                else // Email associated with an account (trying to login)
                {
                    userToSignTokenWith = userWithConfirmedEmail;
                    attemptedOperation = AuthOperation.LoggingIn;
                }
            }
            else // A user is currently locally signed-in (trying to add email)
            {
                userToSignTokenWith = userCurrentlySignedIn;

                if (userWithConfirmedEmail == null) // Email not associated with any other accounts (trying to add a novel email)
                {
                    // Check to see if user reached max logins
                    if (DidReachMaxLoginsAllowed(userCurrentlySignedIn))
                    {
                        return View(nameof(Login), new LoginViewModel
                        {
                            MaxLoginsAllowed = MaxLoginsAllowed,
                            DidReachMaxLoginsAllowed = true
                        });
                    }

                    attemptedOperation = AuthOperation.AddingNovelEmail;
                }
                else // Email associated with another user's account
                {
                    if (userWithConfirmedEmail.Id == userCurrentlySignedIn.Id) // Email already added to user's account
                    {
                        _notice.AddErrors(ModelState, "This email is already in your account.");
                        return View(model);
                    }
                    else // Email associated with another account that's not the user's
                    {
                        attemptedOperation = AuthOperation.AddingOtherUserEmail;
                    }
                }
            }

            var token = "";
            var purpose = "";

            switch (attemptedOperation)
            {
                case AuthOperation.AddingOtherUserEmail:
                    purpose = "AddEmail";
                    break;
                case AuthOperation.AddingNovelEmail:
                    purpose = "AddEmail";
                    token = await _userManager.GenerateUserTokenAsync(userToSignTokenWith, "Email", purpose);
                    break;
                case AuthOperation.Registering:
                case AuthOperation.LoggingIn:
                    purpose = "RegisterOrLogin";
                    token = await _userManager.GenerateUserTokenAsync(userToSignTokenWith, "Email", purpose);
                    break;
            }

            // Add a space every 3 characters for readability
            token = String.Concat(token.SelectMany((c, i)
                                            => (i + 1) % 3 == 0 ? $"{c} " : $"{c}")).Trim();

            var callbackUrl = Url.TokenInputLink(Request.Scheme,
                new TokenInputViewModel
                {
                    Token = token,
                    RememberMe = model.RememberMe,
                    ReturnUrl = returnUrl,
                    Email = email,
                    Purpose = purpose
                });

            // Will not wait for email to be sent
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _emailSender.SendTokenAsync(email, attemptedOperation, callbackUrl, token);
#pragma warning restore CS4014

            return View(nameof(TokenInput),
                new TokenInputViewModel
                {
                    RememberMe = model.RememberMe,
                    ReturnUrl = returnUrl,
                    Email = email,
                    Purpose = purpose
                });
        }


        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Home/Home");
        }

    }
}