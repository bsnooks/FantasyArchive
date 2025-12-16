using System.Collections.Generic;
using System.Xml.Serialization;

namespace FantasyArchive.Api.Models.Yahoo
{
    [XmlRoot(ElementName = "transaction_data")]
    public class TransactionData
    {
        [XmlElement(ElementName = "type", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Type { get; set; }
        [XmlElement(ElementName = "source_type", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string SourceType { get; set; }
        [XmlElement(ElementName = "destination_team_key", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string DestinationTeamKey { get; set; }
        [XmlElement(ElementName = "destination_team_name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string DestinationTeamName { get; set; }
        [XmlElement(ElementName = "source_team_key", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string SourceTeamKey { get; set; }
        [XmlElement(ElementName = "source_team_name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string SourceTeamName { get; set; }
    }

    [XmlRoot(ElementName = "transaction")]
    public class YahooTransaction
    {
        [XmlElement(ElementName = "transaction_key", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TransactionKey { get; set; }
        [XmlElement(ElementName = "transaction_id", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TransactionId { get; set; }
        [XmlElement(ElementName = "type", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Type { get; set; }
        [XmlElement(ElementName = "status", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Status { get; set; }
        [XmlElement(ElementName = "timestamp", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public int Timestamp { get; set; }
        [XmlElement(ElementName = "faab_bid", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public int FaabBid { get; set; }
        [XmlElement(ElementName = "trader_team_key", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TraderTeamKey { get; set; }
        [XmlElement(ElementName = "trader_team_name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TraderTeamName { get; set; }
        [XmlElement(ElementName = "tradee_team_key", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TradeeTeamKey { get; set; }
        [XmlElement(ElementName = "tradee_team_name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string TradeeTeamName { get; set; }
        [XmlElement(ElementName = "players", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public YahooPlayerList PlayerList { get; set; }
    }

    [XmlRoot(ElementName = "transaction")]
    public class YahooTransactions
    {
        [XmlElement(ElementName = "transaction", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public List<YahooTransaction> Transactions { get; set; }
        [XmlAttribute(AttributeName = "count")]
        public string Count { get; set; }
    }
}
