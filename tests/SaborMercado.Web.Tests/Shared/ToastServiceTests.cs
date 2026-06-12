using SaborMercado.Web.Shared;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Shared;

public class ToastServiceTests
{
    [Fact]
    public void Show_AddsToastAndRaisesStateChanged()
    {
        var changed = false;
        var service = new ToastService(TimeProvider.System, TimeSpan.FromMinutes(5));
        service.StateChanged += () => changed = true;

        var id = service.Show("Teste", ToastSeverity.Info);

        Assert.Single(service.Items);
        Assert.Equal(id, service.Items[0].Id);
        Assert.True(changed);
    }

    [Fact]
    public void Dismiss_InvokesCallbackAndRemovesToast()
    {
        var dismissed = false;
        var service = new ToastService(TimeProvider.System, TimeSpan.FromMinutes(5));
        var id = service.Show("Teste", ToastSeverity.Warning, () => dismissed = true);

        service.Dismiss(id);

        Assert.Empty(service.Items);
        Assert.True(dismissed);
    }

    [Fact]
    public void Show_EvictsOldestWhenStackExceedsMax()
    {
        var service = new ToastService(TimeProvider.System, TimeSpan.FromMinutes(5));

        service.Show("1", ToastSeverity.Info);
        service.Show("2", ToastSeverity.Info);
        service.Show("3", ToastSeverity.Info);
        service.Show("4", ToastSeverity.Info);

        Assert.Equal(ToastService.MaxVisible, service.Items.Count);
        Assert.Equal("2", service.Items[0].Text);
        Assert.Equal("4", service.Items[^1].Text);
    }

    [Fact]
    public async Task AutoDismiss_RemovesToastAfterTimeout()
    {
        var service = new ToastService(TimeProvider.System, TimeSpan.FromMilliseconds(80));
        var id = service.Show("Some", ToastSeverity.Info);

        await Task.Delay(150);

        Assert.DoesNotContain(service.Items, t => t.Id == id);
    }
}
