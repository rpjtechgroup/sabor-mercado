using Bunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Components;

public class GoogleSignInButtonTests
{
    [Fact]
    public void WithoutClientId_RendersHostContainer()
    {
        using var ctx = new BunitContext();
        var preferences = new InMemoryPreferencesStore();
        ctx.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        ctx.Services.AddSingleton<IGoogleSignInService>(new StubGoogleSignInService());
        ctx.Services.AddSingleton(TimeProvider.System);
        ctx.Services.AddSingleton<IPreferencesStore>(preferences);
        ctx.Services.AddSingleton(preferences);
        ctx.Services.AddSingleton(new SaborMercadoApiClient(
            new HttpClient { BaseAddress = new Uri("http://localhost:9999") },
            preferences));
        ctx.Services.AddSingleton<IPendingShareStore, InMemoryPendingShareStore>();
        ctx.Services.AddSingleton<ShareService>();
        ctx.Services.AddSingleton<AccountService>();

        var cut = ctx.Render<GoogleSignInButton>();
        Assert.NotNull(cut.Find(".google-signin-host"));
    }

    private sealed class StubGoogleSignInService : IGoogleSignInService
    {
        public ValueTask<bool> RenderButtonAsync(string elementId, string clientId, IGoogleSignInListener listener) =>
            ValueTask.FromResult(false);
    }
}
