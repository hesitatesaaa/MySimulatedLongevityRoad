using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Data.Death;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Systems.Death;

internal static class MclslDeathSystem
{
    private const int CaveDeathDivertCooldownYears = 50;
    private static readonly HashSet<long> CommittedActorIds = new();
    private static readonly string[] KillerMemberNames =
    {
        "last_attacker", "lastAttacker", "_last_attacker", "attacked_by", "attackedBy", "killer", "last_hit_actor", "lastHitActor"
    };

    internal static MclslDeathSnapshot Capture(Actor actor, AttackType attackType)
    {
        if (actor?.data == null || !MclslActorAccessor.IsCultivator(actor)) return MclslDeathSnapshot.Empty;
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DeathArchived, 0) == 1) return MclslDeathSnapshot.Empty;
        string pendingCause = MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathCode, string.Empty);
        string killerName = string.IsNullOrWhiteSpace(pendingCause) && IsCombatDeath(attackType) ? TryGetKillerName(actor) : string.Empty;
        return new MclslDeathSnapshot(
            true,
            actorId,
            SafeName(actor),
            MclslActorAccessor.Realm(actor),
            MclslRuntime.CurrentYear(),
            SafeCultivationYears(actor),
            string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无国" : actor.kingdom.data.name,
            string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "无城" : actor.city.data.name,
            actor.data.x,
            actor.data.y,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientFoundationName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientCoreName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoIntent, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientNascentName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDivineIntent, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineChangeName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulName, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.HeavenlyDuty, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthName, string.Empty),
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyLeap, 0) == 1,
            attackType.ToString(),
            pendingCause,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathSource, string.Empty),
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathDetail, string.Empty),
            killerName,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.PendingDeathImportant, 0) == 1);
    }

    internal static bool TryDivertNativeDeath(Actor actor, AttackType attackType)
    {
        if (!MclslRuntimeSettings.CoreEnabled || !MclslActorAccessor.Alive(actor)) return false;
        if (!MclslActorAccessor.IsCultivator(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathCode, string.Empty))) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return false;
        if (TryDivertNativeDeathByCave(actor, attackType)) return true;
        return TryDivertNativeDeathFirewall(actor, attackType);
    }

    internal static bool HasPotentialCombatDeathDiversion(Actor actor)
    {
        if (!MclslRuntimeSettings.CoreEnabled || !MclslActorAccessor.Alive(actor)) return false;
        if (!MclslActorAccessor.IsCultivator(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathCode, string.Empty))) return false;
        string realm = MclslActorAccessor.Realm(actor);
        int realmIndex = MclslRealmIds.Index(realm);
        if (realmIndex < MclslRealmIds.Index(MclslRealmIds.YuanYing) || realmIndex > MclslRealmIds.Index(MclslRealmIds.HuaShen)) return false;
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveName, string.Empty))) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveIntegrity, 0) < 35) return false;
        int year = MclslRuntime.CurrentYear();
        int lastDivertYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCaveDeathDivertYear, -999999);
        return year - lastDivertYear >= CaveDeathDivertCooldownYears;
    }

    internal static bool TryDivertNativeDeathByCave(Actor actor, AttackType attackType)
    {
        if (!MclslRuntimeSettings.CoreEnabled || !MclslActorAccessor.Alive(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathCode, string.Empty))) return false;
        string realm = MclslActorAccessor.Realm(actor);
        int realmIndex = MclslRealmIds.Index(realm);
        if (realmIndex < MclslRealmIds.Index(MclslRealmIds.YuanYing) || realmIndex > MclslRealmIds.Index(MclslRealmIds.HuaShen)) return false;

        string caveName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveName, string.Empty);
        if (string.IsNullOrWhiteSpace(caveName)) return false;
        int integrity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveIntegrity, 0), 0, 100);
        if (integrity < 35) return false;

        int year = MclslRuntime.CurrentYear();
        int lastDivertYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCaveDeathDivertYear, -999999);
        if (year - lastDivertYear < CaveDeathDivertCooldownYears) return false;
        int stability = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCoreStability, 50), 0, 100);
        int compatibility = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveCompatibility, 50), 0, 100);
        int loss = Math.Clamp(22 + Math.Max(0, 65 - stability) / 5 + Math.Max(0, 70 - compatibility) / 6 + realmIndex * 3, 18, 42);
        int newIntegrity = integrity - loss;
        if (newIntegrity < 8) return false;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, newIntegrity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCaveDeathDivertYear, year);
        bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        MclslCultivationGrowthSystem.ApplyProgressSetback(actor, realm, 12f, ancientLaw);
        string detail = "濒死之际，元婴洞天“" + caveName + "”代其承受死劫，洞天完整度由" + integrity + "%降至" + newIntegrity + "%；此后" + CaveDeathDivertCooldownYears + "年内无法再次代死。";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, detail);
        try
        {
            actor.updateStats();
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max * 0.38f));
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Death-MclslDeathSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs #1: " + mclslEmptyCatchEx.Message); }

        string name = SafeName(actor);
        MclslWorldRunRepository.AddEvent(year, "nascent_cave_death_divert", name + "洞天代死", detail, actor);
        if (MclslRuntimeSettings.SurvivalAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue(name + "借元婴洞天避过死劫，洞天根基受损。", "#79B6B0", 8f, 1);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static bool TryDivertNativeDeathFirewall(Actor actor, AttackType attackType)
    {
        string realm = MclslActorAccessor.Realm(actor);
        if (string.IsNullOrWhiteSpace(realm)) return false;
        string cause = NativeDeathCause(attackType);

        // Battle, disasters, hunger, disease and other real causes must be able to kill.
        // This lightweight guard only prevents WorldBox's base old-age roll from ignoring
        // the cultivation lifespan rules before the actor reaches the corrected limit.
        if (cause != "old_age" || MclslLongevityRules.HasReachedLifespanLimit(actor, realm)) return false;

        int year = MclslRuntime.CurrentYear();
        int lastDivertYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastNativeDeathDivertYear, -999999);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastNativeDeathDivertYear, year);
        StabilizeAfterDivert(actor);

        string detail = "寿尽之劫被修行寿元修正，按" + MclslRealmIds.Display(realm) + "寿元上限" + MclslLongevityRules.ExpectedLifespan(actor, realm) + "年继续存世。";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, detail);
        if (lastDivertYear != year)
            MclslWorldRunRepository.AddEvent(year, "native_death_divert", SafeName(actor) + "避过早衰", detail, actor);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool IsCombatDeath(AttackType attackType)
    {
        string cause = NativeDeathCause(attackType);
        return cause == "battle" || cause == "eaten";
    }

    private static string NativeDeathCause(AttackType attackType)
    {
        string attack = attackType.ToString().ToLowerInvariant();
        if (attack.Contains("hunger") || attack.Contains("starv")) return "starvation";
        if (attack.Contains("age") || attack.Contains("old")) return "old_age";
        if (attack.Contains("drown") || attack == "water") return "drowning";
        if (attack.Contains("fire") || attack.Contains("burn") || attack.Contains("lava")) return "fire";
        if (attack.Contains("frost") || attack.Contains("freeze") || attack.Contains("cold") || attack.Contains("ice")) return "frost";
        if (attack.Contains("lightning") || attack.Contains("thunder") || attack.Contains("electric")) return "lightning";
        if (attack.Contains("infection") || attack.Contains("plague") || attack.Contains("fever") || attack.Contains("tumor") || attack.Contains("disease")) return "disease";
        if (attack.Contains("poison")) return "poison";
        if (attack.Contains("eaten")) return "eaten";
        if (attack.Contains("acid")) return "acid";
        if (attack.Contains("gravity") || attack.Contains("fall")) return "gravity";
        if (attack.Contains("metamorph")) return "metamorphosis";
        if (attack.Contains("explosion") || attack.Contains("bomb")) return "explosion";
        if (attack.Contains("meteor")) return "meteor";
        if (attack.Contains("storm") || attack.Contains("tornado") || attack.Contains("wind")) return "storm";
        if (!string.IsNullOrWhiteSpace(attack) && attack != "none" && attack != "0") return "battle";
        return "unknown";
    }

    private static void StabilizeAfterDivert(Actor actor)
    {
        try
        {
            actor.updateStats();
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max * 0.55f));
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Death-MclslDeathSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs #2: " + mclslEmptyCatchEx.Message); }

        TrySetNumber(actor?.data, "hunger", 100f);
        TrySetNumber(actor?.data, "_hunger", 100f);
        TrySetNumber(actor?.data, "food", 100f);
        TrySetNumber(actor?.data, "nutrition", 100f);
    }

    private static void TrySetNumber(object target, string name, float value)
    {
        if (target == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null)
            {
                if (field.FieldType == typeof(float)) field.SetValue(target, value);
                else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                return;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Death-MclslDeathSystem-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs #3: " + mclslEmptyCatchEx.Message); }
    }

    internal static void Commit(Actor actor, in MclslDeathSnapshot snapshot)
    {
        if (!snapshot.Found || snapshot.ActorId <= 0L || IsAlive(actor) || !CommittedActorIds.Add(snapshot.ActorId)) return;
        if (actor?.data != null) MclslActorAccessor.Set(actor, MclslActorDataKeys.DeathArchived, 1);
        MclslDeathRecord record = BuildRecord(snapshot);
        MclslWorldRunRepository.RegisterDeath(record);
        MclslActorReincarnationSystem.RecordFromDeath(actor, record);
        MclslWorldActorQuery.MarkDirty();
        if (ShouldShowTopDeathAnnouncement(record)) MclslAnnouncementSystem.Enqueue(record.Announcement, DeathColor(record.CauseCode), DeathDuration(record.CauseCode));
    }

    internal static bool ExecuteScriptedDeath(Actor actor, string causeCode, string sourceName, string detail, bool important = true)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathCode, causeCode ?? "scripted");
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathSource, sourceName ?? string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathDetail, detail ?? string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathImportant, important ? 1 : 0);
        Actor priorAttacker = MclslNativeKillStatisticsSystem.DetachScriptedDeathAttacker(actor);
        try
        {
            actor.die(false, (AttackType)11, false, false);
            bool dead = !IsAlive(actor);
            if (!dead)
            {
                ClearPendingDeath(actor);
                MclslNativeKillStatisticsSystem.RestoreScriptedDeathAttacker(actor, priorAttacker);
            }
            return dead;
        }
        catch
        {
            ClearPendingDeath(actor);
            MclslNativeKillStatisticsSystem.RestoreScriptedDeathAttacker(actor, priorAttacker);
            return false;
        }
    }

    internal static void Clear()
    {
        CommittedActorIds.Clear();
    }

    private static MclslDeathRecord BuildRecord(in MclslDeathSnapshot s)
    {
        string causeCode = ResolveCauseCode(s);
        string realmName = MclslRealmIds.Display(s.RealmId);
        string causeText = CauseText(causeCode, s);
        string title = DeathTitle(s);
        string announcement = BuildDeathAnnouncement(s, causeCode, causeText);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(s.RealmId));
        bool meetsRealmThreshold = realmIndex + 1 >= MclslRuntimeSettings.DeathPopupMinRealm;
        bool ruinAllowed = causeCode != "ruin_exploration" || MclslRuntimeSettings.RuinDeathAnnouncementsEnabled;
        bool surface = MclslRuntimeSettings.DeathAnnouncementsEnabled && meetsRealmThreshold && ruinAllowed;
        return new MclslDeathRecord
        {
            ActorId = s.ActorId,
            ActorName = s.ActorName,
            RealmId = s.RealmId,
            RealmName = realmName,
            Year = s.Year,
            Age = s.Age,
            CauseCode = causeCode,
            CauseText = causeText,
            SourceName = s.PendingSource,
            KillerName = s.KillerName,
            KingdomName = s.KingdomName,
            CityName = s.CityName,
            MapX = s.MapX,
            MapY = s.MapY,
            TechniqueName = s.TechniqueName,
            CultivationSystem = s.CultivationSystem,
            AncientFoundationName = s.AncientFoundationName,
            AncientCoreName = s.AncientCoreName,
            AncientDaoIntent = s.AncientDaoIntent,
            AncientNascentName = s.AncientNascentName,
            AncientDivineIntent = s.AncientDivineIntent,
            AncientDaoName = s.AncientDaoName,
            FoundationWonderName = s.FoundationWonderName,
            GoldenCoreLaws = s.GoldenCoreLaws,
            NascentCaveName = s.NascentCaveName,
            NascentEssenceName = s.NascentEssenceName,
            DivineChangeName = s.DivineChangeName,
            DivineMarrowName = s.DivineMarrowName,
            WorldSoulName = s.WorldSoulName,
            HeavenlyDuty = s.HeavenlyDuty,
            InverseTruthName = s.InverseTruthName,
            Title = title,
            Announcement = announcement,
            Surfaced = surface
        };
    }

    private static string ResolveCauseCode(in MclslDeathSnapshot s)
    {
        if (!string.IsNullOrWhiteSpace(s.PendingCauseCode)) return s.PendingCauseCode.Trim();
        if (!string.IsNullOrWhiteSpace(s.KillerName)) return "battle";
        string attack = (s.AttackTypeName ?? string.Empty).ToLowerInvariant();
        if (attack.Contains("hunger") || attack.Contains("starv")) return "starvation";
        if (attack.Contains("age") || attack.Contains("old")) return "old_age";
        if (attack.Contains("drown") || attack == "water") return "drowning";
        if (attack.Contains("fire")) return "fire";
        if (attack.Contains("poison")) return "poison";
        if (attack.Contains("infection") || attack.Contains("plague") || attack.Contains("fever") || attack.Contains("tumor")) return "disease";
        if (attack.Contains("eaten")) return "eaten";
        if (attack.Contains("acid")) return "acid";
        if (attack.Contains("gravity")) return "gravity";
        if (attack.Contains("metamorph")) return "metamorphosis";
        if (s.Age >= 60) return "old_age";
        if (!string.IsNullOrWhiteSpace(attack) && attack != "none" && attack != "0") return "battle";
        return "unknown";
    }

    private static string CauseText(string code, in MclslDeathSnapshot s)
    {
        if (!string.IsNullOrWhiteSpace(s.PendingDetail)) return s.PendingDetail.TrimEnd('。') + "。";
        return code switch
        {
            "battle" => string.IsNullOrWhiteSpace(s.KillerName) ? "争斗之中伤重不治。" : "与" + s.KillerName + "交锋，终究未能脱身。",
            "old_age" => "修道" + ChineseYears(Math.Max(0, s.Age)) + "载后寿尽，道途止步于" + MclslRealmIds.Display(s.RealmId) + "。",
            "starvation" => "灵机断绝、形神俱疲，最终困厄而亡。",
            "ruin_exploration" => "深入“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "无名宗门遗迹" : s.PendingSource) + "”后未能归来。",
            "world_change_backlash" => "强抽“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "天地之变" : s.PendingSource) + "”之髓，遭天地反噬。",
            "world_soul_backlash" => "承接“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "天地之魄" : s.PendingSource) + "”后未能履行天职，魄核反噬形神。",
            "mortal_miasma" => "屡伤凡俗，仙凡瘴积满后反噬修为与形神。",
            "drowning" => "落入深水，灵力与气息相继断绝。",
            "fire" => "陷于烈焰，肉身与修为一并焚毁。",
            "disease" => "疫病侵入形神，调息服药皆未能挽回。",
            "poison" => "身中剧毒，毒性最终侵透经脉与神魂。",
            "eaten" => "遭凶兽吞噬，尸骨无存。",
            "acid" => "遭强酸腐蚀，形神俱损。",
            "gravity" => "自高处坠落，重创不治。",
            "metamorphosis" => "肉身发生不可逆异变，最终失去生机。",
            _ => "死因未明，只余修行痕迹散入天地。"
        };
    }

    private static string BuildDeathAnnouncement(in MclslDeathSnapshot s, string causeCode, string causeText)
    {
        if (s.RealmId == MclslRealmIds.ChangSheng)
        {
            return BuildLongevityDeathAnnouncement(s, causeCode, causeText);
        }

        List<string> lines = new();
        lines.Add("【" + DeathTitle(s) + "】");
        lines.Add(string.Empty);
        lines.Add(DeathSubject(s) + "，凡修道" + ChineseYears(Math.Max(0, s.Age)) + "载。");

        if (s.RealmId == MclslRealmIds.LianQi && !string.IsNullOrWhiteSpace(s.TechniqueName))
        {
            lines.Add("主修《" + s.TechniqueName.Trim('《', '》') + "》，炼天地灵气以御真元。");
        }

        if (!IsAncientDeath(s)
            && MclslRealmIds.Index(s.RealmId) >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && s.HarmonyLeap
            && !string.IsNullOrWhiteSpace(s.WorldSoulName))
        {
            lines.Add(string.Empty);
            lines.Add("昔以凡躯补天地之魄最后一击，");
            lines.Add("夺【" + s.WorldSoulName + "】之魄，越诸境而合道，");
            if (!string.IsNullOrWhiteSpace(s.HeavenlyDuty)) lines.Add("承天职【" + s.HeavenlyDuty + "】。");
        }
        else
        {
            if (IsAncientDeath(s)) AppendAncientLineage(lines, s);
            else AppendNewLawLineage(lines, s);
        }

        string place = BuildPlace(s);
        string cause = DeathCausePhrase(causeCode, s, causeText);
        if (!string.IsNullOrWhiteSpace(place) || !string.IsNullOrWhiteSpace(cause))
        {
            lines.Add(string.Empty);
            if (!string.IsNullOrWhiteSpace(place)) lines.Add("于" + place + "，");
            lines.Add("因" + (string.IsNullOrWhiteSpace(cause) ? "死因未明" : cause) + DeathVerb(s) + "。");
        }

        lines.Add(string.Empty);
        lines.Add(IsAncientDeath(s) ? "旧日仙途至此而止，所修所悟复归山河。" : "大道止于此身，所修之法复归天地。");
        return string.Join("\n", lines);
    }

    private static string BuildLongevityDeathAnnouncement(in MclslDeathSnapshot s, string causeCode, string causeText)
    {
        string truth = string.IsNullOrWhiteSpace(s.InverseTruthName) ? "未载逆理" : s.InverseTruthName;
        List<string> lines = new()
        {
            "【" + DeathTitle(s) + "】",
            string.Empty,
            DeathSubject(s) + "，凡修道" + ChineseYears(Math.Max(0, s.Age)) + "载。",
            "昔逆天地之理【" + truth + "】，以证长生。"
        };

        string place = BuildPlace(s);
        string cause = DeathCausePhrase(causeCode, s, causeText);
        if (!string.IsNullOrWhiteSpace(place) || !string.IsNullOrWhiteSpace(cause))
        {
            lines.Add(string.Empty);
            if (!string.IsNullOrWhiteSpace(place)) lines.Add("于" + place + "，");
            lines.Add("因" + (string.IsNullOrWhiteSpace(cause) ? "死因未明" : cause) + "身陨。");
        }

        lines.Add(string.Empty);
        lines.Add("其身今日陨落，");
        lines.Add("所定之理失其执掌，");
        lines.Add("天地修正由此而生。");
        lines.Add(string.Empty);
        lines.Add("【" + truth + "】开始衰退。");
        return string.Join("\n", lines);
    }

    private static bool IsAncientDeath(in MclslDeathSnapshot s)
    {
        if (string.Equals(s.CultivationSystem, MclslCultivationSystemIds.AncientLaw, StringComparison.Ordinal)) return true;
        if (string.Equals(s.CultivationSystem, MclslCultivationSystemIds.NewLaw, StringComparison.Ordinal)) return false;
        bool hasAncientLineage = !string.IsNullOrWhiteSpace(s.AncientFoundationName)
            || !string.IsNullOrWhiteSpace(s.AncientCoreName)
            || !string.IsNullOrWhiteSpace(s.AncientDaoIntent)
            || !string.IsNullOrWhiteSpace(s.AncientNascentName)
            || !string.IsNullOrWhiteSpace(s.AncientDivineIntent)
            || !string.IsNullOrWhiteSpace(s.AncientDaoName);
        bool hasNewLawLineage = !string.IsNullOrWhiteSpace(s.FoundationWonderName)
            || !string.IsNullOrWhiteSpace(s.GoldenCoreLaws)
            || !string.IsNullOrWhiteSpace(s.NascentCaveName)
            || !string.IsNullOrWhiteSpace(s.NascentEssenceName)
            || !string.IsNullOrWhiteSpace(s.DivineChangeName)
            || !string.IsNullOrWhiteSpace(s.DivineMarrowName)
            || !string.IsNullOrWhiteSpace(s.WorldSoulName);
        return hasAncientLineage && !hasNewLawLineage;
    }

    private static void AppendAncientLineage(List<string> lines, in MclslDeathSnapshot s)
    {
        List<string> phrases = new();
        if (!string.IsNullOrWhiteSpace(s.AncientFoundationName)) phrases.Add("自筑【" + s.AncientFoundationName + "】道基");
        if (!string.IsNullOrWhiteSpace(s.AncientCoreName)) phrases.Add("凝【" + s.AncientCoreName + "】本命金丹");
        if (!string.IsNullOrWhiteSpace(s.AncientNascentName)) phrases.Add("丹破婴生，成【" + s.AncientNascentName + "】元婴");
        if (!string.IsNullOrWhiteSpace(s.AncientDivineIntent)) phrases.Add("神融自身大道【" + s.AncientDivineIntent + "】以化神");
        if (!string.IsNullOrWhiteSpace(s.AncientDaoName)) phrases.Add("以自身之道【" + s.AncientDaoName + "】合于天地");
        if (phrases.Count == 0 && !string.IsNullOrWhiteSpace(s.AncientDaoIntent)) phrases.Add("以【" + s.AncientDaoIntent + "】为本命道意");
        if (phrases.Count == 0) return;
        lines.Add(string.Empty);
        for (int i = 0; i < phrases.Count; i++) lines.Add(phrases[i] + (i == phrases.Count - 1 ? "。" : "，"));
    }

    private static void AppendNewLawLineage(List<string> lines, in MclslDeathSnapshot s)
    {
        List<string> phrases = new();
        if (!string.IsNullOrWhiteSpace(s.FoundationWonderName)) phrases.Add("以奇物【" + s.FoundationWonderName + "】成就道基");
        if (!string.IsNullOrWhiteSpace(s.GoldenCoreLaws)) phrases.Add("以" + FormatLaws(s.GoldenCoreLaws) + "成就金丹");
        string cave = !string.IsNullOrWhiteSpace(s.NascentCaveName) ? s.NascentCaveName : s.NascentEssenceName;
        if (!string.IsNullOrWhiteSpace(cave)) phrases.Add("吞【" + cave + "】之精，以成元婴");
        string change = !string.IsNullOrWhiteSpace(s.DivineChangeName) ? s.DivineChangeName : s.DivineMarrowName;
        if (!string.IsNullOrWhiteSpace(change)) phrases.Add("抽【" + change + "】之髓，以化其神");
        if (!string.IsNullOrWhiteSpace(s.WorldSoulName)) phrases.Add("祭【" + s.WorldSoulName + "】之魄，以身合道");
        if (phrases.Count == 0) return;

        lines.Add(string.Empty);
        for (int i = 0; i < phrases.Count; i++)
        {
            lines.Add(phrases[i] + (i == phrases.Count - 1 ? "。" : "，"));
        }
        if (!string.IsNullOrWhiteSpace(s.WorldSoulName) && !string.IsNullOrWhiteSpace(s.HeavenlyDuty))
        {
            lines.Add("承天职【" + s.HeavenlyDuty + "】。");
        }
    }

    private static string DeathTitle(in MclslDeathSnapshot s)
    {
        return s.RealmId switch
        {
            MclslRealmIds.LianQi => "炼气修士陨落",
            MclslRealmIds.ZhuJi => "筑基修士陨落",
            MclslRealmIds.JinDan => "金丹修士陨落",
            MclslRealmIds.YuanYing => "元婴修士陨落",
            MclslRealmIds.HuaShen => DeathHonorific(s, IsAncientDeath(s) ? "化神神君" : "化神仙君") + "陨落",
            MclslRealmIds.HeDao => DeathHonorific(s, IsAncientDeath(s) ? "合道道君" : "合道仙尊") + "陨落",
            MclslRealmIds.ChangSheng => DeathHonorific(s, IsAncientDeath(s) ? "古道道祖" : "无名天尊") + "陨落",
            _ => "修士陨落"
        };
    }

    private static string DeathSubject(in MclslDeathSnapshot s)
    {
        string name = BaseDeathName(s.ActorName);
        if (MclslRealmIds.Index(s.RealmId) >= MclslRealmIds.Index(MclslRealmIds.HuaShen))
        {
            bool ancientLaw = IsAncientDeath(s);
            string fallback = s.RealmId == MclslRealmIds.HuaShen ? (ancientLaw ? "神君" : "仙君")
                : s.RealmId == MclslRealmIds.HeDao ? (ancientLaw ? "道君" : "仙尊")
                : (ancientLaw ? "道祖" : "天尊");
            return DeathHonorific(s, fallback) + "·" + name;
        }
        return MclslRealmIds.Display(s.RealmId) + "修士" + name;
    }

    private static string LongevityTitle(in MclslDeathSnapshot s) =>
        DeathHonorific(s, IsAncientDeath(s) ? "古道道祖" : "无名天尊");

    private static string DeathHonorific(in MclslDeathSnapshot s, string fallback)
    {
        string actorName = string.IsNullOrWhiteSpace(s.ActorName) ? string.Empty : s.ActorName.Trim();
        int mark = actorName.IndexOf('·');
        if (mark > 0) return actorName[..mark];

        if (!IsAncientDeath(s) && s.RealmId == MclslRealmIds.HeDao)
        {
            string title = MclslHonorificNameCatalog.WorldSoulHonorific(string.Empty, s.WorldSoulName);
            if (!string.IsNullOrWhiteSpace(title)) return title;
        }
        if (!IsAncientDeath(s) && s.RealmId == MclslRealmIds.ChangSheng)
        {
            string title = MclslHonorificNameCatalog.InverseTruthHonorific(string.Empty, s.InverseTruthName);
            if (!string.IsNullOrWhiteSpace(title)) return title;
        }
        return fallback;
    }

    private static string DeathVerb(in MclslDeathSnapshot s)
    {
        int index = MclslRealmIds.Index(s.RealmId);
        return index >= MclslRealmIds.Index(MclslRealmIds.HuaShen) ? "陨落" : "身死";
    }

    private static string DeathCausePhrase(string code, in MclslDeathSnapshot s, string causeText)
    {
        if (!string.IsNullOrWhiteSpace(s.PendingDetail))
        {
            return TrimSentence(s.PendingDetail);
        }
        return code switch
        {
            "battle" => string.IsNullOrWhiteSpace(s.KillerName) ? "争斗伤重" : "为" + s.KillerName + "所杀",
            "old_age" => "寿元耗尽",
            "starvation" => "灵机断绝、形神俱疲",
            "ruin_exploration" => "深入“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "无名宗门遗迹" : s.PendingSource) + "”未归",
            "world_change_backlash" => "强抽“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "天地之变" : s.PendingSource) + "”之髓，遭天地反噬",
            "world_soul_backlash" => "承接“" + (string.IsNullOrWhiteSpace(s.PendingSource) ? "天地之魄" : s.PendingSource) + "”后天职反噬",
            "mortal_miasma" => "仙凡瘴反噬修为与形神",
            "drowning" => "水厄断息",
            "fire" => "烈焰焚身",
            "disease" => "疫病侵入形神",
            "poison" => "毒性侵透经脉与神魂",
            "eaten" => "为凶兽所噬",
            "acid" => "强酸腐蚀形神",
            "gravity" => "坠落重创",
            "metamorphosis" => "肉身异变失控",
            _ => string.IsNullOrWhiteSpace(causeText) ? "死因未明" : TrimSentence(causeText)
        };
    }

    private static string FormatLaws(string laws)
    {
        string[] parts = (laws ?? string.Empty)
            .Split(new[] { ',', '，', '、', '|', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (parts.Length == 0) return "无名法则";
        if (parts.Length == 1)
        {
            string single = parts[0];
            return single.EndsWith("法", StringComparison.Ordinal) ? single : single + "法";
        }
        return string.Join("、", parts) + ChineseNumber(parts.Length) + "法";
    }

    private static string TrimSentence(string value) => (value ?? string.Empty).Trim().TrimEnd('。', '！', '!', '.', '，', ',');

    private static string BaseDeathName(string value)
    {
        string name = string.IsNullOrWhiteSpace(value) ? "无名修士" : value.Trim();
        int mark = name.LastIndexOf('·');
        if (mark >= 0 && mark + 1 < name.Length) name = name[(mark + 1)..];
        foreach (string realm in MclslRealmIds.Ordered)
        {
            string suffix = "-" + MclslRealmIds.Display(realm);
            if (name.EndsWith(suffix, StringComparison.Ordinal)) return name[..^suffix.Length];
        }
        return name;
    }

    private static string ChineseYears(int years)
    {
        if (years <= 0) return "未满一";
        return ChineseNumber(years);
    }

    private static string ChineseNumber(int value)
    {
        if (value <= 0) return "零";
        string[] digits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        if (value < 10) return digits[value];
        if (value == 10) return "十";
        if (value < 20) return "十" + digits[value % 10];
        if (value < 100)
        {
            int tens = value / 10;
            int ones = value % 10;
            return digits[tens] + "十" + (ones == 0 ? string.Empty : digits[ones]);
        }
        if (value < 10000)
        {
            int thousands = value / 1000;
            int hundreds = value / 100 % 10;
            int tens = value / 10 % 10;
            int ones = value % 10;
            List<string> parts = new();
            if (thousands > 0) parts.Add(digits[thousands] + "千");
            if (hundreds > 0) parts.Add(digits[hundreds] + "百");
            else if (thousands > 0 && (tens > 0 || ones > 0)) parts.Add("零");
            if (tens > 0) parts.Add((tens == 1 && thousands == 0 && hundreds == 0 ? string.Empty : digits[tens]) + "十");
            else if ((thousands > 0 || hundreds > 0) && ones > 0 && parts.Count > 0 && parts[^1] != "零") parts.Add("零");
            if (ones > 0) parts.Add(digits[ones]);
            return string.Join(string.Empty, parts);
        }
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string BuildPlace(in MclslDeathSnapshot s)
    {
        if (s.KingdomName == "无国" && s.CityName == "无城") return string.Empty;
        if (s.CityName == "无城") return s.KingdomName;
        if (s.KingdomName == "无国") return s.CityName;
        return s.KingdomName + "·" + s.CityName;
    }


    private static string DeathColor(string causeCode) => causeCode switch
    {
        "old_age" => "#CBBF9A",
        "drowning" => "#5C92C9",
        "fire" => "#D97845",
        "disease" => "#84A06A",
        "ruin_exploration" => "#C06A5A",
        "world_change_backlash" => "#B44ED1",
        "world_soul_backlash" => "#D0B067",
        _ => "#D95B5B"
    };

    private static bool ShouldShowTopDeathAnnouncement(MclslDeathRecord record)
    {
        if (record == null || !record.Surfaced) return false;
        int realmIndex = MclslRealmIds.Index(record.RealmId);
        int configuredIndex = Math.Max(0, MclslRuntimeSettings.DeathPopupMinRealm - 1);
        return realmIndex >= configuredIndex;
    }

    private static float DeathDuration(string causeCode)
    {
        return causeCode == "ruin_exploration" || causeCode == "world_change_backlash" || causeCode == "world_soul_backlash" ? 10f : 8f;
    }

    private static void ClearPendingDeath(Actor actor)
    {
        if (actor?.data == null) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathCode, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathSource, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathDetail, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.PendingDeathImportant, 0);
    }

    private static string TryGetKillerName(Actor actor)
    {
        object[] sources = { actor, actor?.data };
        for (int s = 0; s < sources.Length; s++)
        {
            object source = sources[s];
            if (source == null) continue;
            Type type = source.GetType();
            for (int i = 0; i < KillerMemberNames.Length; i++)
            {
                string name = KillerMemberNames[i];
                try
                {
                    FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    object value = field?.GetValue(source);
                    if (value is Actor fieldActor && fieldActor != actor) return SafeName(fieldActor);
                    PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    value = property?.GetValue(source);
                    if (value is Actor propertyActor && propertyActor != actor) return SafeName(propertyActor);
                }
                catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Death-MclslDeathSystem-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Death/MclslDeathSystem.cs #4: " + mclslEmptyCatchEx.Message); }
            }
        }
        return string.Empty;
    }

    private static bool IsAlive(Actor actor)
    {
        try { return actor?.data != null && actor.isAlive(); } catch { return false; }
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); } catch { return "无名修士"; }
    }

    private static int SafeAge(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); } catch { return 0; }
    }

    private static int SafeCultivationYears(Actor actor)
    {
        int year = MclslRuntime.CurrentYear();
        int start = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0);
        if (start > 0 && year >= start) return Math.Max(0, year - start);
        return SafeAge(actor);
    }
}
