using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CiergeLib.Models.HomeViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CiergeLib.Pages.Home;

namespace CiergeLib.Services
{
    public class NoticeService
    {
        public void AddErrors(ModelStateDictionary modelState,
                                IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError("", error.Description);
            }
        }

        public void AddErrors(ModelStateDictionary modelState,
                                string error = "An unexpected error occured. Please try again later.")
        {
            modelState.AddModelError("", error);
        }

        public IActionResult Success(PageModel page, 
                                        string title = " ", string description = " ", bool showBackButton = false)
        {
            return page.RedirectToPage("Notice", new NoticeModel
            {
                NoticeType = NoticeType.Success,
                Title = title,
                Description = description,
                ShowBackButton = showBackButton
            });

        }

        public IActionResult Error(PageModel page,
                                        string title = " ", string description = " ", bool showBackButton = false)
        {
            return page.RedirectToPage("Notice", new NoticeModel
            {
                NoticeType = NoticeType.Error,
                Title = title,
                Description = description,
                ShowBackButton = showBackButton
            });

        }

        public IActionResult Warning(PageModel page,
                                string title = " ", string description = " ", bool showBackButton = false)
        {
            return page.RedirectToPage("Notice", new NoticeModel
            {
                NoticeType = NoticeType.Warning,
                Title = title,
                Description = description,
                ShowBackButton = showBackButton
            });

        }
    }
}
