using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlantTree;
using PlantTree.Data;

namespace test.Infrastructure.TestServers
{
    public class LocalTestServer<TStartup, TContext>: ITestServer where TStartup: class 
                                                                  where TContext : DbContext
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        public LocalTestServer(string contentRootPath, Func<TContext> createContext)
        {
            //var dir = Directory.GetCurrentDirectory();
            //Console.WriteLine(Directory.GetCurrentDirectory());
            //var path = PlatformServices.Default.Application.ApplicationBasePath;
            _server = new TestServer(new WebHostBuilder()
                .UseEnvironment("TEST")
                .UseContentRoot(contentRootPath)
                .UseStartup<TStartup>()
                .ConfigureServices(services =>
                {
                    services.AddScoped<TContext>(provider => createContext());
                }));

            _client = _server.CreateClient();
        }

        public HttpClient GetClient()
        {
            return _client;
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}