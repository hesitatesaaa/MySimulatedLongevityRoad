using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ai;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.Systems.Visual;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslSpellDefinition
{
    internal string Id;
    internal string Name;
    internal string Law;
    internal int MinRealm;
    internal int ManaCost;
    internal float Cooldown;
    internal float Power;
    internal int MaxTargets;
    internal string Description;
    internal string IconPath => "ui/Spells/" + Id;
}

internal static class MclslSpellSystem
{
    [ThreadStatic] private static bool _applyingSpellDamage;
    internal static bool IsApplyingSpellDamage => _applyingSpellDamage;
    private sealed class RuntimeState
    {
        internal double LastManaTime;
        internal float Remainder;
        internal readonly Dictionary<string, float> NextCast = new(StringComparer.Ordinal);
        internal string KnownRaw;
        internal MclslSpellDefinition[] KnownSpells = Array.Empty<MclslSpellDefinition>();
        internal long ManaRevision = long.MinValue;
        internal int MaximumMana;
    }

    private static ConditionalWeakTable<Actor, RuntimeState> _runtime = new();
    private static readonly string[] SelfSpellPriority = { "S025", "S024", "S020", "S018", "S014", "S011", "S005", "S006", "S003", "S002" };
    private static int _selfScanIndex;
    private static double _lastSelfScanWorldTime = -1d;
    internal static readonly MclslSpellDefinition[] All =
    {
        Spell("S001", "金芒剑气", "金", 0, 18, 4f, "金色剑气直击一敌。"),
        Spell("S002", "青木回春", "木", 0, 24, 12f, "青木生机疗愈自身。"),
        Spell("S003", "水镜护身", "水", 0, 25, 14f, "水镜短暂护身。"),
        Spell("S004", "赤焰术", "火", 0, 22, 6f, "赤焰灼烧一敌。"),
        Spell("S005", "厚土壁", "土", 1, 35, 16f, "厚土成壁，防护自身。"),
        Spell("S006", "御风行", "风", 1, 32, 20f, "御风疾行。"),
        Spell("S007", "雷引诀", "雷", 1, 42, 9f, "雷光轰击一敌。"),
        Spell("S008", "玄阴缚", "阴", 2, 58, 18f, "玄阴法力缠缚敌人。"),
        Spell("S009", "阳华破障", "阳", 2, 60, 16f, "阳华之光破敌护障。"),
        Spell("S010", "五行轮转", "金", 3, 90, 22f, "五行轮转，重创敌人。"),
        Spell("S011", "咫尺遁光", "空间", 4, 120, 30f, "遁光护身，短时疾行。"),
        Spell("S012", "万象归一", "空间", 5, 180, 45f, "万象归一，镇压一敌。"),
        Spell("S013", "霜针诀", "水", 0, 20, 6f, "霜针伤敌，短暂减速。", 1.05f),
        Spell("S014", "纳灵术", "木", 0, 8, 18f, "纳天地灵息，恢复少量灵力。"),
        Spell("S015", "巽风刃", "风", 1, 38, 10f, "风刃连击至多两敌。", 0.9f, 2),
        Spell("S016", "青藤缚", "木", 1, 36, 16f, "青藤伤敌并束缚。", 0.9f),
        Spell("S017", "紫霄破甲", "雷", 2, 60, 14f, "雷光破甲，重击一敌。", 1.35f),
        Spell("S018", "灵泉归元", "水", 2, 52, 24f, "疗愈自身并回转少量灵力。"),
        Spell("S019", "离火轮", "火", 3, 92, 18f, "离火轮灼击至多四敌。", 1.2f, 4),
        Spell("S020", "山岳护体", "土", 3, 80, 24f, "山岳法力增强自身护甲。"),
        Spell("S021", "太阴摄魂", "阴", 4, 120, 25f, "太阴重创一敌并使其迟缓。", 1.6f),
        Spell("S022", "星移剑阵", "空间", 4, 130, 24f, "剑阵攻击至多五敌。", 1.35f, 5),
        Spell("S023", "乾坤镇域", "空间", 5, 185, 32f, "镇域攻击至多六敌并使其迟缓。", 1.4f, 6),
        Spell("S024", "五行化生", "木", 5, 150, 28f, "五行化生，恢复生命与灵力。"),
        Spell("S025", "万灵回天", "木", 6, 250, 42f, "疗愈自身与附近至多五名友方。", 1f, 6),
        Spell("S026", "太初寂光", "阳", 6, 280, 45f, "寂光攻击至多八敌并短暂压制。", 2f, 8)
    };
    private static readonly Dictionary<string, MclslSpellDefinition> ById = All.ToDictionary(x => x.Id, StringComparer.Ordinal);

    private static MclslSpellDefinition Spell(string id, string name, string law, int realm, int mana, float cooldown,
        string description, float power = 0f, int maxTargets = 1)
        => new() { Id = id, Name = name, Law = law, MinRealm = realm, ManaCost = mana,
            Cooldown = cooldown, Description = description, Power = power > 0f ? power : 1f + 0.1f * realm,
            MaxTargets = maxTargets };

    internal static bool TryGet(string id, out MclslSpellDefinition spell)
        => ById.TryGetValue(id ?? string.Empty, out spell);

