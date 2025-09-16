export interface Franchise {
  Id: string; // GUID
  Name: string;
  Owner: string; // Current owner (from latest season)
  Owners: string[]; // All historical owners
  EstablishedDate?: string;
  IsActive: boolean;
  Color: string;
  Teams: TeamSummary[];
  AllTimeRoster?: AllTimeRoster; // Will be calculated by exporter
}

export interface AllTimeRoster {
  Quarterbacks: AllTimePlayer[];
  RunningBacks: AllTimePlayer[];
  WideReceivers: AllTimePlayer[];
  TightEnds: AllTimePlayer[];
}

export interface AllTimePlayer {
  PlayerId: number;
  PlayerName: string;
  Position: string;
  TotalPoints: number;
  WeeksStarted: number;
  AveragePoints: number;
  SeasonsWithFranchise: number[];
  IsBench?: boolean;
}

export interface AllTimeRoster {
  Quarterbacks: AllTimePlayer[];
  RunningBacks: AllTimePlayer[];
  WideReceivers: AllTimePlayer[];
  TightEnds: AllTimePlayer[];
}

export interface TeamSummary {
  Id: string; // GUID
  Year: number;
  TeamName: string;
  Wins: number;
  Losses: number;
  Ties: number;
  Points: number;
  Standing: number;
  Champion: boolean;
  SecondPlace: boolean;
}

export interface Season {
  Year: number;
  Name: string;
  IsActive: boolean;
  Finished?: boolean;
  CurrentWeek: number;
  Teams: TeamDetail[];
}

export interface TeamDetail {
  Id: string; // GUID
  FranchiseId: string; // GUID
  FranchiseName: string;
  Owner: string;
  TeamName: string;
  Wins: number;
  Losses: number;
  Ties: number;
  Points: number;
  Standing: number;
  Champion: boolean;
  SecondPlace: boolean;
  Color: string;
}

// Draft related types
export interface Draft {
  Year: number;
  DraftDate: string;
  Picks: DraftPick[];
}

export interface DraftPick {
  PickNumber: number;
  Round: number;
  PickInRound: number;
  FranchiseId: string;
  FranchiseName: string;
  Owner: string;
  PlayerId: string;
  PlayerName: string;
  Position: string;
  Team: string; // NFL team
}

// Trade related types
export interface Trade {
  TradeId: string;
  TradeDate: string;
  Season: number;
  Week?: number;
  Teams: TradeTeam[];
}

export interface TradeTeam {
  FranchiseId: string;
  FranchiseName: string;
  Owner: string;
  PlayersReceived: TradePlayer[];
  PlayersGiven: TradePlayer[];
  PicksReceived: TradePick[];
  PicksGiven: TradePick[];
}

export interface TradePlayer {
  PlayerId: string;
  PlayerName: string;
  Position: string;
  Team: string; // NFL team
}

export interface TradePick {
  Year: number;
  Round: number;
  OriginalOwner: string;
}

// Record Book types
export interface RecordBook {
  LeagueRecords: Record[];
  SeasonRecords: Record[];
}

export interface Record {
  RecordId: string;
  Category: string;
  Description: string;
  Value: string;
  HolderName: string;
  FranchiseName?: string;
  Season?: number;
  Week?: number;
  Date?: string;
}

// Player Profile types
export interface PlayerProfile {
  PlayerId: string;
  PlayerName: string;
  Position: string;
  NFLTeams: string[];
  DraftInfo?: {
    Year: number;
    Round: number;
    Pick: number;
    DraftedBy: string;
  };
  TransactionHistory: PlayerTransaction[];
  FantasyStats: PlayerFantasyStats[];
}

export interface PlayerTransaction {
  TransactionId: string;
  Date: string;
  Type: 'Draft' | 'Trade' | 'Waiver' | 'Free Agent' | 'Drop';
  FromTeam?: string;
  ToTeam: string;
  Season: number;
  Week?: number;
}

export interface PlayerFantasyStats {
  Season: number;
  FranchiseId: string;
  FranchiseName: string;
  GamesStarted: number;
  TotalPoints: number;
  AveragePoints: number;
  HighestWeek: number;
  LowestWeek: number;
}

// Trade Tree types
export interface TradeTreeIndex {
  Year: number;
  Trades: TradeTreeSummary[];
}

export interface TradeTreeSummary {
  TransactionGroupId: string;
  Date: string;
  Description: string;
  PlayersInvolved: string[];
  TradeSides: TradeSide[];
}

export interface TradeSide {
  FranchiseId: string;
  FranchiseName: string;
  FranchiseColor: string;
  Players: TradedPlayer[];
}

export interface TradedPlayer {
  PlayerId: number;
  PlayerName: string;
  PrimaryPosition: string;
}

export interface TradeTree {
  TransactionGroupId: string;
  TradeDate: string;
  Description: string;
  RootNodes: TradeTreeNode[];
}

export interface TradeTreeNode {
  Transaction: TradeTransaction;
  Children: TradeTreeNode[];
  IsEndNode: boolean;
}

export interface TradeTransaction {
  TransactionId: string;
  TransactionGroupId: string;
  TeamId: string;
  TeamName: string;
  FranchiseId: string;
  FranchiseName: string;
  TransactionType: string;
  PlayerId: number;
  PlayerName: string;
  Date: string;
  Description: string;
  Year: number;
  PlayerTransactionIndex: number;
}