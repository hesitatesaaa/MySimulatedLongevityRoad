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
    }

    private static ConditionalWeakTable<Actor, RuntimeState> _runtime = new();
    private static readonly string[] SelfSpellPriority = { "S002", "S003", "S005", "S006", "S011" };
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
        Spell("S012", "万象归一", "空间", 5, 180, 45f, "万象归一，镇压一敌。")
    };
    private static readonly Dictionary<string, MclslSpellDefinition> ById = All.ToDictionary(x => x.Id, StringComparer.Ordinal);

    private static MclslSpellDefinition Spell(string id, string name, string law, int realm, int mana, float cooldown, string description)
        => new() { Id = id, Name = name, Law = law, MinRealm = realm, ManaCost = mana, Cooldown = cooldown, Description = description };

    internal static void ClearRuntime()
    {
        _runtime = new ConditionalWeakTable<Actor, RuntimeState>();
        _selfScanIndex = 0;
        _lastSelfScanWorldTime = -1d;
        MclslSpellVisualQueue.Clear();
    }

    internal static int MaxMana(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.IsCultivator(actor)) return 0;
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
        => KnownCached(actor).Any(x => string.Equals(x.Id, spellId, StringComparison.Ordinal));

    internal static List<MclslSpellDefinition> Known(Actor actor)
        => new(KnownCached(actor));

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
        return state.KnownSpells = result.ToArray();
    }

    internal static bool Learn(Actor actor, string spellId)
    {
        if (!MclslActorAccessor.Alive(actor) || !ById.TryGetValue(spellId, out MclslSpellDefinition spell)
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm || Knows(actor, spellId)) return false;
        List<MclslSpellDefinition> known = Known(actor);
        known.Add(spell);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LearnedSpells, "v1:" + string.Join(",", known.Select(x => x.Id)));
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
        List<MclslSpellDefinition> known = Known(actor);
        if (MclslAncientMentorshipSystem.TryGetTeacher(actor, out Actor teacher))
        {
            MclslMentorshipGiftSystem.TryGift(actor, teacher, year);
            List<MclslSpellDefinition> taught = Known(teacher).Where(x => !known.Contains(x)
                && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= x.MinRealm).ToList();
            if (taught.Count > 0 && StableRoll(actor, year, "teacher") < 35)
            {
                Learn(actor, taught[StableRoll(actor, year, "teacher_pick") % taught.Count].Id);
                known = Known(actor);
            }
        }
        known = Known(actor);
        if (known.Count == 0 || StableRoll(actor, year, "scroll") >= 8) return;
        MclslBagState bag = MclslBagSystem.Read(actor);
        if (MclslBagSystem.Count(bag, "F01") < 1 || MclslBagSystem.Count(bag, "A07") < 1) return;
        MclslBagSystem.Remove(bag, "F01");
        MclslBagSystem.Remove(bag, "A07");
        string scrollId = known[StableRoll(actor, year, "scroll_pick") % known.Count].Id + "_SCROLL";
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
        MclslSpellDefinition[] options = All.Where(x => x.MinRealm <= current && !Knows(actor, x.Id))
            .OrderByDescending(x => Array.IndexOf(roots, x.Law) >= 0)
            .ThenByDescending(x => Array.IndexOf(technique.LawPool, x.Law) >= 0).ToArray();
        if (options.Length > 0) Learn(actor, options[StableRoll(actor, year, "realm_pick_" + current) % Math.Min(3, options.Length)].Id);
    }

    private static int StableRoll(Actor actor, int year, string salt)
    {
        unchecked
        {
            int hash = 23;
            string seed = MclslActorAccessor.Id(actor) + "|" + year + "|" + salt;
            foreach (char c in seed) hash = hash * 37 + c;
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
            for (int i = 0; i < count; i++)
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
                "S002" => healthFraction < 0.80f,
                "S003" => healthFraction < 0.75f && !actor.hasStatus("mclsl_spell_water_guard"),
                "S005" => healthFraction < 0.75f && !actor.hasStatus("mclsl_spell_earth_guard"),
                "S006" => healthFraction < 0.55f && !actor.hasStatus("mclsl_spell_wind_step"),
                "S011" => healthFraction < 0.55f && !actor.hasStatus("mclsl_spell_void_step"),
                _ => false
            };
            if (!needed) continue;
            ApplySelfEffect(actor, id);
            CommitCast(actor, state, spell, mana);
            return true;
        }
        return false;
    }

    private static void ApplySelfEffect(Actor actor, string spellId)
    {
        switch (spellId)
        {
            case "S002": actor.restoreHealthPercent(0.18f); break;
            case "S003": actor.addStatusEffect("mclsl_spell_water_guard", 8f); break;
            case "S005": actor.addStatusEffect("mclsl_spell_earth_guard", 10f); break;
            case "S006": actor.addStatusEffect("mclsl_spell_wind_step", 12f); break;
            case "S011": actor.addStatusEffect("mclsl_spell_void_step", 8f); break;
        }
    }

    private static void CommitCast(Actor actor, RuntimeState state, MclslSpellDefinition spell, int mana)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManaCurrent, mana - spell.ManaCost);
        PersistManaTime(actor);
        state.NextCast[spell.Id] = Time.time + spell.Cooldown;
        bool shown = MclslSpellVisualQueue.Request(actor, spell.Id);
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
            CommitCast(actor, state, spell, mana);
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
        float damage = Math.Max(1f, actor.stats.get("damage")) * (1f + 0.10f * spell.MinRealm);
        switch (spell.Id)
        {
            case "S008": target.addStatusEffect("mclsl_spell_yin_bind", 2f); break;
            case "S007": target.addStatusEffect("mclsl_spell_shock", 1.5f); break;
            case "S009": actor.restoreHealthPercent(0.05f); break;
            case "S010": return ApplyAreaSpell(actor, target, damage * 1.5f, false);
            case "S012": return ApplyAreaSpell(actor, target, damage * 2f, true);
        }
        _applyingSpellDamage = true;
        try { target.getHit(damage, true, spell.Id == "S004" ? AttackType.Fire : AttackType.Divine, actor); }
        finally { _applyingSpellDamage = false; }
        return true;
    }

    private static bool ApplyAreaSpell(Actor caster, Actor mainTarget, float damage, bool bind)
    {
        WorldTile center = mainTarget.current_tile;
        if (center == null) return false;
        int hits = 0;
        foreach (Actor candidate in Finder.getUnitsFromChunk(center, 1, 3f, false))
        {
            if (!MclslActorAccessor.Alive(candidate) || candidate == caster) continue;
            if (candidate != mainTarget && candidate.attack_target != caster
                && (mainTarget.kingdom == null || candidate.kingdom != mainTarget.kingdom)) continue;
            if (bind) candidate.addStatusEffect("mclsl_spell_all_bind", 3f);
            _applyingSpellDamage = true;
            try { candidate.getHit(damage, true, AttackType.Divine, caster); }
            finally { _applyingSpellDamage = false; }
            if (++hits >= 8) break;
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

internal static class MclslSpellVisualQueue
{
    private const int Capacity = 192;
    private const int ExpiryFrames = 120;

    private readonly struct VisualKey : IEquatable<VisualKey>
    {
        internal readonly long ActorId;
        internal readonly string SpellId;
        internal readonly int Frame;

        internal VisualKey(long actorId, string spellId, int frame)
        {
            ActorId = actorId;
            SpellId = spellId;
            Frame = frame;
        }

        public bool Equals(VisualKey other) => ActorId == other.ActorId && Frame == other.Frame
            && string.Equals(SpellId, other.SpellId, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is VisualKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ActorId, SpellId, Frame);
    }

    private readonly struct RequestState
    {
        internal readonly VisualKey Key;
        internal readonly int DueFrame;

        internal RequestState(VisualKey key, int dueFrame)
        {
            Key = key;
            DueFrame = dueFrame;
        }
    }

    private static readonly Queue<RequestState> Pending = new();
    private static readonly HashSet<VisualKey> PendingKeys = new();

    internal static bool Request(Actor actor, string spellId)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || string.IsNullOrEmpty(spellId)) return false;
        if (Pending.Count >= Capacity)
        {
            MclslStaggeredWorkPolicy.RecordCanceled(MclslWorkCategory.Visual);
            return false;
        }
        VisualKey key = new(actorId, spellId, Time.frameCount);
        if (!PendingKeys.Add(key))
        {
            MclslStaggeredWorkPolicy.RecordDeduplicated(MclslWorkCategory.Visual);
            return true;
        }
        Pending.Enqueue(new RequestState(key, MclslStaggeredWorkPolicy.AssignVisualDueFrame(actorId, spellId)));
        return true;
    }

    internal static void Tick()
    {
        int scan = Pending.Count;
        int frame = Time.frameCount;
        while (scan-- > 0 && Pending.Count > 0)
        {
            RequestState request = Pending.Dequeue();
            if (frame - request.Key.Frame > ExpiryFrames)
            {
                PendingKeys.Remove(request.Key);
                MclslStaggeredWorkPolicy.RecordCanceled(MclslWorkCategory.Visual);
                continue;
            }
            if (request.DueFrame > frame)
            {
                Pending.Enqueue(request);
                continue;
            }
            if (!MclslStaggeredWorkPolicy.TryBegin(MclslWorkCategory.Visual, out long started))
            {
                Pending.Enqueue(request);
                break;
            }
            try
            {
                if (MclslActorRegistry.Resolve(request.Key.ActorId, out Actor actor)
                    && MclslActorAccessor.Alive(actor))
                    MclslSpellEffect.Show(actor, request.Key.SpellId);
                else
                    MclslStaggeredWorkPolicy.RecordCanceled(MclslWorkCategory.Visual);
            }
            finally
            {
                PendingKeys.Remove(request.Key);
                MclslStaggeredWorkPolicy.End(MclslWorkCategory.Visual, started);
            }
        }
    }

    internal static void Clear()
    {
        Pending.Clear();
        PendingKeys.Clear();
    }
}