    internal static void ClearRuntime()
    {
        _runtime = new ConditionalWeakTable<Actor, RuntimeState>();
        _selfScanIndex = 0;
        _lastSelfScanWorldTime = -1d;
        MclslSpellProgression.ClearRuntime();
        MclslSpellTerrain.Clear();
    }

    internal static int MaxMana(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.IsCultivator(actor)) return 0;
        RuntimeState state = _runtime.GetOrCreateValue(actor);
        long revision = MclslRuntimeChanges.DataRevision(MclslActorAccessor.Id(actor));
        if (state.ManaRevision == revision) return state.MaximumMana;
        state.MaximumMana = ComputeMaxMana(actor);
        state.ManaRevision = revision;
        return state.MaximumMana;
    }

    private static int ComputeMaxMana(Actor actor)
    {
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        int baseline = realm switch { 0 => 100, 1 => 180, 2 => 320, 3 => 560, 4 => 900, 5 => 1450, _ => 2200 };
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        int maximum = Math.Max(1, baseline + baseline * (aptitude - 50) / 500);
        int roots = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritualRootCount, 1), 1, 5);
        maximum += baseline * (roots - 1) / 25;
        int purity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCorePurity, 50), 0, 100);
        if (realm >= 2) maximum += baseline * (purity - 50) / 1000;
        if (MclslCultivationCatalog.TryTechnique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId), out MclslTechniqueDefinition technique))
        {
            string primary = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootPrimary);
            if (Array.IndexOf(technique.LawPool, primary) >= 0) maximum += baseline / 10;
        }
        if (MclslArtifactSystem.EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Amulet) == "B081")
            maximum += baseline / 2;
        return actor.hasTrait(MclslTraitRegistration.BaiTraitId) || actor.hasTrait(MclslTraitRegistration.ChuanfaTraitId)
            ? maximum * 10 : maximum;
    }

    internal static int CurrentMana(Actor actor)
    {
        int maximum = MaxMana(actor);
        if (maximum == 0) return 0;
        RuntimeState state = _runtime.GetOrCreateValue(actor);
        double now = World.world == null ? 0d : Convert.ToDouble(World.world.getCurWorldTime());
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManaInitialized) != 1)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaInitialized, 1);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, maximum);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaLastWorldTime, now.ToString("R", CultureInfo.InvariantCulture));
            return maximum;
        }
        int storedMana = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManaCurrent);
        int current = Math.Clamp(storedMana, 0, maximum);
        if (current != storedMana) MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, current);
        if (state.LastManaTime <= 0d)
        {
            string storedTime = MclslActorAccessor.GetString(actor, MclslActorDataKeys.ManaLastWorldTime);
            state.LastManaTime = double.TryParse(storedTime, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? parsed : now;
        }
        float elapsed = (float)Math.Clamp(now - state.LastManaTime, 0d, 3600d);
        state.LastManaTime = now;
        if (elapsed <= 0f || current >= maximum) return current;
        float rate = actor.has_attack_target ? 0.004f : 0.012f;
        if (MclslArtifactSystem.EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Amulet) == "B080") rate *= 1.5f;
        if (actor.hasStatus("mclsl_item_D016")) rate *= 1.25f;
        if (actor.hasStatus("mclsl_item_F007")) rate *= 1.15f;
        float recovery = elapsed * maximum * rate + state.Remainder;
        int gained = Mathf.FloorToInt(recovery);
        state.Remainder = recovery - gained;
        if (gained <= 0) return current;
        current = Math.Min(maximum, current + gained);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, current);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaLastWorldTime, now.ToString("R", CultureInfo.InvariantCulture));
        return current;
    }

    internal static int RestoreMana(Actor actor, float fraction)
    {
        int current = CurrentMana(actor);
        int maximum = MaxMana(actor);
        int restored = Math.Clamp(current + Mathf.CeilToInt(maximum * fraction), 0, maximum);
        if (restored != current) MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, restored);
        if (restored != current) PersistManaTime(actor);
        return restored - current;
    }

    private static void PersistManaTime(Actor actor)
    {
        double now = World.world == null ? 0d : Convert.ToDouble(World.world.getCurWorldTime());
        _runtime.GetOrCreateValue(actor).LastManaTime = now;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaLastWorldTime, now.ToString("R", CultureInfo.InvariantCulture));
    }

    internal static bool Knows(Actor actor, string spellId)
    {
        var known = KnownCached(actor);
        for (int i = 0; i < known.Length; i++) if (known[i].Id == spellId) return true;
        return false;
    }

    internal static List<MclslSpellDefinition> Known(Actor actor)
        => new(KnownCached(actor));

    internal static IReadOnlyList<MclslSpellDefinition> KnownView(Actor actor) => KnownCached(actor);
    internal static void Forget(Actor actor) { if (actor != null) _runtime.Remove(actor); }

    private static MclslSpellDefinition[] KnownCached(Actor actor)
    {
        if (actor?.data == null) return Array.Empty<MclslSpellDefinition>();
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.LearnedSpells, string.Empty);
        RuntimeState state = _runtime.GetOrCreateValue(actor);
        if (string.Equals(stored, state.KnownRaw, StringComparison.Ordinal)) return state.KnownSpells;
        state.KnownRaw = stored;
        if (stored.StartsWith("v1:", StringComparison.Ordinal)) stored = stored.Substring(3);
        List<MclslSpellDefinition> result = new();
        foreach (string id in stored.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            if (ById.TryGetValue(id, out MclslSpellDefinition spell) && !result.Contains(spell)) result.Add(spell);
        result.Sort((a, b) => a.MinRealm != b.MinRealm ? a.MinRealm.CompareTo(b.MinRealm)
            : string.CompareOrdinal(a.Id, b.Id));
        return state.KnownSpells = result.ToArray();
    }

    internal static bool Learn(Actor actor, string spellId)
        => LearnCore(actor, spellId, false);

    internal static bool LearnFromPlayer(Actor actor, string spellId)
        => LearnCore(actor, spellId, true);

    private static bool LearnCore(Actor actor, string spellId, bool bypassLearningRealm)
    {
        if (!MclslActorAccessor.Alive(actor) || !ById.TryGetValue(spellId, out MclslSpellDefinition spell)
            || (!bypassLearningRealm && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm)
            || Knows(actor, spellId)) return false;
        List<MclslSpellDefinition> known = Known(actor);
        known.Add(spell);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LearnedSpells, "v1:" + string.Join(",", known.Select(x => x.Id)));
        MclslSpellProgression.OnLearned(actor, spellId);
        return true;
    }

    internal static bool TryStudyScroll(Actor actor, string itemId)
    {
        if (itemId == null || !itemId.EndsWith("_SCROLL", StringComparison.Ordinal)) return false;
        string spellId = itemId.Substring(0, itemId.Length - "_SCROLL".Length);
        if (!ById.ContainsKey(spellId)) return false;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (MclslBagSystem.Count(bag, itemId) < 1 || !Learn(actor, spellId)) return false;
        if (!MclslBagSystem.Remove(bag, itemId)) return false;
        MclslBagSystem.Write(actor, bag);
        return true;
    }

    internal static void StudyEligibleOwnedScrolls(Actor actor)
    {
        if (actor?.data == null) return;
        foreach (MclslSpellDefinition spell in All)
            if (!Knows(actor, spell.Id) && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= spell.MinRealm)
                TryStudyScroll(actor, spell.Id + "_SCROLL");
    }

    internal static void TryProgressAnnual(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        StudyEligibleOwnedScrolls(actor);
        MclslSpellProgression.ProgressAnnual(actor);
        if (MaxMana(actor) <= 0) return;
        if (MclslAncientMentorshipSystem.TryGetTeacher(actor, out Actor teacher))
        {
            MclslMentorshipGiftSystem.TryGift(actor, teacher, year);
            if (MclslSpellAcquisitionPolicy.Succeeds(StableRoll(actor, year, "teacher")))
            {
                MclslSpellDefinition[] teacherSpells = KnownCached(teacher);
                MclslSpellDefinition[] known = KnownCached(actor);
                int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
                int[] priorities = new int[teacherSpells.Length];
                for (int i = 0; i < teacherSpells.Length; i++)
                    priorities[i] = MclslSpellAcquisitionPolicy.TeacherPriority(realm,
                        teacherSpells[i].MinRealm, Array.IndexOf(known, teacherSpells[i]) >= 0);
                int requested = MclslSpellAcquisitionPolicy.RequestedCount(StableRoll(actor, year, "teacher_count"));
                int[] picks = MclslSpellAcquisitionPolicy.SelectIndices(priorities, requested,
                    slot => StableRoll(actor, year, "teacher_pick_" + slot));
                foreach (int index in picks) Learn(actor, teacherSpells[index].Id);
            }
        }
        MclslSpellScrollCrafting.ProgressAnnual(actor, year);
    }

    internal static void OnRealmAdvanced(Actor actor, string oldRealm, string newRealm)
    {
        StudyEligibleOwnedScrolls(actor);
        int previous = MclslRealmIds.Index(oldRealm);
        int current = MclslRealmIds.Index(newRealm);
        if (previous < 0 || current <= previous || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastSpellInsightRealm, -1) >= current) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastSpellInsightRealm, current);
        int year = MclslRuntime.CurrentYear();
        if (!MclslSpellAcquisitionPolicy.Succeeds(StableRoll(actor, year, "realm_insight_" + current))) return;
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId));
        MclslSpellDefinition[] knownSpells = KnownCached(actor);
        int[] realmPriorities = new int[All.Length];
        for (int i = 0; i < All.Length; i++)
        {
            MclslSpellDefinition spell = All[i];
            realmPriorities[i] = MclslSpellAcquisitionPolicy.RealmPriority(current, spell.MinRealm,
                Array.IndexOf(knownSpells, spell) >= 0, Array.IndexOf(roots, spell.Law) >= 0,
                Array.IndexOf(technique.LawPool, spell.Law) >= 0);
        }
        int realmRequested = MclslSpellAcquisitionPolicy.RequestedCount(
            StableRoll(actor, year, "realm_count_" + current));
        int[] realmPicks = MclslSpellAcquisitionPolicy.SelectIndices(realmPriorities, realmRequested,
            slot => StableRoll(actor, year, "realm_pick_" + current + "_" + slot));
        foreach (int index in realmPicks) Learn(actor, All[index].Id);
    }

    private static int StableRoll(Actor actor, int year, string salt)
    {
        unchecked
        {
            int hash = 23;
            string digits = MclslActorAccessor.Id(actor).ToString(CultureInfo.InvariantCulture);
            for (int i = 0; i < digits.Length; i++) hash = hash * 37 + digits[i];
            hash = hash * 37 + '|';
            digits = year.ToString(CultureInfo.InvariantCulture);
            for (int i = 0; i < digits.Length; i++) hash = hash * 37 + digits[i];
            hash = hash * 37 + '|';
            for (int i = 0; i < salt.Length; i++) hash = hash * 37 + salt[i];
            return (hash & int.MaxValue) % 100;
        }
    }

    internal static void TickSelfCasting(int frameCounter)
    {
        if (frameCounter <= 0 || frameCounter % 10 != 0 || World.world == null) return;
        double worldTime = Convert.ToDouble(World.world.getCurWorldTime());
        if (worldTime <= _lastSelfScanWorldTime) return; // No casting while the simulation is paused.
        _lastSelfScanWorldTime = worldTime;
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        if (actors.Count == 0) { _selfScanIndex = 0; return; }
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            int count = Math.Min(MclslRuntimeWorkBudget.ScaleCount(64, 1), actors.Count);
            MclslPerformanceProbe.RecordCultivatorPollActors(count);
            for (int i = 0; i < count && !MclslFrameDeadline.Expired; i++)
            {
                if (_selfScanIndex >= actors.Count) _selfScanIndex = 0;
                Actor actor = actors[_selfScanIndex++];
                if (!MclslActorAccessor.Alive(actor) || actor.has_attack_target
                    || string.IsNullOrEmpty(MclslActorAccessor.GetString(actor, MclslActorDataKeys.LearnedSpells))) continue;
                TryCastSelf(actor);
            }
        }
        finally { MclslPerformanceProbe.End("法术.脱战轮询", sample); }
    }

    private static bool TryCastSelf(Actor actor)
    {
        MclslSpellDefinition[] known = KnownCached(actor);
        if (known.Length == 0) return false;
        float maxHealth = actor.getMaxHealth();
        if (maxHealth <= 0f) return false;
        float healthFraction = actor.getHealth() / maxHealth;
        RuntimeState state = _runtime.GetOrCreateValue(actor);
        int mana = CurrentMana(actor);
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        foreach (string id in SelfSpellPriority)
        {
            MclslSpellDefinition spell = null;
            for (int i = 0; i < known.Length; i++)
                if (known[i].Id == id) { spell = known[i]; break; }
            if (spell == null || realm < spell.MinRealm || mana < spell.ManaCost
                || state.NextCast.TryGetValue(id, out float next) && Time.time < next) continue;
            bool needed = id switch
            {
                "S025" => healthFraction < 0.9f || HasInjuredAlly(actor),
                "S024" => healthFraction < 0.65f || mana < MaxMana(actor) * 0.35f,
                "S020" => healthFraction < 0.75f && !actor.hasStatus("mclsl_spell_mountain_guard"),
                "S018" => healthFraction < 0.7f,
                "S014" => mana < MaxMana(actor) * 0.35f,
                "S002" => healthFraction < 0.80f,
                "S003" => healthFraction < 0.75f && !actor.hasStatus("mclsl_spell_water_guard"),
                "S005" => healthFraction < 0.75f && !actor.hasStatus("mclsl_spell_earth_guard"),
                "S006" => healthFraction < 0.55f && !actor.hasStatus("mclsl_spell_wind_step"),
                "S011" => healthFraction < 0.55f && !actor.hasStatus("mclsl_spell_void_step"),
                _ => false
            };
            if (!needed) continue;
            CommitCast(actor, state, spell, mana);
            ApplySelfEffect(actor, id);
            return true;
        }
        return false;
    }

    private static void ApplySelfEffect(Actor actor, string spellId)
    {
        float strength = MclslSpellProgression.Strength(actor, spellId);
        switch (spellId)
        {
            case "S002": actor.restoreHealthPercent(0.18f * strength); break;
            case "S003": actor.addStatusEffect("mclsl_spell_water_guard", 8f * strength); break;
            case "S005": actor.addStatusEffect("mclsl_spell_earth_guard", 10f * strength); break;
            case "S006": actor.addStatusEffect("mclsl_spell_wind_step", 12f * strength); break;
            case "S011": actor.addStatusEffect("mclsl_spell_void_step", 8f * strength); break;
            case "S014": RestoreManaFlat(actor, Mathf.CeilToInt(Math.Min(MaxMana(actor) * 0.15f, 30f) * strength)); break;
            case "S018": actor.restoreHealthPercent(0.20f * strength); RestoreMana(actor, 0.05f * strength); break;
            case "S020": actor.addStatusEffect("mclsl_spell_mountain_guard", 10f * strength); break;
            case "S024": actor.restoreHealthPercent(0.30f * strength); RestoreMana(actor, 0.10f * strength); break;
            case "S025": HealNearbyAllies(actor, 0.25f * strength, 6); break;
        }
    }

    private static void RestoreManaFlat(Actor actor, int amount)
    {
        int restored = Math.Min(MaxMana(actor), CurrentMana(actor) + Math.Max(0, amount));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, restored);
        PersistManaTime(actor);
    }

    private static bool HasInjuredAlly(Actor actor)
    {
        if (actor.current_tile == null) return false;
        foreach (Actor candidate in Finder.getUnitsFromChunk(actor.current_tile, 1, 3f, false))
            if (IsFriendly(actor, candidate) && candidate.getMaxHealth() > 0f
                && candidate.getHealth() / candidate.getMaxHealth() < 0.65f) return true;
        return false;
    }

    private static void HealNearbyAllies(Actor actor, float fraction, int limit)
    {
        actor.restoreHealthPercent(fraction);
        if (actor.current_tile == null) return;
        int healed = 1;
        foreach (Actor candidate in Finder.getUnitsFromChunk(actor.current_tile, 1, 3f, false))
        {
            if (candidate == actor || !IsFriendly(actor, candidate) || candidate.getMaxHealth() <= 0f
                || candidate.getHealth() >= candidate.getMaxHealth()) continue;
            candidate.restoreHealthPercent(fraction);
            if (++healed >= limit) break;
        }
    }

    private static bool IsFriendly(Actor actor, Actor candidate)
        => MclslActorAccessor.Alive(candidate) && actor.kingdom != null && candidate.kingdom == actor.kingdom;

    private static void CommitCast(Actor actor, RuntimeState state, MclslSpellDefinition spell, int mana,
        Actor target = null, WorldTile hitTile = null)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, mana - spell.ManaCost);
        PersistManaTime(actor);
        state.NextCast[spell.Id] = Time.time + spell.Cooldown;
        MclslSpellProgression.RecordCast(actor, spell, MclslRuntime.CurrentYear());
        // Visual anchoring is separate from the gameplay target. Area attacks
        // strike around the selected enemy but play once on the caster.
        bool areaAttack = IsAreaAttack(spell.Id);
        bool shown = MclslSpellEffect.Show(actor, areaAttack ? null : target,
            spell.Id, areaAttack ? null : hitTile);
        if (target != null && (spell.Id is "S019" or "S022" or "S023" or "S026"))
            MclslSpellTerrain.Queue(hitTile ?? target.current_tile, spell.Id);
        if (MclslDiagnostics.Enabled)
            MclslDiagnostics.Throttle("spell-cast-" + spell.Id, Time.frameCount, 3600,
                "法术施放 " + spell.Id + "，修士=" + MclslActorAccessor.Id(actor)
                + "，灵力=" + (mana - spell.ManaCost) + "，特效=" + (shown ? "已生成" : "未生成"));
    }

    private static bool IsAreaAttack(string spellId) => spellId is
        "S010" or "S012" or "S015" or "S019" or "S022" or "S023" or "S026";

    internal static void TryCastCombat(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor) || actor.attack_target is not Actor target || !MclslActorAccessor.Alive(target)) return;
        string learnedSpells = MclslActorAccessor.GetString(actor, MclslActorDataKeys.LearnedSpells, string.Empty);
        if (string.IsNullOrWhiteSpace(learnedSpells))
        {
            MclslPerformanceProbe.RecordCombatSpellFastReject();
            return;
        }
        if (TryCastSelf(actor)) return;
        RuntimeState state = _runtime.GetOrCreateValue(actor);
        int mana = CurrentMana(actor);
        MclslSpellDefinition[] known = KnownCached(actor);
        if (known.Length == 0 && MclslDiagnostics.Enabled)
            MclslDiagnostics.Throttle("spell-no-known", Time.frameCount, 3600,
                "战斗中的修士尚未学会法术，修士=" + MclslActorAccessor.Id(actor));
        foreach (MclslSpellDefinition spell in known)
        {
            if (Array.IndexOf(SelfSpellPriority, spell.Id) >= 0) continue;
            if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm || mana < spell.ManaCost
                || state.NextCast.TryGetValue(spell.Id, out float next) && Time.time < next) continue;
            WorldTile hitTile = target.current_tile;
            if (!ApplyCombatEffect(actor, target, spell)) continue;
            CommitCast(actor, state, spell, mana, target, hitTile);
            return;
        }
        if (known.Length > 0 && MclslDiagnostics.Enabled)
        {
            int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
            bool hasOffensive = false;
            bool gradeAllows = false;
            bool manaAllows = false;
            bool cooldownAllows = false;
            foreach (MclslSpellDefinition spell in known)
            {
                if (Array.IndexOf(SelfSpellPriority, spell.Id) >= 0) continue;
                hasOffensive = true;
                if (realm < spell.MinRealm) continue;
                gradeAllows = true;
                if (mana < spell.ManaCost) continue;
                manaAllows = true;
                if (!state.NextCast.TryGetValue(spell.Id, out float next) || Time.time >= next)
                    cooldownAllows = true;
            }
            string reason = !hasOffensive ? "仅掌握自用法术，当前无需施放"
                : !gradeAllows ? "境界不足" : !manaAllows ? "灵力不足"
                : !cooldownAllows ? "仍在冷却" : "攻击条件不满足";
            MclslDiagnostics.Throttle("spell-no-cast-" + reason, Time.frameCount, 3600,
                "战斗法术未施放：" + reason + "；修士=" + MclslActorAccessor.Id(actor));
        }
    }

    private static bool ApplyCombatEffect(Actor actor, Actor target, MclslSpellDefinition spell)
    {
        float strength = MclslSpellProgression.Strength(actor, spell.Id);
        float damage = Math.Max(1f, actor.stats.get("damage")) * spell.Power * strength;
        if (spell.Law == "火") damage *= 1f + MclslPhysiqueSystem.FireDamagePercent(actor) / 100f;
        switch (spell.Id)
        {
            case "S008": target.addStatusEffect("mclsl_spell_yin_bind", ControlDuration(2f, strength)); break;
            case "S007": target.addStatusEffect("mclsl_spell_shock", ControlDuration(1.5f, strength)); break;
            case "S009": actor.restoreHealthPercent(0.05f * strength); break;
            case "S010": return ApplyAreaSpell(actor, target, damage * 1.5f, 8);
            case "S012": return ApplyAreaSpell(actor, target, damage * 2f, 8, "mclsl_spell_all_bind", ControlDuration(3f, strength));
            case "S013": target.addStatusEffect("mclsl_spell_frost_slow", ControlDuration(1f, strength)); break;
            case "S015": return ApplyAreaSpell(actor, target, damage, 2);
            case "S016": target.addStatusEffect("mclsl_spell_vine_bind", ControlDuration(2f, strength)); break;
            case "S017": target.addStatusEffect("mclsl_spell_armor_break", ControlDuration(4f, strength)); break;
            case "S019": return ApplyAreaSpell(actor, target, damage, 4);
            case "S021": target.addStatusEffect("mclsl_spell_soul_slow", ControlDuration(3f, strength)); break;
            case "S022": return ApplyAreaSpell(actor, target, damage, 5);
            case "S023": return ApplyAreaSpell(actor, target, damage, 6, "mclsl_spell_domain_slow", ControlDuration(2.5f, strength));
            case "S026": return ApplyAreaSpell(actor, target, damage, 8, "mclsl_spell_origin_slow", ControlDuration(1.5f, strength));
        }
        _applyingSpellDamage = true;
        try { target.getHit(damage, true, spell.Id == "S004" ? AttackType.Fire : AttackType.Divine, actor); }
        finally { _applyingSpellDamage = false; }
        return true;
    }

    private static float ControlDuration(float duration, float strength) => Math.Min(4.5f, duration * strength);

    private static bool ApplyAreaSpell(Actor caster, Actor mainTarget, float damage, int limit,
        string status = null, float statusDuration = 0f)
    {
        WorldTile center = mainTarget.current_tile;
        if (center == null) return false;
        int hits = 0;
        foreach (Actor candidate in Finder.getUnitsFromChunk(center, 1, 3f, false))
        {
            if (!MclslActorAccessor.Alive(candidate) || candidate == caster) continue;
            if (candidate != mainTarget && candidate.attack_target != caster
                && (mainTarget.kingdom == null || candidate.kingdom != mainTarget.kingdom)) continue;
            if (status != null) candidate.addStatusEffect(status, statusDuration);
            _applyingSpellDamage = true;
            try { candidate.getHit(damage, true, AttackType.Divine, caster); }
            finally { _applyingSpellDamage = false; }
            if (++hits >= Math.Min(8, limit)) break;
        }
        return hits > 0;
    }

    internal static void RegisterStatuses()
    {
        Status("mclsl_spell_water_guard", "S003", 8f, "armor", 12f);
        Status("mclsl_spell_earth_guard", "S005", 10f, "armor", 20f);
        Status("mclsl_spell_wind_step", "S006", 12f, "multiplier_speed", 0.2f);
        Status("mclsl_spell_void_step", "S011", 8f, "multiplier_speed", 0.4f);
        Status("mclsl_spell_yin_bind", "S008", 2f, "multiplier_speed", -0.3f);
        Status("mclsl_spell_all_bind", "S012", 3f, "multiplier_speed", -0.4f);
        Status("mclsl_spell_shock", "S007", 1.5f, "multiplier_speed", -0.5f);
        Status("mclsl_spell_frost_slow", "S013", 1f, "multiplier_speed", -0.2f);
        Status("mclsl_spell_vine_bind", "S016", 2f, "multiplier_speed", -0.4f);
        Status("mclsl_spell_armor_break", "S017", 4f, "armor", -10f);
        Status("mclsl_spell_mountain_guard", "S020", 10f, "armor", 25f);
        Status("mclsl_spell_soul_slow", "S021", 3f, "multiplier_speed", -0.35f);
        Status("mclsl_spell_domain_slow", "S023", 2.5f, "multiplier_speed", -0.4f);
        Status("mclsl_spell_origin_slow", "S026", 1.5f, "multiplier_speed", -0.5f);
    }

    private static void Status(string id, string spellId, float duration, string stat, float value)
    {
        if (AssetManager.status.get(id) != null) return;
        StatusAsset status = new() { id = id, duration = duration, base_stats = new BaseStats(),
            path_icon = ById[spellId].IconPath, locale_id = spellId, locale_description = spellId + " Description" };
        MclslItemUseSystem.TrySetStat(status.base_stats, stat, value);
        AssetManager.status.add(status);
    }
}

