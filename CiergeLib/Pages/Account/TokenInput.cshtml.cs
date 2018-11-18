using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CiergeLib.Data;
using CiergeLib.Filters;
using CiergeLib.Models;
using CiergeLib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CiergeLib.Pages.Account
{
    public class TokenInputModel : PageModelWithHelpers
    {
        public TokenInputModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Purpose { get; set; }

        public void OnGet()
        {
        }

        [ServiceFilter(typeof(ValidateRecaptchaAttribute))]
        public async Task<IActionResult> OnPostasync()
        {
            if (!ModelState.IsValid ||
                    String.IsNullOrWhiteSpace(Email) ||
                    String.IsNullOrWhiteSpace(Purpose) ||
                    String.IsNullOrWhiteSpace(Token))
                return Page();

            var email = _userManager.NormalizeKey(Email);
            Token = Token.Replace(" ", "");

            var userWithConfirmedEmail = await _userManager.FindByLoginAsync("Email", email);
            var userCurrentlySignedIn = await _userManager.GetUserAsync(User);
            var userEmpty = new ApplicationUser()
            {
                Id = email,
                Email = email,
                SecurityStamp = "TODOREPLACE"
            };

            var isTokenValid = false;

            if (Purpose == "RegisterOrLogin") // Trying to register or login
            {
                await _signInManager.SignOutAsync();

                isTokenValid = await _userManager.VerifyUserTokenAsync(
                    userWithConfirmedEmail  // Case: logging-in
                    ?? userEmpty,           // Case: registering,
                    "Email", Purpose, Token);
            }
            else // Trying to add email
            {
                if (userCurrentlySignedIn == null) // If the user is not signed in, prompt them to, with the return url leading back here
                    return RedirectToPage("/Account/Login", new 
                    {
                        returnUrl = Request.Path + Request.QueryString
                    });

                isTokenValid = await _userManager.VerifyUserTokenAsync(
                    userCurrentlySignedIn,
                    "Email", Purpose, Token);
            }

            if (!isTokenValid)
            {
                _notice.AddErrors(ModelState, "Error validating code, it might have expired. Please try again!");
                return Page();
            }

            // Invalidates all tokens for user when trying to login or add login
            // Note: this also invalidates any attempts to add more logins than allowed
            if ((userCurrentlySignedIn ?? userWithConfirmedEmail) != null)
            {
                var updateSecStampResult = await _userManager.UpdateSecurityStampAsync(userCurrentlySignedIn ?? userWithConfirmedEmail);
                if (!updateSecStampResult.Succeeded)
                {
                    _notice.AddErrors(ModelState);
                    return Page();
                }
            }

            // Valid {token + email (user) + purpose} supplied

            if (Purpose == "RegisterOrLogin") // Trying to register or login
            {
                if (userWithConfirmedEmail == null) // Success trying to register
                {
                    var token = await _userManager.GenerateUserTokenAsync(userEmpty, "Default", "Register");

                    return RedirectToPage("/Account/Register", new RegisterModel
                    {
                        RememberMe = RememberMe,
                        Email = email,
                        UserName = GenerateUserName(email),
                        Token = token,
                        ReturnUrl = ReturnUrl
                    });
                }
                else // Success trying to login
                {
                    await _events.AddEvent(AuthEventType.Login, JsonConvert.SerializeObject(new
                    {
                        LoginProvider = "Email",
                        ProviderKey = Email
                    }), userWithConfirmedEmail);

                    await _signInManager.SignInAsync(userWithConfirmedEmail, isPersistent: RememberMe);
                }
            }
            else // Trying to add email
            {
                var userWithConfirmedEmailToAdd = await _userManager.FindByLoginAsync("Email", email);

                if (userWithConfirmedEmailToAdd == null) // Email to be added never seen before, add email to userCurrentlySignedIn
                {

                    var addLoginResult = await _userManager.AddLoginAsync(userCurrentlySignedIn,
                        new UserLoginInfo("Email", email, "Email"));

                    if (!addLoginResult.Succeeded)
                    {
                        _notice.AddErrors(ModelState, addLoginResult);
                        return Page();
                    }

                    userCurrentlySignedIn.Email = email;
                    userCurrentlySignedIn.EmailConfirmed = true;
                    var updateUserResult = await _userManager.UpdateAsync(userCurrentlySignedIn);

                    if (!updateUserResult.Succeeded)
                    {
                        _notice.AddErrors(ModelState, updateUserResult);
                        return Page();
                    }

                    await _events.AddEvent(AuthEventType.AddLogin, JsonConvert.SerializeObject(new
                    {
                        LoginProvider = "Email",
                        ProviderKey = Email
                    }), userCurrentlySignedIn);

                }
                else // Email to be added is in use
                {
                    // Note: this area is unlikely to be reached since security stamp is changed once a login is added
                    if (userWithConfirmedEmailToAdd.Id == userCurrentlySignedIn.Id) // Email is already in user's account 
                    {
                        _notice.AddErrors(ModelState, "This email is already in your account.");
                        return Page();
                    }
                    else // Email associated with another account (same user since both verified!)
                    {
                        _notice.AddErrors(ModelState, "This email is in another user's account. Try logging in using that email instead.");
                        return Page();
                    }
                }
            }

            // Success
            return RedirectToLocal(ReturnUrl);

        }

    }
}