using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTimelineSystem
{
    internal static void TickAnnual(int year)
    {
        if (!MclslRuntimeSettings.TimelineEnabled || year < 0) return;
        MclslWorldRunRepository.EnsureCurrentRun(year);
        var run = MclslWorldRunRepository.Current;
        run.LastProcessedYear = year;
        foreach (var state in run.TimelineAnchors)
        {
            if (state.Resolved || year < state.ScheduledYear) continue;
            if (!MclslWorldEpochSystem.IsNewLawActive(year)) continue;
            Resolve(state, year);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void Resolve(MclslTimelineAnchorState state, int year)
    {
        MclslTimelineAnchorDefinition definition = MclslKnowledgeCatalog.GetAnchor(state.AnchorId);
        if (definition == null) return;
        state.Resolved = true;
        state.Succeeded = true;
        state.ResolvedYear = year;
        state.OutcomeCode = "background_resolved";
        ApplyBackgroundEffect(state.AnchorId, year);
        MclslWorldRunRepository.AddDiscovery(definition.KnowledgeId, year, state.AnchorId);
        MclslWorldRunRepository.AddEvent(year, "timeline", definition.Name, definition.Description);
        MclslAnnouncementSystem.Enqueue("玄黄纪事：" + definition.Name, "#CFC7B2", definition.Terminal ? 12f : 8f, 1);
        if (definition.Terminal)
        {
            var run = MclslWorldRunRepository.Current;
            run.IsTerminal = true;
            run.TerminalReason = definition.Name;
            if (MclslRuntimeSettings.AutoSealTerminalCycle) MclslReincarnationService.SealCurrentCycle(definition.Name, year);
        }
    }

    private static void ApplyBackgroundEffect(string id, int year)
    {
        var state = MclslWorldRunRepository.Current.BackgroundFactions ??= new MclslBackgroundFactionState();
        switch (id)
        {
            case "anchor_alliance_rise":
                state.WanXianAllianceInfluence = Math.Max(state.WanXianAllianceInfluence, 65);
                state.AllianceOrderPressure = 25;
                state.AlliancePolicy = "整合修仙秩序";
                break;
            case "anchor_five_elders_current":
                state.FiveEldersInfluence = Math.Max(state.FiveEldersInfluence, 55);
                state.FiveEldersSubversion = 30;
                state.FiveEldersPolicy = "渗透诸国与反制万仙盟";
                break;
            case "anchor_lock_spirit_plan":
                state.WanXianAllianceInfluence = Math.Min(100, state.WanXianAllianceInfluence + 15);
                state.AllianceOrderPressure = Math.Min(100, state.AllianceOrderPressure + 35);
                state.AlliancePolicy = "推进锁灵体系";
                break;
            case "anchor_black_tide":
                state.WanXianAllianceInfluence = Math.Min(100, state.WanXianAllianceInfluence + 8);
                state.FiveEldersInfluence = Math.Min(100, state.FiveEldersInfluence + 10);
                state.AllianceOrderPressure = Math.Min(100, state.AllianceOrderPressure + 15);
                state.FiveEldersSubversion = Math.Min(100, state.FiveEldersSubversion + 18);
                state.FiveEldersPolicy = "借黑潮扰动传承";
                break;
            case "anchor_white_mist":
                if (!MclslWorldRunRepository.Current.WorldChanges.Exists(x => x.SourceType == "timeline_white_mist"))
                {
                    MclslWorldChangeRecord change = MclslWorldChangeSystem.CreateFromTimeline(
                        year,
                        "白雾吞界",
                        new[] { "空间", "隐匿", "毁灭" });
                    if (change != null) change.SourceType = "timeline_white_mist";
                }
                break;
            case "anchor_end_dharma":
                state.WanXianAllianceInfluence = Math.Min(100, state.WanXianAllianceInfluence + 10);
                state.FiveEldersInfluence = Math.Min(100, state.FiveEldersInfluence + 10);
                state.AllianceOrderPressure = Math.Min(100, state.AllianceOrderPressure + 20);
                state.FiveEldersSubversion = Math.Min(100, state.FiveEldersSubversion + 12);
                state.AlliancePolicy = "收束末法资源";
                state.FiveEldersPolicy = "暗夺末法余机";
                break;
            case "anchor_xuanhuang_terminal":
                state.AllianceOrderPressure = Math.Min(100, state.AllianceOrderPressure + 25);
                state.FiveEldersSubversion = Math.Min(100, state.FiveEldersSubversion + 25);
                state.AlliancePolicy = "封存终局档案";
                state.FiveEldersPolicy = "争夺终局真值";
                break;
        }
    }
}