internal static class MclslSpellTerrain
{
    private const int MaxPending = 64;
    private static readonly (int X, int Y)[] Offsets = { (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1) };
    private static readonly Queue<(MapBox World, int X, int Y, int Index, string Spell)> Pending = new();

    internal static void Queue(WorldTile center, string spell)
    {
        if (center == null || Pending.Count >= MaxPending || World.world == null) return;
        Pending.Enqueue((World.world, center.pos.x, center.pos.y, 0, spell));
    }

    internal static void Tick()
    {
        int budget = MclslRuntimeWorkBudget.ScaleCount(8, 2);
        while (budget-- > 0 && Pending.Count > 0)
        {
            var task = Pending.Dequeue();
            if (!ReferenceEquals(task.World, World.world)) continue;
            var offset = Offsets[task.Index];
            int x = task.X + offset.X, y = task.Y + offset.Y;
            if (x >= 0 && y >= 0 && x < MapBox.width && y < MapBox.height)
            {
                try
                {
                    WorldTile tile = task.World.GetTileSimple(x, y);
                    // pDamage=true invokes the native Gaia's Covenant check.
                    if (tile != null && MapAction.checkTileDamageGaiaCovenant(tile, true))
                    {
                        TerraformOptions options = new() { damage = 0 };
                        if (task.Spell == "S019" || task.Spell == "S026")
                        {
                            options.set_fire = true;
                            options.add_burned = true;
                        }
                        MapAction.decreaseTile(tile, true, options);
                    }
                }
                catch (Exception ex) { MclslDiagnostics.Throttle("spell-terrain", Time.frameCount, 3600, ex.Message); }
            }
            if (++task.Index < Offsets.Length) Pending.Enqueue(task);
        }
    }

