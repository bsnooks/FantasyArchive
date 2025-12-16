using Microsoft.AspNetCore.Mvc;
using FantasyArchive.Api.Services;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace FantasyArchive.Api.Controllers;

[ApiController]
[Route("/oauth")]
public class OAuthCallbackController : ControllerBase
{
    private readonly ILogger<OAuthCallbackController> _logger;
    private readonly IYahooAuthService _yahooAuthService;

    public OAuthCallbackController(ILogger<OAuthCallbackController> logger, IYahooAuthService yahooAuthService)
    {
        _logger = logger;
        _yahooAuthService = yahooAuthService;
    }

    [HttpGet("callback")]
    public IActionResult HandleCallback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? state)
    {
        _logger.LogInformation("OAuth callback received - Code: {HasCode}, Error: {Error}", 
            !string.IsNullOrEmpty(code), error);

        // Redirect to the main UI with the parameters
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(code))
            queryParams.Add($"code={Uri.EscapeDataString(code)}");
        
        if (!string.IsNullOrEmpty(error))
            queryParams.Add($"error={Uri.EscapeDataString(error)}");
            
        if (!string.IsNullOrEmpty(state))
            queryParams.Add($"state={Uri.EscapeDataString(state)}");

        var redirectUrl = queryParams.Count > 0 ? $"/?{string.Join("&", queryParams)}" : "/";
        
        return Redirect(redirectUrl);
    }
}