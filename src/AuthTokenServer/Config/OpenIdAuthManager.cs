using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Server;
using AuthTokenServer.Common;
using AuthTokenServer.Exceptions;
using AuthTokenServer.ExternalLogin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthTokenServer.Config
{
    public class OpenIdAuthManager<TUser> where TUser : IdentityUser, new()
    {
        private readonly HandleTokenRequestContext _context;
        private readonly UserManager<TUser> _manager;
        private readonly ILogger _logger;

        public OpenIdAuthManager(HandleTokenRequestContext context)
        {
            this._context = context;
            _manager = context.HttpContext.RequestServices.GetService<UserManager<TUser>>();
            _logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>().CreateLogger<OpenIdAuthManager<TUser>>();
        }

        public async Task<TUser> Authenticate()
        {
            switch (_context.Request.GrantType.ToLower())
            {
                case "password":
                    return await AuthenticateLocal();
                case "external":
                    return await AuthenticateExternal();
                default:
                    return null;
            }

        }

        private async Task<TUser> AuthenticateLocal()
        {
            var user = await _manager.FindByNameAsync(_context.Request.Username);

            if (user == null)
                return null;

            // Ensure the user is not already locked out.
            if (_manager.SupportsUserLockout && await _manager.IsLockedOutAsync(user))
                return null;

            // Ensure the password is valid.
            if (!await _manager.CheckPasswordAsync(user, _context.Request.Password))
            {
                if (_manager.SupportsUserLockout)
                {
                    await _manager.AccessFailedAsync(user);
                }
                return null;
            }

            if (_manager.SupportsUserLockout)
            {
                await _manager.ResetAccessFailedCountAsync(user);
            }

            return user;
        }


        private async Task<TUser> AuthenticateExternal()
        {
            var accessToken = _context.Request.AccessToken;
            var identityProvider = _context.Request.GetProperty<string>("identity_provider");
            if (string.IsNullOrEmpty(accessToken))
                throw new AuthException("AccessToken is missing. Parameter access_token can't be empty");
            if (string.IsNullOrEmpty(identityProvider))
                throw new AuthException("IdentityProvider is missing. Parameter identity_provider can't be empty");

            IExternalHandler handler;
            switch (identityProvider.ToLower())
            {
                case "facebook":
                    handler = _context.HttpContext.RequestServices.GetService<FacebookHandler>();
                    break;
                case "google":
                    handler = _context.HttpContext.RequestServices.GetService<GoogleIdTokenHandler>();
                    break;
                default:
                    throw new AuthException($"IdentityProvider exception: IdentityProvider {identityProvider} is not supported");
            }

            try
            {
                var externalUser = await handler.GetUserInfo(accessToken);
                var user = await _manager.FindByLoginAsync(identityProvider, externalUser.Id) ??
                           await RegisterUser(identityProvider, externalUser);
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        private async Task<TUser> RegisterUser(string identityProvider, UserInfo externalUser)
        {
            //var login = externalUser.Email ?? $"{loginProvider}_{externalUser.Id}";
            var login = $"{identityProvider}_{externalUser.Id}";

            var user = await _manager.FindByNameAsync(login);
            if (user == null)
            {
                user = new TUser() { UserName = login, Email = externalUser.Email };
                var claim = new IdentityUserClaim<string>()
                {
                    ClaimType = ClaimTypes.Role,
                    ClaimValue = UserRoles.Social
                };
                user.Claims.Add(claim);


                // TODO: add other fields to user
                var createResult = await _manager.CreateAsync(user);
                if (!createResult.Succeeded) throw GenException(createResult.Errors);
            }

            // var principal = await _signInManager.CreateUserPrincipalAsync(user);
            ExternalLoginInfo info = new ExternalLoginInfo(null, identityProvider, externalUser.Id, null);
            var result = await _manager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                await _manager.DeleteAsync(user);
                throw GenException(result.Errors);
            }
            return user;
        }

        private Exception GenException(IEnumerable<IdentityError> resultErrors)
        {
            var errorMessage = string.Join("; ", resultErrors.Select(x => $"{x.Code}: {x.Description}"));
            _logger.LogError(errorMessage);
            return new Exception(errorMessage);
        }

    }
}