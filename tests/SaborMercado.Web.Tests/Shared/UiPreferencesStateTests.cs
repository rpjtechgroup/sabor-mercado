using SaborMercado.Web.Shared;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Shared;

public sealed class UiPreferencesStateTests
{
    [Fact]
    public async Task InitializeAsync_UsesStoredPreference()
    {
        var store = new InMemoryPreferencesStore { ShowIconLabels = false };
        var state = new UiPreferencesState();

        await state.InitializeAsync(store);

        Assert.False(state.ShowIconLabels);
    }

    [Fact]
    public async Task SetShowIconLabelsAsync_PersistsAndRaisesChanged()
    {
        var store = new InMemoryPreferencesStore { ShowIconLabels = true };
        var state = new UiPreferencesState();
        await state.InitializeAsync(store);

        var changes = 0;
        state.Changed += () => changes++;

        await state.SetShowIconLabelsAsync(store, false);

        Assert.False(state.ShowIconLabels);
        Assert.False(store.ShowIconLabels);
        Assert.Equal(1, changes);
    }

    [Fact]
    public async Task SetShowIconLabelsAsync_SkipsWhenUnchanged()
    {
        var store = new InMemoryPreferencesStore { ShowIconLabels = true };
        var state = new UiPreferencesState();
        await state.InitializeAsync(store);

        var changes = 0;
        state.Changed += () => changes++;

        await state.SetShowIconLabelsAsync(store, true);

        Assert.Equal(0, changes);
    }
}
