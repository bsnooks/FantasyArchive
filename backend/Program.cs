using System;
using System.Linq;
using FantasyArchive.Api.Services;
using FantasyArchive.Data;
using FantasyArchive.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add Entity Framework
builder.Services.AddDbContext<FantasyArchiveContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure())
    .EnableSensitiveDataLogging(false)
    .EnableDetailedErrors(false)
    .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.SensitiveDataLoggingEnabledWarning)));

// Add HTTP client for Yahoo API calls
builder.Services.AddHttpClient<IYahooAuthService, YahooAuthService>();

// Register Yahoo sync services
builder.Services.AddScoped<IYahooAuthService, YahooAuthService>();
builder.Services.AddScoped<IYahooSyncService, YahooSyncService>();

var services = builder.Services;
services.AddScoped<LeagueRepository>();
services.AddScoped<FranchiseRepository>();
services.AddScoped<DraftRepository>();
services.AddScoped<DraftPickRepository>();
services.AddScoped<PlayerRepository>();
services.AddScoped<TransactionRepository>();
services.AddScoped<MatchRepository>();
services.AddScoped<TeamRepository>();
services.AddScoped<RecordRepository>();
services.AddScoped<FranchiseTradeRepository>();
services.AddScoped<SeasonRepository>();
services.AddScoped<AnalysisRepository>();

var optionsBuilder = new DbContextOptionsBuilder<FantasyArchiveContext>();
optionsBuilder.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddSingleton(s => new Func<FantasyArchiveContext>(() => new FantasyArchiveContext(optionsBuilder.Options)));

// Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();

// Enable serving static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Add middleware to extract token from authorization header
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
    if (authHeader != null && authHeader.StartsWith("Bearer "))
    {
        var token = authHeader.Substring("Bearer ".Length).Trim();
        context.Items["Token"] = token;
    }
    await next();
});

app.MapControllers();

app.Run();
