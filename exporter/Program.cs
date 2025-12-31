using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FantasyArchive.Data;
using FantasyArchive.Data.JsonModels;
using FantasyArchive.Data.Services;
using FantasyArchive.Data.Models;

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
Directory.CreateDirectory(Path.Combine(outputPath, "trade-trees"));
Directory.CreateDirectory(Path.Combine(outputPath, "trade-trees", "mermaid"));
Directory.CreateDirectory(Path.Combine(outputPath, "wrapped"));
Directory.CreateDirectory(Path.Combine(outputPath, "wrapped", "franchises"));
Directory.CreateDirectory(Path.Combine(outputPath, "wrapped", "seasons"));

// Setup Entity Framework
var options = new DbContextOptionsBuilder<FantasyArchiveContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new FantasyArchiveContext(options);

try
{
    // Console.WriteLine("Exporting franchises...");
    // await ExportFranchises(context, outputPath);
    
    // Console.WriteLine("Exporting seasons...");
    // await ExportSeasons(context, outputPath);
    
    // Console.WriteLine("Exporting records...");
    // await ExportRecords(context, outputPath);
    
    // Console.WriteLine("Exporting trade trees...");
    // await ExportTradeTrees(context, outputPath);
    
    Console.WriteLine("Exporting franchise wrapped data...");
    await ExportFranchiseWrappedSimple(context, outputPath);
    
    // Console.WriteLine("Testing specific trade tree...");
    // await TestSpecificTradeTree(context);
    
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
        .Where(pw => pw.Started && pw.TeamId.HasValue && teamIds.Contains(pw.TeamId.Value))
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
                PlayerID = g.Key,
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
            PlayerID = p.PlayerID,
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
            PlayerID = p.PlayerID,
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
            PlayerID = p.PlayerID,
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
            PlayerID = p.PlayerID,
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
            PlayerID = record.PlayerID,
            PlayerName = record.Player?.Name,
            PlayerPosition = record.Player?.Position,
            RecordValue = record.RecordValue,
            RecordNumericValue = record.RecordNumericValue,
            Year = record.Year,
            Week = record.Week
        }).ToList()
    }).ToList();
}

static async Task ExportTradeTrees(FantasyArchiveContext context, string outputPath)
{
    var tradeTreeService = new TradeTreeService(context);
    
    // Export trades by year
    Console.WriteLine("  Exporting trades by year...");
    var tradesByYear = await tradeTreeService.GetTradesByYearAsync();
    var tradesByYearJson = tradesByYear.Select(kvp => new TradesByYearJson
    {
        Year = kvp.Key,
        Trades = kvp.Value.Select(t => new TradeGroupJson
        {
            TransactionGroupId = t.TransactionGroupId.ToString(),
            Date = t.Date,
            Description = t.Description,
            PlayersInvolved = t.PlayersInvolved,
            TradeSides = t.TradeSides.Select(ts => new TradeSideJson
            {
                FranchiseId = ts.FranchiseId,
                FranchiseName = ts.FranchiseName,
                FranchiseColor = ts.FranchiseColor,
                Players = ts.Players.Select(p => new TradedPlayerJson
                {
                    PlayerID = p.PlayerID,
                    PlayerName = p.PlayerName,
                    PrimaryPosition = p.PrimaryPosition
                }).ToList()
            }).ToList()
        }).ToList()
    }).OrderBy(t => t.Year).ToList();
    
    var tradesByYearJsonString = JsonSerializer.Serialize(tradesByYearJson, new JsonSerializerOptions { WriteIndented = true });
    var tradesByYearFilename = Path.Combine(outputPath, "trade-trees", "index.json");
    await File.WriteAllTextAsync(tradesByYearFilename, tradesByYearJsonString);
    Console.WriteLine($"    Exported: {tradesByYearFilename}");

    // Export individual trade tree Mermaid diagrams only
    foreach (var yearTrades in tradesByYear)
    {
        foreach (var trade in yearTrades.Value)
        {
            Console.WriteLine($"  Exporting trade tree for {trade.Description}...");
            try
            {
                var tradeTree = await tradeTreeService.CalculateTradeTreeAsync(trade.TransactionGroupId);
                var tradeTreeJson = ConvertToJsonTradeTree(tradeTree);
                
                // Export Mermaid diagram file only (no individual JSON files)
                var mermaidDiagram = await MermaidGeneratorService.GenerateDetailedMermaidDiagramAsync(tradeTreeJson, context);
                var mermaidFilename = Path.Combine(outputPath, "trade-trees", "mermaid", $"{trade.TransactionGroupId}.mmd");
                await File.WriteAllTextAsync(mermaidFilename, mermaidDiagram);
                Console.WriteLine($"    Exported: {mermaidFilename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Error exporting trade tree for {trade.TransactionGroupId}: {ex.Message}");
            }
        }
    }
}

