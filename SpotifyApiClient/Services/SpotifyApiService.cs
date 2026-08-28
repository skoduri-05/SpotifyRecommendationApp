using SpotifyApiClient.Interfaces;

namespace SpotifyApiClient.Services
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly ILogger<SpotifyApiService> _logger;

        public SpotifyApiService(ILogger<SpotifyApiService> logger)
        {
            _logger = logger;
        }
    }
}
