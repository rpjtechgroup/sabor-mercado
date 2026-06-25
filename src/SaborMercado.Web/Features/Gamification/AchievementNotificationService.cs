using SaborMercado.Shared.Community;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Gamification;

public sealed class AchievementNotificationService(ToastService toast)
{
    public event Action<IReadOnlyList<AchievementDto>>? AchievementsUnlocked;

    public void NotifyNewAchievements(IReadOnlyList<AchievementDto> achievements)
    {
        if (achievements.Count == 0)
        {
            return;
        }

        AchievementsUnlocked?.Invoke(achievements);

        foreach (var achievement in achievements)
        {
            toast.Show($"Conquista desbloqueada: {achievement.Title}", ToastSeverity.Success);
        }
    }
}
