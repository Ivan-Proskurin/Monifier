using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Monifier.Web.Pages.Auth
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public Task OnGetASync()
        {
            return Task.CompletedTask;
        }
    }
}