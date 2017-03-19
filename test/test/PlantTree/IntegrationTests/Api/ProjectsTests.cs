using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
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
                        var context = new AppDbContext(_options);
                        Misc.PopulateContext(context);
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
            var projects = JsonConvert.DeserializeObject<Project[]>(responseContent);
            Assert.AreEqual(20, projects.Length);
            Assert.AreEqual(6, projects[5].Id);
        }

        [Test]
        public async Task GetUserProjectsNonAuthTest()
        {
            var response = await _client.GetAsync("/api/projects/user");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}