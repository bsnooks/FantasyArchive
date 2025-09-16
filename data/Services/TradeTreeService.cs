using Microsoft.EntityFrameworkCore;
using FantasyArchive.Data.Models;

namespace FantasyArchive.Data.Services
{
    public class TradeTreeNode
    {
        public Transaction Transaction { get; set; } = null!;
        public List<TradeTreeNode> Children { get; set; } = new List<TradeTreeNode>();
        public bool IsEndNode { get; set; } // True if this is a draft or drop
    }

    public class TradeTree
    {
        public Guid TransactionGroupId { get; set; }
        public DateTime TradeDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<TradeTreeNode> RootNodes { get; set; } = new List<TradeTreeNode>();
    }

    public class TradeTreeService
    {
        private readonly FantasyArchiveContext _context;

        public TradeTreeService(FantasyArchiveContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all trades by year for displaying the trade list
        /// </summary>
        public async Task<Dictionary<int, List<TradeGroup>>> GetTradesByYearAsync()
        {
            var trades = await _context.TransactionGroups
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Player)
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(t => t!.Franchise)
                .Where(tg => tg.Transactions.Any(t => t.TransactionType == TransactionType.Traded))
                .OrderBy(tg => tg.Date)
                .ToListAsync();

            var tradesByYear = new Dictionary<int, List<TradeGroup>>();

            foreach (var tradeGroup in trades)
            {
                var year = tradeGroup.Date.Year;
                var tradeDescription = GenerateTradeDescription(tradeGroup);
                
                if (!tradesByYear.ContainsKey(year))
                    tradesByYear[year] = new List<TradeGroup>();

                // Get detailed trade sides information
                var tradedTransactions = tradeGroup.Transactions
                    .Where(t => t.TransactionType == TransactionType.Traded)
                    .GroupBy(t => new { 
                        FranchiseId = t.Team?.Franchise?.FranchiseId ?? Guid.Empty,
                        FranchiseName = t.Team?.Franchise?.MainName ?? "Unknown Team",
                        FranchiseColor = t.Team?.Franchise?.Color ?? "#808080"
                    })
                    .ToList();

                var tradeSides = tradedTransactions.Select(group => new TradeSide
                {
                    FranchiseId = group.Key.FranchiseId.ToString(),
                    FranchiseName = group.Key.FranchiseName,
                    FranchiseColor = group.Key.FranchiseColor,
                    Players = group.Select(t => new TradedPlayer
                    {
                        PlayerId = t.Player?.PlayerID ?? 0,
                        PlayerName = t.Player?.Name ?? "Unknown Player",
                        PrimaryPosition = t.Player?.PrimaryPosition ?? "Unknown"
                    }).ToList()
                }).ToList();

                tradesByYear[year].Add(new TradeGroup
                {
                    TransactionGroupId = tradeGroup.TransactionGroupId,
                    Date = tradeGroup.Date,
                    Description = tradeDescription,
                    PlayersInvolved = tradeGroup.Transactions
                        .Where(t => t.TransactionType == TransactionType.Traded)
                        .Select(t => t.Player?.Name ?? "Unknown Player")
                        .ToList(),
                    TradeSides = tradeSides
                });
            }

            return tradesByYear;
        }

        /// <summary>
        /// Calculates the complete trade tree for a given transaction group
        /// </summary>
        public async Task<TradeTree> CalculateTradeTreeAsync(Guid transactionGroupId)
        {
            var tradeGroup = await _context.TransactionGroups
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Player)
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(t => t!.Franchise)
                .FirstOrDefaultAsync(tg => tg.TransactionGroupId == transactionGroupId);

            if (tradeGroup == null)
            {
                throw new ArgumentException($"Transaction group {transactionGroupId} not found");
            }

            var tradeTree = new TradeTree
            {
                TransactionGroupId = transactionGroupId,
                TradeDate = tradeGroup.Date,
                Description = GenerateTradeDescription(tradeGroup)
            };

            // Get all traded transactions in this group
            var tradedTransactions = tradeGroup.Transactions
                .Where(t => t.TransactionType == TransactionType.Traded)
                .ToList();

