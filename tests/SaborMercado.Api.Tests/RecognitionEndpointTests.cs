using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Api.Tests.Infrastructure;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Api.Tests;

public class RecognitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RecognitionEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithIsolatedSqlite("recognition", builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            }));

    [Fact]
    public async Task PostRecognition_ReturnsStructuredResult()
    {
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var image = new ByteArrayContent([0xFF, 0xD8, 0xFF] );
        image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(image, "image", "label.jpg");

        var response = await client.PostAsync("/api/v1/recognitions", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RecognitionResultDto>();
        Assert.NotNull(result);
        Assert.Equal("Óleo De Soja Liza", result.ProductName);
        Assert.Equal(8.99m, result.Price);
    }
}
