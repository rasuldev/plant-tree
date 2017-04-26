using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PlantTree.Controllers.Api;
using PlantTree.Data;
using PlantTree.Infrastructure.Common;
using test.Infrastructure;
using DbSeeder = test.Infrastructure.DbSeeder;

namespace test.PlantTree.UnitTests.Api
{
    [TestFixture]
    public abstract class BaseControllerTests<T> where T : IDisposable
    {
        private SqliteTestDb<AppDbContext> _testDb;
        protected AppDbContext Context;
        protected T Controller;
        public static ControllerContext EmptyControllerContext => new ControllerContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ControllerActionDescriptor()));

        [SetUp]
        public void Init()
        {
            _testDb = new SqliteTestDb<AppDbContext>();
            var options = _testDb.Options;
            Context = new AppDbContext(options);
            var repo = new Repository(Context, new MemoryCache(new MemoryCacheOptions()), Mock.Of<ILogger<Repository>>())
            {
                UseCache = false
            };
            DbSeeder.PopulateContext(Context);
            Controller = CreateController(Context, repo);
        }

        protected abstract T CreateController(AppDbContext context, Repository repo);

        [TearDown]
        public void CleanUp()
        {
            Context.Dispose();
            _testDb.Dispose();
            Controller.Dispose();
        }
    }
}