using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using SaborMercado.Modules.Recognition.Data;
using SaborMercado.Modules.Recognition.Endpoints;
using SaborMercado.Modules.Recognition.Services;

namespace SaborMercado.Modules.Recognition;

public static class RecognitionModule
{
    public static IServiceCollection AddRecognitionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RecognitionOptions>(configuration.GetSection(RecognitionOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IRecognitionQuotaStore, InMemoryRecognitionQuotaStore>();

        var connectionString = configuration.GetConnectionString("Recognition")
            ?? "Data Source=recognition.db";

        services.AddDbContextPool<RecognitionDbContext>(options =>
            options.UseSqlite(connectionString));

        services
            .AddHttpClient<IGeminiVisionClient, GeminiVisionClient>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddScoped<ShelfLabelRecognitionService>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("recognition", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
            options.AddPolicy("feedback", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0,
                    }));
        });

        return services;
    }

    public static async Task InitializeRecognitionModuleAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RecognitionDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public static IEndpointRouteBuilder MapRecognitionModule(this IEndpointRouteBuilder app) =>
        app.MapRecognitionEndpoints();

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(500));
}
