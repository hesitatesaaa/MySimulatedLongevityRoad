using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldCalamitySystem
{
    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;

        MclslWorldStateModifiers state = MclslWorldStateModifierSystem.Current(year);
        string previousId = run.CurrentWorldCalamityId ?? string.Empty;
        if (string.Equals(previousId, state.Id, StringComparison.Ordinal)
            && run.CurrentWorldCalamityStartYear > 0)
        {
            ProcessLongStatePulse(run, state, year);
            return;
        }

        run.CurrentWorldCalamityId = state.Id;
        run.CurrentWorldCalamityName = state.Name;
        run.CurrentWorldCalamityStartYear = Math.Max(0, year);
        run.CurrentWorldCalamityEndYear = IsLongState(state.Id) ? 0 : MclslWorldStateModifierSystem.EstimateCurrentStateEndYear(year);

        if (!string.Equals(state.Id, MclslWorldStateModifierSystem.Normal, StringComparison.Ordinal))
        {
            MclslWorldRunRepository.AddEvent(year, EventType(state.Id), state.Name, state.Summary);
            ProcessLongStatePulse(run, state, year);
        }
        else if (!string.IsNullOrWhiteSpace(previousId) && !string.Equals(previousId, MclslWorldStateModifierSystem.Normal, StringComparison.Ordinal))
            MclslWorldRunRepository.AddEvent(year, "world_calamity_cleared", "天地复平", "前劫余波渐散，玄黄灵机暂归平衡。");

        MclslWorldArchiveStore.MarkDirty();
    }

    private static string EventType(string stateId)
    {
        return stateId switch
        {
            MclslWorldStateModifierSystem.SpiritTide => "world_calamity_spirit_tide",
            MclslWorldStateModifierSystem.LockSpiritAfterwave => "world_calamity_lock_spirit",
            MclslWorldStateModifierSystem.BlackTideTribulation => "world_calamity_black_tide",
            MclslWorldStateModifierSystem.WhiteMistEncroachment => "world_calamity_white_mist",
            MclslWorldStateModifierSystem.EndDharmaTerminal => "world_calamity_end_dharma",
            MclslWorldStateModifierSystem.XuanhuangTerminal => "world_calamity_xuanhuang_terminal",
            _ => "world_calamity_unknown"
        };
    }

    private static void ProcessLongStatePulse(MclslWorldRunState run, MclslWorldStateModifiers state, int year)
    {
        if (run == null || !IsLongState(state.Id)) return;
        int start = Math.Max(0, run.CurrentWorldCalamityStartYear);
        if (year <= start) return;
        int interval = PulseInterval(state.Id);
        if (interval <= 0 || (year - start) % interval != 0) return;

        int pulseIndex = Math.Max(1, (year - start) / interval);
        string firedKey = "world_calamity_pulse|" + state.Id + "|" + pulseIndex;
        if (HasFired(run, firedKey)) return;
        run.FiredHistoricalEvents.Add(firedKey);

        bool changed = state.Id switch
        {
            MclslWorldStateModifierSystem.LockSpiritAfterwave => ApplyLockSpiritPulse(run, year),
            MclslWorldStateModifierSystem.BlackTideTribulation => ApplyBlackTidePulse(run, year),
            MclslWorldStateModifierSystem.WhiteMistEncroachment => ApplyWhiteMistPulse(run, year),
            MclslWorldStateModifierSystem.EndDharmaTerminal => ApplyEndDharmaPulse(run, year),
            MclslWorldStateModifierSystem.XuanhuangTerminal => ApplyXuanhuangTerminalPulse(run, year),
            _ => false
        };

        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static bool ApplyLockSpiritPulse(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new();
        run.BackgroundFactions.WanXianAllianceInfluence = Math.Min(100, run.BackgroundFactions.WanXianAllianceInfluence + 1);
        run.BackgroundFactions.AllianceOrderPressure = Math.Min(200, run.BackgroundFactions.AllianceOrderPressure + 2);
        RegisterPressure(year, "wanxian_alliance", "万仙盟", "锁灵清册", 2, "锁灵余波渐深，贡献与功法兑换更受仙盟钳制。");
        MclslWorldRunRepository.AddEvent(year, "world_calamity_lock_spirit_pulse", "锁灵余波", "灵机受束，仙盟清册渐严。");
        return true;
    }

    private static bool ApplyBlackTidePulse(MclslWorldRunState run, int year)
    {
        MclslTechniqueLineageRecord lineage = PickLineageForStress(run, year);
        if (lineage == null)
        {
            MclslWorldRunRepository.AddEvent(year, "world_calamity_black_tide_pulse", "黑潮过境", "黑潮伏行山河，旧痕暗生。");
            return true;
        }

        bool severed = lineage.CurrentPractitioners <= 2 || MclslRealmIds.Index(lineage.PeakRealm) < MclslRealmIds.Index(MclslRealmIds.JinDan);
        if (severed)
        {
            lineage.State = "失传";
            lineage.LostYear = lineage.LostYear > 0 ? lineage.LostYear : year;
            lineage.CurrentPractitioners = 0;
            lineage.LifecycleState = "道统断绝";
            lineage.LifecycleYear = Math.Max(lineage.LifecycleYear, year);
            lineage.LastLifecycleEventYear = year;
            lineage.Summary = "黑潮过境，《" + lineage.Name + "》传承断落，残篇沉入山河。";
            TryCreateLineageRuin(run, lineage, year, "黑潮残府", "黑潮过境");
            MclslWorldRunRepository.AddEvent(year, "world_calamity_black_tide_lineage", "《" + lineage.Name + "》断传", "黑潮过境，《" + lineage.Name + "》传承断落，余痕沉入山河。");
        }
        else
        {
            int loss = Math.Max(1, lineage.CurrentPractitioners / 5);
            lineage.CurrentPractitioners = Math.Max(1, lineage.CurrentPractitioners - loss);
            lineage.LifecycleState = "法脉流传";
            lineage.LastLifecycleEventYear = year;
            lineage.Summary = "黑潮暗涌，《" + lineage.Name + "》传承受损，仍有余脉未绝。";
            MclslWorldRunRepository.AddEvent(year, "world_calamity_black_tide_lineage", "《" + lineage.Name + "》受劫", "黑潮暗涌，《" + lineage.Name + "》法脉受损，余脉未绝。");
        }
        return true;
    }

    private static bool ApplyWhiteMistPulse(MclslWorldRunState run, int year)
    {
        run.SectRuins ??= new();
        MclslSectRuinRecord ruin = PickRuin(run, year);
        if (ruin == null)
        {
            ruin = CreateCalamityRuin(run, year, "白雾吞界", "白雾遗府", new[] { "空间", "隐匿", "迁跃" }, 2);
            if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return false;
        }
        else
        {
            ruin.State = "显世";
            ruin.Danger = Math.Clamp(ruin.Danger + 4, 20, 95);
            ruin.RemainingValue = Math.Clamp(ruin.RemainingValue + 1, 0, 12);
            ruin.Description = "白雾拂过，" + ruin.Description;
        }
        MclslWorldRunRepository.AddEvent(year, "world_calamity_white_mist_ruin", "白雾显府", "白雾吞界，旧府禁制浮出尘世。");
        return true;
    }

    private static bool ApplyEndDharmaPulse(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new();
        run.BackgroundFactions.AllianceOrderPressure = Math.Min(220, run.BackgroundFactions.AllianceOrderPressure + 3);
        run.BackgroundFactions.FiveEldersSubversion = Math.Min(180, run.BackgroundFactions.FiveEldersSubversion + 1);
        RegisterPressure(year, "world_terminal", "玄黄世局", "末法渐深", 3, "灵机日衰，诸法修行愈艰。");
        MclslWorldRunRepository.AddEvent(year, "world_calamity_end_dharma_pulse", "末法渐深", "灵机日衰，诸法修行愈艰。");
        return true;
    }

    private static bool ApplyXuanhuangTerminalPulse(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new();
        run.BackgroundFactions.AllianceOrderPressure = Math.Min(240, run.BackgroundFactions.AllianceOrderPressure + 4);
        run.BackgroundFactions.FiveEldersSubversion = Math.Min(220, run.BackgroundFactions.FiveEldersSubversion + 4);
        RegisterPressure(year, "world_terminal", "玄黄世局", "玄黄终局", 5, "天地修正加剧，万法皆受压迫。");
        MclslWorldRunRepository.AddEvent(year, "world_calamity_xuanhuang_terminal_pulse", "玄黄终局", "天地修正加剧，万法皆受压迫。");
        return true;
    }

    private static void RegisterPressure(int year, string factionId, string factionName, string policy, int delta, string summary)
    {
        MclslWorldRunRepository.RegisterFactionPressure(new MclslFactionPressureRecord
        {
            Id = "calamity_pressure_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = Math.Max(0, year),
            FactionId = factionId,
            FactionName = factionName,
            PolicyName = policy,
            PressureDelta = delta,
            WorldEffect = summary,
            Summary = summary
        });
    }

    private static MclslTechniqueLineageRecord PickLineageForStress(MclslWorldRunState run, int year)
    {
        if (run?.TechniqueLineages == null || run.TechniqueLineages.Count == 0) return null;
        MclslTechniqueLineageRecord best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
            if (lineage == null || string.IsNullOrWhiteSpace(lineage.Id)) continue;
            if (string.Equals(lineage.State, "失传", StringComparison.Ordinal)) continue;
            int score = StableHash(lineage.Id + "|" + year) & 0x3f;
            score += Math.Max(0, 20 - lineage.CurrentPractitioners);
            score += Math.Max(0, 6 - MclslRealmIds.Index(lineage.PeakRealm)) * 3;
            if (best == null || score > bestScore)
            {
                best = lineage;
                bestScore = score;
            }
        }
        return best;
    }

    private static MclslSectRuinRecord PickRuin(MclslWorldRunState run, int year)
    {
        if (run?.SectRuins == null || run.SectRuins.Count == 0) return null;
        MclslSectRuinRecord best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < run.SectRuins.Count; i++)
        {
            MclslSectRuinRecord ruin = run.SectRuins[i];
            if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) continue;
            if (ruin.RemainingValue <= 0) continue;
            int score = (StableHash(ruin.Id + "|" + year) & 0x7f) + Math.Max(0, 100 - ruin.Danger);
            if (best == null || score > bestScore)
            {
                best = ruin;
                bestScore = score;
            }
        }
        return best;
    }

    private static void TryCreateLineageRuin(MclslWorldRunState run, MclslTechniqueLineageRecord lineage, int year, string category, string reason)
    {
        if (run?.SectRuins == null || lineage == null || !string.IsNullOrWhiteSpace(lineage.LinkedRuinId)) return;
        if (MclslRealmIds.Index(lineage.MaxRealm) < MclslRealmIds.Index(MclslRealmIds.JinDan))
            lineage.MaxRealm = MclslRealmIds.JinDan;
        MclslSectRuinRecord ruin = CreateCalamityRuin(run, year, reason, category, MclslGeneratedObjectFactory.SplitTags(lineage.LawTags), Math.Clamp(1 + MclslRealmIds.Index(lineage.PeakRealm) / 2, 2, 4));
        ruin.SourceTechniqueId = lineage.Id;
        ruin.SourceTechniqueName = lineage.Name;
        ruin.LinkedLineageId = lineage.Id;
        ruin.SourceTechniqueLostYear = lineage.LostYear > 0 ? lineage.LostYear : year;
        ruin.Description = reason + "，《" + lineage.Name + "》残篇与旧物沉入此地。";
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return;
        lineage.LinkedRuinId = ruin.Id;
        lineage.LifecycleState = "遗府私传";
    }

    private static MclslSectRuinRecord CreateCalamityRuin(MclslWorldRunState run, int year, string location, string category, string[] tags, int quality)
    {
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, location, "无主", tags, Math.Clamp(quality, 1, 4), category);
        ruin.Description = category + "，受" + location + "牵动而显，残存“" + string.Join("、", MclslGeneratedObjectFactory.SplitTags(ruin.LawTags)) + "”旧痕。";
        return ruin;
    }

    private static bool IsLongState(string stateId)
    {
        return stateId == MclslWorldStateModifierSystem.LockSpiritAfterwave
            || stateId == MclslWorldStateModifierSystem.BlackTideTribulation
            || stateId == MclslWorldStateModifierSystem.WhiteMistEncroachment
            || stateId == MclslWorldStateModifierSystem.EndDharmaTerminal
            || stateId == MclslWorldStateModifierSystem.XuanhuangTerminal;
    }

    private static int PulseInterval(string stateId)
    {
        return stateId switch
        {
            MclslWorldStateModifierSystem.LockSpiritAfterwave => 96,
            MclslWorldStateModifierSystem.BlackTideTribulation => 72,
            MclslWorldStateModifierSystem.WhiteMistEncroachment => 84,
            MclslWorldStateModifierSystem.EndDharmaTerminal => 96,
            MclslWorldStateModifierSystem.XuanhuangTerminal => 120,
            _ => 0
        };
    }

    private static bool HasFired(MclslWorldRunState run, string key)
    {
        run.FiredHistoricalEvents ??= new();
        for (int i = 0; i < run.FiredHistoricalEvents.Count; i++)
            if (string.Equals(run.FiredHistoricalEvents[i], key, StringComparison.Ordinal))
                return true;
        return false;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string safe = value ?? string.Empty;
            for (int i = 0; i < safe.Length; i++) hash = hash * 31 + safe[i];
            return hash;
        }
    }
}
