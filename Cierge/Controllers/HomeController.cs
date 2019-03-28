﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cierge.Data;
using Cierge.Models.HomeViewModels;

namespace Cierge.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User))
                return RedirectToAction(nameof(AccountController.Login), "Account");
            else
                return View();
        }

        [HttpGet]
        public IActionResult Notice(NoticeViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        [Route("health")]
        public IActionResult Health()
        {
            return Ok();
        }
    }
}