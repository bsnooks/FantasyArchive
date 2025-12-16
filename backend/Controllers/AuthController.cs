using System;
using System.Threading.Tasks;
using FantasyArchive.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FantasyArchive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IYahooAuthService authService;

        public AuthController(IYahooAuthService authService)
        {
            this.authService = authService;
        }

        /// <summary>
        /// Get the Yahoo OAuth authorization URL
        /// </summary>
        [HttpGet("yahoo/url")]
        public IActionResult GetYahooAuthUrl([FromQuery] string redirectUri, [FromQuery] string? state = null)
        {
            try
            {
                var authUrl = authService.GetAuthorizationUrl(redirectUri, state ?? "");
                return Ok(new { authUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exchange Yahoo OAuth code for access token
        /// </summary>
        [HttpGet("yahoo/token")]
        public async Task<IActionResult> GetYahooToken([FromQuery] string code, [FromQuery] string redirectUri)
        {
            try
            {
                var result = await authService.AuthenticateCode(code, redirectUri);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}