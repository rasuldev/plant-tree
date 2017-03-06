using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthTokenServer.Exceptions;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AuthTokenServer.ExternalLogin
{
    public class FacebookHandler : IExternalHandler
    {
        private readonly FacebookOptions _options;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public FacebookHandler(FacebookOptions options, HttpClient client, ILogger<FacebookHandler> logger)
        {
            _options = options;
            _client = client;
            _logger = logger;
        }

        public async Task<string> GetUserInfoRaw(string accessToken)
        {
            var endpoint = QueryHelpers.AddQueryString(_options.UserInformationEndpoint, "access_token", accessToken);
            if (_options.SendAppSecretProof)
            {
                //endpoint = QueryHelpers.AddQueryString(endpoint, "appsecret_proof", GenerateAppSecretProof(accessToken));
            }
            if (_options.Fields.Count > 0)
            {
                endpoint = QueryHelpers.AddQueryString(endpoint, "fields", string.Join(",", _options.Fields));
            }

            //var proxy = "http://iprogram.biz/api/proxy?url=" + WebUtility.UrlEncode(endpoint);
            var response = await _client.GetAsync(endpoint); //, HttpContext.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var errorMessage =
                    $"Failed to retrieve Facebook user information ({response.StatusCode}) Please check if the authentication information is correct and the corresponding Facebook Graph API is enabled.";
                _logger.LogError(errorMessage + " Facebook response: " + content);
                throw new ExternalAuthException(errorMessage, "Facebook response: " + content);
            }

            return await response.Content.ReadAsStringAsync();
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
                Id = FacebookHelper.GetId(payload),
                Name = FacebookHelper.GetName(payload),
                Email = FacebookHelper.GetEmail(payload)
            };

            var gender = FacebookHelper.GetGender(payload);
            if (!string.IsNullOrEmpty(gender))
            {
                user.Gender = gender.ToLower() == "male" ? Gender.Male : Gender.Female;
            }

            // TODO: process birthday

            return user;
        }

        private string GenerateAppSecretProof(string accessToken)
        {
            using (var algorithm = new HMACSHA256(Encoding.ASCII.GetBytes(_options.AppSecret)))
            {
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                var builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
        }
    }
}