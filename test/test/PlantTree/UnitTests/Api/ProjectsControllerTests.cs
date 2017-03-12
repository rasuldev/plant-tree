using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PlantTree.Controllers.Api;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

namespace test.PlantTree.UnitTests.Api
{
    public class ProjectsControllerTests : BaseControllerTests<ProjectsController>
    {
        protected override ProjectsController CreateController(AppDbContext context, Repository repo)
        {
            var httpContext = new Mock<HttpContext>();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "c2cad623-a1af-4d99-8b01-6292fb25bbb0"));
            var principal = new ClaimsPrincipal(identity);
            httpContext.Setup(c => c.User).Returns(principal);

            httpContext.Setup(c => c.Request.Headers["Authorization"]).Returns("bearer 123");
            
            var controller = new ProjectsController(context, repo, Mock.Of<ILogger<ProjectsController>>())
            {
                ControllerContext =
                        new ControllerContext(new ActionContext(httpContext.Object,
                                                                new RouteData(),
                                                                new ControllerActionDescriptor()))
            };
            
            return controller;
        }

        [Test]
        public async Task GetFirstPageProjectsTest()
        {
            var result = await Controller.GetProjects(1, 20) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var projects = result.Value as List<Project>;
            Assert.IsNotNull(projects);
            Assert.AreEqual(20, projects.Count);
            Assert.AreEqual(11, projects[10].Id);
        }

        [Test]
        public async Task GetSecondPageProjectsTest()
        {
            var result = await Controller.GetProjects(2, 10) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var projects = result.Value as List<Project>;
            Assert.IsNotNull(projects);
            Assert.AreEqual(10, projects.Count);
            Assert.AreEqual(11, projects[0].Id);
        }
    }
}