using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Api.Tests.Infrastructure;
using SaborMercado.Infrastructure.Email;
using SaborMercado.Modules.Identity.Services;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Modules.Support.Contracts;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Api.Tests;

public class GoogleAuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GoogleAuthEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithIsolatedSqlite("google", builder =>
        {
            builder.UseSetting("GoogleAuth:ClientId", "test-client-id.apps.googleusercontent.com");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
                services.RemoveAll<IGoogleIdTokenValidator>();
                services.AddSingleton<IGoogleIdTokenValidator, StubGoogleIdTokenValidator>();
            });
        });

    [Fact]
    public async Task GoogleLogin_InvalidToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest("bad-token"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_LinksExistingPasswordAccount()
    {
        var client = _factory.CreateClient();
        var email = $"google-link-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"));
        register.EnsureSuccessStatusCode();
        var passwordAuth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(passwordAuth);

        var google = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginRequest($"valid:{email}:google-subject-123"));
        google.EnsureSuccessStatusCode();
        var googleAuth = await google.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(googleAuth);
        Assert.Equal(passwordAuth.PseudonymId, googleAuth.PseudonymId);
    }
}

public class FeedbackEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FeedbackEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithIsolatedSqlite("feedback", builder =>
        {
            builder.UseSetting("Email:Host", "smtp.gmail.com");
            builder.UseSetting("Email:Port", "587");
            builder.UseSetting("Email:UserName", "test@example.com");
            builder.UseSetting("Email:Password", "test-password");
            builder.UseSetting("Email:FromAddress", "test@example.com");
            builder.UseSetting("Email:SupportToAddress", "support@example.com");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<CapturingEmailSender>();
                services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<CapturingEmailSender>());
            });
        });

    [Fact]
    public async Task Feedback_InvalidCategory_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/feedback", new
        {
            category = "invalid",
            subject = "Teste",
            message = "Mensagem de teste válida.",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Feedback_ValidRequest_ReturnsAcceptedAndSendsEmail()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/feedback", new
        {
            category = FeedbackCategories.Bug,
            subject = "Erro no carrinho",
            message = "Ao adicionar item o total não atualiza.",
            contactEmail = "user@example.com",
            diagnostics = new { route = "/compras" },
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<CapturingEmailSender>();
        Assert.Equal(1, sender.SendCount);
        Assert.NotNull(sender.LastMessage);
        Assert.Equal("support@example.com", sender.LastMessage.ToAddress);
        Assert.Equal("user@example.com", sender.LastMessage.ReplyToAddress);
    }
}
