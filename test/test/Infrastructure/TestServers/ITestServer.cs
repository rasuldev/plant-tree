using System;
using System.Net.Http;

namespace test.Infrastructure.TestServers
{
    public interface ITestServer : IDisposable
    {
        HttpClient GetClient();
    }
}