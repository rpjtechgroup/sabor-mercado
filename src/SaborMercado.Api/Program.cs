using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SaborMercado.Modules.Identity;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Modules.Recognition;
using SaborMercado.Modules.Rewards;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.SharedCatalog;
using SaborMercado.Modules.SharedCatalog.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRecognitionModule(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddSharedCatalogModule(builder.Configuration);
builder.Services.AddRewardsModule(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>("identity-db", tags: ["ready"])
    .AddDbContextCheck<SharedCatalogDbContext>("shared-catalog-db", tags: ["ready"])
    .AddDbContextCheck<RewardsDbContext>("rewards-db", tags: ["ready"]);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5052"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    },
});
app.MapRecognitionModule();
app.MapIdentityModule();
app.MapSharedCatalogModule();
app.MapRewardsModule();

await app.InitializeRecognitionModuleAsync();
await app.InitializeIdentityModuleAsync();
await app.InitializeSharedCatalogModuleAsync();
await app.InitializeRewardsModuleAsync();

app.Run();

public partial class Program;