    internal static void Clear() => Pending.Clear();
}


internal sealed class MclslSpellEffect : MonoBehaviour
{
    private const float VisualScale = 18f;
    private const int MaxVisibleEffects = 96;
    private static readonly Stack<MclslSpellEffect> Pool = new();
    private static int _activeCount;
    private static Camera _camera;
    private static int _nextCameraRefreshFrame;
    private SpriteRenderer _renderer;
    private float _end;
    private float _born;
    private float _duration;
    private int _frameCount;
    private string _spellId;
    private Actor _caster;
    private Actor _target;
    private Vector3 _anchor;
    private bool _active;

    internal static bool Show(Actor caster, Actor target, string spellId, WorldTile hitTile = null)
    {
        if (caster?.data == null) { ReportSuppressed("施法者无效", spellId); return false; }
        if (_activeCount >= MaxVisibleEffects) { ReportSuppressed("同屏特效达到上限", spellId); return false; }
        if (!TryPosition(target ?? caster, out Vector3 position))
        {
            if (hitTile == null) { ReportSuppressed("无法取得世界坐标", spellId); return false; }
            position = hitTile.posV3;
        }
        if (_camera == null || Time.frameCount >= _nextCameraRefreshFrame)
        {
            _camera = Camera.main;
            _nextCameraRefreshFrame = Time.frameCount + 120;
        }
        Camera camera = _camera;
        if (camera != null)
        {
            Vector3 viewport = camera.WorldToViewportPoint(position);
            float xMargin = 64f / Math.Max(1, Screen.width);
            float yMargin = 64f / Math.Max(1, Screen.height);
            if (viewport.z <= 0f || viewport.x < -xMargin || viewport.x > 1f + xMargin
                || viewport.y < -yMargin || viewport.y > 1f + yMargin)
            {
                ReportSuppressed("施法者在镜头外", spellId);
                return false;
            }
        }
        Sprite sprite = MclslSpellEffectFrames.Get(spellId, 0);
        if (sprite == null) { ReportSuppressed("贴图无效", spellId); return false; }
        MclslSpellEffect component = null;
        while (Pool.Count > 0 && component == null) component = Pool.Pop();
        if (component == null)
        {
            GameObject effect = new("MclslSpellEffect");
            effect.hideFlags = HideFlags.DontSave;
            if (World.world != null) effect.transform.SetParent(((Component)World.world).transform, true);
            component = effect.AddComponent<MclslSpellEffect>();
            component._renderer = effect.AddComponent<SpriteRenderer>();
        }
        if (!MclslWorldSpriteRenderLayer.TryConfigureFrontEffect(component._renderer))
        {
            component._renderer.sortingLayerID = SortingLayer.NameToID("Objects");
            component._renderer.sortingOrder = 101;
            ReportSuppressed("原生特效层暂不可用，已使用世界物体层", spellId);
        }
        component.gameObject.SetActive(true);
        component._renderer.sprite = sprite;
        component._renderer.color = Color.white;
        component._born = Time.time;
        component._duration = DurationFor(spellId);
        component._frameCount = MclslSpellEffectFrames.Count(spellId);
        component._end = Time.time + component._duration;
        component._spellId = spellId;
        component._caster = caster;
        component._target = target;
        component._anchor = position;
        component._active = true;
        component.transform.position = position + new Vector3(0f, 0.38f, -0.01f);
        component.transform.localScale = Vector3.one * ScreenScale(camera);
        component.transform.rotation = Quaternion.identity;
        _activeCount++;
        return true;
    }

