using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Reflection;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslImmortalActorRegistration
{
    internal const string BaiId = "mclsl_bai_xiansheng";
    internal const string ChuanfaId = "mclsl_chuanfa_tianzun";
    private static readonly Dictionary<string, Sprite[]> Frames = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, PowerButton> Buttons = new(StringComparer.Ordinal);
    private sealed class MovementState
    {
        internal int X;
        internal int Y;
        internal int Frame = -1000;
        internal bool Initialized;
    }

    private static ConditionalWeakTable<Actor, MovementState> LastPosition = new();
    private static readonly Dictionary<string, int> PlacementTraceCounts = new(StringComparer.Ordinal);
    private static bool _registered;

    internal static bool IsImmortal(Actor actor) => actor?.asset?.id is BaiId or ChuanfaId;
    internal static Sprite TryGetBannerSprite(Subspecies subspecies)
    {
        try
        {
            string id = subspecies?.getActorAsset()?.id;
            if (id is not BaiId and not ChuanfaId) return null;
            if (Frames.TryGetValue(id, out Sprite[] frames) && frames?.Length > 0 && frames[0] != null)
                return frames[0];
            return SpriteTextureLoader.getSprite("actors/Immortals/" + (id == BaiId ? "Bai" : "Chuanfa") + "/Idle/1")
                ?? AssetManager.actor_library.get(id)?.cached_sprite
                ?? AssetManager.actor_library.get("sheep")?.cached_sprite;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路] 仙道人物旗帜取图失败: " + ex.Message);
            return null;
        }
    }
    internal static void ClearRuntime() => LastPosition = new ConditionalWeakTable<Actor, MovementState>();

    // The previous release's age repair could erase these actors' authored realm.
    // Called only during the bounded post-load world bootstrap, not each frame.
    internal static void RepairRealmAfterLoad(Actor actor, int year)
    {
        if (!IsImmortal(actor) || !MclslActorAccessor.Alive(actor)) return;
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        bool newLaw = system == MclslCultivationSystemIds.NewLaw
            || system != MclslCultivationSystemIds.AncientLaw && MclslWorldEpochSystem.IsNewLawActive(year);
        string expected = newLaw ? MclslRealmIds.ChangSheng : MclslRealmIds.HeDao;
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= MclslRealmIds.Index(expected)) return;
        string traitId = newLaw ? MclslTraitRegistration.TraitIdForRealm(expected)
            : MclslTraitRegistration.LegacyTraitIdForRealm(expected);
        MclslTraitGrantRouter.SuppressRouting(() =>
            MclslCultivationSystem.ApplyManualRealmGrant(actor, traitId, expected, year,
                forceAncient: !newLaw, forceNewLaw: newLaw));
        if (MclslActorAccessor.Realm(actor) != expected)
            Debug.LogWarning("[模拟长生路] 仙道人物旧档境界修复失败: " + actor.asset.id
                + " 目标=" + expected + " 当前=" + MclslActorAccessor.Realm(actor));
    }

    internal static void Init()
    {
        if (_registered || AssetManager.actor_library == null) return;
        _registered = true;
        Register(BaiId, "白先生", "Bai", "#E5EDF1");
        Register(ChuanfaId, "传法天尊", "Chuanfa", "#D5B66A");
    }

    private static void Register(string id, string name, string folder, string color)
    {
        try
        {
            MclslLocalizationBridge.RegisterKey(id, name);
            MclslLocalizationBridge.RegisterKey(id + " Description", "可放置的友好仙道人物。");
            // Sheep supplies the vanilla peaceful-animal decision/job pipeline.
            ActorAsset asset = AssetManager.actor_library.get(id) ?? AssetManager.actor_library.clone(id, "sheep");
            asset.name_locale = id;
            asset.icon = folder;
            asset.show_icon_inspect_window_id = folder;
            asset.show_icon_inspect_window = true;
            asset.color_hex = color;
            asset.color = Toolbox.makeColor(color);
            asset.unit_other = true;
            asset.has_advanced_textures = false;
            asset.has_baby_form = false;
            asset.use_phenotypes = false;
            asset.can_evolve_into_new_species = false;
            ActorAsset sheep = AssetManager.actor_library.get("sheep");
            if (sheep != null)
            {
                asset.job = sheep.job;
                asset.job_kingdom = sheep.job_kingdom;
                asset.job_baby = sheep.job_baby;
                asset.decision_ids = sheep.decision_ids == null ? null : new List<string>(sheep.decision_ids);
            }
            asset.can_evolve_into_new_species = false;
            if (sheep?.default_subspecies_traits != null)
                asset.default_subspecies_traits = sheep.default_subspecies_traits
                    .FindAll(trait => !trait.StartsWith("reproduction_", StringComparison.Ordinal)
                        && !trait.StartsWith("gestation_", StringComparison.Ordinal)
                        && !trait.StartsWith("population_", StringComparison.Ordinal));
            asset.source_meat = false;
            asset.source_meat_insect = false;
            asset.can_attack_buildings = false;
            asset.can_attack_brains = false;
            SetBool(asset, false, "can_attack", "canAttack", "can_fight", "canFight", "can_reproduce", "canReproduce", "can_marry", "canMarry");
            asset.animation_idle = new[] { "Idle_01", "Idle_02", "Idle_03", "Idle_04" };
            asset.animation_walk = new[] { "Walk_01", "Walk_02", "Walk_03", "Walk_04" };
            asset.animation_idle_speed = 0.14f;
            asset.animation_walk_speed = 0.14f;
            asset.texture_asset = new ActorTextureSubAsset("actors/Immortals/" + folder, false);
            Frames[id] = LoadFrames(folder);
            asset._cached_sprite = Frames[id][0];
            asset.cached_sprite = Frames[id][0];
            asset.get_override_sprite = GetSprite;
            asset.has_override_sprite = true;
            asset.base_stats["scale"] = 0.25f;
            asset.base_stats["size"] = 0.6f;
            AssetManager.actor_library.loadTexturesAndSprites(asset);
            asset._cached_sprite = Frames[id][0];
            asset.cached_sprite = Frames[id][0];
            asset.get_override_sprite = GetSprite;
            asset.has_override_sprite = true;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 仙道人物注册失败 " + id + ": " + ex.Message); }
    }

    private static Sprite[] LoadFrames(string folder)
    {
        Sprite[] frames = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            string path = "actors/Immortals/" + folder + "/" + (i < 4 ? "Idle/" + (i + 1) : "Walk/" + (i - 3));
            frames[i] = SpriteTextureLoader.getSprite(path);
        }
        return frames;
    }

    private static Sprite GetSprite(Actor actor)
    {
        if (actor?.asset == null || !Frames.TryGetValue(actor.asset.id, out Sprite[] frames)) return null;
        int offset = 0;
        if (actor.data != null)
        {
            int x = actor.data.x, y = actor.data.y;
            MovementState previous = LastPosition.GetOrCreateValue(actor);
            if (previous.Initialized
                && (previous.X != x || previous.Y != y || Time.frameCount - previous.Frame <= 12))
            {
                offset = 4;
                if (previous.X != x || previous.Y != y) previous.Frame = Time.frameCount;
            }
            else previous.Frame = -1000;
            previous.X = x;
            previous.Y = y;
            previous.Initialized = true;
        }
        int frame = offset + (Time.frameCount / 14) % 4;
        return frames[frame] ?? frames[0];
    }

    private static void SetBool(object target, bool value, params string[] names)
    {
        foreach (string name in names)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field?.FieldType == typeof(bool)) field.SetValue(target, value);
        }
    }

    internal static void AddPowers(PowersTab tab)
    {
        Init();
        AddPower(tab, BaiId, "白先生", "Bai");
        AddPower(tab, ChuanfaId, "传法天尊", "Chuanfa");
    }

    private static void AddPower(PowersTab tab, string id, string name, string folder)
    {
        if (tab == null) return;
        string powerId = id + "_place";
        MclslLocalizationBridge.RegisterKey(powerId, name);
        MclslLocalizationBridge.RegisterKey(powerId + " Description", "放置友好的仙道人物；境界随世界纪元而定。");
        GodPower power = AssetManager.powers.get(powerId);
        if (power == null)
        {
            power = new GodPower { id = powerId, name = id, click_action = (tile, _) => Place(id, tile), ignore_cursor_icon = true };
            AssetManager.powers.add(power);
        }
        if (!Buttons.TryGetValue(powerId, out PowerButton button) || button == null)
        {
            button = PowerButtonCreator.CreateGodPowerButton(powerId, SpriteTextureLoader.getSprite("actors/Immortals/" + folder + "/Idle/1"));
            Buttons[powerId] = button;
            PowersTabExtension.AddPowerButton(tab, "tab", button);
        }
    }

    private static bool Place(string id, WorldTile tile)
    {
        if (tile == null || World.world?.units == null) return false;
        int year = MclslRuntime.CurrentYear();
        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        string traceKey = id + (newLaw ? "|new" : "|ancient");
        PlacementTraceCounts.TryGetValue(traceKey, out int traceCount);
        PlacementTraceCounts[traceKey] = traceCount + 1;
        bool trace = traceCount < 2;
        Stopwatch watch = Stopwatch.StartNew();
        TracePlacement(trace, id, "create.begin", watch);
        try
        {
            Actor actor = World.world.units.createNewUnit(id, tile, false, 0f, null, null, true, false, false, false);
            if (actor?.data == null) return false;
            TracePlacement(trace, id, "create.done", watch);
            string name = id == BaiId ? "白先生" : "传法天尊";
            actor.data.name = name;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, 100);
            string gift = MclslTraitRegistration.GiftTraitIdForAptitude(100);
            ActorTrait giftTrait = AssetManager.traits.get(gift);
            string traitId = id == BaiId ? MclslTraitRegistration.BaiTraitId : MclslTraitRegistration.ChuanfaTraitId;
            ActorTrait trait = AssetManager.traits.get(traitId);
            // The gift's normal route enters cultivation immediately. These
            // hand-placed actors receive one explicit high-realm grant below.
            MclslTraitGrantRouter.SuppressRouting(() =>
            {
                if (giftTrait != null) actor.addTrait(giftTrait, true);
                if (trait != null) actor.addTrait(trait, true);
            });
            TracePlacement(trace, id, "traits.done", watch);
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, newLaw ? MclslCultivationSystemIds.NewLaw : MclslCultivationSystemIds.AncientLaw);
            string techniqueId = id == BaiId ? "gongfa_wuxing" : "gongfa_wanxiang";
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(techniqueId);
            if (technique == null) throw new InvalidOperationException("未找到仙道人物功法: " + techniqueId);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, newLaw ? technique.Id : "spiritual_" + technique.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, technique.Name);
            MclslTechniqueRealmLimit.SetMaxRealm(actor, newLaw ? MclslRealmIds.ChangSheng : MclslRealmIds.HeDao);
            string realm = newLaw ? MclslRealmIds.ChangSheng : MclslRealmIds.HeDao;
            string realmTrait = newLaw ? MclslTraitRegistration.TraitIdForRealm(realm)
                : MclslTraitRegistration.LegacyTraitIdForRealm(realm);
            TracePlacement(trace, id, "realm.begin", watch);
            MclslTraitGrantRouter.SuppressRouting(() =>
                MclslCultivationSystem.ApplyManualRealmGrant(actor, realmTrait, realm, year,
                    forceAncient: !newLaw, forceNewLaw: newLaw));
            TracePlacement(trace, id, "realm.done", watch);
            string artifactId = id == BaiId ? "B080" : "B081";
            MclslBagSystem.Add(actor, artifactId, 1, year);
            MclslWorldRunRepository.AddItemAcquisitionEvent(
                year, actor, artifactId, 1, "仙道人物初始法宝");
            TracePlacement(trace, id, "bag.done", watch);
            MclslCultivationWake.EnsureAwake(actor, enqueueAnnual: true, refreshUi: false);
            TracePlacement(trace, id, "awake.done", watch);
            if (MclslActorAccessor.Realm(actor) != realm)
            {
                Debug.LogWarning("[模拟长生路] 仙道人物放置后境界不符: " + id
                    + " 目标=" + realm + " 当前=" + MclslActorAccessor.Realm(actor));
                return false;
            }
            return true;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 仙道人物放置失败 " + id + ": " + ex); return false; }
    }

    private static void TracePlacement(bool enabled, string id, string stage, Stopwatch watch)
    {
        if (enabled) Debug.Log("[模拟长生路][人物放置] " + id + " " + stage + " " + watch.ElapsedMilliseconds + "ms");
    }
}
