using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AuthTokenServer.Exceptions;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AuthTokenServer.ExternalLogin
{
    public class GoogleHandler : IExternalHandler
    {
        private readonly GoogleOptions _options;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public GoogleHandler(GoogleOptions options, HttpClient client, ILogger<GoogleHandler> logger)
        {
            _options = options;
            _client = client;
            _logger = logger;
        }

        public async Task<string> GetUserInfoRaw(string accessToken)
        {
            // Get the Google user
            var request = new HttpRequestMessage(HttpMethod.Get, _options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return content;
            var errorMessage = $"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding Google+ API is enabled.";
            _logger.LogError(errorMessage + " Google response: " + content);
            throw new ExternalAuthException(errorMessage, "Google response: " + content);
        }

        /// <summary>
        /// Returns user info using access_token
        /// </summary>
        /// <param name="token">access_token</param>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfo(string token)
        {
            var payload = JObject.Parse(await GetUserInfoRaw(token));
            var user = new UserInfo()
            {
                Id = GoogleHelper.GetId(payload),
                Name = GoogleHelper.GetName(payload),
                Email = GoogleHelper.GetEmail(payload)
            };

            //var gender = GoogleHelper.GetGender(payload);
            //if (!string.IsNullOrEmpty(gender))
            //{
            //    user.Gender = gender.ToLower() == "male" ? Gender.Male : Gender.Female;
            //}

            // TODO: process birthday
            return user;
        }

        
    }
}