internal sealed class MclslSpellEffect : MonoBehaviour
{
    private const float VisualScale = 10f;
    private const int MaxVisibleEffects = 96;
    private const float Duration = 0.90f;
    private static readonly Stack<MclslSpellEffect> Pool = new();
    private static int _activeCount;
    private static Camera _camera;
    private static int _nextCameraRefreshFrame;
    private SpriteRenderer _renderer;
    private float _end;
    private float _born;
    private string _spellId;
    private Actor _caster;
    private bool _active;

    internal static bool Show(Actor caster, string spellId)
    {
        Sprite sprite = MclslSpellEffectFrames.Get(spellId, 0);
        if (sprite == null || caster?.data == null) { ReportSuppressed("贴图或施法者无效", spellId); return false; }
        if (_activeCount >= MaxVisibleEffects) { ReportSuppressed("同屏特效达到上限", spellId); return false; }
        if (!TryPosition(caster, out Vector3 position)) { ReportSuppressed("无法取得世界坐标", spellId); return false; }
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
        if (Time.time >= _end || !TryPosition(_caster, out Vector3 position)) { ReturnToPool(); return; }
        float progress = Mathf.Clamp01((Time.time - _born) / Duration);
        _renderer.sprite = MclslSpellEffectFrames.Get(_spellId, Math.Min(3, (int)(progress * 4f)));
        float remaining = 1f - progress;
        transform.localScale = Vector3.one * VisualScale * (1.0f + 0.20f * Mathf.Sin(progress * Mathf.PI));
        transform.position = position + new Vector3(0f, 0.38f + progress * 0.10f, -0.01f);
        transform.rotation = Quaternion.Euler(0f, 0f, _spellId is "S003" or "S010" or "S012" ? progress * 180f : 0f);
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
        gameObject.SetActive(false);
        Pool.Push(this);
    }

