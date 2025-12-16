using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyArchive.Data.JsonModels;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Services
{
    public class MermaidConfig
    {
        public bool IncludeTeamNodes { get; set; } = true;
        public bool IncludeTransactionDates { get; set; } = true;
        public bool IncludePlayerIds { get; set; } = false;
        public int? MaxDepth { get; set; } = null;
    }

    public class NodeCounter
    {
        public int Value { get; set; } = 0;
        
        public int Next() => ++Value;
    }

    public class MermaidGeneratorService
    {
        private static readonly MermaidConfig DefaultConfig = new MermaidConfig
        {
            IncludeTeamNodes = true,
            IncludeTransactionDates = true,
            IncludePlayerIds = false,
            MaxDepth = null
        };

        /// <summary>
        /// Generates a detailed Mermaid flowchart diagram from a trade tree
        /// </summary>
        public static async Task<string> GenerateDetailedMermaidDiagramAsync(TradeTreeJson tradeTree, FantasyArchiveContext context)
        {
            return await GenerateMermaidDiagramAsync(tradeTree, DefaultConfig, context);
        }

        /// <summary>
        /// Generates a simplified Mermaid diagram focusing on the main trade flow
        /// </summary>
        public static async Task<string> GenerateSimplifiedMermaidDiagramAsync(TradeTreeJson tradeTree, FantasyArchiveContext context)
        {
            var config = new MermaidConfig
            {
                IncludeTeamNodes = true,
                IncludeTransactionDates = false,
                IncludePlayerIds = false,
                MaxDepth = 3
            };
            return await GenerateMermaidDiagramAsync(tradeTree, config, context);
        }

        /// <summary>
        /// Generates a Mermaid flowchart diagram from a trade tree
        /// </summary>
        private static async Task<string> GenerateMermaidDiagramAsync(TradeTreeJson tradeTree, MermaidConfig config, FantasyArchiveContext context)
        {
            var nodes = new List<string>();
            var edges = new HashSet<string>(); // Use HashSet to automatically prevent duplicate edges
            var nodeIds = new HashSet<string>();
            var teamNodeIds = new HashSet<string>();
            var championshipNodeIds = new HashSet<string>();
            var nonRootPlayerNodeIds = new HashSet<string>(); // Track non-root team players
            var championshipNodes = new Dictionary<string, string>(); // Track championship nodes by team-year
            var tradeNodes = new Dictionary<string, string>(); // Track trade nodes by transaction group ID
            var tradeNodeIds = new HashSet<string>(); // Track all trade node IDs for styling
            var nodeCounter = new NodeCounter(); // For unique team node generation

            // Get original trade teams for scope control
            var originalTradeTeamIds = new HashSet<string>();
            foreach (var rootNode in tradeTree.RootNodes)
            {
                originalTradeTeamIds.Add(rootNode.Transaction.FranchiseId);
            }

            // Start with the main trade node
            const string mainTradeId = "TRADE_MAIN";
            var tradeDate = FormatDate(tradeTree.TradeDate);
            var tradeDateText = config.IncludeTransactionDates ? $" - {tradeDate}" : "";

            nodes.Add($"{mainTradeId}(\"🔄 Initial Trade{tradeDateText}\")");
            nodeIds.Add(mainTradeId);
            tradeNodeIds.Add(mainTradeId);

            // Group root nodes by franchise to create initial team nodes
            var franchiseGroups = tradeTree.RootNodes
                .GroupBy(n => n.Transaction.FranchiseId)
                .ToList();

            var initialTeamNodes = new Dictionary<string, string>();

            // Create initial team nodes for each franchise involved in the trade
            foreach (var franchiseGroup in franchiseGroups)
            {
                var firstTransaction = franchiseGroup.First().Transaction;
                var teamNodeId = $"TEAM_{nodeCounter.Next()}";
                var teamName = EscapeNodeLabel(firstTransaction.FranchiseName);
                
                nodes.Add($"{teamNodeId}(\"{teamName}\")");
                teamNodeIds.Add(teamNodeId);
                nodeIds.Add(teamNodeId);
                initialTeamNodes[firstTransaction.FranchiseId] = teamNodeId;
                
                // Connect trade to team
                edges.Add($"{mainTradeId} --> {teamNodeId}");
            }

            // Process each root node (player from the initial trade)
            foreach (var rootNode in tradeTree.RootNodes)
            {
                var parentTeamId = initialTeamNodes[rootNode.Transaction.FranchiseId];
                await ProcessTradeTreeNodeAsync(
                    rootNode,
                    parentTeamId,
                    nodes,
                    edges,
                    nodeIds,
                    teamNodeIds,
                    championshipNodeIds,
                    nonRootPlayerNodeIds,
                    championshipNodes,
                    tradeNodes,
                    tradeNodeIds,
                    originalTradeTeamIds,
                    config,
                    context,
                    nodeCounter,
                    0,
                    isRootConnection: true,
                    previousFranchiseId: rootNode.Transaction.FranchiseId
                );
            }

            // Build the complete diagram with improved layout
            var diagram = new StringBuilder();
            diagram.AppendLine("flowchart TD");  // Top-Down layout to reduce crossings
            diagram.AppendLine();

            foreach (var node in nodes)
            {
                diagram.AppendLine($"    {node}");
            }

            diagram.AppendLine();

            foreach (var edge in edges)
            {
                diagram.AppendLine($"    {edge}");
            }

            diagram.AppendLine();
            diagram.AppendLine("    %% Node styling");
            diagram.AppendLine("    classDef teamNode fill:#e3f2fd,stroke:#1976d2,stroke-width:2px");
            diagram.AppendLine("    classDef tradeNode fill:#fff3e0,stroke:#f57c00,stroke-width:2px");
            diagram.AppendLine("    classDef playerNode fill:#f3e5f5,stroke:#7b1fa2,stroke-width:1px");
            diagram.AppendLine("    classDef nonRootPlayerNode fill:#f5f5f5,stroke:#757575,stroke-width:1px");
            diagram.AppendLine("    classDef endNode fill:#ffebee,stroke:#c62828,stroke-width:2px");
            diagram.AppendLine("    classDef waiverNode fill:#e8f5e8,stroke:#388e3c,stroke-width:2px");
            diagram.AppendLine("    classDef championshipNode fill:#fff9c4,stroke:#f57f17,stroke-width:3px");
            diagram.AppendLine();

            if (teamNodeIds.Count > 0)
            {
                diagram.AppendLine($"    class {string.Join(",", teamNodeIds)} teamNode");
            }

            if (championshipNodeIds.Count > 0)
            {
                diagram.AppendLine($"    class {string.Join(",", championshipNodeIds)} championshipNode");
            }

            if (nonRootPlayerNodeIds.Count > 0)
            {
                diagram.AppendLine($"    class {string.Join(",", nonRootPlayerNodeIds)} nonRootPlayerNode");
            }

            if (tradeNodeIds.Count > 0)
            {
                diagram.AppendLine($"    class {string.Join(",", tradeNodeIds)} tradeNode");
            }

            return diagram.ToString();
        }

        private static async Task ProcessTradeTreeNodeAsync(
            TradeTreeNodeJson node,
            string parentNodeId,
            List<string> nodes,
            HashSet<string> edges,
            HashSet<string> nodeIds,
            HashSet<string> teamNodeIds,
            HashSet<string> championshipNodeIds,
            HashSet<string> nonRootPlayerNodeIds,
            Dictionary<string, string> championshipNodes,
            Dictionary<string, string> tradeNodes,
            HashSet<string> tradeNodeIds,
            HashSet<string> originalTradeTeamIds,
            MermaidConfig config,
            FantasyArchiveContext context,
            NodeCounter nodeCounter,
            int depth,
            bool isRootConnection = false,
            string? previousFranchiseId = null)
        {
            // Check max depth
            if (config.MaxDepth.HasValue && depth >= config.MaxDepth.Value)
            {
                return;
            }

            var transaction = node.Transaction;
            var nodeId = $"NODE_{transaction.TransactionId}";

            // Skip if we've already processed this node
            if (nodeIds.Contains(nodeId))
            {
                return;
            }

            // Determine if we need a team node or trade node
            bool needsTeamNode = false;
            bool needsTradeNode = false;
            string? teamNodeId = null;
            string? tradeNodeId = null;
            
            // Check if this is a trade transaction that needs a trade node
            // For root connections, we don't create additional trade nodes since we already have TRADE_MAIN
            if (transaction.TransactionType == "Traded" && !isRootConnection)
            {
                needsTradeNode = true;
                
                // Create unique trade node key based on date and transaction details  
                // This should align with the grouping logic in ProcessTradeGroup
                var tradeGroupKey = transaction.TransactionGroupId ?? transaction.TransactionId.ToString();
                
                // Check if we've already created a node for this trade
                if (!tradeNodes.TryGetValue(tradeGroupKey, out tradeNodeId))
                {
                    var tradeIdForNode = transaction.TransactionGroupId ?? transaction.TransactionId.ToString();
                    tradeNodeId = $"TRADE_{tradeIdForNode}_{nodeCounter.Next()}";
                    var tradeLabel = $"🔄 Trade - {FormatDate(transaction.Date)}";
                    nodes.Add($"{tradeNodeId}(\"{EscapeNodeLabel(tradeLabel)}\")");
                    nodeIds.Add(tradeNodeId);
                    tradeNodeIds.Add(tradeNodeId);
                    
                    // Track this trade node for future reuse
                    tradeNodes[tradeGroupKey] = tradeNodeId;
                }
            }
            
            if (config.IncludeTeamNodes)
            {
                // For root connections, we don't need additional team nodes since they're already created
                // For subsequent transactions, create team node if franchise changes or if we have a trade node
                if (!isRootConnection)
                {
                    needsTeamNode = (previousFranchiseId != null && previousFranchiseId != transaction.FranchiseId) || needsTradeNode;
                }
                
                // Special case: If we have a trade node but no team node was set above, create one anyway
                // This ensures trade nodes are properly connected through team nodes
                if (needsTradeNode && !needsTeamNode)
                {
                    needsTeamNode = true;
                }
                
                if (needsTeamNode)
                {
                    // Create unique team node for this transition
                    teamNodeId = $"TEAM_{transaction.FranchiseId}_{nodeCounter.Next()}";
                    nodes.Add($"{teamNodeId}[\"{EscapeNodeLabel(transaction.FranchiseName)}\"]");
                    teamNodeIds.Add(teamNodeId);
                    nodeIds.Add(teamNodeId);
                }
            }

            // Create transaction node
            var dateText = config.IncludeTransactionDates ? $" - {FormatDate(transaction.Date)}" : "";
            var playerIdText = config.IncludePlayerIds ? $" ({transaction.PlayerID})" : "";
            var transactionIcon = GetTransactionIcon(transaction.TransactionType);

            var nodeLabel = $"{transactionIcon} {transaction.PlayerName}{playerIdText}";
            if (!string.Equals(transaction.TransactionType, "Traded", StringComparison.OrdinalIgnoreCase))
            {
                nodeLabel += $" - {transaction.TransactionType}";
            }
            nodeLabel += dateText;

            // Clean the label to avoid Mermaid syntax issues
            nodeLabel = EscapeNodeLabel(nodeLabel);

            // Determine node shape based on transaction type
            var shape = GetNodeShape(transaction.TransactionType, node.IsEndNode);
            nodes.Add($"{nodeId}{shape.Replace("CONTENT", nodeLabel)}");
            nodeIds.Add(nodeId);

            // Track if this player goes to a non-root team
            if (!originalTradeTeamIds.Contains(transaction.FranchiseId))
            {
                nonRootPlayerNodeIds.Add(nodeId);
            }

            // Check for championship and create championship node if applicable
            string? championshipNodeId = null;
            if (await IsChampionshipYear(context, transaction.FranchiseId, transaction.Date.Year))
            {
                var championshipKey = $"{transaction.FranchiseId}_{transaction.Date.Year}";
                if (!championshipNodes.TryGetValue(championshipKey, out championshipNodeId))
                {
                    championshipNodeId = $"CHAMP_{championshipKey}";
                    var championshipLabel = $"🏆 {transaction.FranchiseName} - {transaction.Date.Year} Champions!";
                    nodes.Add($"{championshipNodeId}[\"{EscapeNodeLabel(championshipLabel)}\"]");
                    championshipNodeIds.Add(championshipNodeId);
                    nodeIds.Add(championshipNodeId);
                    championshipNodes[championshipKey] = championshipNodeId;
                }
            }

            // Connect nodes based on whether we have trade and/or team transitions
            if (needsTradeNode && tradeNodeId != null)
            {
                // Parent -> Trade Node -> Team -> Transaction (or Parent -> Trade Node -> Transaction if no team)
                edges.Add($"{parentNodeId} --> {tradeNodeId}");
                
                if (needsTeamNode && teamNodeId != null)
                {
                    edges.Add($"{tradeNodeId} --> {teamNodeId}");
                    edges.Add($"{teamNodeId} --> {nodeId}");
                }
                else
                {
                    edges.Add($"{tradeNodeId} --> {nodeId}");
                }
                
                // Connect to championship if applicable
                if (championshipNodeId != null)
                {
                    edges.Add($"{nodeId} --> {championshipNodeId}");
                }
            }
            else if (needsTeamNode && teamNodeId != null)
            {
                // Parent -> Team -> Transaction
                edges.Add($"{parentNodeId} --> {teamNodeId}");
                edges.Add($"{teamNodeId} --> {nodeId}");
                
                // Connect to championship if applicable
                if (championshipNodeId != null)
                {
                    edges.Add($"{nodeId} --> {championshipNodeId}");
                }
            }
            else
            {
                // Direct connection from parent to transaction
                edges.Add($"{parentNodeId} --> {nodeId}");
                
                // Connect to championship if applicable
                if (championshipNodeId != null)
                {
                    edges.Add($"{nodeId} --> {championshipNodeId}");
                }
            }

            // Process children recursively
            if (node.Children != null && node.Children.Count > 0)
            {
                // Group all trade children by date to ensure proper trade → team → player structure
                var tradeGroups = node.Children
                    .Where(child => child.Transaction.TransactionType == "Traded")
                    .GroupBy(child => child.Transaction.Date)
                    .Where(group => group.Count() >= 1)
                    .ToList();

                var processedChildrenIds = new HashSet<string>();

            // Process trade groups first
            foreach (var tradeGroup in tradeGroups)
            {
                var tradeChildren = tradeGroup.ToList();
                
                await ProcessTradeGroup(tradeChildren, nodeId, nodes, edges, nodeIds, teamNodeIds, championshipNodeIds, nonRootPlayerNodeIds, championshipNodes, tradeNodes, tradeNodeIds, originalTradeTeamIds, config, context, nodeCounter, depth);
                
                // Mark these children as processed
                foreach (var child in tradeChildren)
                {
                    processedChildrenIds.Add(child.Transaction.TransactionId);
                }
            }                // Process remaining children individually
                foreach (var childNode in node.Children)
                {
                    if (!processedChildrenIds.Contains(childNode.Transaction.TransactionId))
                    {
                        await ProcessTradeTreeNodeAsync(
                            childNode,
                            nodeId,
                            nodes,
                            edges,
                            nodeIds,
                            teamNodeIds,
                            championshipNodeIds,
                            nonRootPlayerNodeIds,
                            championshipNodes,
                            tradeNodes,
                            tradeNodeIds,
                            originalTradeTeamIds,
                            config,
                            context,
                            nodeCounter,
                            depth + 1,
                            isRootConnection: false,
                            previousFranchiseId: transaction.FranchiseId
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Processes a group of trade children that belong to the same trade transaction
        /// </summary>
        private static async Task ProcessTradeGroup(
            List<TradeTreeNodeJson> tradeChildren,
            string parentNodeId,
            List<string> nodes,
            HashSet<string> edges,
            HashSet<string> nodeIds,
            HashSet<string> teamNodeIds,
            HashSet<string> championshipNodeIds,
            HashSet<string> nonRootPlayerNodeIds,
            Dictionary<string, string> championshipNodes,
            Dictionary<string, string> tradeNodes,
            HashSet<string> tradeNodeIds,
            HashSet<string> originalTradeTeamIds,
            MermaidConfig config,
            FantasyArchiveContext context,
            NodeCounter nodeCounter,
            int depth)
        {
            if (tradeChildren.Count == 0) return;

            var firstTrade = tradeChildren.First().Transaction;
            
            // Create a unique key for this trade group based on date and participating teams
            var participatingTeams = tradeChildren
                .Select(child => child.Transaction.FranchiseId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            var tradeGroupKey = $"{firstTrade.Date:yyyy-MM-dd}_{string.Join("_", participatingTeams)}";
            
            // Check if we've already created a node for this trade
            if (tradeNodes.TryGetValue(tradeGroupKey, out var existingTradeNodeId))
            {
                // Trade already exists, just connect parent to existing trade node
                edges.Add($"{parentNodeId} --> {existingTradeNodeId}");
                return;
            }
            
            // Create a new trade node for this transaction group
            var tradeIdForNode = firstTrade.TransactionGroupId ?? firstTrade.TransactionId.ToString();
            var tradeNodeId = $"TRADE_{tradeIdForNode}_{nodeCounter.Next()}";
            var tradeLabel = $"🔄 Trade - {FormatDate(firstTrade.Date)}";
            nodes.Add($"{tradeNodeId}(\"{EscapeNodeLabel(tradeLabel)}\")");
            nodeIds.Add(tradeNodeId);
            tradeNodeIds.Add(tradeNodeId);
            
            // Track this trade node for future reuse
            tradeNodes[tradeGroupKey] = tradeNodeId;
            
            // Connect parent to trade node
            edges.Add($"{parentNodeId} --> {tradeNodeId}");
            
            // Group players by team to create team nodes
            var playersByTeam = tradeChildren
                .GroupBy(child => new { child.Transaction.FranchiseId, child.Transaction.FranchiseName })
                .ToList();

            foreach (var teamGroup in playersByTeam)
            {
                // Create team node for this team
                var teamNodeId = $"TEAM_{teamGroup.Key.FranchiseId}_{nodeCounter.Next()}";
                nodes.Add($"{teamNodeId}[\"{EscapeNodeLabel(teamGroup.Key.FranchiseName)}\"]");
                teamNodeIds.Add(teamNodeId);
                nodeIds.Add(teamNodeId);
                
                // Connect trade to team
                edges.Add($"{tradeNodeId} --> {teamNodeId}");
                
                // Process each player in this team
                foreach (var childNode in teamGroup)
                {
                    var transaction = childNode.Transaction;
                    var playerNodeId = $"NODE_{transaction.TransactionId}";
                    
                    // Skip if already processed
                    if (nodeIds.Contains(playerNodeId))
                    {
                        continue;
                    }
                    
                    // Create player node
                    var dateText = config.IncludeTransactionDates ? $" - {FormatDate(transaction.Date)}" : "";
                    var playerIdText = config.IncludePlayerIds ? $" ({transaction.PlayerID})" : "";
                    var transactionIcon = GetTransactionIcon(transaction.TransactionType);

                    var nodeLabel = $"{transactionIcon} {transaction.PlayerName}{playerIdText}{dateText}";
                    nodeLabel = EscapeNodeLabel(nodeLabel);

                    var shape = GetNodeShape(transaction.TransactionType, childNode.IsEndNode);
                    nodes.Add($"{playerNodeId}{shape.Replace("CONTENT", nodeLabel)}");
                    nodeIds.Add(playerNodeId);

                    // Track if this player goes to a non-root team
                    if (!originalTradeTeamIds.Contains(transaction.FranchiseId))
                    {
                        nonRootPlayerNodeIds.Add(playerNodeId);
                    }
                    
                    // Connect team to player
                    edges.Add($"{teamNodeId} --> {playerNodeId}");
                    
                    // Check for championship and create championship node if applicable
                    if (await IsChampionshipYear(context, transaction.FranchiseId, transaction.Date.Year))
                    {
                        var championshipKey = $"{transaction.FranchiseId}_{transaction.Date.Year}";
                        if (!championshipNodes.TryGetValue(championshipKey, out var championshipNodeId))
                        {
                            championshipNodeId = $"CHAMP_{championshipKey}";
                            var championshipLabel = $"🏆 {transaction.FranchiseName} - {transaction.Date.Year} Champions!";
                            nodes.Add($"{championshipNodeId}[\"{EscapeNodeLabel(championshipLabel)}\"]");
                            championshipNodeIds.Add(championshipNodeId);
                            nodeIds.Add(championshipNodeId);
                            championshipNodes[championshipKey] = championshipNodeId;
                        }
                        edges.Add($"{playerNodeId} --> {championshipNodeId}");
                    }

                    // Process children recursively
                    if (childNode.Children != null && childNode.Children.Count > 0)
                    {
                        foreach (var grandchild in childNode.Children)
                        {
                            await ProcessTradeTreeNodeAsync(
                                grandchild,
                                playerNodeId,
                                nodes,
                                edges,
                                nodeIds,
                                teamNodeIds,
                                championshipNodeIds,
                                nonRootPlayerNodeIds,
                                championshipNodes,
                                tradeNodes,
                                tradeNodeIds,
                                originalTradeTeamIds,
                                config,
                                context,
                                nodeCounter,
                                depth + 1,
                                isRootConnection: false,
                                previousFranchiseId: transaction.FranchiseId
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a franchise won a championship in the given year
        /// </summary>
        private static async Task<bool> IsChampionshipYear(FantasyArchiveContext context, string franchiseId, int year)
        {
            if (!Guid.TryParse(franchiseId, out var parsedFranchiseId))
            {
                return false;
            }

            var championship = await context.Teams
                .Where(t => t.FranchiseId == parsedFranchiseId && t.Year == year && t.Champion)
                .FirstOrDefaultAsync();

            return championship != null;
        }

        private static string GetTransactionIcon(string transactionType)
        {
            return transactionType.ToLower() switch
            {
                "traded" => "🔄",
                "draftpicked" => "📝",
                "dropped" => "❌",
                "waiver" or "added" => "➕",
                "freeagent" => "🆓",
                "kept" => "🔒",
                _ => "🔹"
            };
        }

        private static string GetNodeShape(string transactionType, bool isEndNode)
        {
            if (isEndNode)
            {
                return "(\"CONTENT\")";
            }

            return transactionType.ToLower() switch
            {
                "traded" => "(\"CONTENT\")",
                "kept" => "(\"CONTENT\")",
                "waiver" or "added" => "(\"CONTENT\")",
                _ => "(\"CONTENT\")"
            };
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("MMM d, yyyy");
        }

        private static string EscapeNodeLabel(string label)
        {
            // Clean the label to avoid Mermaid syntax issues
            return label.Replace("\"", "'").Replace("\n", " ").Replace("\r", "");
        }
    }
}