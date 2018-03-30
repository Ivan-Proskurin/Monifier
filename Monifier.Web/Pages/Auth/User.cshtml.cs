using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;
using Monifier.Web.Auth;
using Monifier.Web.Models;
using Monifier.Web.Models.Auth;

namespace Monifier.Web.Pages.Auth
{
    public class UserModel : PageModel
    {
        private readonly ICurrentSession _currentSession;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthCommands _authCommands;

        public UserModel(ICurrentSession currentSession, IUnitOfWork unitOfWork, IAuthCommands authCommands)
        {
            _currentSession = currentSession;
            _unitOfWork = unitOfWork;
            _authCommands = authCommands;
        }

        public async Task OnGetAsync()
        {
            if (!_currentSession.IsAuthenticated)
                throw new AuthenticationException("User is not authenticated");
            var user = await _unitOfWork.GetQueryRepository<User>().GetById(_currentSession.UserId);
            User = user.ToViewModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await User.ProcessAsync(ModelState,
                async () =>
                {
                    var user = await _unitOfWork.GetQueryRepository<User>().GetById(_currentSession.UserId);
                    var newName = user.Name != User.Name ? User.Name : null;
                    if (newName != null || !User.Password.IsNullOrEmpty())
                    {
                        await _authCommands.UpdateUser(_currentSession.UserId, newName, User.Password);
                        user.Name = newName;
                        await HttpContext.SignInAsync(user);
                    }
                    return RedirectToPage("/Accounts/AccountsList");
                },
                () => Task.FromResult(Page() as IActionResult));
        }

        [BindProperty]
        public new UserViewModel User { get; set; }
    }
}