using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FantasyArchive.Data;
using FantasyArchive.Data.JsonModels;
using FantasyArchive.Data.Services;

Console.WriteLine("Fantasy Archive Data Exporter");
Console.WriteLine("==============================");

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Get configuration values
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");
var outputPath = configuration["ExportSettings:OutputPath"] ?? "./exports";

Console.WriteLine($"Using output path: {outputPath}");

// Ensure output directory structure exists
Directory.CreateDirectory(outputPath);
Directory.CreateDirectory(Path.Combine(outputPath, "franchises"));
Directory.CreateDirectory(Path.Combine(outputPath, "seasons"));
Directory.CreateDirectory(Path.Combine(outputPath, "records"));
Directory.CreateDirectory(Path.Combine(outputPath, "records", "all-time"));
Directory.CreateDirectory(Path.Combine(outputPath, "records", "seasons"));
Directory.CreateDirectory(Path.Combine(outputPath, "records", "franchises"));

// Setup Entity Framework
var options = new DbContextOptionsBuilder<FantasyArchiveContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new FantasyArchiveContext(options);

try
{
    Console.WriteLine("Exporting franchises...");
    await ExportFranchises(context, outputPath);
    
    Console.WriteLine("Exporting seasons...");
    await ExportSeasons(context, outputPath);
    
    Console.WriteLine("Exporting records...");
    await ExportRecords(context, outputPath);
    Console.WriteLine("Export completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during export: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}

return 0;

static async Task ExportFranchises(FantasyArchiveContext context, string outputPath)
{
    var franchises = await context.Franchises
        .Include(f => f.Teams)
            .ThenInclude(t => t.Owner)
        .Include(f => f.Owners)
        .ToListAsync();

    var franchiseJsonList = new List<FranchiseJson>();

    foreach (var f in franchises)
    {
        Console.WriteLine($"  Processing franchise: {f.MainName}");
        
        // Get all historical owners from teams across all seasons
        var teamOwners = f.Teams
            .Where(t => t.Owner != null)
            .Select(t => t.Owner!.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        // Get current owner from the most recent season
        var latestTeam = f.Teams
            .Where(t => t.Owner != null)
            .OrderByDescending(t => t.Year)
            .FirstOrDefault();
        
        var currentOwner = latestTeam?.Owner?.Name ?? 
                          f.Owners.FirstOrDefault(o => o.Active == true)?.Name ?? 
                          "Unknown";

        // Combine historical owners from both teams and franchise owners table
        var franchiseOwners = f.Owners.Select(o => o.Name).Distinct();
        var allHistoricalOwners = teamOwners.Concat(franchiseOwners)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        // Calculate all-time roster
        AllTimeRosterJson? allTimeRoster = null;
        try
        {
            allTimeRoster = await CalculateAllTimeRoster(context, f.FranchiseId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating all-time roster for {f.MainName}: {ex.Message}");
        }

        var franchiseJson = new FranchiseJson
        {
            Id = f.FranchiseId,
            Name = f.MainName,
            Owner = currentOwner,
            Owners = allHistoricalOwners,
            EstablishedDate = f.Owners.OrderBy(o => o.StartDate).FirstOrDefault()?.StartDate,
            IsActive = f.Owners.Any(o => o.Active == true),
            Color = f.Color,
            AllTimeRoster = allTimeRoster,
            Teams = f.Teams.Select(t => new TeamSummaryJson
            {
                Id = t.TeamId,
                Year = t.Year,
                TeamName = t.Name,
                Wins = t.Wins,
                Losses = t.Loses, // Note: keeping original "Loses" spelling
                Ties = t.Ties,
                Points = t.Points,
                Standing = t.Standing,
                Champion = t.Champion,
                SecondPlace = t.SecondPlace
            }).OrderBy(t => t.Year).ToList()
        };

        franchiseJsonList.Add(franchiseJson);
    }

    // Export individual franchise files
    foreach (var franchise in franchiseJsonList)
    {
        var franchiseJson = JsonSerializer.Serialize(franchise, new JsonSerializerOptions { WriteIndented = true });
        var filename = Path.Combine(outputPath, "franchises", $"{franchise.Id}.json");
        await File.WriteAllTextAsync(filename, franchiseJson);
        Console.WriteLine($"  Exported: {filename}");
    }

    // Export franchises list
    var franchisesListJson = JsonSerializer.Serialize(franchiseJsonList, new JsonSerializerOptions { WriteIndented = true });
    var franchisesListFilename = Path.Combine(outputPath, "franchises", "index.json");
    await File.WriteAllTextAsync(franchisesListFilename, franchisesListJson);
    Console.WriteLine($"  Exported: {franchisesListFilename}");
}

static async Task ExportSeasons(FantasyArchiveContext context, string outputPath)
{
    var seasons = await context.Seasons
        .Include(s => s.Teams)
            .ThenInclude(t => t.Franchise)
        .Include(s => s.Teams)
            .ThenInclude(t => t.Owner)
        .OrderBy(s => s.Year)
        .ToListAsync();

    var seasonJsonList = seasons.Select(s => new SeasonJson
    {
        Year = s.Year,
        Finished = s.Finished,
        CurrentWeek = s.CurrentWeek,
        Teams = s.Teams.Select(t => new TeamDetailJson
        {
            Id = t.TeamId,
            FranchiseId = t.FranchiseId,
            FranchiseName = t.Franchise.MainName,
            Owner = t.Owner?.Name ?? "Unknown",
            TeamName = t.Name,
            Wins = t.Wins,
            Losses = t.Loses, // Note: keeping original "Loses" spelling
            Ties = t.Ties,
            Points = t.Points,
            Standing = t.Standing,
            Champion = t.Champion,
            SecondPlace = t.SecondPlace,
            Color = t.Franchise.Color
        }).OrderBy(t => t.Standing).ToList()
    }).ToList();

    // Export individual season files
    foreach (var season in seasonJsonList)
    {
        var seasonJson = JsonSerializer.Serialize(season, new JsonSerializerOptions { WriteIndented = true });
        var filename = Path.Combine(outputPath, "seasons", $"{season.Year}.json");
        await File.WriteAllTextAsync(filename, seasonJson);
        Console.WriteLine($"  Exported: {filename}");
    }

    // Export seasons list
    var seasonsListJson = JsonSerializer.Serialize(seasonJsonList, new JsonSerializerOptions { WriteIndented = true });
    var seasonsListFilename = Path.Combine(outputPath, "seasons", "index.json");
    await File.WriteAllTextAsync(seasonsListFilename, seasonsListJson);
    Console.WriteLine($"  Exported: {seasonsListFilename}");
}

static async Task<AllTimeRosterJson> CalculateAllTimeRoster(FantasyArchiveContext context, Guid franchiseId)
{
    // Get team IDs for this franchise
    var teamIds = await context.Teams
        .Where(t => t.FranchiseId == franchiseId)
        .Select(t => t.TeamId)
        .ToListAsync();

    // Get player weeks for these teams
    var playerWeeks = await context.PlayerWeeks
        .Where(pw => pw.Started && teamIds.Contains(pw.TeamId))
        .ToListAsync();

    // Load players separately
    var playerIds = playerWeeks.Select(pw => pw.PlayerID).Distinct().ToList();
    var players = await context.Players
        .Where(p => playerIds.Contains(p.PlayerID))
        .ToDictionaryAsync(p => p.PlayerID, p => p);

    // Calculate stats in memory
    var playerStats = playerWeeks
        .Where(pw => players.ContainsKey(pw.PlayerID))
        .GroupBy(pw => pw.PlayerID)
        .Select(g => 
        {
            var player = players[g.Key];
            return new
            {
                PlayerId = g.Key,
                PlayerName = player.Name,
                Position = player.Position ?? "UNKNOWN",
                // Calculate fantasy points using standard scoring
                TotalPoints = g.Sum(pw => 
                    (pw.PassYards / 25.0m) + 
                    (pw.PassTDs * 4) + 
                    (pw.RushYards / 10.0m) + 
                    (pw.RushTDs * 6) + 
                    (pw.RecYards / 10.0m) + 
                    (pw.RecTDs * 6) +
                    (pw.TwoPointConvert * 2) -
                    (pw.Interceptions * 2) -
                    (pw.FumblesLost * 2)),
                WeeksStarted = g.Count(),
                SeasonsWithFranchise = g.Select(pw => pw.Year).Distinct().OrderBy(y => y).ToList()
            };
        })
        .ToList();

    var allTimeRoster = new AllTimeRosterJson();

    // Get top 3 quarterbacks (2 starters + 1 bench)
    var quarterbacks = playerStats
        .Where(p => p.Position == "QB")
        .OrderByDescending(p => p.TotalPoints)
        .Take(3)
        .Select((p, index) => new AllTimePlayerJson
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            Position = p.Position,
            TotalPoints = Math.Round(p.TotalPoints, 1),
            WeeksStarted = p.WeeksStarted,
            AveragePoints = p.WeeksStarted > 0 ? Math.Round(p.TotalPoints / p.WeeksStarted, 1) : 0,
            SeasonsWithFranchise = p.SeasonsWithFranchise,
            IsBench = index >= 2 // First 2 are starters, rest are bench
        })
        .ToList();
    allTimeRoster.Quarterbacks = quarterbacks;

    // Get top 3 running backs (2 starters + 1 bench)
    var runningBacks = playerStats
        .Where(p => p.Position == "RB")
        .OrderByDescending(p => p.TotalPoints)
        .Take(3)
        .Select((p, index) => new AllTimePlayerJson
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            Position = p.Position,
            TotalPoints = Math.Round(p.TotalPoints, 1),
            WeeksStarted = p.WeeksStarted,
            AveragePoints = p.WeeksStarted > 0 ? Math.Round(p.TotalPoints / p.WeeksStarted, 1) : 0,
            SeasonsWithFranchise = p.SeasonsWithFranchise,
            IsBench = index >= 2 // First 2 are starters, rest are bench
        })
        .ToList();
    allTimeRoster.RunningBacks = runningBacks;

    // Get top 4 wide receivers (3 starters + 1 bench)
    var wideReceivers = playerStats
        .Where(p => p.Position == "WR")
        .OrderByDescending(p => p.TotalPoints)
        .Take(4)
        .Select((p, index) => new AllTimePlayerJson
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            Position = p.Position,
            TotalPoints = Math.Round(p.TotalPoints, 1),
            WeeksStarted = p.WeeksStarted,
            AveragePoints = p.WeeksStarted > 0 ? Math.Round(p.TotalPoints / p.WeeksStarted, 1) : 0,
            SeasonsWithFranchise = p.SeasonsWithFranchise,
            IsBench = index >= 3 // First 3 are starters, rest are bench
        })
        .ToList();
    allTimeRoster.WideReceivers = wideReceivers;

    // Get top 2 tight ends (1 starter + 1 bench)
    var tightEnds = playerStats
        .Where(p => p.Position == "TE")
        .OrderByDescending(p => p.TotalPoints)
        .Take(2)
        .Select((p, index) => new AllTimePlayerJson
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            Position = p.Position,
            TotalPoints = Math.Round(p.TotalPoints, 1),
            WeeksStarted = p.WeeksStarted,
            AveragePoints = p.WeeksStarted > 0 ? Math.Round(p.TotalPoints / p.WeeksStarted, 1) : 0,
            SeasonsWithFranchise = p.SeasonsWithFranchise,
            IsBench = index >= 1 // First 1 is starter, rest are bench
        })
        .ToList();
    allTimeRoster.TightEnds = tightEnds;

    return allTimeRoster;
}

