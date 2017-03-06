using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using AspNet.Security.OpenIdConnect.Extensions;
using AuthTokenServer.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AuthTokenServer.Config
{
    public sealed class AuthorizationProvider<TUser> : OpenIdConnectServerProvider where TUser : IdentityUser, new()
    {
        // Implement OnValidateAuthorizationRequest to support interactive flows (code/implicit/hybrid).
        //public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        //{

        //}

        // Implement OnValidateTokenRequest to support flows using the token endpoint
        // (code/refresh token/password/client credentials/custom grant).
        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            if (!context.Request.IsPasswordGrantType() && !context.Request.IsRefreshTokenGrantType()
                && !context.Request.IsClientCredentialsGrantType() && context.Request.GrantType != "external")
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only grant_type=password and refresh_token " +
                                 "requests are accepted by this server.");

                return;
            }

            // Reject the token request if client_id or client_secret is missing.
            //if (string.IsNullOrEmpty(context.ClientId) || string.IsNullOrEmpty(context.ClientSecret))
            //{
            //    context.Reject(
            //        error: OpenIdConnectConstants.Errors.InvalidRequest,
            //        description: "Missing credentials: ensure that your credentials were correctly " +
            //                     "flowed in the request body or in the authorization header");
            //}

            // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
            // a key derivation function like PBKDF2 to slow down the secret validation process.
            // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
            // For that, you can use the CryptoHelper library developed by @henkmollema:
            // https://github.com/henkmollema/CryptoHelper. If you don't need .NET Core support,
            // SecurityDriven.NET/inferno is a rock-solid alternative: http://securitydriven.net/inferno/
            //if (string.Equals(context.ClientId, "client_id", StringComparison.Ordinal) &&
            //    string.Equals(context.ClientSecret, "client_secret", StringComparison.Ordinal))
            //{
            //    context.Validate();

            //    return;
            //}

            context.Skip();
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            // Resolve ASP.NET Core Identity's user manager from the DI container.
            var manager = context.HttpContext.RequestServices.GetService<UserManager<TUser>>();

            // Only handle grant_type=password requests and let ASOS
            // process grant_type=refresh_token requests automatically.
            if (context.Request.IsPasswordGrantType() || context.Request.GrantType == "external")
            {
                var authManager = new OpenIdAuthManager<TUser>(context);
                TUser user = null;
                try
                {
                    user = await authManager.Authenticate();
                }
                catch (AuthException e)
                {
                    // TODO: hide exception details from api result
                    context.Reject(error: nameof(AuthException), description: e.Message);
                    return;
                }
                catch (Exception e)
                {
                    // TODO: hide exception details from api result
                    context.Reject(error: e.Message);
                    return;
                }

                if (user == null)
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Invalid credentials.");
                    return;
                }

                // Reject the token request if two-factor authentication has been enabled by the user.
                if (manager.SupportsUserTwoFactor && await manager.GetTwoFactorEnabledAsync(user))
                {
                    context.Reject(
                        error: OpenIdConnectConstants.Errors.InvalidGrant,
                        description: "Two-factor authentication is required for this account.");

                    return;
                }

                var ticket = GenerateTicketWithToken(user.Id, await manager.GetUserNameAsync(user));
                context.Validate(ticket);
            }
        }




        public static AuthenticationTicket GenerateTicketWithToken(string userId, string userName)
        {
            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var authScheme = OpenIdConnectServerDefaults.AuthenticationScheme;
            var identity = new ClaimsIdentity(authScheme);

            // Copy the unique identifier associated with the logged-in user to the new identity.
            // Note: the name identifier is always included in both identity and
            // access tokens, even if an explicit destination is not specified.
            identity.AddClaim(ClaimTypes.NameIdentifier, userId);

            // When adding custom claims, you MUST specify one or more destinations.
            // Read "part 7" for more information about custom claims and scopes.
            identity.AddClaim(ClaimTypes.Name, userName,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                authScheme);

            // Set the list of scopes granted to the client application.
            // Note: this sample always grants the "openid", "email" and "profile" scopes
            // when they are requested by the client application: a real world application
            // would probably display a form allowing to select the scopes to grant.
            ticket.SetScopes(
                /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                /* email: */ OpenIdConnectConstants.Scopes.Email,
                /* profile: */ OpenIdConnectConstants.Scopes.Profile,
                OpenIdConnectConstants.Scopes.OfflineAccess);

            // Set the resource servers the access token should be issued for.
            ticket.SetResources("resource_server");

            // Returning a SignInResult will ask ASOS to serialize the specified identity
            // to build appropriate tokens. You should always make sure the identities
            // you return contain ClaimTypes.NameIdentifier claim. In this sample,
            // the identity always contains the name identifier returned by the external provider.
            return ticket;
            // return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

    }
}