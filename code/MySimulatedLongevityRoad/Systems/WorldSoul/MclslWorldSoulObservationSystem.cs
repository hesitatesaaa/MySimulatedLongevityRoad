using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

// 旧法时代只观山河道痕，不生成、显化或观摩新法天地之魄。
// 新法天地之魄由 MclslWorldSoulSystem 统一处理。
internal static class MclslWorldSoulObservationSystem
{
    internal static void TickAnnual(int year) { }

    internal static bool TryManualObservation(string soulId, int year)
    {
        MclslAnnouncementSystem.Enqueue("旧法时代尚无可显化、祭炼的天地之魄。", "#D0B067", 7f, 1);
        return false;
    }

    internal static void Clear() { }
}
