using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using PlantTree;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

namespace test.PlantTree.IntegrationTests.Api
{
    public class ProjectsTests
    {
        private TestServer _server;
        private HttpClient _client;
        private SqliteTestDb _testdb;
        private DbContextOptions<AppDbContext> _options;

        [OneTimeSetUp]//[SetUp] 
        public void Init()
        {
            var dir = Directory.GetCurrentDirectory();
            Console.WriteLine(Directory.GetCurrentDirectory());

            _testdb = new SqliteTestDb();
            _options = _testdb.Options;

            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseContentRoot(@"..\..\..\..\..\src\PlantTree")
                .ConfigureServices(services =>
                {
                    var dbDescriptors = services.Where(s => s.ServiceType == typeof(AppDbContext));
                    foreach (var descriptor in dbDescriptors)
                    {
                        services.Remove(descriptor);
                    }
                    //services.AddDbContext<AppDbContext>(options => options.Us);
                    services.AddScoped<AppDbContext>(provider =>
                    {
                        var cache = provider.GetRequiredService<AppDbContextCache>();
                        var context = new AppDbContext(_options, cache);
                        context.Add(new Project() {Id = 1, Name = "Посади дерево", Description = "Tree"});
                        context.Add(new Project() { Id = 2, Name = "Посади дерево", Description = "Tree" });
                        context.Add(new Project() { Id = 3, Name = "Посади дерево", Description = "Tree" });
                        context.SaveChanges();
                        return context;
                    });

                }));
            _client = _server.CreateClient();
        }

        [OneTimeTearDown]//[TearDown]
        public void CleanUp()
        {
            _client.Dispose();
            _server.Dispose();
            _testdb.Dispose();
        }

        [Test]
        public async Task GetProjectsTest()
        {
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

        }
    }
}