static async Task ExportRecords(FantasyArchiveContext context, string outputPath)
{
    var weeklyRecordsService = new WeeklyRecordsService(context);
    var recordsService = new RecordsService(context, weeklyRecordsService);
    
    // Export all-time records
    Console.WriteLine("  Exporting all-time records...");
    var allTimeRecords = await recordsService.GetRecordsAsync();
    var allTimeRecordsJson = ConvertToJsonRecords(allTimeRecords);
    var allTimeFilename = Path.Combine(outputPath, "records", "all-time", "index.json");
    var allTimeJsonString = JsonSerializer.Serialize(allTimeRecordsJson, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(allTimeFilename, allTimeJsonString);
    Console.WriteLine($"    Exported: {allTimeFilename}");

    // Export season-specific records for each season
    var seasons = await context.Seasons.Select(s => s.Year).Distinct().OrderBy(y => y).ToListAsync();
    foreach (var year in seasons)
    {
        Console.WriteLine($"  Exporting {year} season records...");
        var seasonRecords = await recordsService.GetRecordsAsync(season: year);
        var seasonRecordsJson = ConvertToJsonRecords(seasonRecords);
        var seasonFilename = Path.Combine(outputPath, "records", "seasons", $"{year}.json");
        var seasonJsonString = JsonSerializer.Serialize(seasonRecordsJson, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(seasonFilename, seasonJsonString);
        Console.WriteLine($"    Exported: {seasonFilename}");
    }

    // Export franchise-specific records for each franchise
    var franchises = await context.Franchises.ToListAsync();
    foreach (var franchise in franchises)
    {
        Console.WriteLine($"  Exporting {franchise.MainName} franchise records...");
        var franchiseRecords = await recordsService.GetRecordsAsync(franchiseId: franchise.FranchiseId);
        var franchiseRecordsJson = ConvertToJsonRecords(franchiseRecords);
        var franchiseFilename = Path.Combine(outputPath, "records", "franchises", $"{franchise.FranchiseId}.json");
        var franchiseJsonString = JsonSerializer.Serialize(franchiseRecordsJson, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(franchiseFilename, franchiseJsonString);
        Console.WriteLine($"    Exported: {franchiseFilename}");
    }
}

static List<LeagueRecordsJson> ConvertToJsonRecords(List<FantasyArchive.Data.Models.LeagueRecords> records)
{
    return records.Select(r => new LeagueRecordsJson
    {
        RecordTitle = r.RecordTitle,
        PositiveRecord = r.PositiveRecord,
        RecordType = r.RecordType.ToString(),
        Records = r.Records.Select(record => new LeagueRecordJson
        {
            Rank = record.Rank,
            FranchiseId = record.FranchiseId?.ToString(),
            FranchiseName = record.Franchise?.MainName,
            OtherFranchiseId = record.OtherFranchiseId?.ToString(),
            OtherFranchiseName = record.OtherFranchise?.MainName,
            PlayerId = record.PlayerId,
            PlayerName = record.Player?.Name,
            PlayerPosition = record.Player?.Position,
            RecordValue = record.RecordValue,
            RecordNumericValue = record.RecordNumericValue,
            Year = record.Year,
            Week = record.Week
        }).ToList()
    }).ToList();
}
