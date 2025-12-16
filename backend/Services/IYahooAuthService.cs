using System.Threading.Tasks;
using FantasyArchive.Api.Models;

namespace FantasyArchive.Api.Services
{
    public interface IYahooAuthService
    {
        Task<YahooAuthResult> AuthenticateCode(string code, string redirectUri);
        string GetAuthorizationUrl(string redirectUri, string state = "");
    }
}