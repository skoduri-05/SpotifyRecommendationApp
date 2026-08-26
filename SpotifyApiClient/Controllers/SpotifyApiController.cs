using Microsoft.AspNetCore.Mvc;
using SpotifyApiClient.Interfaces;


namespace SpotifyApiClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyApiController : ControllerBase
    {
        private readonly ILogger<SpotifyApiController> _logger;

        public SpotifyApiController(ILogger<SpotifyApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult DoSomething()
        {
            _logger.LogInformation("Doing sum");
            return new OkObjectResult(200);
        }

    }
}
