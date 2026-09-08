using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Modules;

internal sealed class MclslPersistenceModule : MclslModuleBase
{
    internal override bool HasAnnualStep => true;
    internal override string Name => "Persistence";
    internal override int Order => 0;

    internal override void Init() => MclslReincarnationProfileStore.EnsureLoaded();

    internal override void OnWorldLoaded(int year)
    {
        MclslWorldStateModifierSystem.ClearCache();
        MclslWorldArchiveStore.Load();
        MclslWorldRunRepository.EnsureCurrentRun(year);
        MclslNativeHistoryBridge.EnsureRegistered();
        MclslNativeKillStatisticsSystem.OnWorldLoaded(year);
    }

    internal override void TickAnnual(int year)
    {
        MclslWorldRunRepository.EnsureCurrentRun(year);
        MclslNativeHistoryBridge.EnsureRegistered();
        MclslNativeKillStatisticsSystem.TickAnnualRepair(year);
    }

    internal override void TickFrame(int frameCounter)
    {
        if (frameCounter % 600 == 0) MclslWorldArchiveStore.Load();
    }

    internal override void PrepareForSave()
    {
        MclslReincarnationProfileStore.Flush();
        MclslWorldArchiveStore.SaveNow();
    }

    internal override void Clear()
    {
        MclslWorldRunRepository.ResetWorld();
        MclslWorldArchiveStore.Clear();
        MclslEligibility.Clear();
        MclslDeathSystem.Clear();
        MclslNativeKillStatisticsSystem.Clear();
        MclslWorldStateModifierSystem.ClearCache();
    }
}

internal sealed class MclslHuanzhenModule : MclslModuleBase
{
    internal override bool HasAnnualStep => true;
    internal override string Name => "Huanzhen";
    internal override int Order => 10;
    internal override bool RunsWhenCoreDisabled => true;

    internal override void Init() => MclslHuanzhenSystem.EnsureLoaded();
    internal override void OnWorldLoaded(int year) => MclslHuanzhenSystem.OnWorldLoaded();
    internal override void TickRealtime() => MclslHuanzhenSystem.Tick();
    internal override void TickAnnual(int year) => MclslHuanzhenSystem.TickAnnual(year);
    internal override void Clear() => MclslHuanzhenSystem.ClearRuntime();
}

internal sealed class MclslTimelineModule : MclslModuleBase
{
    internal override bool HasAnnualStep => true;
    internal override string Name => "Timeline";
    internal override int Order => 20;
    internal override bool HasLoadRecovery => true;
    internal override void TickLoadRecovery(int year) => MclslTimelineSystem.TickAnnual(year);
    internal override void TickAnnual(int year) => MclslTimelineSystem.TickAnnual(year);
}

internal sealed class MclslWorldEpochModule : MclslModuleBase
{
    internal override bool HasAnnualStep => true;
    internal override string Name => "WorldEpoch";
    internal override int Order => 25;
    internal override bool HasLoadRecovery => true;
    internal override void TickLoadRecovery(int year) => MclslWorldRunRepository.EnsureAnnualWorldState(year);
    internal override void TickAnnual(int year) => MclslWorldRunRepository.EnsureAnnualWorldState(year);
}

internal sealed class MclslWorldSoulModule : MclslModuleBase
{
    internal override string Name => "WorldSoul";
    internal override int Order => 30;
    internal override void OnWorldLoaded(int year)
    {
        MclslWorldSoulSystem.OnWorldLoaded();
    }
    internal override void TickFrame(int frameCounter) => MclslWorldSoulSystem.TickFrame(frameCounter);
    internal override void Clear() => MclslWorldSoulSystem.Clear();
}

internal sealed class MclslInverseTruthModule : MclslModuleBase
{
    internal override string Name => "InverseTruth";
    internal override int Order => 40;
    internal override void Clear() => MclslInverseTruthSystem.Clear();
}

internal sealed class MclslRuntimeCadenceModule : MclslModuleBase
{
    internal override bool HasAnnualStep => true;
    internal override string Name => "RuntimeCadence";
    internal override int Order => 26;
    internal override bool HasLoadRecovery => true;
    internal override void OnWorldLoaded(int year) => MclslRuntimeCadence.InitializeAfterLoad(year);
    internal override void TickLoadRecovery(int year) => MclslScheduler.ScheduleAnnualWorld(year);
    internal override void TickAnnual(int year) => MclslScheduler.ScheduleAnnualWorld(year);
    internal override void TickFrame(int frameCounter) => MclslRuntimeCadence.Tick(frameCounter);
    internal override void PrepareForSave() => MclslScheduler.FlushPendingAnnualStatesForSave();
    internal override void Clear() => MclslRuntimeCadence.Clear();
}

internal sealed class MclslFactionMissionModule : MclslModuleBase
{
    internal override string Name => "FactionMission";
    internal override int Order => 45;
    internal override void Clear() => MclslFactionMissionSystem.Clear();
}

internal sealed class MclslFactionPressureModule : MclslModuleBase
{
    internal override string Name => "FactionPressure";
    internal override int Order => 46;
    internal override void Clear() => MclslFactionPressureSystem.Clear();
}

internal sealed class MclslAnnouncementModule : MclslModuleBase
{
    internal override string Name => "Announcement";
    internal override int Order => 90;
    internal override void TickFrame(int frameCounter) => MclslAnnouncementSystem.Tick();
    internal override void Clear() => MclslAnnouncementSystem.Clear();
}

internal sealed class MclslArchiveMaintenanceModule : MclslModuleBase
{
    internal override string Name => "ArchiveMaintenance";
    internal override int Order => 120;
    internal override void TickFrame(int frameCounter) => MclslWorldArchiveStore.TickPeriodic(frameCounter);
}
