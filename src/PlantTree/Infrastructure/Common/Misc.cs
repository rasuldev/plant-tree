using System;
using System.Globalization;
using System.Threading.Tasks;
using AuthTokenServer.ExternalLogin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PlantTree.Data.Entities;
using PlantTree.Models.Api;
using UserInfo = PlantTree.Models.Api.UserInfo;

namespace PlantTree.Infrastructure.Common
{
    public static class Misc
    {
        public static async Task SetUserInfo(HttpContext context, UserInfo info)
        {
            var userManager = context.RequestServices.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);
            user.Name = info.Name;
            user.LastName = info.LastName;
            user.Gender = Misc.StringToEnum<Gender>(info.Gender); ;
            if (string.IsNullOrEmpty(info.Birthday))
                user.Birthday = null;
            else
                user.Birthday = DateTime.ParseExact(info.Birthday, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            await userManager.UpdateAsync(user);
        }

        public static T? StringToEnum<T>(string s) where T : struct
        {
            T enumVar;
            if (!Enum.TryParse(s, true, out enumVar) || !Enum.IsDefined(typeof(T), enumVar))
                return null;
            return enumVar;
        }

    }
}