            // Create root nodes for each traded player
            var originalTradeTeamIds = tradedTransactions.Select(t => t.Team!.FranchiseId).Distinct().ToHashSet();
            
            foreach (var tradedTransaction in tradedTransactions)
            {
                var rootNode = new TradeTreeNode
                {
                    Transaction = tradedTransaction,
                    IsEndNode = false
                };

                // Follow this player's transaction history forward
                await BuildTreeForward(rootNode, tradedTransaction.PlayerId, tradedTransaction.Date, tradedTransaction.TransactionId.ToString(), null, originalTradeTeamIds);
                tradeTree.RootNodes.Add(rootNode);
            }

            return tradeTree;
        }

        /// <summary>
        /// Recursively builds the forward transaction tree for a player
        /// </summary>
        private async Task BuildTreeForward(TradeTreeNode currentNode, int playerId, DateTime fromDate, string? excludeTransactionId = null, HashSet<string>? visitedTransactions = null, HashSet<Guid>? originalTradeTeamIds = null)
        {
            // Initialize visited transactions set if not provided
            visitedTransactions ??= new HashSet<string>();

            // Find the next transaction for this player after the current date, excluding the current transaction
            var query = _context.Transactions
                .Include(t => t.Player)
                .Include(t => t.Team)
                    .ThenInclude(t => t!.Franchise)
                .Where(t => t.PlayerId == playerId && t.Date >= fromDate);
                
            if (excludeTransactionId != null)
            {
                query = query.Where(t => t.TransactionId.ToString() != excludeTransactionId);
            }
                
            var nextTransaction = await query
                .OrderBy(t => t.Date)
                .ThenBy(t => t.TransactionId) // Use TransactionId for consistent ordering
                .FirstOrDefaultAsync();

            if (nextTransaction == null)
            {
                // No more transactions - player is still on the team
                currentNode.IsEndNode = true;
                return;
            }

            // Check if we've already processed this transaction to prevent infinite loops
            var transactionKey = $"{nextTransaction.TransactionId}_{playerId}";
            if (visitedTransactions.Contains(transactionKey))
            {
                currentNode.IsEndNode = true;
                return;
            }

            // Add this transaction to visited set
            visitedTransactions.Add(transactionKey);

            // If the next transaction is an end transaction, mark current node as end and stop
            if (IsEndTransaction(nextTransaction.TransactionType))
            {
                currentNode.IsEndNode = true;
                return;
            }

            // If this is a trade transaction, we need to expand the trade to show all players involved
            if (nextTransaction.TransactionType == TransactionType.Traded)
            {
                await ExpandTradeTransaction(currentNode, nextTransaction, visitedTransactions, originalTradeTeamIds);
                return;
            }

            // Create child node for the next transaction
            var childNode = new TradeTreeNode
            {
                Transaction = nextTransaction,
                IsEndNode = false
            };

            currentNode.Children.Add(childNode);

            // Continue following the tree from this child node
            await BuildTreeForward(childNode, playerId, nextTransaction.Date, nextTransaction.TransactionId.ToString(), visitedTransactions, originalTradeTeamIds);
        }

        /// <summary>
        /// Expands a trade transaction to show all players involved in the trade
        /// </summary>
        private async Task ExpandTradeTransaction(TradeTreeNode currentNode, Transaction tradeTransaction, HashSet<string> visitedTransactions, HashSet<Guid>? originalTradeTeamIds)
        {
            // Get the transaction group for this trade to find all players involved
            var transactionGroup = await _context.TransactionGroups
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Player)
                .Include(tg => tg.Transactions)
                    .ThenInclude(t => t.Team)
                        .ThenInclude(t => t!.Franchise)
                .FirstOrDefaultAsync(tg => tg.TransactionGroupId == tradeTransaction.TransactionGroupId);

            if (transactionGroup == null)
            {
                // Fallback to simple child node if we can't find the transaction group
                var childNode = new TradeTreeNode
                {
                    Transaction = tradeTransaction,
                    IsEndNode = false
                };
                currentNode.Children.Add(childNode);
                await BuildTreeForward(childNode, tradeTransaction.PlayerId, tradeTransaction.Date, tradeTransaction.TransactionId.ToString(), visitedTransactions, originalTradeTeamIds);
                return;
            }

