using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using PlantTree;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;
using test.Infrastructure;
using test.Infrastructure.TestServers;
using DbSeeder = test.Infrastructure.DbSeeder;

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
        public async Task GetActiveProjectsTest()
        {
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<Project[]>(responseContent, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
            Assert.AreEqual(20, projects.Length);
            projects.Select(p => p.Id)
                .ShouldAllBeEquivalentTo(DbSeeder.Projects
                                                 .Where(p => p.Status == ProjectStatus.Active)
                                                 .Take(20).Select(p => p.Id));
        }

        [Test]
        public async Task GetProjectsTest(
            [Values("active", "reached", "finished", "completed")] string status,
            [Values(null, 1, 2, 3)] int? page,
            [Values(null, 5, 10, 15, 20)] int? pagesize)
        {
            var requestUrl = "/api/projects";
            if (status != null)
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "status", status);
            else
                status = "active";

            if (page.HasValue)
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "page", page.Value.ToString());
            else
                page = 1;
            if (pagesize.HasValue)
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "pagesize", pagesize.Value.ToString());
            else
                pagesize = 20;

            Console.WriteLine($"Requesting url: {requestUrl}...");
            var response = await _client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<Project[]>(responseContent, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = new List<JsonConverter>() { new StringEnumConverter(camelCaseText: true) },
            });

            Assert.That(projects.All(p => p.Status.ToString().ToLower() == status));

            projects.Select(p => p.Id)
                .ShouldAllBeEquivalentTo(DbSeeder.Projects
                    .Where(p => p.Status == Misc.StringToEnum<ProjectStatus>(status))
                    .Skip((page.Value - 1) * pagesize.Value).Take(pagesize.Value)
                    .Select(p => p.Id));
        }

        [Test]
        public async Task GetUserProjectsNonAuthTest()
        {
            var response = await _client.GetAsync("/api/projects/user");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    
}