    private static void ReportSuppressed(string reason, string spellId)
    {
        if (!MclslDiagnostics.Enabled) return;
        MclslDiagnostics.Throttle("spell-effect-" + reason, Time.frameCount, 3600,
            "法术特效 " + spellId + "：" + reason);
    }

    private void Update()
    {
        if (Time.time >= _end) { ReturnToPool(); return; }
        if (!TryPosition(_target ?? _caster, out Vector3 position)) position = _anchor;
        float progress = Mathf.Clamp01((Time.time - _born) / _duration);
        _renderer.sprite = MclslSpellEffectFrames.Get(_spellId,
            Math.Min(_frameCount - 1, (int)(progress * _frameCount)));
        float remaining = progress < 0.78f ? 1f : Mathf.Clamp01((1f - progress) / 0.22f);
        transform.localScale = Vector3.one * ScreenScale(_camera) * (1.0f + 0.12f * Mathf.Sin(progress * Mathf.PI));
        transform.position = position + new Vector3(0f, 0.38f + progress * 0.10f, -0.01f);
        transform.rotation = Quaternion.Euler(0f, 0f, _spellId is "S003" or "S010" or "S012" or "S019" or "S022" or "S023" ? progress * 90f : 0f);
        _renderer.color = new Color(1f, 1f, 1f, remaining);
    }

