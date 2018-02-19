using Microsoft.AspNetCore.Http;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;

namespace Monifier.Web.Auth
{
    public class CurrentSession : ICurrentSession
    {
        private readonly bool _isAuthenticated;
        private readonly int _userId;

        public CurrentSession(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext.User;
            _isAuthenticated = user.Identity.IsAuthenticated;
            var userIdClaim = user.FindFirst(MonifierClaimTypes.UserId); 
            if (_isAuthenticated && userIdClaim != null)
                int.TryParse(userIdClaim.Value, out _userId);
        }

        public bool IsAuthenticated => _isAuthenticated;
        public int UserId => _isAuthenticated ? _userId : throw new AuthException("Not authenticated");
    }
}