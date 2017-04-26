using System.Net.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using PlantTree.Data.Entities;
using test.Infrastructure.TestServers;

namespace test.PlantTree.IntegrationTests.Api
{
    [TestFixture]
    public class TransactionsTests
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
        public async void GetFirstPageTest()
        {
            var response = await _client.GetAsync("/api/transactions");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<Transaction[]>(responseContent, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }
    }
}