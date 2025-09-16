namespace FantasyArchive.Data.JsonModels
{
    public class TransactionJson
    {
        public string TransactionId { get; set; } = string.Empty;
        public string? TransactionGroupId { get; set; }
        public string TeamId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string FranchiseId { get; set; } = string.Empty;
        public string FranchiseName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int Year { get; set; }
        public int PlayerTransactionIndex { get; set; }
    }

    public class TradeTreeNodeJson
    {
        public TransactionJson Transaction { get; set; } = null!;
        public List<TradeTreeNodeJson> Children { get; set; } = new List<TradeTreeNodeJson>();
        public bool IsEndNode { get; set; }
    }

    public class TradeTreeJson
    {
        public string TransactionGroupId { get; set; } = string.Empty;
        public DateTime TradeDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<TradeTreeNodeJson> RootNodes { get; set; } = new List<TradeTreeNodeJson>();
    }

    public class TradeGroupJson
    {
        public string TransactionGroupId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> PlayersInvolved { get; set; } = new List<string>();
        public List<TradeSideJson> TradeSides { get; set; } = new List<TradeSideJson>();
    }

    public class TradeSideJson
    {
        public string FranchiseId { get; set; } = string.Empty;
        public string FranchiseName { get; set; } = string.Empty;
        public string FranchiseColor { get; set; } = string.Empty;
        public List<TradedPlayerJson> Players { get; set; } = new List<TradedPlayerJson>();
    }

    public class TradedPlayerJson
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string PrimaryPosition { get; set; } = string.Empty;
    }

    public class TradesByYearJson
    {
        public int Year { get; set; }
        public List<TradeGroupJson> Trades { get; set; } = new List<TradeGroupJson>();
    }
}