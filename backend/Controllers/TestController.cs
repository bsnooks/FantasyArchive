using System;
using System.Threading;
using System.Threading.Tasks;
using FantasyArchive.Api.Models.Yahoo;
using FantasyArchive.Api.Services;
using FantasyArchive.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FantasyArchive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public TestController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Test Yahoo API connectivity with a given token
        /// </summary>
        [HttpGet("yahoo/connection")]
        public async Task<IActionResult> TestYahooConnection([FromQuery] string token, [FromQuery] string leagueKey)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(leagueKey))
            {
                return BadRequest("Token and leagueKey are required");
            }

            try
            {
                // Create a test context
                var context = new TestSyncContext(token, leagueKey);
                
                using var client = new YahooClient(context, configuration);
                
                // Try to get basic league information
                var league = await client.GetAsync<YahooLeague>($"/fantasy/v2/league/{leagueKey}", CancellationToken.None);
                
                if (league != null)
                {
                    return Ok(new 
                    { 
                        message = "Connection successful",
                        league = new 
                        {
                            name = league.Name,
                            leagueKey = league.LeagueKey,
                            numTeams = league.NumTeams,
                            season = league.Season,
                            currentWeek = league.CurrentWeek
                        }
                    });
                }
                else
                {
                    return NotFound("League not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private class TestSyncContext : ISyncContext
        {
            public string Token { get; }
            public string LeagueKey { get; }
            public League League => throw new NotImplementedException();

            public TestSyncContext(string token, string leagueKey)
            {
                Token = token;
                LeagueKey = leagueKey;
            }
        }
    }
}