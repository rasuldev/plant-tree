using System;
using System.Net.Http;

namespace test.Infrastructure.TestServers
{
    public class RemoteTestServer : ITestServer
    {
        private readonly HttpClient _client;

        public RemoteTestServer()
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri("http://rasuldev-001-site28.btempurl.com")
            };
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public HttpClient GetClient()
        {
            return _client;
        }
    }
}