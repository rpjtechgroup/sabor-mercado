using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Components;

public class VoiceInputButtonTests : BunitContext
{
    [Fact]
    public void WhenUnsupported_DoesNotRenderMicrophoneButton()
    {
        Services.AddSingleton<ISpeechRecognitionService>(new StubSpeechRecognitionService(supported: false));
        var cut = Render<VoiceInputButton>();

        Assert.DoesNotContain("voice-input-btn", cut.Markup);
    }

    [Fact]
    public void WhenSupported_RendersMicrophoneButton()
    {
        Services.AddSingleton<ISpeechRecognitionService>(new StubSpeechRecognitionService(supported: true));
        var cut = Render<VoiceInputButton>();

        Assert.Contains("voice-input-btn", cut.Markup);
        Assert.Contains(AppIcons.Microphone, cut.Markup);
    }

    private sealed class StubSpeechRecognitionService(bool supported) : ISpeechRecognitionService
    {
        public ValueTask<bool> IsSupportedAsync() => ValueTask.FromResult(supported);

        public ValueTask StartListeningAsync(ISpeechRecognitionListener listener, string lang = "pt-BR") =>
            ValueTask.CompletedTask;

        public ValueTask StopListeningAsync() => ValueTask.CompletedTask;
    }
}
