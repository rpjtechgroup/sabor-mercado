using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Shared;

public sealed class UiPreferencesState
{
    public bool ShowIconLabels { get; private set; } = true;

    public event Action? Changed;

    public async Task InitializeAsync(IPreferencesStore preferences)
    {
        ShowIconLabels = await preferences.GetShowIconLabelsAsync();
        Changed?.Invoke();
    }

    public async Task SetShowIconLabelsAsync(IPreferencesStore preferences, bool showLabels)
    {
        if (ShowIconLabels == showLabels)
        {
            return;
        }

        ShowIconLabels = showLabels;
        await preferences.SetShowIconLabelsAsync(showLabels);
        Changed?.Invoke();
    }
}
