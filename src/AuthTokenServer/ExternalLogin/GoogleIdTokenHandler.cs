using System.Net.Http;
using System.Threading.Tasks;
using AuthTokenServer.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthTokenServer.ExternalLogin
{
    public class GoogleIdTokenHandler : IExternalHandler
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        
        public GoogleIdTokenHandler(HttpClient client, ILogger<GoogleIdTokenHandler> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<string> GetUserInfoRaw(string idToken)
        {
            const string googleTokenInfoEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo";
            var endpoint = QueryHelpers.AddQueryString(googleTokenInfoEndpoint, "id_token", idToken);
            // Get the Google user
            var response = await _client.GetAsync(endpoint); //, HttpContext.RequestAborted);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return content;
            
            var errorMessage =
                $"Failed to retrieve id_token info from Google ({response.StatusCode}).";
            _logger.LogError(errorMessage + " Google response: " + content);
            throw new ExternalAuthException(errorMessage, "Google response: " + content);
        }

        /// <summary>
        /// Returns user info using id_token
        /// </summary>
        /// <param name="token">id_token</param>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfo(string token)
        {
            var idToken = JsonConvert.DeserializeObject<GoogleIdToken>(await GetUserInfoRaw(token));
            // TODO: check aud of idToken to be equal to known client_id
            var user = new UserInfo()
            {
                Id = idToken.Sub,
                Name = idToken.Name,
                Email = idToken.Email
            };
            return user;
        }

        
    }
}