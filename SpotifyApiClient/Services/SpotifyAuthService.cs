using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SpotifyApiClient.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using SpotifyApiClient.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SpotifyApiClient.Services
{
    public class SpotifyAuthService : ISpotifyAuthService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<SpotifyAuthService> _logger;

        public SpotifyAuthService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<SpotifyAuthService> logger)
        {
            _config = config;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public (string Url, string State) CreateLoginUrl()
        {
            _logger.LogInformation("Starting {Method}", nameof(CreateLoginUrl));

            var clientId = _config["Spotify:ClientId"] ?? string.Empty;
            var redirectUri = _config["Spotify:RedirectUri"] ?? string.Empty;
            var scope = _config["Spotify:Scope"] ?? "user-read-private user-read-email";

            var state = GenerateRandomString(16);

            var query = new StringBuilder();
            query.Append("response_type=code");
            query.Append("&client_id=").Append(Uri.EscapeDataString(clientId));
            query.Append("&scope=").Append(Uri.EscapeDataString(scope));
            query.Append("&redirect_uri=").Append(Uri.EscapeDataString(redirectUri));
            query.Append("&state=").Append(Uri.EscapeDataString(state));

            var url = "https://accounts.spotify.com/authorize?" + query.ToString();
            _logger.LogInformation("Finished {Method} returning UrlLength={UrlLen} StateLen={StateLen}", nameof(CreateLoginUrl), url.Length, state.Length);
            return (url, state);
        }

        public async Task<SpotifyToken?> ExchangeCodeForTokenAsync(string code)
        {
            _logger.LogInformation("Starting {Method}", nameof(ExchangeCodeForTokenAsync));

            var clientId = _config["Spotify:ClientId"] ?? string.Empty;
            var clientSecret = _config["Spotify:ClientSecret"] ?? string.Empty;
            var redirectUri = _config["Spotify:RedirectUri"] ?? string.Empty;

            // Use the named SpotifyApi client so Program.cs configuration is applied
            var client = _httpFactory.CreateClient("SpotifyApi");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var form = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            };

            request.Content = new FormUrlEncodedContent(form);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Finished {Method} returning null due to non-success response: {Status}", nameof(ExchangeCodeForTokenAsync), response.StatusCode);
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var token = JsonSerializer.Deserialize<SpotifyToken>(content, options);

            if (token == null)
            {
                _logger.LogInformation("Finished {Method} returning null (deserialization failed)", nameof(ExchangeCodeForTokenAsync));
                return null;
            }

            _logger.LogInformation("Finished {Method} returning token: AccessPresent={HasAccess} RefreshPresent={HasRefresh} ExpiresIn={Expires}", nameof(ExchangeCodeForTokenAsync), !string.IsNullOrEmpty(token.AccessToken), !string.IsNullOrEmpty(token.RefreshToken), token.ExpiresIn);
            return token;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting {Method}", nameof(InitializeAsync));

            var clientId = _config["Spotify:ClientId"] ?? string.Empty;
            var clientSecret = _config["Spotify:ClientSecret"] ?? string.Empty;

            var client = _httpFactory.CreateClient("SpotifyApi");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var form = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            request.Content = new FormUrlEncodedContent(form);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Finished {Method} with no token (status {Status})", nameof(InitializeAsync), response.StatusCode);
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var token = JsonSerializer.Deserialize<SpotifyToken>(content, options);

            if (token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                // Set the Authorization header on this HttpClient instance.
                // Note: callers that request a new HttpClient instance from the factory
                // will not automatically receive this header. For more robust scenarios
                // consider a delegating handler or token provider.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                _logger.LogInformation("Finished {Method} set Authorization header. AccessPresent={HasAccess}", nameof(InitializeAsync), !string.IsNullOrEmpty(token.AccessToken));
            }
            else
            {
                _logger.LogInformation("Finished {Method} no token present after initialization", nameof(InitializeAsync));
            }
        }

        private string GenerateRandomString(int length)
        {
            _logger.LogInformation("Starting {Method}", nameof(GenerateRandomString));

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var data = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                var idx = data[i] % chars.Length;
                result[i] = chars[idx];
            }

            _logger.LogInformation("Finished {Method} returning string length {Len}", nameof(GenerateRandomString), length);
            return new string(result);
        }
    }
}
