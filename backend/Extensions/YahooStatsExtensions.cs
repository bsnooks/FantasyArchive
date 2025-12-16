using System.Linq;
using FantasyArchive.Api.Models;
using FantasyArchive.Api.Models.Yahoo;

namespace FantasyArchive.Api.Extensions
{
    public static class YahooStatsExtensions
    {
        public static int GetStatValue(this YahooStats stats, YahooStatType statType)
        {
            var stat = stats.Stat.FirstOrDefault(s => s.StatId == (int)statType);
            if (stat != null)
            {
                return stat.Value;
            }

            return 0;
        }

        public static double CalculatePoints(this YahooStats yahooStats)
        {
            var passingYards = yahooStats.GetStatValue(YahooStatType.PassingYards);
            var passingTouchdowns = yahooStats.GetStatValue(YahooStatType.PassingTouchdowns);
            var rushingYards = yahooStats.GetStatValue(YahooStatType.RushingYards);
            var rushingTouchdowns = yahooStats.GetStatValue(YahooStatType.RushingTouchdowns);
            var receivingYards = yahooStats.GetStatValue(YahooStatType.ReceivingYards);
            var receivingTouchdowns = yahooStats.GetStatValue(YahooStatType.ReceivingTouchdowns);

            return passingYards / 25.0 + passingTouchdowns * 4 + rushingYards / 10.0 + rushingTouchdowns * 6 + receivingYards / 10.0 + receivingTouchdowns * 6;
        }
    }
}