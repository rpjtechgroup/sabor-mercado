using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Api.Tests.Infrastructure;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Api.Tests;

public class AuthRefreshTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthRefreshTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithIsolatedSqlite("auth", builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            }));

    [Fact]
    public async Task Refresh_ReturnsNewAccessToken()
    {
        var client = _factory.CreateClient();
        var email = $"refresh-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"));
        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        var refresh = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshRequest(auth.RefreshToken));
        refresh.EnsureSuccessStatusCode();

        var renewed = await refresh.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(renewed);
        Assert.NotEqual(auth.RefreshToken, renewed.RefreshToken);
    }

    [Fact]
    public async Task DeleteAccount_RemovesAccess()
    {
        var client = _factory.CreateClient();
        var email = $"delete-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"));
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var delete = await client.DeleteAsync("/api/v1/auth/me");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, delete.StatusCode);

        var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