            // Get all traded transactions in this group, excluding the current player to avoid infinite loops
            var tradedTransactions = transactionGroup.Transactions
                .Where(t => t.TransactionType == TransactionType.Traded && t.PlayerId != tradeTransaction.PlayerId)
                .ToList();

            // First, create the child node for the current player's trade
            var currentPlayerChildNode = new TradeTreeNode
            {
                Transaction = tradeTransaction,
                IsEndNode = false
            };
            currentNode.Children.Add(currentPlayerChildNode);

            // Only continue following the current player's path if they go to an original trade team
            if (originalTradeTeamIds == null || originalTradeTeamIds.Contains(tradeTransaction.Team!.FranchiseId))
            {
                await BuildTreeForward(currentPlayerChildNode, tradeTransaction.PlayerId, tradeTransaction.Date, tradeTransaction.TransactionId.ToString(), visitedTransactions, originalTradeTeamIds);
            }
            else
            {
                // Mark as end node if this player goes to a team not in the original trade
                currentPlayerChildNode.IsEndNode = true;
            }

            // Create child nodes for each other traded player in this transaction group
            foreach (var tradedTx in tradedTransactions)
            {
                var childNode = new TradeTreeNode
                {
                    Transaction = tradedTx,
                    IsEndNode = false
                };
                currentNode.Children.Add(childNode);

                // Only continue following each player's path if they go to an original trade team
                if (originalTradeTeamIds == null || originalTradeTeamIds.Contains(tradedTx.Team!.FranchiseId))
                {
                    await BuildTreeForward(childNode, tradedTx.PlayerId, tradedTx.Date, tradedTx.TransactionId.ToString(), visitedTransactions, originalTradeTeamIds);
                }
                else
                {
                    // Mark as end node if this player goes to a team not in the original trade
                    childNode.IsEndNode = true;
                }
            }
        }

        /// <summary>
        /// Determines if a transaction type represents an end point in the tree
        /// </summary>
        private static bool IsEndTransaction(TransactionType transactionType)
        {
            return transactionType == TransactionType.DraftPicked || 
                   transactionType == TransactionType.Dropped ||
                   transactionType == TransactionType.Added;
        }

        /// <summary>
        /// Generates a human-readable description of a trade
        /// </summary>
        private static string GenerateTradeDescription(TransactionGroup tradeGroup)
        {
            var tradedTransactions = tradeGroup.Transactions
                .Where(t => t.TransactionType == TransactionType.Traded)
                .GroupBy(t => t.Team?.Franchise?.MainName ?? "Unknown Team")
                .ToList();

            if (tradedTransactions.Count == 2)
            {
                var team1 = tradedTransactions[0];
                var team2 = tradedTransactions[1];
                
                var team1Players = string.Join(", ", team1.Select(t => t.Player?.Name ?? "Unknown"));
                var team2Players = string.Join(", ", team2.Select(t => t.Player?.Name ?? "Unknown"));
                
                return $"{team1.Key} trades {team1Players} to {team2.Key} for {team2Players}";
            }
            else
            {
                // Multi-team trade
                var teams = string.Join(", ", tradedTransactions.Select(g => g.Key));
                var playerCount = tradedTransactions.Sum(g => g.Count());
                return $"Multi-team trade involving {teams} ({playerCount} players)";
            }
        }
    }

    /// <summary>
    /// Represents a trade group for display purposes
    /// </summary>
    public class TradeGroup
    {
        public Guid TransactionGroupId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> PlayersInvolved { get; set; } = new List<string>();
        public List<TradeSide> TradeSides { get; set; } = new List<TradeSide>();
    }

    public class TradeSide
    {
        public string FranchiseId { get; set; } = string.Empty;
        public string FranchiseName { get; set; } = string.Empty;
        public string FranchiseColor { get; set; } = string.Empty;
        public List<TradedPlayer> Players { get; set; } = new List<TradedPlayer>();
    }

    public class TradedPlayer
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string PrimaryPosition { get; set; } = string.Empty;
    }
}