using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Shared;

/// <summary>Preferências de exibição da interface (ícones vs ícones+texto).</summary>
public sealed class UiPreferencesState
{
    public bool ShowIconLabels { get; private set; }

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
        await preferences.SetShowIconLabelsAsync(showLabels);
        Changed?.Invoke();
    }
}
