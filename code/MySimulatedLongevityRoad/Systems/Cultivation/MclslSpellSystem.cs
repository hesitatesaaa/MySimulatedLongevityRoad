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
        result.Sort((a, b) => b.MinRealm != a.MinRealm ? b.MinRealm.CompareTo(a.MinRealm)
            : string.CompareOrdinal(a.Id, b.Id));
        return state.KnownSpells = result.ToArray();
    }

    internal static bool Learn(Actor actor, string spellId)
    {
        if (!MclslActorAccessor.Alive(actor) || !ById.TryGetValue(spellId, out MclslSpellDefinition spell)
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm || Knows(actor, spellId)) return false;
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

    internal static void TryProgressAnnual(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || MaxMana(actor) <= 0) return;
        MclslSpellProgression.ProgressAnnual(actor);
        MclslSpellDefinition[] known = KnownCached(actor);
        if (MclslAncientMentorshipSystem.TryGetTeacher(actor, out Actor teacher))
        {
            MclslMentorshipGiftSystem.TryGift(actor, teacher, year);
            if (StableRoll(actor, year, "teacher") < 35)
            {
                var teacherSpells = KnownCached(teacher);
                int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor)), eligible = 0;
                for (int i = 0; i < teacherSpells.Length; i++)
                    if (realm >= teacherSpells[i].MinRealm && Array.IndexOf(known, teacherSpells[i]) < 0) eligible++;
                if (eligible > 0)
                {
                    int pick = StableRoll(actor, year, "teacher_pick") % eligible;
                    for (int i = 0; i < teacherSpells.Length; i++)
                    {
                        MclslSpellDefinition spell = teacherSpells[i];
                        if (realm < spell.MinRealm || Array.IndexOf(known, spell) >= 0) continue;
                        if (pick-- == 0) { Learn(actor, spell.Id); break; }
                    }
                }
            }
        }
        known = KnownCached(actor);
        if (known.Length == 0 || StableRoll(actor, year, "scroll") >= 8) return;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (MclslBagSystem.Count(bag, "F01") < 1 || MclslBagSystem.Count(bag, "A07") < 1) return;
        MclslBagSystem.Remove(bag, "F01");
        MclslBagSystem.Remove(bag, "A07");
        string scrollId = known[StableRoll(actor, year, "scroll_pick") % known.Length].Id + "_SCROLL";
        MclslBagSystem.Add(bag, scrollId, acquiredYear: year);
        MclslBagSystem.Write(actor, bag);
    }

    internal static void OnRealmAdvanced(Actor actor, string oldRealm, string newRealm)
    {
        int previous = MclslRealmIds.Index(oldRealm);
        int current = MclslRealmIds.Index(newRealm);
        if (previous < 0 || current <= previous || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastSpellInsightRealm, -1) >= current) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastSpellInsightRealm, current);
        int year = MclslRuntime.CurrentYear();
        if (StableRoll(actor, year, "realm_insight_" + current) >= 35) return;
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId));
        MclslSpellDefinition first = null, second = null, third = null;
        int count = 0;
        for (int weight = 3; weight >= 0 && count < 3; weight--)
            for (int i = 0; i < All.Length && count < 3; i++)
            {
                MclslSpellDefinition spell = All[i];
                int priority = (Array.IndexOf(roots, spell.Law) >= 0 ? 2 : 0)
                    + (Array.IndexOf(technique.LawPool, spell.Law) >= 0 ? 1 : 0);
                if (priority != weight || spell.MinRealm > current || Knows(actor, spell.Id)) continue;
                if (count == 0) first = spell; else if (count == 1) second = spell; else third = spell;
                count++;
            }
        if (count > 0)
        {
            int pick = StableRoll(actor, year, "realm_pick_" + current) % count;
            Learn(actor, (pick == 0 ? first : pick == 1 ? second : third).Id);
        }
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

    private static void CommitCast(Actor actor, RuntimeState state, MclslSpellDefinition spell, int mana, Actor target = null)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, mana - spell.ManaCost);
        PersistManaTime(actor);
        state.NextCast[spell.Id] = Time.time + spell.Cooldown;
        MclslSpellProgression.RecordCast(actor, spell, MclslRuntime.CurrentYear());
        bool shown = MclslSpellEffect.Show(actor, target, spell.Id);
        if (MclslDiagnostics.Enabled)
            MclslDiagnostics.Throttle("spell-cast-" + spell.Id, Time.frameCount, 3600,
                "法术施放 " + spell.Id + "，修士=" + MclslActorAccessor.Id(actor)
                + "，灵力=" + (mana - spell.ManaCost) + "，特效=" + (shown ? "已生成" : "未生成"));
    }

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
            if (!ApplyCombatEffect(actor, target, spell)) continue;
            CommitCast(actor, state, spell, mana, target);
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