    private static float DurationFor(string spellId) => spellId switch
    {
        "S007" or "S013" or "S015" or "S017" => 0.72f,
        "S002" or "S003" or "S005" or "S018" or "S020" or "S024" or "S025" => 1.28f,
        "S010" or "S012" or "S019" or "S022" or "S023" or "S026" => 1.42f,
        _ => 0.96f
    };

    private static bool TryPosition(Actor actor, out Vector3 position)
    {
        position = default;
        if (actor?.data == null) return false;
        try
        {
            position = actor.current_position;
            if (!float.IsNaN(position.x) && !float.IsNaN(position.y)) return true;
        }
        catch { /* A removed actor may no longer have a render position. */ }
        try
        {
            WorldTile tile = actor.current_tile;
            if (tile == null) return false;
            position = tile.posV3;
            return true;
        }
        catch { return false; }
    }

    private static float ScreenScale(Camera camera)
    {
        if (camera == null || !camera.orthographic) return VisualScale;
        float pixelsPerWorld = Math.Max(0.01f, Screen.height / Math.Max(0.01f, 2f * camera.orthographicSize));
        return Math.Clamp(VisualScale, 48f / pixelsPerWorld, 128f / pixelsPerWorld);
    }

    private void ReturnToPool()
    {
        if (!_active) return;
        _active = false;
        _activeCount = Math.Max(0, _activeCount - 1);
        _caster = null;
        _target = null;
        gameObject.SetActive(false);
        Pool.Push(this);
    }

