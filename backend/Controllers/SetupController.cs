using Microsoft.AspNetCore.Mvc;
using FantasyArchive.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FantasyArchive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly FantasyArchiveContext _dbContext;

    public SetupController(FantasyArchiveContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("franchises")]
    public async Task<IActionResult> GetFranchises()
    {
        try
        {
            var franchises = await _dbContext.Franchises
                .Select(f => new
                {
                    f.FranchiseId,
                    f.MainName,
                    f.Color,
                    f.LeagueID
                })
                .ToListAsync();

            return Ok(franchises);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("owners")]
    public async Task<IActionResult> GetOwners()
    {
        try
        {
            var owners = await _dbContext.Owners
                .Select(o => new
                {
                    o.OwnerId,
                    o.Name,
                    o.Active,
                    o.FranchiceId
                })
                .ToListAsync();

            return Ok(owners);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("teams/{leagueKey}/{year}")]
    public async Task<IActionResult> GetYahooTeams(string leagueKey, int year)
    {
        try
        {
            var keyParts = leagueKey.Split('.');
            var gameId = int.Parse(keyParts[0]);
            var leagueId = int.Parse(keyParts[2]);

            var season = await _dbContext.Seasons
                .FirstOrDefaultAsync(s => s.YahooGameId == gameId && s.YahooLeagueId == leagueId && s.Year == year);

            if (season == null)
            {
                return BadRequest(new { error = "Season not found. Please sync league data first." });
            }

            var teams = await _dbContext.Teams
                .Where(t => t.LeagueID == season.LeagueID && t.Year == year)
                .Include(t => t.Franchise)
                .Include(t => t.Owner)
                .Select(t => new
                {
                    t.TeamId,
                    t.Name,
                    t.YahooTeamId,
                    t.FranchiseId,
                    FranchiseName = t.Franchise != null ? t.Franchise.MainName : null,
                    t.OwnerId,
                    OwnerName = t.Owner != null ? t.Owner.Name : null,
                    IsMapped = t.FranchiseId != Guid.Empty && t.OwnerId != null
                })
                .ToListAsync();

            return Ok(teams);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("map-team")]
    public async Task<IActionResult> MapTeam([FromBody] TeamMappingRequest request)
    {
        try
        {
            var team = await _dbContext.Teams
                .FirstOrDefaultAsync(t => t.TeamId == request.TeamId);

            if (team == null)
            {
                return NotFound(new { error = "Team not found" });
            }

            team.FranchiseId = request.FranchiseId;
            team.OwnerId = request.OwnerId;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Team mapping updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("setup-status/{leagueKey}/{year}")]
    public async Task<IActionResult> GetSetupStatus(string leagueKey, int year)
    {
        try
        {
            var keyParts = leagueKey.Split('.');
            var gameId = int.Parse(keyParts[0]);
            var leagueId = int.Parse(keyParts[2]);

            var season = await _dbContext.Seasons
                .FirstOrDefaultAsync(s => s.YahooGameId == gameId && s.YahooLeagueId == leagueId && s.Year == year);

            bool leagueSynced = season != null;
            bool teamsSynced = false;
            bool teamsMapped = false;

            if (leagueSynced)
            {
                var teams = await _dbContext.Teams
                    .Where(t => t.LeagueID == season.LeagueID && t.Year == year)
                    .ToListAsync();

                teamsSynced = teams.Any();
                teamsMapped = teams.Any() && teams.All(t => t.FranchiseId != Guid.Empty && t.OwnerId.HasValue);
            }

            return Ok(new
            {
                LeagueSynced = leagueSynced,
                TeamsSynced = teamsSynced,
                TeamsMapped = teamsMapped,
                CanSyncOtherData = leagueSynced && teamsSynced && teamsMapped
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class TeamMappingRequest
{
    public Guid TeamId { get; set; }
    public Guid FranchiseId { get; set; }
    public Guid? OwnerId { get; set; }
}