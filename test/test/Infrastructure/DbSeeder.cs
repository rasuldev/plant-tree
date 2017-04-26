using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework.Internal;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace test.Infrastructure
{
    public static class DbSeeder
    {
        public static List<Project> Projects = new List<Project>();
        public static List<ApplicationUser> Users = new List<ApplicationUser>();
        public static List<ProjectUser> ProjectUsers = new List<ProjectUser>();
        public static List<Transaction> Transactions = new List<Transaction>();

        private static readonly Randomizer Randomizer = new Randomizer();

        static DbSeeder()
        {
            InitData();
        }

        private static void InitData()
        {
            // Projects
            for (int i = 0; i < 100; i++)
            {
                var project = new Project() { Id = i + 1, Name = $"Посади дерево {i + 1}", Description = $"Tree {i + 1}" };
                Projects.Add(project);
            }

            var finishedProjectsInds = new List<int>(GetRandomArray(0, 50, 10));
            var reachedProjectsInds = new List<int>(GetRandomArray(50, 100, 10));

            foreach (var id in finishedProjectsInds)
            {
                Projects[id].Status = ProjectStatus.Finished;
            }

            foreach (var id in reachedProjectsInds)
            {
                Projects[id].Status = ProjectStatus.Reached;
            }

            // Users
            for (int i = 0; i < 10; i++)
            {
                var user = new ApplicationUser()
                {
                    Id = Randomizer.GetString(37, "abcdefghijkmnopqrstuvwxyz0123456789-"),
                    UserName = $"User {i}",
                };
                Users.Add(user);

                // Likes
                var likedProjectsCount = Randomizer.Next(3, 10);
                var likedProjectsIds = GetRandomArray(1, 101, likedProjectsCount);
                // WriteLine(likedProjectsIds);
                for (int j = 0; j < likedProjectsIds.Length; j++)
                {
                    ProjectUsers.Add(new ProjectUser() { ProjectId = likedProjectsIds[j], UserId = user.Id });
                }
            }

            // Transactions
            for (int i = 1; i <= 100; i++)
            {
                var transaction = new Transaction()
                {
                    Id = i, Amount = Randomizer.NextDecimal(300,1000),
                    Status = Randomizer.NextEnum<TransactionStatus>(),
                    ProjectId = Randomizer.Next(1,101),
                    PaymentMethod = "visa",
                    TreeCount = Randomizer.Next(1,10),
                    UserId = GetRandomElement(Users.Select(u => u.Id).ToArray()),
                    CreationDate = DateTime.Now - TimeSpan.FromDays(Randomizer.NextDouble(30))
                };
                transaction.FinishedDate = transaction.CreationDate + TimeSpan.FromDays(10 + Randomizer.NextDouble(30));
                if (transaction.FinishedDate > DateTime.Now)
                    transaction.FinishedDate = null;
                Transactions.Add(transaction);
            }
        }

        private static int[] GetRandomArray(int min, int max, int approximateCount)
        {
            var numbers = new int[approximateCount];
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = Randomizer.Next(min, max);
            }
            return numbers.Distinct().ToArray();
        }

        private static T GetRandomElement<T>(IList<T> items)
        {
            var ind = Randomizer.Next(0, items.Count());
            return items[ind];
        }

        public static void PopulateContext(AppDbContext context)
        {
            context.AddRange(Projects);
            context.AddRange(Users);
            context.AddRange(ProjectUsers);
            context.AddRange(Transactions);
            //WriteLine(ProjectUsers.Select(p => p.ProjectId));
            context.SaveChanges();
        }

        private static void WriteLine<T>(IEnumerable<T> arr)
        {
            foreach (var item in arr)
            {
                Trace.WriteLine(item);
            }
        }
    }
}