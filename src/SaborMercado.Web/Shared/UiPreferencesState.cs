using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Shared;

public sealed class UiPreferencesState
{
    public bool ShowIconLabels { get; private set; } = true;

    public event Action? Changed;

    public async Task InitializeAsync(IPreferencesStore preferences)
    {
        ShowIconLabels = await preferences.GetShowIconLabelsAsync();
    }

    public async Task SetShowIconLabelsAsync(IPreferencesStore preferences, bool showLabels)
    {
        if (ShowIconLabels == showLabels)
        {
            return;
        }

        ShowIconLabels = showLabels;
        Changed?.Invoke();
        await preferences.SetShowIconLabelsAsync(showLabels);
    }
}
