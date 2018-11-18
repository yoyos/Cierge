using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using Newtonsoft.Json;

namespace CiergeLib.Pages.Account
{

    public class RegisterModel : PageModelWithAdditionalUserInfo
    {
        public RegisterModel(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        public bool RememberMe { get; set; }
        public string Token { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string ExternalLoginProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }
        

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid ||
                String.IsNullOrWhiteSpace(Email)) //  Note: this means that external logins not providing an email are unusable.
                return Page();

            var email = _userManager.NormalizeKey(Email);

            UserLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            var userEmpty = new ApplicationUser()
            {
                UserName = UserName,
                Email = email,
                DateCreated = DateTimeOffset.UtcNow,
                SecurityStamp = "TODOREPLACE",

                FullName = FullName,
                FavColor = FavColor, // !! ADDING FIELDS: If you want users to input field on register
            };

            userEmpty.Email = email;

            //userEmpty.FavColor = "Red"; // !! ADDING FIELDS: If you want to set default value for all registering users

            if (info == null) // User trying to register locally
            {
                userEmpty.EmailConfirmed = true;

                var userWithConfirmedEmail = await _userManager.FindByLoginAsync("Email", email);

                userEmpty.Id = email; // Only for token verification, is set to null later
                var isTokenValid = await _userManager.VerifyUserTokenAsync(userEmpty, "Default", "Register", Token);

                if (isTokenValid && userWithConfirmedEmail == null) // Supplied email is verified & user does not exist
                {
                    userEmpty.Id = null;
                    info = new UserLoginInfo("Email", userEmpty.Email, "Email");
                }
                else
                {
                    _notice.AddErrors(ModelState);
                    return Page();
                }

            }
            else // User trying to register after external login
            {
                userEmpty.EmailConfirmed = false;
            }

            var createResult = await _userManager.CreateAsync(userEmpty);
            if (createResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(userEmpty, info);
                if (addLoginResult.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(UserName); // This works because usernames are unique

                    // If this is the first user ever created, make an Administrator
                    if (_userManager.Users.Count() == 1)
                    {
                        var makeAdminResult = await _userManager.AddToRoleAsync(user, "Administrator");
                    }

                    await _events.AddEvent(AuthEventType.Register, JsonConvert.SerializeObject(new
                    {
                        LoginProvider = info?.LoginProvider ?? "Email",
                        ProviderKey = info?.ProviderKey ?? email
                    }), user);

                    await _signInManager.SignInAsync(user, isPersistent: RememberMe);
                    return RedirectToLocal(ReturnUrl); // Success
                }
                else
                {
                    _notice.AddErrors(ModelState, addLoginResult);
                }
            }
            else
            {
                _notice.AddErrors(ModelState, createResult);
            }


            await _userManager.DeleteAsync(userEmpty); // TODO: make atomic
            return Page();
        }

    }
}