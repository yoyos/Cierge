using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CiergeLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CiergeLib.Pages.Home
{
    public class NoticeModel : PageModel
    {
        public NoticeType NoticeType { get; set; }

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Title { get; set; } = "An unexpected error occured.";

        [Display(Name = "Description")]
        public string Description { get; set; } = "Please try again later.";

        public bool ShowBackButton { get; set; } = false;

        public void OnGet()
        {
        }

    }
}