    private void OnDestroy()
    {
        if (_active) _activeCount = Math.Max(0, _activeCount - 1);
    }
}

// Four hand-shaped pixel frames per spell. UI icons stay in ui/Spells and are
// never used as combat sprites; all textures are point-filtered and cached.
internal static class MclslSpellEffectFrames
{
    private static readonly Dictionary<string, Sprite[]> Cache = new(StringComparer.Ordinal);
    private const int Size = 16;

    internal static Sprite Get(string id, int frame)
    {
        if (!Cache.TryGetValue(id, out Sprite[] frames))
        {
            frames = new Sprite[4];
            for (int i = 0; i < frames.Length; i++) frames[i] = Create(id, i);
            Cache[id] = frames;
        }
        return frames[Math.Clamp(frame, 0, 3)];
    }

    private static Sprite Create(string id, int frame)
    {
        Color32[] pixels = new Color32[Size * Size];
        Color32 gold = new(255, 215, 101, 255), blue = new(98, 210, 245, 255);
        Color32 green = new(106, 220, 130, 255), red = new(255, 102, 66, 255);
        Color32 violet = new(177, 109, 230, 255), white = new(241, 243, 218, 255);
        int r = 3 + frame;
        switch (id)
        {
            case "S001":
                Line(pixels, 2 + frame, 3, 12 + frame / 2, 13, gold);
                Line(pixels, 3 + frame, 3, 12 + frame / 2, 12, white);
                break;
            case "S002":
                Ring(pixels, 8, 8, r, green);
                Line(pixels, 8, 4, 8, 12, white);
                Line(pixels, 8, 8, 5 + frame, 10, green);
                break;
            case "S003":
                Ring(pixels, 8, 8, r, blue);
                Ring(pixels, 8, 8, Math.Max(2, r - 2), white);
                break;
            case "S004":
                for (int y = 3; y <= 12; y++)
                {
                    int width = Math.Max(0, 4 - Math.Abs(y - (6 + frame)) / 2);
                    for (int x = 8 - width; x <= 8 + width; x++) Pixel(pixels, x, y, y < 7 ? gold : red);
                }
                Line(pixels, 8, 3, 7 + frame / 2, 13, white);
                break;
            case "S005":
                for (int x = 3; x <= 12; x++)
                    for (int y = 3; y <= 5 + frame; y++)
                        if ((x + y) % 3 != 0) Pixel(pixels, x, y, gold);
                Line(pixels, 2, 3, 13, 3, white);
                break;
            case "S006":
                for (int i = 0; i < 3; i++)
                    Line(pixels, 2 + frame, 4 + i * 3, 12 - i + frame / 2, 5 + i * 3, green);
                break;
            case "S007":
                Line(pixels, 8, 14, 5 + frame, 9, white);
                Line(pixels, 5 + frame, 9, 10, 9, blue);
                Line(pixels, 10, 9, 5 + frame / 2, 2, white);
                break;
            case "S008":
                Ring(pixels, 8, 8, r, violet);
                Line(pixels, 2, 2 + frame, 13, 13 - frame, violet);
                Line(pixels, 13, 2 + frame, 2, 13 - frame, white);
                break;
            case "S009":
                Ring(pixels, 8, 8, Math.Max(2, r - 1), gold);
                for (int i = 0; i < 8; i++)
                {
                    double angle = i * Math.PI / 4d;
                    Line(pixels, 8 + (int)Math.Round(Math.Cos(angle) * 4), 8 + (int)Math.Round(Math.Sin(angle) * 4),
                        8 + (int)Math.Round(Math.Cos(angle) * (5 + frame / 2)), 8 + (int)Math.Round(Math.Sin(angle) * (5 + frame / 2)), white);
                }
                break;
            case "S010":
                Color32[] elements = { gold, green, blue, red, violet };
                for (int i = 0; i < 5; i++)
                {
                    double angle = (i * 2d * Math.PI / 5d) + frame * 0.3d;
                    Pixel(pixels, 8 + (int)Math.Round(Math.Cos(angle) * r), 8 + (int)Math.Round(Math.Sin(angle) * r), elements[i]);
                    Line(pixels, 8, 8, 8 + (int)Math.Round(Math.Cos(angle) * r), 8 + (int)Math.Round(Math.Sin(angle) * r), elements[i]);
                }
                break;
            case "S011":
                Ring(pixels, 8, 8, r, violet);
                Ring(pixels, 8, 8, 2, blue);
                Line(pixels, 7, 8, 9, 8, white);
                break;
            case "S012":
                Line(pixels, 8, 1 + frame / 2, 14 - frame / 2, 8, gold);
                Line(pixels, 14 - frame / 2, 8, 8, 14 - frame / 2, white);
                Line(pixels, 8, 14 - frame / 2, 1 + frame / 2, 8, violet);
                Line(pixels, 1 + frame / 2, 8, 8, 1 + frame / 2, blue);
                Ring(pixels, 8, 8, 2 + frame / 2, white);
                break;
        }
        Texture2D texture = new(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    private static void Pixel(Color32[] pixels, int x, int y, Color32 color)
    {
        if ((uint)x < Size && (uint)y < Size) pixels[y * Size + x] = color;
    }

    private static void Line(Color32[] pixels, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, error = dx + dy;
        while (true)
        {
            Pixel(pixels, x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            int doubled = 2 * error;
            if (doubled >= dy) { error += dy; x0 += sx; }
            if (doubled <= dx) { error += dx; y0 += sy; }
        }
    }

    private static void Ring(Color32[] pixels, int cx, int cy, int radius, Color32 color)
    {
        for (int i = 0; i < 32; i++)
        {
            double angle = i * Math.PI / 16d;
            Pixel(pixels, cx + (int)Math.Round(Math.Cos(angle) * radius),
                cy + (int)Math.Round(Math.Sin(angle) * radius), color);
        }
    }
}
