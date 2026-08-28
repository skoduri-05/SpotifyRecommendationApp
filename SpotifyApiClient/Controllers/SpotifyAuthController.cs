using Microsoft.AspNetCore.Mvc;
using SpotifyApiClient.Interfaces;
using SpotifyApiClient.Models;

namespace SpotifyApiClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly ILogger<SpotifyAuthController> _logger;
        private readonly ISpotifyAuthService _authService;

        public SpotifyAuthController(ILogger<SpotifyAuthController> logger, ISpotifyAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            _logger.LogInformation("Starting {Method}", nameof(Login));

            var (url, state) = _authService.CreateLoginUrl();

            // set state cookie for CSRF verification on callback
            Response.Cookies.Append("spotify_auth_state", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });

            _logger.LogInformation("Finished {Method} returning authorize URL to caller", nameof(Login));
            // Return the authorize URL to the client so the caller can perform a top-level navigation.
            // This avoids Swagger/UI fetch following the external redirect which causes CORS/fetch failures.
            return Ok(new { url });
        }

        [HttpGet("/callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error)
        {
            _logger.LogInformation("Starting {Method}", nameof(Callback));

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Spotify callback returned error: {Error}", error);
                _logger.LogInformation("Finished {Method} returning BadRequest with error {Error}", nameof(Callback), error);
                return BadRequest(new { error });
            }

            var expectedState = Request.Cookies["spotify_auth_state"];
            if (string.IsNullOrEmpty(state) || state != expectedState)
            {
                _logger.LogWarning("State mismatch in Spotify callback. Expected {Expected} got {Got}", expectedState, state);
                _logger.LogInformation("Finished {Method} returning BadRequest state_mismatch", nameof(Callback));
                return BadRequest(new { error = "state_mismatch" });
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogInformation("Finished {Method} returning BadRequest missing_code", nameof(Callback));
                return BadRequest(new { error = "missing_code" });
            }

            var token = await _authService.ExchangeCodeForTokenAsync(code);
            if (token == null)
            {
                _logger.LogInformation("Finished {Method} returning 502 token_exchange_failed", nameof(Callback));
                return StatusCode(502, new { error = "token_exchange_failed" });
            }

            // Store tokens in HttpOnly cookies (for demo/dev). In production consider server-side session storage.
            Response.Cookies.Append("spotify_access_token", token.AccessToken ?? string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                Expires = System.DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn)
            });

            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                Response.Cookies.Append("spotify_refresh_token", token.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps
                });
            }

            // Redirect to frontend app or return token info. Here we redirect to root.
            _logger.LogInformation("Finished {Method} returning redirect to root. Token present: {HasAccessToken}, Refresh present: {HasRefresh}", nameof(Callback), !string.IsNullOrEmpty(token.AccessToken), !string.IsNullOrEmpty(token.RefreshToken));
            return Redirect("/");
        }
    }
}
