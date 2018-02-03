using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DemoClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace DemoClient.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            string accessToken = "";
            string idToken = "";
            if (User.Identity.IsAuthenticated)
            {
                accessToken = await HttpContext.GetTokenAsync("access_token");
                idToken = await HttpContext.GetTokenAsync("id_token");
            }

            return View(new Tokens {
                AccessToken = accessToken,
                IdToken = idToken
            });
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult AdminArea()
        {
            return View();
        }
        
        [ActionName("signout-oidc")]
        public IActionResult Signout()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