    private void OnDestroy()
    {
        if (_active) _activeCount = Math.Max(0, _activeCount - 1);
    }
}

// Each spell uses one atlas; frame sprites are created on demand and reused.
internal static class MclslSpellEffectFrames
{
    private sealed class Atlas
    {
        internal Sprite Source;
        internal Sprite[] Frames;
        internal int Rows;
    }

    private static readonly Dictionary<string, Atlas> Cache = new(StringComparer.Ordinal);
    private const int Size = 192;
    private const int Columns = 4;

    internal static int Count(string id)
    {
        if (!MclslSpellSystem.TryGet(id, out MclslSpellDefinition spell)) return 0;
        return spell.MinRealm <= 1 ? 16 : spell.MinRealm <= 3 ? 20 : 24;
    }

    internal static Sprite Get(string id, int frame)
    {
        if (!Cache.TryGetValue(id, out Atlas atlas))
        {
            int count = Count(id);
            if (count == 0) return null;
            int rows = (count + Columns - 1) / Columns;
            string path = "effects/Spells/" + id;
            Sprite source = null;
            long loadSample = MclslPerformanceProbe.Begin();
            try { source = SpriteTextureLoader.getSprite(path); }
            catch (Exception ex)
            {
                MclslDiagnostics.Error("spell-effect-atlas-" + id, ex.Message);
            }
            finally { MclslPerformanceProbe.End("法术特效.图集加载", loadSample); }
            // NML may trim transparent margins from the returned Sprite, while
            // its underlying texture still contains the complete atlas.
            if (source?.texture == null || source.texture.width != Columns * Size
                || source.texture.height != rows * Size)
            {
                MclslDiagnostics.Error("spell-effect-atlas-" + id,
                    "法术图集缺失或尺寸错误：" + path + "（需要 "
                    + (Columns * Size) + "×" + (rows * Size) + " PNG；实际 "
                    + (source?.texture == null ? "未加载" : source.texture.width + "×" + source.texture.height) + "）");
                return null;
            }
            source.texture.filterMode = FilterMode.Bilinear;
            atlas = new Atlas { Source = source, Frames = new Sprite[count], Rows = rows };
            Cache[id] = atlas;
        }
        int index = Math.Clamp(frame, 0, atlas.Frames.Length - 1);
        if (atlas.Frames[index] != null) return atlas.Frames[index];
        int row = index / Columns;
        Rect frameRect = new(index % Columns * Size,
            (atlas.Rows - row - 1) * Size, Size, Size);
        long frameSample = MclslPerformanceProbe.Begin();
        try
        {
            atlas.Frames[index] = Sprite.Create(atlas.Source.texture, frameRect,
                new Vector2(0.5f, 0.5f), Size);
            return atlas.Frames[index];
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("spell-effect-frame-" + id + "-" + index, ex.Message);
            return index > 0 ? Get(id, index - 1) : null;
        }
        finally { MclslPerformanceProbe.End("法术特效.切帧", frameSample); }
    }

}
