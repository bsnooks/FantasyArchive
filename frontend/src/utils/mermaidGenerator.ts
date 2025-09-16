import { TradeTree, TradeTreeNode } from '../types/fantasy';

export interface MermaidConfig {
  includeTeamNodes: boolean;
  includeTransactionDates: boolean;
  includePlayerIds: boolean;
  maxDepth?: number;
}

export const defaultMermaidConfig: MermaidConfig = {
  includeTeamNodes: true,
  includeTransactionDates: true,
  includePlayerIds: false,
  maxDepth: undefined
};

/**
 * Generates a Mermaid flowchart diagram from a trade tree
 */
export function generateMermaidDiagram(
  tradeTree: TradeTree, 
  config: MermaidConfig = defaultMermaidConfig
): string {
  const nodes: string[] = [];
  const edges: string[] = [];
  const nodeIds = new Set<string>();
  const teamNodeIds = new Set<string>();
  
  // Start with the main trade node
  const mainTradeId = 'TRADE_MAIN';
  const tradeDate = formatDate(tradeTree.TradeDate);
  const tradeDateText = config.includeTransactionDates ? ` - ${tradeDate}` : '';
  
  nodes.push(`${mainTradeId}("🔄 Initial Trade${tradeDateText}")`);
  nodeIds.add(mainTradeId);

  // Process each root node (player from the initial trade)
  tradeTree.RootNodes.forEach((rootNode, index) => {
    processTradeTreeNode(
      rootNode, 
      mainTradeId, 
      nodes, 
      edges, 
      nodeIds, 
      teamNodeIds, 
      config,
      0
    );
  });

  // Build the complete diagram with improved layout
  const mermaidDiagram = [
    'flowchart TD',  // Top-Down layout to reduce crossings
    '',
    ...nodes.map((node: string) => `    ${node}`),
    '',
    ...edges.map((edge: string) => `    ${edge}`),
    '',
    '    %% Node styling',
    '    classDef teamNode fill:#e3f2fd,stroke:#1976d2,stroke-width:2px',
    '    classDef tradeNode fill:#fff3e0,stroke:#f57c00,stroke-width:2px',
    '    classDef playerNode fill:#f3e5f5,stroke:#7b1fa2,stroke-width:1px',
    '    classDef endNode fill:#ffebee,stroke:#c62828,stroke-width:2px',
    '    classDef waiverNode fill:#e8f5e8,stroke:#388e3c,stroke-width:2px',
    '',
    `    class ${Array.from(teamNodeIds).join(',')} teamNode`,
    `    class ${mainTradeId} tradeNode`
  ].join('\n');

  return mermaidDiagram;
}

function processTradeTreeNode(
  node: TradeTreeNode,
  parentNodeId: string,
  nodes: string[],
  edges: string[],
  nodeIds: Set<string>,
  teamNodeIds: Set<string>,
  config: MermaidConfig,
  depth: number
): void {
  // Check max depth
  if (config.maxDepth && depth >= config.maxDepth) {
    return;
  }

  const transaction = node.Transaction;
  const nodeId = `NODE_${transaction.TransactionId}`;
  
  // Skip if we've already processed this node
  if (nodeIds.has(nodeId)) {
    return;
  }

  // Create team node if needed and not already created
  let teamNodeId: string | null = null;
  if (config.includeTeamNodes) {
    teamNodeId = `TEAM_${transaction.FranchiseId}_${transaction.Year}`;
    if (!teamNodeIds.has(teamNodeId)) {
      nodes.push(`${teamNodeId}["${transaction.FranchiseName}"]`);
      teamNodeIds.add(teamNodeId);
      nodeIds.add(teamNodeId);
    }
  }

  // Create transaction node
  const dateText = config.includeTransactionDates ? ` - ${formatDate(transaction.Date)}` : '';
  const playerIdText = config.includePlayerIds ? ` (${transaction.PlayerId})` : '';
  const transactionText = getTransactionIcon(transaction.TransactionType);
  
  let nodeLabel = `${transactionText} ${transaction.PlayerName}${playerIdText}`;
  if (transaction.TransactionType.toLowerCase() !== 'traded') {
    nodeLabel += ` - ${transaction.TransactionType}`;
  }
  nodeLabel += dateText;

  // Clean the label to avoid Mermaid syntax issues
  nodeLabel = nodeLabel.replace(/"/g, "'").replace(/\n/g, ' ');

  // Determine node shape and class based on transaction type
  const { shape } = getNodeShapeAndClass(transaction.TransactionType, node.IsEndNode);
  nodes.push(`${nodeId}${shape.replace('CONTENT', nodeLabel)}`);
  nodeIds.add(nodeId);

  // Connect from parent to team (if team nodes enabled)
  if (config.includeTeamNodes && teamNodeId) {
    edges.push(`${parentNodeId} --> ${teamNodeId}`);
    edges.push(`${teamNodeId} --> ${nodeId}`);
  } else {
    // Direct connection from parent to transaction
    edges.push(`${parentNodeId} --> ${nodeId}`);
  }

  // Process children recursively
  if (node.Children && node.Children.length > 0) {
    node.Children.forEach(childNode => {
      processTradeTreeNode(
        childNode,
        nodeId,
        nodes,
        edges,
        nodeIds,
        teamNodeIds,
        config,
        depth + 1
      );
    });
  }
}

function getTransactionIcon(transactionType: string): string {
  switch (transactionType.toLowerCase()) {
    case 'traded':
      return '🔄';
    case 'draftpicked':
      return '📝';
    case 'dropped':
      return '❌';
    case 'waiver':
    case 'added':
      return '➕';
    case 'freeagent':
      return '🆓';
    case 'kept':
      return '🔒';
    default:
      return '🔹';
  }
}

function getNodeShapeAndClass(transactionType: string, isEndNode: boolean): { shape: string; cssClass: string } {
  const type = transactionType.toLowerCase();
  
  if (isEndNode) {
    return { shape: '("CONTENT")', cssClass: 'endNode' };
  }

  switch (type) {
    case 'traded':
      return { shape: '("CONTENT")', cssClass: 'tradeNode' };
    case 'kept':
      return { shape: '("CONTENT")', cssClass: 'keepNode' };
    case 'waiver':
    case 'added':
      return { shape: '("CONTENT")', cssClass: 'waiverNode' };
    default:
      return { shape: '("CONTENT")', cssClass: 'keepNode' };
  }
}

function formatDate(dateString: string): string {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  } catch {
    return dateString;
  }
}

/**
 * Generates a simplified Mermaid diagram focusing on the main trade flow
 */
export function generateSimplifiedMermaidDiagram(tradeTree: TradeTree): string {
  return generateMermaidDiagram(tradeTree, {
    includeTeamNodes: true,
    includeTransactionDates: false,
    includePlayerIds: false,
    maxDepth: 3
  });
}

/**
 * Generates a detailed Mermaid diagram with all information
 */
export function generateDetailedMermaidDiagram(tradeTree: TradeTree): string {
  return generateMermaidDiagram(tradeTree, {
    includeTeamNodes: true,
    includeTransactionDates: true,
    includePlayerIds: true,
    maxDepth: undefined
  });
}