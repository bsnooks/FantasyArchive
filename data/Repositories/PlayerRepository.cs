using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class PlayerRepository
    {

        private readonly Func<FantasyArchiveContext> dbFunc;

        public PlayerRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<Player> GetOne(int id)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Players
                .Include(p => p.PlayerSeasons)
                .Include(p => p.PlayerWeeks)
                .SingleOrDefaultAsync(p => p.PlayerID == id);
            }
        }

        public async Task<Player> GetOneByName(string name)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Players
                .SingleOrDefaultAsync(p => p.Name.Contains(name));
            }
        }

        public async Task<IEnumerable<Player>> GetPlayersBySeason(params int[] years)
        {
            using (var dbContext = dbFunc())
            {
                try
                {
                    return await dbContext.Players
                    .Where(p => p.PlayerSeasons.Any(s => years.Contains(s.Year)))
                    .Include(p => p.PlayerSeasons)
                    .Include(p => p.PlayerWeeks)
                    .OrderByDescending(p => p.PlayerID)
                    .ToListAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IEnumerable<PlayerSeason>> GetPlayerSeasonComparison(IList<int> years, string position = null, bool allSlices = false)
        {
            int starterThreshold = 0;
            switch (position)
            {
                case "WR":
                    starterThreshold = 30;
                    break;
                case "TE":
                    starterThreshold = 10;
                    break;
                case "RB":
                case "QB":
                    starterThreshold = 20;
                    break;
                default:
                    starterThreshold = 30;
                    break;
            }

            if (starterThreshold == 0)
            {
                return Enumerable.Empty<PlayerSeason>();
            }

            int[] positionSlices = allSlices ?
                Enumerable.Range(1, starterThreshold).ToArray() :
                new int[] { 1, (starterThreshold / 2), starterThreshold };

            using (var dbContext = dbFunc())
            {
                try
                {
                    return await dbContext.PlayerSeasons
                       .Where(s => years.Contains(s.Year) &&
                           (position == null || s.Player.Position == position) &&
                           positionSlices.Contains(s.PositionRank))
                       .Include(s => s.Player)
                       .Include(s => s.EndTeam)
                       .ThenInclude(t => t.Franchise)
                       .OrderByDescending(s => s.Year)
                       .ThenBy(s => s.PositionRank)
                       .ThenBy(s => s.Player.Position)
                       .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<PlayerSeason>();
                }
            }
        }

        public async Task<IEnumerable<PlayerSeason>> GetPlayerSeasons(int year, string[] positions = null)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.PlayerSeasons
                    .Where(ps => ps.Year == year &&
                        (positions == null || positions.Contains(ps.Player.PrimaryPosition)))
                    .Include(ps => ps.Player)
                    .OrderBy(ps => ps.PlayerID)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PlayerWeek>> GetPlayerWeeks(int? year = null, string[] positions = null)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.PlayerWeeks
                    .Where(ps => (year == null || ps.Year == year) &&
                        (positions == null || positions.Contains(ps.Player.PrimaryPosition)))
                    .Include(ps => ps.Player)
                    .OrderBy(ps => ps.PlayerID)
                    .ThenBy(ps => ps.Year)
                    .ThenBy(ps => ps.Week)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PlayerWeek>> GetPlayersWeeks(int playerId, int? year = null, string[] positions = null)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.PlayerWeeks
                    .Where(ps => ps.PlayerID == playerId &&
                        (year == null || ps.Year == year) &&
                        (positions == null || positions.Contains(ps.Player.PrimaryPosition)))
                    .Include(pw => pw.Player)
                    .Include(pw => pw.Team)
                    .ThenInclude(t => t.Franchise)
                    .OrderBy(pw => pw.PlayerID)
                    .ThenBy(pw => pw.Year)
                    .ThenBy(pw => pw.Week)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PlayerSeason>> GetPlayerSeasons(Franchise franchise)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.PlayerSeasons
                    .Where(ps => ps.EndTeam.FranchiseId == franchise.FranchiseId)
                    .Include(ps => ps.Player)
                    .ToListAsync();
            }
        }


        public async Task<IEnumerable<Player>> LookupPlayer(string name, string position, int offset, int limit)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Players
                .Where(p => p.Name.Contains(name) && (string.IsNullOrEmpty(position) || p.Position == position))
                .OrderBy(p => p.Name)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
            }
        }

        public async Task<int> CreatePlayer(Player newPlayer)
        {
            using (var dbContext = dbFunc())
            {
                try
                {
                    await dbContext.AddAsync(newPlayer);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var d = ex;
                }
            }

            return newPlayer.PlayerID;
        }

        public async Task UpdatePlayerSeasons(PlayerSeason playerSeason)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.PlayerSeasons.AddAsync(playerSeason);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdatePlayersSeasons(IEnumerable<PlayerSeason> playerSeasons, int year, bool updatePositionRanks = true)
        {
            using (var dbContext = dbFunc())
            {
                dbContext.PlayerSeasons.UpdateRange(playerSeasons);
                await dbContext.SaveChangesAsync();

                if (updatePositionRanks)
                {
                    dbContext.Database.ExecuteSqlRaw("EXECUTE [dbo].[UpdatePositionRanks] {0}", year);
                }
            }
        }

        public async Task CreatePlayersWeeks(IEnumerable<PlayerWeek> playerWeeks)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.PlayerWeeks.AddRangeAsync(playerWeeks);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdatePlayersWeeks(IEnumerable<PlayerWeek> playerWeeks)
        {
            using (var dbContext = dbFunc())
            {
                dbContext.PlayerWeeks.UpdateRange(playerWeeks);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TeamPlayer>> GetPlayersByTeam(Team team, bool? started = null, bool? currentRoster = null)
        {
            using (var dbContext = dbFunc())
            {
                var teamId = team.TeamId;
                return await dbContext.Players
                    .Where(p => p.PlayerWeeks.Any(pw =>pw.TeamId == teamId) &&
                        (currentRoster != true || p.PlayerSeasons.Any(ps => ps.EndTeamId.HasValue && ps.EndTeamId.Value == teamId))
                    )
                    .Include(p => p.PlayerSeasons)
                    .Select(p => new TeamPlayer()
                    {
                        PlayerID = p.PlayerID,
                        Player = p,
                        GamesStarted = p.PlayerWeeks.Count(x => x.TeamId == teamId && x.Started),
                        GamesBenched = p.PlayerWeeks.Count(x => x.TeamId == teamId && !x.Started),
                        PassYards = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.PassYards),
                        PassTDs = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.PassTDs),
                        RushYards = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.RushYards),
                        RushTDs = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.RushTDs),
                        RecYards = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.RecYards),
                        RecTDs = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.RecTDs),
                        Interceptions = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.Interceptions),
                        FumblesLost = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.FumblesLost),
                        TwoPointConvert = p.PlayerWeeks.Where(x => x.TeamId == teamId && (started == null || x.Started == started)).Sum(x => x.TwoPointConvert),
                        SeasonPoints = p.PlayerWeeks.Where(x => x.Year == team.Year).Sum(x => x.PassYards / 25.0 + x.PassTDs * 4 + x.RushYards / 10.0 + x.RushTDs * 6 + x.RecYards / 10.0 + x.RecTDs * 6),
                    })
                    .OrderByDescending(x => x.PassYards / 25.0 + x.PassTDs * 4 + x.RushYards / 10.0 + x.RushTDs * 6 + x.RecYards / 10.0 + x.RecTDs * 6)
                    .ToListAsync();
            }
        }
    }
}
