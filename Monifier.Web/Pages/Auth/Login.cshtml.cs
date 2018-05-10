using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.Common.Extensions;
using Monifier.Common.Validation;
using Monifier.Web.Auth;
using Monifier.Web.Models;
using Monifier.Web.Models.Auth;

namespace Monifier.Web.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ISessionCommands _sessionCommands;

        public LoginModel(ISessionCommands sessionCommands)
        {
            _sessionCommands = sessionCommands;
        }
        
        [BindProperty]
        public Login Login { get; set; }
        
        public void OnGet(string returnUrl)
        {
            Login = new Login { ReturnUrl = returnUrl };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Login.ProcessAsync(ModelState, nameof(Login),
                () => Task.FromResult<IActionResult>(Redirect(Login.ReturnUrl ?? "/")),
                () => Task.FromResult<IActionResult>(Page()),

                async vrList =>
                {
                    try
                    {
                        if (!Login.UserName.IsNullOrEmpty() && !Login.Password.IsNullOrEmpty())
                        {
                            var user = await _sessionCommands.CheckUser(Login.UserName, Login.Password);
                            await HttpContext.SignInAsync(user, Login.TimeZoneOffset);
                        }
                    }
                    catch (AuthException exc)
                    {
                        vrList.Add(new ModelValidationResult(string.Empty, exc.Message));
                    }
                });
        }
    }
}