using System.IO;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthTokenServer;
using Common.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlantTree.Data.Entities;

namespace PlantTree.Infrastructure.Common
{
    public static class TokenServerExtensions
    {
        public static IApplicationBuilder UseTokenServer(this IApplicationBuilder app, string certPath, string certPassword)
        {
            return app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {
                TokenServer.Configure<ApplicationUser>(branch, certPath, certPassword, async context =>
                {
                    // var settings = branch.ApplicationServices.GetService<JsonSerializerSettings>();
                    // JsonSerializer.Create(settings).;
                    // Intercept response to change default error format
                    context.HttpContext.Response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                    context.HttpContext.Response.StatusCode = 400;
                    var errorType = ApiErrorTypes.System;
                    if (context.Response.ErrorDescription == "Invalid credentials.")
                        errorType = ApiErrorTypes.User; 
                    ApiError error = new ApiError(context.Response.ErrorDescription, context.Response.Error, errorType);
                    var response = JsonConvert.SerializeObject(new[] { error }, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    await context.HttpContext.Response.WriteAsync(response);
                    //context.HttpContext.Response.
                    context.HandleResponse();

                    //return Task.FromResult<object>(null);
                    //return Task.CompletedTask;
                    //return base.OnApplyTokenResponse(context);
                });
                
            });
        }
    }
}