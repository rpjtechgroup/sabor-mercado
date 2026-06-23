using SaborMercado.Web.Features.Voice;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Tests.Features;

public class VoiceFieldExtractorServiceTests
{
    [Fact]
    public async Task ExtractAsync_WhenModelFails_UsesDeterministicFallback()
    {
        var service = new VoiceFieldExtractorService(new ThrowingInterop());

        var result = await service.ExtractAsync("leite três unidades");

        Assert.Equal(VoiceExtractionSource.DeterministicFallback, result.Source);
        Assert.Equal("Leite", result.Fields.Name);
        Assert.Equal(3, result.Fields.Quantity);
    }

    [Fact]
    public async Task ExtractAsync_WhenModelReturnsJson_UsesLocalModelSource()
    {
        const string json = """{"name":"Feijão","brand":null,"unitPrice":6.5,"quantity":1,"quantityValue":1,"quantityUnit":"kg"}""";
        var service = new VoiceFieldExtractorService(new StubInterop(json));

        var result = await service.ExtractAsync("feijão seis e cinquenta");

        Assert.Equal(VoiceExtractionSource.LocalModel, result.Source);
        Assert.Equal("Feijão", result.Fields.Name);
        Assert.Equal(6.5m, result.Fields.UnitPrice);
    }

    private sealed class ThrowingInterop : IVoiceFieldExtractorInterop
    {
        public ValueTask<bool> IsModelSupportedAsync() => ValueTask.FromResult(true);

        public ValueTask<string> ExtractProductFieldsAsync(string transcript) =>
            throw new InvalidOperationException("model offline");

        public ValueTask DisposeModelAsync() => ValueTask.CompletedTask;
    }

    private sealed class StubInterop(string output) : IVoiceFieldExtractorInterop
    {
        public ValueTask<bool> IsModelSupportedAsync() => ValueTask.FromResult(true);

        public ValueTask<string> ExtractProductFieldsAsync(string transcript) => ValueTask.FromResult(output);

        public ValueTask DisposeModelAsync() => ValueTask.CompletedTask;
    }
}
