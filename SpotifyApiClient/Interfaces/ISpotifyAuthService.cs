using System.Threading.Tasks;
using SpotifyApiClient.Models;

namespace SpotifyApiClient.Interfaces
{
    public interface ISpotifyAuthService
    {
        /// <summary>
        /// Create the Spotify authorization URL and a state value to use for CSRF protection.
        /// Returns a tuple (Url, State).
        /// </summary>
        public (string Url, string State) CreateLoginUrl();

        /// <summary>
        /// Exchange an authorization code for access/refresh tokens.
        /// </summary>
        public Task<SpotifyToken?> ExchangeCodeForTokenAsync(string code);

        /// <summary>
        /// Initialize the auth service at startup (e.g., obtain an access token) and
        /// configure the HttpClient as needed.
        /// </summary>
        public Task InitializeAsync();
    }
}