static TradeTreeJson ConvertToJsonTradeTree(TradeTree tradeTree)
{
    return new TradeTreeJson
    {
        TransactionGroupId = tradeTree.TransactionGroupId.ToString(),
        TradeDate = tradeTree.TradeDate,
        Description = tradeTree.Description,
        RootNodes = tradeTree.RootNodes.Select(ConvertToJsonTradeTreeNode).ToList()
    };
}

static TradeTreeNodeJson ConvertToJsonTradeTreeNode(TradeTreeNode node)
{
    return new TradeTreeNodeJson
    {
        Transaction = new TransactionJson
        {
            TransactionId = node.Transaction.TransactionId.ToString(),
            TransactionGroupId = node.Transaction.TransactionGroupId?.ToString(),
            TeamId = node.Transaction.TeamId.ToString(),
            TeamName = node.Transaction.Team?.Name ?? "Unknown Team",
            FranchiseId = node.Transaction.Team?.FranchiseId.ToString() ?? string.Empty,
            FranchiseName = node.Transaction.Team?.Franchise?.MainName ?? "Unknown Franchise",
            TransactionType = node.Transaction.TransactionType.ToString(),
            PlayerID = node.Transaction.PlayerID,
            PlayerName = node.Transaction.Player?.Name ?? "Unknown Player",
            Date = node.Transaction.Date,
            Description = node.Transaction.Description,
            Year = node.Transaction.Year,
            PlayerTransactionIndex = node.Transaction.PlayerTransactionIndex
        },
        Children = node.Children.Select(ConvertToJsonTradeTreeNode).ToList(),
        IsEndNode = node.IsEndNode
    };
}

