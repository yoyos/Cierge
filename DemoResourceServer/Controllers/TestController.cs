using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemoResourceServer.Controllers
{
    public class User
    {
        public string Id { get; set; }
    }

    [Route("[controller]/[action]")]
    public class TestController : Controller
    {
        private readonly UserManager<User> _userManager;

        protected TestController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Open(string text)
        {
            return Ok(text);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Closed(string text)
        {
            return Ok($"Text: {text}" +
                      $"\nUserId: {_userManager.GetUserId(User)}" +     // No DB calls
                      $"\nUsername: {_userManager.GetUserName(User)}");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Admins(string text)
        {
            return Ok($"Text: {text}" +
                      $"\nUserId: {_userManager.GetUserId(User)}" +
                      $"\nUsername: {_userManager.GetUserName(User)}");
        }
    }
}
