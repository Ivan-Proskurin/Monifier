using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.Web.Auth
{
    public static class AuthHelper
    {
        public static async Task SignInAsync(this HttpContext httpContext, User user, string timeZoneOffset)
        {
            var claims = new List<Claim>
            {
                new Claim(MonifierClaimTypes.UserId, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(MonifierClaimTypes.TimeZoneOffset, timeZoneOffset)
            };
            var identity = new ClaimsIdentity(claims, AuthConsts.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(AuthConsts.AuthenticationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(2)
                });
        }
    }
}