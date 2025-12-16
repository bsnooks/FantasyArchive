using Microsoft.AspNetCore.Mvc;
using FantasyArchive.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FantasyArchive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly FantasyArchiveContext _dbContext;

    public DatabaseController(FantasyArchiveContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            var seasons = await _dbContext.Seasons.ToListAsync();
            var teams = await _dbContext.Teams.Include(t => t.Franchise).ToListAsync();
            var franchises = await _dbContext.Franchises.ToListAsync();

            return Ok(new
            {
                DatabaseStatus = "Connected",
                RecordCounts = new
                {
                    Seasons = seasons.Count,
                    Teams = teams.Count,
                    Franchises = franchises.Count
                },
                RecentSeasons = seasons.Take(5).Select(s => new
                {
                    s.LeagueID,
                    s.Year,
                    s.YahooGameId,
                    s.YahooLeagueId
                }),
                RecentTeams = teams.Take(5).Select(t => new
                {
                    t.TeamId,
                    t.Name,
                    t.Year,
                    t.YahooTeamId,
                    FranchiseName = t.Franchise?.MainName
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}