static async Task ExportFranchiseWrappedSimple(FantasyArchiveContext context, string outputPath)
{
    Console.WriteLine("  Getting franchise and season data...");
    
    // Get all franchises and seasons
    var franchises = await context.Franchises.ToListAsync();
    var seasons = await context.Seasons
        .Where(s => s.Finished == true) // Only export wrapped data for completed seasons
        .Select(s => s.Year)
        .OrderBy(y => y)
        .ToListAsync();

    foreach (var franchise in franchises)
    {
        Console.WriteLine($"  Processing wrapped data for {franchise.MainName}...");
        
        foreach (var year in seasons)
        {
            try
            {
                var wrappedData = await GenerateFranchiseWrappedSimple(context, franchise.FranchiseId, year);
                if (wrappedData != null)
                {
                    // Export individual franchise-season wrapped file
                    var wrappedJson = JsonSerializer.Serialize(wrappedData, new JsonSerializerOptions { WriteIndented = true });
                    var filename = Path.Combine(outputPath, "wrapped", "franchises", $"{franchise.FranchiseId}_{year}.json");
                    await File.WriteAllTextAsync(filename, wrappedJson);
                    Console.WriteLine($"    Exported: {filename}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Error generating wrapped data for {franchise.MainName} {year}: {ex.Message}");
            }
        }
    }

    // Create season indexes
    foreach (var year in seasons)
    {
        Console.WriteLine($"  Creating wrapped index for {year}...");
        var seasonWrapped = new List<object>();
        
        foreach (var franchise in franchises)
        {
            var filename = Path.Combine(outputPath, "wrapped", "franchises", $"{franchise.FranchiseId}_{year}.json");
            if (File.Exists(filename))
            {
                seasonWrapped.Add(new 
                {
                    FranchiseId = franchise.FranchiseId.ToString(),
                    FranchiseName = franchise.MainName,
                    Year = year,
                    FilePath = $"franchises/{franchise.FranchiseId}_{year}.json"
                });
            }
        }
        
        var seasonIndexJson = JsonSerializer.Serialize(seasonWrapped, new JsonSerializerOptions { WriteIndented = true });
        var seasonIndexFilename = Path.Combine(outputPath, "wrapped", "seasons", $"{year}.json");
        await File.WriteAllTextAsync(seasonIndexFilename, seasonIndexJson);
        Console.WriteLine($"    Exported: {seasonIndexFilename}");
    }
}

static async Task<FranchiseWrappedJson?> GenerateFranchiseWrappedSimple(FantasyArchiveContext context, Guid franchiseId, int year)
{
    // Get the team for this franchise and year
    var team = await context.Teams
        .Include(t => t.Franchise)
        .Include(t => t.Owner)
        .FirstOrDefaultAsync(t => t.FranchiseId == franchiseId && t.Year == year);
    
    if (team == null) return null;

    // Get all teams for the season (for comparisons)
    var allTeams = await context.Teams
        .Include(t => t.Franchise)
        .Where(t => t.Year == year)
        .ToListAsync();

    // Get player data for this team
    var playerWeeks = await context.PlayerWeeks
        .Include(pw => pw.Player)
        .Where(pw => pw.Year == year && pw.TeamId == team.TeamId)
        .ToListAsync();

    // Get team scores for this team
    var teamScores = await context.TeamScores
        .Where(ts => ts.TeamID == team.TeamId && ts.Year == year)
        .OrderBy(ts => ts.Week)
        .ToListAsync();

    var wrappedData = new FranchiseWrappedJson
    {
        FranchiseId = franchiseId.ToString(),
        FranchiseName = team.Franchise.MainName,
        Owner = team.Owner?.Name ?? "Unknown",
        Year = year
    };

    // Generate basic season summary
    wrappedData.SeasonSummary = new SeasonSummaryJson
    {
        Wins = team.Wins,
        Losses = team.Loses,
        Ties = team.Ties,
        Points = (decimal)team.Points,
        Standing = team.Standing,
        TotalTeams = allTeams.Count,
        Champion = team.Champion,
        SecondPlace = team.SecondPlace,
        MadePlayoffs = team.Standing <= (allTeams.Count / 2),
        SeasonOutcome = team.Champion ? "Champion" : 
                       team.SecondPlace ? "Runner-up" : 
                       team.Standing <= (allTeams.Count / 2) ? "Made Playoffs" : "Missed Playoffs"
    };

    // Generate performance highlights
    if (teamScores.Any())
    {
        var bestWeek = teamScores.OrderByDescending(ts => ts.Points).First();
        var worstWeek = teamScores.OrderBy(ts => ts.Points).First();
        
        wrappedData.Performance = new PerformanceHighlightsJson
        {
            BestWeek = new WeekPerformanceJson
            {
                Week = bestWeek.Week,
                Points = (decimal)bestWeek.Points,
                Opponent = "TBD", // Would need match data
                Won = false, // Would need match data
                Margin = 0 // Would need match data
            },
            WorstWeek = new WeekPerformanceJson
            {
                Week = worstWeek.Week,
                Points = (decimal)worstWeek.Points,
                Opponent = "TBD",
                Won = false,
                Margin = 0
            },
            AverageWeeklyScore = (decimal)teamScores.Average(ts => ts.Points),
            HighestWeeklyScore = (decimal)teamScores.Max(ts => ts.Points),
            LowestWeeklyScore = (decimal)teamScores.Min(ts => ts.Points),
            WeeksAsHighScorer = await GetWeeksAsHighScorer(context, team, teamScores),
            WeeksAsLowScorer = await GetWeeksAsLowScorer(context, team, teamScores)
        };
    }

    // Generate player highlights
    if (playerWeeks.Any())
    {
        var playerStats = playerWeeks
            .Where(pw => pw.Player != null)
            .GroupBy(pw => pw.PlayerID)
            .Select(g => 
            {
                var player = g.First().Player!;
                var totalPoints = g.Sum(pw => CalculateFantasyPoints(pw));
                var gamesStarted = g.Count(pw => pw.Started);
                var gamesOwned = g.Count();
                
                return new PlayerSeasonStatsJson
                {
                    PlayerID = player.PlayerID,
                    PlayerName = player.Name,
                    Position = player.Position ?? "UNKNOWN",
                    TotalPoints = totalPoints,
                    AveragePoints = gamesOwned > 0 ? totalPoints / gamesOwned : 0,
                    GamesStarted = gamesStarted,
                    GamesOwned = gamesOwned,
                    HighlightReason = "Top performer for your team"
                };
            })
            .OrderByDescending(p => p.TotalPoints)
            .ToList();

        var mvp = playerStats.FirstOrDefault() ?? new PlayerSeasonStatsJson();
        mvp.HighlightReason = "Your highest scoring player this season";

        wrappedData.Players = new PlayerHighlightsJson
        {
            MVP = mvp,
            TopStarters = playerStats.Take(5).ToList()
        };
    }

    // Generate league comparisons
    var sortedByPoints = allTeams.OrderByDescending(t => t.Points).ToList();
    var pointsRank = sortedByPoints.FindIndex(t => t.TeamId == team.TeamId) + 1;
    var leagueAveragePoints = allTeams.Average(t => t.Points);

    // Get bench and roster data for all teams
    var allTeamPlayerData = new Dictionary<Guid, List<PlayerWeek>>();
    foreach (var otherTeam in allTeams)
    {
        var otherPlayerWeeks = await context.PlayerWeeks
            .Include(pw => pw.Player)
            .Where(pw => pw.Year == year && pw.TeamId == otherTeam.TeamId)
            .ToListAsync();
        allTeamPlayerData[otherTeam.TeamId] = otherPlayerWeeks;
    }

    // Calculate bench statistics for all teams
    var benchStats = new Dictionary<Guid, decimal>();
    var rosterDiversity = new Dictionary<Guid, int>();
    var positionUsage = new Dictionary<Guid, Dictionary<string, int>>();

    foreach (var (teamId, teamPlayerWeeks) in allTeamPlayerData)
    {
        // Calculate bench points (rostered but not started)
        var benchPoints = teamPlayerWeeks
            .Where(pw => !pw.Started && pw.Player != null)
            .Sum(pw => CalculateFantasyPoints(pw));
        benchStats[teamId] = benchPoints;

        // Calculate roster diversity (unique players started)
        var uniqueStartedPlayers = teamPlayerWeeks
            .Where(pw => pw.Started)
            .Select(pw => pw.PlayerID)
            .Distinct()
            .Count();
        rosterDiversity[teamId] = uniqueStartedPlayers;

        // Calculate position usage
        var posUsage = teamPlayerWeeks
            .Where(pw => pw.Started && pw.Player != null)
            .GroupBy(pw => pw.Player!.Position ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Select(pw => pw.PlayerID).Distinct().Count());
        positionUsage[teamId] = posUsage;
    }

    // Calculate our team's bench and diversity stats
    var ourBenchPoints = benchStats.ContainsKey(team.TeamId) ? benchStats[team.TeamId] : 0m;
    var ourTotalPlayersUsed = rosterDiversity.ContainsKey(team.TeamId) ? rosterDiversity[team.TeamId] : 0;
    var ourPositionUsage = positionUsage.ContainsKey(team.TeamId) ? positionUsage[team.TeamId] : new Dictionary<string, int>();

    // Calculate rankings
    var benchRank = benchStats.OrderByDescending(x => x.Value).ToList().FindIndex(x => x.Key == team.TeamId) + 1;
    var diversityRank = rosterDiversity.OrderByDescending(x => x.Value).ToList().FindIndex(x => x.Key == team.TeamId) + 1;
    
    // Find best bench player
    PlayerSeasonStatsJson? topBenchPlayer = null;
    if (playerWeeks.Any(pw => !pw.Started))
    {
        var benchPlayerStats = playerWeeks
            .Where(pw => !pw.Started && pw.Player != null)
            .GroupBy(pw => pw.PlayerID)
            .Select(g => 
            {
                var player = g.First().Player!;
                var totalPoints = g.Sum(pw => CalculateFantasyPoints(pw));
                return new PlayerSeasonStatsJson
                {
                    PlayerID = player.PlayerID,
                    PlayerName = player.Name,
                    Position = player.Position ?? "UNKNOWN",
                    TotalPoints = totalPoints,
                    GamesOwned = g.Count(),
                    HighlightReason = "Your best bench performer"
                };
            })
            .OrderByDescending(p => p.TotalPoints)
            .FirstOrDefault();
        topBenchPlayer = benchPlayerStats;
    }

    // Position usage rankings
    var positionRanks = new Dictionary<string, int>();
    foreach (var position in new[] { "QB", "RB", "WR", "TE", "K", "DST" })
    {
        var ourUsage = ourPositionUsage.ContainsKey(position) ? ourPositionUsage[position] : 0;
        var sortedUsage = positionUsage
            .Select(x => x.Value.ContainsKey(position) ? x.Value[position] : 0)
            .OrderByDescending(x => x)
            .ToList();
        var rank = sortedUsage.FindIndex(x => x == ourUsage) + 1;
        positionRanks[position] = rank;
    }
    
    // Calculate position strength (total starter points by position)
    var allTeamPositionPoints = new Dictionary<Guid, Dictionary<string, decimal>>();
    foreach (var otherTeam in allTeams)
    {
        var teamPlayerWeeks = allTeamPlayerData[otherTeam.TeamId];
        var positionPoints = new Dictionary<string, decimal>
        {
            ["QB"] = 0, ["RB"] = 0, ["WR"] = 0, ["TE"] = 0, ["K"] = 0, ["DST"] = 0
        };
        
        foreach (var pw in teamPlayerWeeks.Where(pw => pw.Started && pw.Player != null))
        {
            var position = pw.Player!.Position ?? "UNKNOWN";
            if (positionPoints.ContainsKey(position))
            {
                positionPoints[position] += CalculateFantasyPoints(pw);
            }
        }
        
        allTeamPositionPoints[otherTeam.TeamId] = positionPoints;
    }
    
    var ourPositionPoints = allTeamPositionPoints[team.TeamId];
    
    // Calculate position strength rankings
    var positionStrengthRanks = new Dictionary<string, int>();
    var avgPositionPoints = new Dictionary<string, decimal>();
    foreach (var position in new[] { "QB", "RB", "WR", "TE", "K", "DST" })
    {
        var ourPoints = ourPositionPoints.ContainsKey(position) ? ourPositionPoints[position] : 0;
        var allTeamPoints = allTeamPositionPoints.Values
            .Select(x => x.ContainsKey(position) ? x[position] : 0)
            .OrderByDescending(x => x)
            .ToList();
        var rank = allTeamPoints.FindIndex(x => x == ourPoints) + 1;
        positionStrengthRanks[position] = rank;
        avgPositionPoints[position] = allTeamPoints.Average();
    }

    wrappedData.LeagueComparisons = new LeagueComparisonsJson
    {
        PointsRank = pointsRank,
        PointsRankSuffix = GetOrdinalSuffix(pointsRank),
        PointsAboveAverage = (decimal)team.Points - (decimal)leagueAveragePoints,
        WinsRank = allTeams.OrderByDescending(t => t.Wins).ToList().FindIndex(t => t.TeamId == team.TeamId) + 1,
        WinsRankSuffix = GetOrdinalSuffix(allTeams.OrderByDescending(t => t.Wins).ToList().FindIndex(t => t.TeamId == team.TeamId) + 1),
        
        // Bench Analysis
        BenchPoints = ourBenchPoints,
        BenchPointsRank = benchRank,
        BenchPointsRankSuffix = GetOrdinalSuffix(benchRank),
        BenchPointsAboveAverage = ourBenchPoints - benchStats.Values.Average(),
        TopBenchPlayer = topBenchPlayer,
        
        // Roster Diversity
        TotalPlayersUsed = ourTotalPlayersUsed,
        TotalPlayersUsedRank = diversityRank,
        TotalPlayersUsedRankSuffix = GetOrdinalSuffix(diversityRank),
        AvgPlayersUsedInLeague = (decimal)rosterDiversity.Values.Average(),
        
        // Position Diversity
        QuarterbacksUsed = ourPositionUsage.ContainsKey("QB") ? ourPositionUsage["QB"] : 0,
        RunningBacksUsed = ourPositionUsage.ContainsKey("RB") ? ourPositionUsage["RB"] : 0,
        WideReceiversUsed = ourPositionUsage.ContainsKey("WR") ? ourPositionUsage["WR"] : 0,
        TightEndsUsed = ourPositionUsage.ContainsKey("TE") ? ourPositionUsage["TE"] : 0,
        KickersUsed = ourPositionUsage.ContainsKey("K") ? ourPositionUsage["K"] : 0,
        DefensesUsed = ourPositionUsage.ContainsKey("DST") ? ourPositionUsage["DST"] : 0,
        
        // Position Usage Rankings
        QBUsageRank = positionRanks.ContainsKey("QB") ? positionRanks["QB"] : allTeams.Count,
        RBUsageRank = positionRanks.ContainsKey("RB") ? positionRanks["RB"] : allTeams.Count,
        WRUsageRank = positionRanks.ContainsKey("WR") ? positionRanks["WR"] : allTeams.Count,
        TEUsageRank = positionRanks.ContainsKey("TE") ? positionRanks["TE"] : allTeams.Count,
        KUsageRank = positionRanks.ContainsKey("K") ? positionRanks["K"] : allTeams.Count,
        DSTUsageRank = positionRanks.ContainsKey("DST") ? positionRanks["DST"] : allTeams.Count,
        
        // Position Strength
        QuarterbackPoints = ourPositionPoints.ContainsKey("QB") ? ourPositionPoints["QB"] : 0,
        RunningBackPoints = ourPositionPoints.ContainsKey("RB") ? ourPositionPoints["RB"] : 0,
        WideReceiverPoints = ourPositionPoints.ContainsKey("WR") ? ourPositionPoints["WR"] : 0,
        TightEndPoints = ourPositionPoints.ContainsKey("TE") ? ourPositionPoints["TE"] : 0,
        KickerPoints = ourPositionPoints.ContainsKey("K") ? ourPositionPoints["K"] : 0,
        DefensePoints = ourPositionPoints.ContainsKey("DST") ? ourPositionPoints["DST"] : 0,
        QBPointsRank = positionStrengthRanks.ContainsKey("QB") ? positionStrengthRanks["QB"] : allTeams.Count,
        RBPointsRank = positionStrengthRanks.ContainsKey("RB") ? positionStrengthRanks["RB"] : allTeams.Count,
        WRPointsRank = positionStrengthRanks.ContainsKey("WR") ? positionStrengthRanks["WR"] : allTeams.Count,
        TEPointsRank = positionStrengthRanks.ContainsKey("TE") ? positionStrengthRanks["TE"] : allTeams.Count,
        KPointsRank = positionStrengthRanks.ContainsKey("K") ? positionStrengthRanks["K"] : allTeams.Count,
        DSTPointsRank = positionStrengthRanks.ContainsKey("DST") ? positionStrengthRanks["DST"] : allTeams.Count,
        AvgQBPointsInLeague = avgPositionPoints.ContainsKey("QB") ? avgPositionPoints["QB"] : 0,
        AvgRBPointsInLeague = avgPositionPoints.ContainsKey("RB") ? avgPositionPoints["RB"] : 0,
        AvgWRPointsInLeague = avgPositionPoints.ContainsKey("WR") ? avgPositionPoints["WR"] : 0,
        AvgTEPointsInLeague = avgPositionPoints.ContainsKey("TE") ? avgPositionPoints["TE"] : 0,
        AvgKPointsInLeague = avgPositionPoints.ContainsKey("K") ? avgPositionPoints["K"] : 0,
        AvgDSTPointsInLeague = avgPositionPoints.ContainsKey("DST") ? avgPositionPoints["DST"] : 0
    };

    // Generate fun facts
    var facts = new List<string>();
    
    if (team.Champion)
        facts.Add("👑 You are the CHAMPION! What a season!");
    else if (team.SecondPlace)
        facts.Add("🥈 You made it to the championship game - so close!");
    
    if (pointsRank == 1)
        facts.Add("🏆 You scored the most points in the league!");
    else if (pointsRank <= allTeams.Count / 2)
        facts.Add($"You were in the top {pointsRank} in points scored this season!");

    if (playerWeeks.Any())
    {
        var totalPlayerChanges = playerWeeks.Select(pw => pw.PlayerID).Distinct().Count();
        facts.Add($"You rostered {totalPlayerChanges} different players throughout the season!");
        
        // Starter diversity insights
        if (diversityRank == 1)
            facts.Add("🔄 You used the most different players in your starting lineup - ultimate tinkerer!");
        else if (diversityRank == allTeams.Count)
            facts.Add("🎯 You were the most loyal to your core players - stick to what works!");
        else if (diversityRank <= 3)
            facts.Add($"You were #{diversityRank} in roster diversity - always looking for an edge!");
    }

    // Bench insights
    if (benchRank == 1)
        facts.Add("💪 Your bench scored the most points in the league - depth for days!");
    else if (benchRank <= 3)
        facts.Add($"Your bench was ranked #{benchRank} in scoring - great depth!");
    else if (benchRank >= allTeams.Count - 2)
        facts.Add("Your bench was... economical. Quality over quantity, right?");

    // Position usage insights
    var mostUsedPosition = positionRanks.OrderBy(x => x.Value).First();
    var leastUsedPosition = positionRanks.OrderByDescending(x => x.Value).First();
    
    if (mostUsedPosition.Value == 1)
        facts.Add($"You used the most different {GetPositionFullName(mostUsedPosition.Key)}s - variety is the spice of life!");
    
    if (leastUsedPosition.Value == allTeams.Count && leastUsedPosition.Key != "K" && leastUsedPosition.Key != "DST")
        facts.Add($"You were most loyal to your {GetPositionFullName(leastUsedPosition.Key)} position - when you find 'the one,' you stick with them!");

    // Bench vs starter balance
    var starterPoints = playerWeeks.Where(pw => pw.Started).Sum(pw => CalculateFantasyPoints(pw));
    if (starterPoints > 0)
    {
        var benchPercentage = (ourBenchPoints / starterPoints) * 100;
        if (benchPercentage > 25)
            facts.Add($"Your bench scored {benchPercentage:F0}% as many points as your starters - incredible depth!");
        else if (benchPercentage < 5)
            facts.Add("You maximized your starting lineup efficiency - every point counted!");
    }

    wrappedData.FunFacts = facts;

    // Generate achievements
    var achievements = new List<AchievementJson>();
    
    if (team.Champion)
    {
        achievements.Add(new AchievementJson
        {
            Title = "League Champion",
            Description = "Won the fantasy football championship!",
            Icon = "👑",
            IsRare = true
        });
    }

    if (pointsRank == 1)
    {
        achievements.Add(new AchievementJson
        {
            Title = "League Leading Scorer",
            Description = "Scored the most points in the league this season!",
            Icon = "🎯",
            IsRare = true
        });
    }

    wrappedData.Achievements = achievements;

    // Initialize empty head-to-head for now (would need match data)
    wrappedData.HeadToHeadRecords = new List<HeadToHeadJson>();

    return wrappedData;
}

// Helper methods
static decimal CalculateFantasyPoints(PlayerWeek pw)
{
    return (pw.PassYards / 25m) + (pw.PassTDs * 4) + (pw.RushYards / 10m) + (pw.RushTDs * 6) + 
           (pw.RecYards / 10m) + (pw.RecTDs * 6) + (pw.TwoPointConvert * 2) - 
           (pw.Interceptions * 2) - (pw.FumblesLost * 2);
}

static string GetOrdinalSuffix(int number)
{
    if (number % 100 is 11 or 12 or 13) return "th";
    return (number % 10) switch
    {
        1 => "st",
        2 => "nd", 
        3 => "rd",
        _ => "th"
    };
}

static string GetPositionFullName(string position)
{
    return position switch
    {
        "QB" => "quarterback",
        "RB" => "running back", 
        "WR" => "wide receiver",
        "TE" => "tight end",
        "K" => "kicker",
        "DST" => "defense",
        _ => position.ToLower()
    };
}

static async Task<int> GetWeeksAsHighScorer(FantasyArchiveContext context, Team team, List<TeamScore> teamScores)
{
    int highScoreWeeks = 0;
    
    foreach (var score in teamScores)
    {
        var weekHighScore = await context.TeamScores
            .Where(ts => ts.Year == team.Year && ts.Week == score.Week)
            .MaxAsync(ts => ts.Points);
        
        if (Math.Abs(score.Points - weekHighScore) < 0.01) // Account for floating point precision
            highScoreWeeks++;
    }
    
    return highScoreWeeks;
}

static async Task<int> GetWeeksAsLowScorer(FantasyArchiveContext context, Team team, List<TeamScore> teamScores)
{
    int lowScoreWeeks = 0;
    
    foreach (var score in teamScores)
    {
        var weekLowScore = await context.TeamScores
            .Where(ts => ts.Year == team.Year && ts.Week == score.Week)
            .MinAsync(ts => ts.Points);
        
        if (Math.Abs(score.Points - weekLowScore) < 0.01) // Account for floating point precision
            lowScoreWeeks++;
    }
    
    return lowScoreWeeks;
}
