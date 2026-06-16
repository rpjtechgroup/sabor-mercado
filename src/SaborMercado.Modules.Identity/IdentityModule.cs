using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SaborMercado.Infrastructure.EntityFramework;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Modules.Identity.Endpoints;
using SaborMercado.Modules.Identity.Services;

namespace SaborMercado.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<GoogleAuthService>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();

        services.AddDbContextPool<IdentityDbContext>(options =>
            DatabaseBootstrap.ConfigureDbContext<IdentityDbContext>(
                options,
                configuration,
                "Identity",
                postgresSchema: "identity"));

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static Task InitializeIdentityModuleAsync(this WebApplication app) =>
        DatabaseBootstrap.InitializeModuleAsync<IdentityDbContext>(app);

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder app) =>
        app.MapAuthEndpoints();
}
