using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using PlantTree;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;
using test.Infrastructure.TestServers;

namespace test.PlantTree.IntegrationTests.Api
{
    public class ProjectsTests
    {
        private ITestServer _server;
        private HttpClient _client;

        [OneTimeSetUp]//[SetUp] 
        public void Init()
        {
            _server = new MyLocalTestServer();
            //_server = new RemoteTestServer();
            _client = _server.GetClient();
        }

        [OneTimeTearDown]//[TearDown]
        public void CleanUp()
        {
            _server.Dispose();
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