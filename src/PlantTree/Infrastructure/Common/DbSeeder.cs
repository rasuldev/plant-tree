using System;
using System.Collections.Generic;
using System.Linq;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace PlantTree.Infrastructure.Common
{
    public class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            var transactions = new List<Transaction>();
            var random = new Random();
            // Transactions
            for (int i = 1; i <= 1000; i++)
            {
                var transaction = new Transaction()
                {
                    //Id = i,
                    Amount = (decimal)random.NextDouble() * 300 + 200,
                    Status = (TransactionStatus)random.Next(0, 3),
                    ProjectId = GetRandomElement(context.Projects.Select(p => p.Id).ToArray()),
                    PaymentMethod = GetRandomElement(new[] {"visa", "yandex", "webmoney"}),
                    TreeCount = random.Next(1, 10),
                    UserId = GetRandomElement(context.Users.Select(u => u.Id).ToArray()),
                    CreationDate = DateTime.Now - TimeSpan.FromDays(random.NextDouble() * 30)
                };
                transaction.FinishedDate = transaction.CreationDate + TimeSpan.FromDays(10 + random.NextDouble() * 30);
                if (transaction.FinishedDate > DateTime.Now)
                    transaction.FinishedDate = null;
                transactions.Add(transaction);
            }

            context.Transactions.AddRange(transactions);
            context.SaveChanges();

            // News
            var newsList = new List<News>();
            for (int i = 0; i < 1000; i++)
            {
                var news = new News()
                {
                    ProjectId = GetRandomElement(context.Projects.Select(p => p.Id).ToArray()),
                    Title = Faker.Lorem.Sentence(),
                    ShortText = Faker.Lorem.Paragraph(),
                    Text = string.Join(" ",Faker.Lorem.Paragraphs(3)),
                    Date = DateTime.Now - TimeSpan.FromDays(random.NextDouble() * 30),
                    PhotoId = GetRandomElement(context.Images.Select(im => im.Id).ToArray())
                };
                newsList.Add(news);
            }
            context.News.AddRange(newsList);
            context.SaveChanges();
        }

        private static T GetRandomElement<T>(IList<T> items)
        {
            var random = new Random();
            var ind = random.Next(0, items.Count());
            return items[ind];
        }
    }
}