internal sealed class MclslSpellEffect : MonoBehaviour
{
    private const float VisualScale = 10f;
    private const int MaxVisibleEffects = 96;
    private const float Duration = 0.80f;
    private static readonly Stack<MclslSpellEffect> Pool = new();
    private static int _activeCount;
    private static Camera _camera;
    private static int _nextCameraRefreshFrame;
    private SpriteRenderer _renderer;
    private float _end;
    private float _born;
    private string _spellId;
    private Actor _caster;
    private Actor _target;
    private bool _active;

    internal static bool Show(Actor caster, Actor target, string spellId)
    {
        Sprite sprite = MclslSpellEffectFrames.Get(spellId, 0);
        if (sprite == null || caster?.data == null) { ReportSuppressed("贴图或施法者无效", spellId); return false; }
        if (_activeCount >= MaxVisibleEffects) { ReportSuppressed("同屏特效达到上限", spellId); return false; }
        if (!TryPosition(target ?? caster, out Vector3 position)) { ReportSuppressed("无法取得世界坐标", spellId); return false; }
        if (_camera == null || Time.frameCount >= _nextCameraRefreshFrame)
        {
            _camera = Camera.main;
            _nextCameraRefreshFrame = Time.frameCount + 120;
        }
        Camera camera = _camera;
        if (camera != null)
        {
            Vector3 viewport = camera.WorldToViewportPoint(position);
            if (viewport.z <= 0f || viewport.x < -0.05f || viewport.x > 1.05f
                || viewport.y < -0.05f || viewport.y > 1.05f)
            {
                ReportSuppressed("施法者在镜头外", spellId);
                return false;
            }
        }
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
        component._end = Time.time + Duration;
        component._spellId = spellId;
        component._caster = caster;
        component._target = target;
        component._active = true;
        component.transform.position = position + new Vector3(0f, 0.38f, -0.01f);
        component.transform.localScale = Vector3.one * VisualScale;
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
        if (Time.time >= _end || !TryPosition(_target ?? _caster, out Vector3 position)) { ReturnToPool(); return; }
        float progress = Mathf.Clamp01((Time.time - _born) / Duration);
        _renderer.sprite = MclslSpellEffectFrames.Get(_spellId, Math.Min(7, (int)(progress * 8f)));
        float remaining = 1f - progress;
        transform.localScale = Vector3.one * VisualScale * (1.0f + 0.12f * Mathf.Sin(progress * Mathf.PI));
        transform.position = position + new Vector3(0f, 0.38f + progress * 0.10f, -0.01f);
        transform.rotation = Quaternion.Euler(0f, 0f, _spellId is "S003" or "S010" or "S012" or "S019" or "S022" or "S023" ? progress * 90f : 0f);
        _renderer.color = new Color(1f, 1f, 1f, remaining);
    }

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

// Eight-frame sprite sheets are loaded and sliced only once per spell.
internal static class MclslSpellEffectFrames
{
    private static readonly Dictionary<string, Sprite[]> Cache = new(StringComparer.Ordinal);
    private const int Size = 24;

    internal static Sprite Get(string id, int frame)
    {
        if (!Cache.TryGetValue(id, out Sprite[] frames))
        {
            frames = new Sprite[8];
            Sprite sheet = SpriteTextureLoader.getSprite("effects/Spells/" + id);
            Texture2D texture = sheet?.texture;
            if (texture != null && texture.width >= Size * 8 && texture.height >= Size)
            {
                texture.filterMode = FilterMode.Point;
                for (int i = 0; i < frames.Length; i++)
                    frames[i] = Sprite.Create(texture, new Rect(i * Size, 0, Size, Size),
                        new Vector2(0.5f, 0.5f), Size);
            }
            Cache[id] = frames;
        }
        return frames[Math.Clamp(frame, 0, 7)];
    }

}
