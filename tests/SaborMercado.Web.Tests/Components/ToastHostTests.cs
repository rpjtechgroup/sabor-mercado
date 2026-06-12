using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Components;

public class ToastHostTests : BunitContext
{
    public ToastHostTests()
    {
        Services.AddLocalization();
        Services.AddScoped<StatusMessageLocalizer>();
        Services.AddSingleton(TimeProvider.System);
        Services.AddScoped<ToastService>(_ => new ToastService(TimeProvider.System, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void BudgetHalf_RendersLocalizedTextViaToastHost()
    {
        var toasts = Services.GetRequiredService<ToastService>();
        var localizer = Services.GetRequiredService<StatusMessageLocalizer>();
        var message = StatusMessage.Create(StatusCodes.BudgetHalf, ("R", MoneyFormat.Format(45m)));
        toasts.Show(localizer.Localize(message), StatusMessageSeverity.FromCode(message.Code));

        var cut = Render<ToastHost>();

        Assert.Contains("metade do orçamento", cut.Markup);
        Assert.Contains("45,00", cut.Markup);
        Assert.Contains("aria-live=\"polite\"", cut.Markup);
    }

    [Fact]
    public void BudgetExceeded_RendersDangerSeverity()
    {
        var toasts = Services.GetRequiredService<ToastService>();
        var localizer = Services.GetRequiredService<StatusMessageLocalizer>();
        var message = StatusMessage.Create(StatusCodes.BudgetExceeded, ("excess", MoneyFormat.Format(27m)));
        toasts.Show(localizer.Localize(message), StatusMessageSeverity.FromCode(message.Code));

        var cut = Render<ToastHost>();

        Assert.Contains("ultrapassado", cut.Markup);
        Assert.Contains("27,00", cut.Markup);
        Assert.Contains("toast-danger", cut.Find(".toast-item").ClassList);
    }

    [Fact]
    public void SessionFinished_UnderVariant_UsesSavingText()
    {
        var toasts = Services.GetRequiredService<ToastService>();
        var localizer = Services.GetRequiredService<StatusMessageLocalizer>();
        var message = StatusMessage.Create(
            StatusCodes.SessionFinished,
            ("n", "7"), ("T", MoneyFormat.Format(80m)), ("variant", "under"), ("saving", MoneyFormat.Format(20m)));
        toasts.Show(localizer.Localize(message), StatusMessageSeverity.FromCode(message.Code));

        var cut = Render<ToastHost>();

        Assert.Contains("economia de", cut.Markup);
        Assert.Contains("7 itens", cut.Markup);
    }

    [Fact]
    public void EmptyItems_RendersNothing()
    {
        var cut = Render<ToastHost>();

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void CloseButton_DismissesToast()
    {
        var toasts = Services.GetRequiredService<ToastService>();
        var dismissed = false;
        toasts.Show("Aviso", ToastSeverity.Info, () => dismissed = true);

        var cut = Render<ToastHost>();
        cut.Find(".toast-close").Click();

        Assert.True(dismissed);
        Assert.Empty(toasts.Items);
    }
}
