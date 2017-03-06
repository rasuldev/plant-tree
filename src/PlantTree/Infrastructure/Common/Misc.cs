using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PlantTree.Data.Entities;
using PlantTree.Models.Api;

namespace PlantTree.Infrastructure.Common
{
    public static class Misc
    {
        public static async Task SetUserInfo(HttpContext context, UserInfoModel info)
        {
            var userManager = context.RequestServices.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);
            user.Name = info.Name;
            user.Gender = info.Gender;
            if (string.IsNullOrEmpty(info.Birthday))
                user.Birthday = null;
            else
                user.Birthday = DateTime.ParseExact(info.Birthday, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            await userManager.UpdateAsync(user);
        }

    }
}