using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AuthTokenServer.Common;
using AuthTokenServer.Config;
using AuthTokenServer.ExternalLogin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthTokenServer
{
    public static class TokenServer
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
                        {
                            options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                            options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                            //options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                        });
            // External login configuration
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            services.AddSingleton(client);

            services.AddSingleton(new FacebookOptions()
            {
                AppId = configuration["Authentication:Facebook:AppId"],
                AppSecret = configuration["Authentication:Facebook:AppSecret"],
                Fields = { "id", "name", "birthday", "gender", "email", "birthday" },
            });
            services.AddSingleton<FacebookHandler>();

            services.AddSingleton(new GoogleOptions()
            {
                ClientId = configuration["Authentication:Google:ClientId"],
                ClientSecret = configuration["Authentication:Google:ClientSecret"]
            });
            //services.AddSingleton<GoogleHandler>();
            services.AddSingleton<GoogleIdTokenHandler>();
        }

        public static void Configure<TUser>(IApplicationBuilder app, string certificatePath, string certificatePassword,
            Func<ApplyTokenResponseContext, Task> onApplyTokenResponse = null)
            where TUser : IdentityUser, new()
        {
            // Calling app.UseOAuthValidation() will register the middleware
            // in charge of validating the bearer tokens issued by ASOS.
            app.UseOAuthValidation();

            //app.Use(async (context, next) =>
            //{
            //    context.Response.OnStarting(() =>
            //    {
            //        Console.WriteLine(context.GetOpenIdConnectResponse());
            //        //context.SetOpenIdConnectResponse(null);
            //        //var inst = context.Features.Get<OpenIdConnectServerFeature>();
            //        //context.Response.Clear();
            //        //context.Response.Body = new MemoryStream();
            //        //context.Response.Body.SetLength(0);

            //        //context.Response.WriteAsync("OK");
            //        return Task.FromResult(0);
            //    });


            //    await next.Invoke();

            //});


            app.UseOpenIdConnectServer(options =>
            {
                // Create your own authorization provider by subclassing
                // the OpenIdConnectServerProvider base class.
                options.Provider = new AuthorizationProvider<TUser>();

                if (onApplyTokenResponse != null)
                    options.Provider.OnApplyTokenResponse = onApplyTokenResponse;

                // Enable the authorization and token endpoints.
                options.AuthorizationEndpointPath = "/api/connect/authorize" + "";

                options.SigningCredentials.AddCertificate(Misc.GetX509Certificate2(certificatePath, certificatePassword));
                // To obtain access_token you should send POST request with x-www-form-url-encoded body that contains
                // params: username, password, grant_type='password', scope='opedid offline_access'.
                // If you have refresh_token, then you can get access_token by sending POST request
                // with x-www-form-url-encoded body that contains params:
                // grant_type='refresh_token', refresh_token='<refresh_token>'
                options.TokenEndpointPath = "/api/connect/token";

                // During development, you can set AllowInsecureHttp
                // to true to disable the HTTPS requirement.
                options.AllowInsecureHttp = true;
                options.RefreshTokenLifetime = TimeSpan.FromDays(30);

                // Note: uncomment this line to issue JWT tokens.
                // options.AccessTokenHandler = new JwtSecurityTokenHandler();
                //options.AccessTokenLifetime = TimeSpan.FromSeconds(5);
            });


        }
    }
}