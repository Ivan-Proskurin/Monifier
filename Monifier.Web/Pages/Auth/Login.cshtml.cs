using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.Web.Auth;
using Monifier.Web.Models;
using Monifier.Web.Models.Auth;
using Monifier.Web.Models.Validation;

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
            return await Login.ProcessAsync(ModelState,
                () => Task.FromResult<IActionResult>(RedirectToPage(Login.ReturnUrl)),
                () => Task.FromResult<IActionResult>(Page()),

                async vrList =>
                {
                    try
                    {
                        var session = await _sessionCommands.CreateSession(Login.UserName, Login.Password);
                        var claims = new List<Claim>
                        {
                            new Claim(MonifierClaimTypes.SessionId, session.Token.ToString()),
                            new Claim(MonifierClaimTypes.UserId, session.UserId.ToString()),
                            new Claim(ClaimTypes.Name, session.User.Name)
                        };
                        var identity = new ClaimsIdentity(claims, AuthConsts.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(AuthConsts.AuthenticationScheme, principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = session.Expiration != null,
                                ExpiresUtc = session.Expiration
                            });
                    }
                    catch (AuthException exc)
                    {
                        vrList.Add(new ModelValidationResult(exc.Message));
                    }
                });
        }
    }
}