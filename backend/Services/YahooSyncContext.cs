using FantasyArchive.Data.Models;
using Microsoft.AspNetCore.Http;

namespace FantasyArchive.Api.Services
{
    public interface ISyncContext
    {
        string Token { get; }
        string LeagueKey { get; }
        League League { get; }
    }

    public class YahooSyncContext : ISyncContext
    {
        public string Token { get; private set; }
        public string LeagueKey { get; private set; }
        public League League { get; private set; }

        private YahooSyncContext(HttpContext httpContext, string leagueKey, League league)
        {
            if (httpContext?.Items.ContainsKey("Token") == true)
            {
                Token = httpContext.Items["Token"]?.ToString() ?? string.Empty;
            }
            else
            {
                Token = string.Empty;
            }

            LeagueKey = leagueKey;
            League = league;
        }

        public static ISyncContext Create(HttpContext httpContext, string leagueKey, League league) 
            => new YahooSyncContext(httpContext, leagueKey, league);
    }
}