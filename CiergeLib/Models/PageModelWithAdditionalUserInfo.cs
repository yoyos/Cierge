using CiergeLib.Data;
using CiergeLib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;


namespace CiergeLib.Models
{
    public interface IAdditionalUserInfo
    {
        string UserName { get; set; }
        string FullName { get; set; }

        string FavColor { get; set; } // !! ADDING FIELDS: If you want field to exist
    }

    public partial class PageModelWithAdditionalUserInfo : PageModelWithHelpers, IAdditionalUserInfo
    {
        public PageModelWithAdditionalUserInfo(EventsService events, NoticeService notice, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, ILogger<PageModelWithHelpers> logger) : base(events, notice, configuration, userManager, signInManager, context, emailSender, logger)
        {
        }

        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Your username should be between 4 and 15 characters in length.")]
        [Display(Name = "Username", Prompt = "unique, short, no spaces")]
        public string UserName { get; set; }

        [Display(Name = "Name", Prompt = "optional full name")]
        [StringLength(20, ErrorMessage = "Your name can't be more than 20 characters.")]
        public string FullName { get; set; }


        // !! ADDING FIELDS: If If you want field to exist
        //                   Attributes used for registering & profile editing pages

        [Display(Name = "Favourite Color", Prompt = "optional")]
        [MinLength(2)]
        public string FavColor { get; set; }
    }
}
