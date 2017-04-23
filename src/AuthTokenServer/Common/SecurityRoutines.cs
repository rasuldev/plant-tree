using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthTokenServer.Common
{
    public class SecurityRoutines
    {
        /// <summary>
        /// If header contains Authorization then this request should be considered as authorized request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsRequestAuthorized(HttpContext context)
        {
            return context.Request.Headers["Authorization"].ToString().ToLower().StartsWith("bearer ");
        }

        public static string GetUserId(HttpContext context)
        {
            return context.User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
        }

        public static string GetUserEmail(HttpContext context)
        {
            return context.User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
        }

        public static async Task<bool> IsUserLocal<TUser>(HttpContext context) where TUser : IdentityUser
        {
            var userManager = context.RequestServices.GetService<UserManager<TUser>>();
            var applicationUser = await userManager.GetUserAsync(context.User);
            return !string.IsNullOrEmpty(applicationUser.PasswordHash);

            //context.User.HasClaim(ClaimTypes.Role, UserRoles.Local)
        }

    }
}