using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Web.Features.Voice;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Components;

public class VoiceLiteralPanelTests : BunitContext
{
    [Fact]
    public void RendersTranscriptFieldAndSubmitButton()
    {
        RegisterServices();
        var cut = Render<VoiceLiteralPanel>();

        Assert.Contains("voice-literal-text", cut.Markup);
        Assert.Contains(AppIcons.Send, cut.Markup);
        Assert.Contains("Enviar", cut.Markup);
    }

    [Fact]
    public async Task Submit_InvokesExtractorAndCallback()
    {
        VoiceFieldExtractionResult? captured = null;
        RegisterServices();

        var cut = Render<VoiceLiteralPanel>(parameters => parameters
            .Add(p => p.InitialTranscript, "óleo de soja oito e noventa")
            .Add(p => p.OnExtracted, (VoiceFieldExtractionResult result) =>
            {
                captured = result;
                return Task.CompletedTask;
            }));

        await cut.Find("button[title='Enviar']").ClickAsync();

        Assert.NotNull(captured);
        Assert.Equal("Óleo de soja", captured!.Fields.Name);
    }

    private void RegisterServices()
    {
        Services.AddSingleton<ISpeechRecognitionService>(new StubSpeechRecognitionService(supported: true));
        Services.AddSingleton<IVoiceFieldExtractorInterop>(new StubVoiceFieldExtractorInterop());
        Services.AddSingleton(new VoiceFieldExtractorService(
            new StubVoiceFieldExtractorInterop()));
    }

    private sealed class StubSpeechRecognitionService(bool supported) : ISpeechRecognitionService
    {
        public ValueTask<bool> IsSupportedAsync() => ValueTask.FromResult(supported);

        public ValueTask StartListeningAsync(ISpeechRecognitionListener listener, string lang = "pt-BR") =>
            ValueTask.CompletedTask;

        public ValueTask StopListeningAsync() => ValueTask.CompletedTask;
    }

    private sealed class StubVoiceFieldExtractorInterop : IVoiceFieldExtractorInterop
    {
        public ValueTask<bool> IsModelSupportedAsync() => ValueTask.FromResult(false);

        public ValueTask<string> ExtractProductFieldsAsync(string transcript) =>
            ValueTask.FromResult(string.Empty);

        public ValueTask DisposeModelAsync() => ValueTask.CompletedTask;
    }
}
