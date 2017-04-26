using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlantTree;
using PlantTree.Data;
using PlantTree.Infrastructure.Common;

namespace test.Infrastructure.TestServers
{
    public class MyLocalTestServer : ITestServer
    {
        private readonly LocalTestServer<Startup, AppDbContext> _testServer;
        private readonly SqliteTestDb<AppDbContext> _testdb;

        public MyLocalTestServer() 
        {
            _testdb = new SqliteTestDb<AppDbContext>(DbSeeder.PopulateContext);
            _testServer = new LocalTestServer<Startup, AppDbContext>(@"..\..\..\..\..\..\src\PlantTree",
                () =>
                {
                    _testdb.ResetDb();
                    var context = _testdb.CreateContext();
                    return context;
                });
        }

        public HttpClient GetClient()
        {
            return _testServer.GetClient();
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _testdb.Dispose();
        }

        
    }
}