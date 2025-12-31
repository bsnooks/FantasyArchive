using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using FantasyArchive.Api.Models;
using FantasyArchive.Api.Services;
using FantasyArchive.Data;
using FantasyArchive.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyArchive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YahooSyncController : ControllerBase
    {
        private readonly IYahooSyncService yahooSyncService;
        private readonly IServiceProvider serviceProvider;
        private readonly SeasonRepository seasonRepository;
        private readonly LeagueRepository leagueRepository;
        private readonly Guid DefaultLeague = new Guid("645B611A-EF6A-4D06-AE92-BD4887240C31");

        public YahooSyncController(IYahooSyncService yahooSyncService, IServiceProvider serviceProvider,
            LeagueRepository leagueRepository,
            SeasonRepository seasonRepository)
        {
            this.yahooSyncService = yahooSyncService;
            this.serviceProvider = serviceProvider;
            this.leagueRepository = leagueRepository;
            this.seasonRepository = seasonRepository;
        }

        /// <summary>
        /// Sync entire season information from Yahoo (comprehensive sync)
        /// </summary>
        [HttpPost("season")]
        public async Task<IActionResult> SyncSeason([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncSeason(context, season, cancellationToken);
                return Ok(new { message = "Season sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncSeason error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync keepers information from Yahoo
        /// </summary>
        [HttpPost("keepers")]
        public async Task<IActionResult> SyncKeepers([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncKeepers(context, season, cancellationToken);
                return Ok(new { message = "Keepers sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncKeepers error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync transaction information from Yahoo
        /// </summary>
        [HttpPost("transactions")]
        public async Task<IActionResult> SyncTransactions([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncTransactions(context, season, cancellationToken);
                return Ok(new { message = "Transactions sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncTransactions error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync matchup information from Yahoo
        /// </summary>
        [HttpPost("matchups")]
        public async Task<IActionResult> SyncMatchups([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncMatchups(context, season, cancellationToken);
                return Ok(new { message = "Matchups sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncMatchups error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync standings information from Yahoo
        /// </summary>
        [HttpPost("standings")]
        public async Task<IActionResult> SyncStandings([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncStandings(context, season, cancellationToken);
                return Ok(new { message = "Standings sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncStandings error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync draft information from Yahoo
        /// </summary>
        [HttpPost("draft")]
        public async Task<IActionResult> SyncDraft([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncDraft(context, season, cancellationToken);
                return Ok(new { message = "Draft sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncDraft error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync current week information from Yahoo
        /// </summary>
        [HttpPost("current-week")]
        public async Task<IActionResult> SyncCurrentWeek([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncCurrentWeek(context, season, cancellationToken);
                return Ok(new { message = "Current week sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncCurrentWeek error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync player stats from Yahoo
        /// </summary>
        [HttpPost("player-stats")]
        public async Task<IActionResult> SyncPlayerStats([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncPlayerStats(context, season, cancellationToken);
                return Ok(new { message = "Player stats sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncPlayerStats error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync player roster information from Yahoo
        /// </summary>
        [HttpPost("player-roster")]
        public async Task<IActionResult> SyncPlayerRoster([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncPlayerRoster(context, season, cancellationToken);
                return Ok(new { message = "Player roster sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncPlayerRoster error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync weekly roster information from Yahoo
        /// </summary>
        [HttpPost("weekly-roster")]
        public async Task<IActionResult> SyncWeeklyRoster([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncWeeklyRoster(context, season, cancellationToken);
                return Ok(new { message = "Weekly roster sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncWeeklyRoster error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync weekly player stats from Yahoo
        /// </summary>
        [HttpPost("weekly-player-stats")]
        public async Task<IActionResult> SyncWeeklyPlayerStats([FromBody] SyncRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.LeagueKey))
            {
                return BadRequest("LeagueKey is required");
            }

            try
            {
                var context = YahooSyncContext.Create(Request.HttpContext, request.LeagueKey, await leagueRepository.GetOne(DefaultLeague));
                var season = await GetSeason(context, request.Year);
                
                if (season == null)
                    return BadRequest(new { error = "Season not found. Please sync league data first." });
                    
                await yahooSyncService.SyncWeeklyPlayerStats(context, season, cancellationToken);
                return Ok(new { message = "Weekly player stats sync completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SyncWeeklyPlayerStats error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<FantasyArchive.Data.Models.Season?> GetSeason(ISyncContext context, int year)
        {
            return await seasonRepository.GetSeason(year);
        }
    }
}