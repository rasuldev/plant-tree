using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace test
{
    public class Misc
    {
        public static void PopulateContext(AppDbContext context)
        {
            for (int i = 0; i < 100; i++)
            {
                context.Add(new Project() { Id = i + 1, Name = $"Посади дерево {i + 1}", Description = $"Tree {i + 1}" });
            }

            var user = new ApplicationUser() { Id = "c2cad623-a1af-4d99-8b01-6292fb25bbb0", UserName = "User1" };
            context.ApplicationUser.Add(user);

            context.AddRange(
                new ProjectUser() { ProjectId = 10, User = user },
                new ProjectUser() { ProjectId = 15, User = user },
                new ProjectUser() { ProjectId = 41, User = user }
            );

            context.SaveChanges();
        }
    }
}