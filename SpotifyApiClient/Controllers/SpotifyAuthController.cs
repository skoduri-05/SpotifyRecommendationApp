using Microsoft.AspNetCore.Mvc;
using SpotifyApiClient.Interfaces;

namespace SpotifyApiClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyAuthController
    {
        private readonly ILogger<SpotifyAuthController> _logger;

        public SpotifyAuthController(ILogger<SpotifyAuthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Auth()
        {
            _logger.LogInformation("Authenticating and Authorizing");
            return new StatusCodeResult(200);
        }

    }
}
