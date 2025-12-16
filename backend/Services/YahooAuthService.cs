using FantasyArchive.Api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FantasyArchive.Api.Services
{
    public class YahooAuthService : IYahooAuthService
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;

        public YahooAuthService(IConfiguration configuration, HttpClient httpClient)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
        }

        public string GetAuthorizationUrl(string redirectUri, string state = "")
        {
            var clientId = configuration["Yahoo:ClientId"];
            var authUrl = "https://api.login.yahoo.com/oauth2/request_auth";
            
            var queryParams = new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["redirect_uri"] = redirectUri,
                ["response_type"] = "code",
                ["language"] = "en-us"
            };

            if (!string.IsNullOrEmpty(state))
            {
                queryParams["state"] = state;
            }

            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            return $"{authUrl}?{query}";
        }

        public async Task<YahooAuthResult> AuthenticateCode(string code, string redirectUri)
        {
            var clientId = configuration["Yahoo:ClientId"];
            var clientSecret = configuration["Yahoo:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("Yahoo OAuth configuration is missing");
            }

            var tokenUrl = "https://api.login.yahoo.com/oauth2/get_token";
            
            var requestData = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var requestContent = new FormUrlEncodedContent(requestData);

            try
            {
                var response = await httpClient.PostAsync(tokenUrl, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new UnauthorizedAccessException($"Yahoo OAuth failed: {responseContent}");
                }

                var tokenResponse = JsonConvert.DeserializeObject<YahooTokenResponse>(responseContent);
                
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new UnauthorizedAccessException("Invalid token response from Yahoo");
                }

                return new YahooAuthResult
                {
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    TokenType = tokenResponse.TokenType
                };
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to communicate with Yahoo OAuth service: {ex.Message}", ex);
            }
        }
    }
}