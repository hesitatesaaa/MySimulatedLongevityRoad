using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslBeastActorRegistration
{
    private static bool _initialized;
    private static readonly Dictionary<string, PowerButton> Buttons = new(StringComparer.Ordinal);
    internal static void Init()
    {
        if (_initialized || AssetManager.actor_library == null) return;
        ActorAsset orc = AssetManager.actor_library.get("orc");
        if (orc == null || AssetManager.actor_library.get("sheep") == null) return;
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
        {
            try
            {
                Register(beast, orc);
            }
            catch (Exception ex)
            {
                MclslDiagnostics.Error("beast-register:" + beast.Id, "妖兽注册失败: " + ex);
                throw;
            }
        }
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.Named)
        {
            try { Register(beast, orc); }
            catch (Exception ex)
            {
                MclslDiagnostics.Error("beast-register:" + beast.Id, "具名妖兽注册失败: " + ex);
                throw;
            }
        }
        _initialized = true;
    }

    internal static bool IsBeast(Actor actor)
        => MclslBeastCatalog.ForAsset(actor?.asset?.id) != null;

    internal static bool IsOrdinary(Actor actor)
        => IsBeast(actor) && actor.asset.id.StartsWith("mclsl_beast_", StringComparison.Ordinal);

    internal static void AddPowers(PowersTab tab)
    {
        Init();
        if (tab == null) return;
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
        {
            string powerId = beast.BeastAssetId + "_place";
            MclslLocalizationBridge.RegisterKey(powerId, beast.Name);
            MclslLocalizationBridge.RegisterKey(powerId + " Description", "手动放置" + beast.Name + "兽形个体。");
            if (AssetManager.powers.get(powerId) == null)
                AssetManager.powers.add(new GodPower
                {
                    id = powerId,
                    name = beast.BeastAssetId,
                    click_action = (tile, _) => Place(beast, tile),
                    ignore_cursor_icon = true
                });
            if (Buttons.ContainsKey(powerId)) continue;
            Sprite icon = MclslActorPixelResources.PlacementIcon(beast.BeastAssetId);
            if (icon == null) continue;
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(powerId, icon);
            if (button == null) continue;
            MclslActorPixelResources.ConfigurePlacementButton(button);
            Buttons.Add(powerId, button);
            PowersTabExtension.AddPowerButton(tab, "tab", button);
        }
    }

    private static bool Place(MclslBeastDefinition beast, WorldTile tile)
    {
        if (tile == null || World.world?.units == null) return false;
        Actor actor = World.world.units.createNewUnit(beast.BeastAssetId, tile, false, 0f,
            null, null, true, false, false, false);
        if (actor?.data == null) return false;
        EnsureCultivationSeed(actor);
        if(IsOrdinary(actor))MclslBeastCultivation.SetRealm(actor,
            MclslRealmIds.Ordered[Math.Clamp(MclslBeastWorld.ManualRealm,0,6)],MclslRuntime.CurrentYear());
        return true;
    }

    internal static bool IsAscended(Actor actor)
    {
        MclslBeastDefinition beast = MclslBeastCatalog.ForAsset(actor?.asset?.id);
        return beast != null && string.Equals(actor.asset.id, beast.AscendedAssetId, StringComparison.Ordinal);
    }

    internal static bool HasYuanyingWisdom(Actor actor)
        => IsBeast(actor) && !IsOrdinary(actor) && CanTransform(actor);

    internal static bool CanTransform(Actor actor)
        => IsBeast(actor) && MclslRealmIds.Index(MclslActorAccessor.Realm(actor))
            >= MclslRealmIds.Index(MclslRealmIds.YuanYing);

    internal static string DisplayName(Actor actor, string realm)
    {
        MclslBeastDefinition beast = MclslBeastCatalog.ForAsset(actor?.asset?.id);
        if (beast == null) return null;
        string name = beast.Name;
        if (beast.AssetPrefix == "mclsl_beast_" && actor.data != null)
        {
            const string key = "mclsl.beast.individual_name";
            name = MclslActorAccessor.GetString(actor, key);
            if (string.IsNullOrEmpty(name))
            {
                long id = MclslActorAccessor.Id(actor);
                const string first = "玄青苍赤云霜月星灵幽玉紫";
                const string second = "岚霄渊羽辰霖岳华川尘澜珑";
                int pairCount = first.Length * second.Length;
                int roll = MclslBeastWorld.Roll(beast.Id + "|name|" + id, pairCount * pairCount);
                int leading = roll / pairCount, trailing = roll % pairCount;
                name = beast.Name + "·" + first[leading / second.Length] + second[leading % second.Length]
                    + first[trailing / second.Length] + second[trailing % second.Length];
                MclslActorAccessor.Set(actor, key, name);
            }
        }
        return MclslRealmIds.Index(realm) >= 0
            ? name + "·" + MclslRealmIds.Display(realm) : name;
    }

    internal static void EnsureCultivationSeed(Actor actor)
    {
        if (!IsBeast(actor) || actor.data == null) return;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, 60);
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 60);
        if(!IsOrdinary(actor))MclslTraitRegistration.SyncGiftTrait(actor, aptitude);
        if (IsOrdinary(actor)) MclslBeastCultivation.Ensure(actor);
        MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
    }

    internal static void ReconcileForm(Actor actor)
    {
        if (actor?.asset == null || actor.data == null) return;
        MclslBeastDefinition beast = MclslBeastCatalog.ForAsset(actor.asset.id);
        if (beast == null) return;
        EnsureLineage(actor, beast);
        string wanted = CanTransform(actor) ? beast.AscendedAssetId : beast.BeastAssetId;
        if (string.Equals(actor.asset.id, wanted, StringComparison.Ordinal)) return;
        ActorAsset target = AssetManager.actor_library.get(wanted);
        if (target == null) return;
        Subspecies lineage = actor.subspecies;
        actor.setAsset(target);
        // Keep the same original subspecies: its edited Harmony limit covers both forms.
        if (lineage != null && actor.subspecies != lineage) actor.setSubspecies(lineage);
        actor.registerDecisions();
        MclslBeastAnimation.Forget(actor);
    }

    private static void EnsureLineage(Actor actor, MclslBeastDefinition beast)
    {
        MclslActorPresentation.ReconcileLineage(actor);
    }

    private static void Register(MclslBeastDefinition beast, ActorAsset orc)
    {
        MclslLocalizationBridge.RegisterKey(beast.BeastAssetId, beast.Name);
        MclslLocalizationBridge.RegisterKey(beast.AscendedAssetId, beast.Name + "·元婴");
        MclslLocalizationBridge.RegisterKey(beast.BeastAssetId + " Description", "元婴前无智慧的修炼妖兽。");
        bool named = beast.AssetPrefix == "mclsl_named_";
        MclslLocalizationBridge.RegisterKey(beast.AscendedAssetId + " Description", named
            ? "具名妖兽，元婴后保留人物规则。" : "元婴后化形，仍无智慧，不建立国家。");
        ActorAsset template = AssetManager.actor_library.get(beast.AnimalTemplate)
            ?? AssetManager.actor_library.get("sheep");
        if (template == null || orc == null) throw new InvalidOperationException("缺少原版动物或兽人模板");

        ActorAsset animal = AssetManager.actor_library.get(beast.BeastAssetId)
            ?? AssetManager.actor_library.clone(beast.BeastAssetId, template.id);
        if (animal == null) throw new InvalidOperationException("无法创建兽形单位");
        ConfigureShared(animal, beast, template, false);
        // WorldBox creates an editable subspecies for this civilization-capable
        // lineage, while per-actor isSapient remains false below YuanYing.
        animal.civ = true;
        animal.auto_civ = false;
        animal.unit_other = true;
        animal.kingdom_id_civilization = string.Empty;
        animal.kingdom_id_wild = template.kingdom_id_wild;
        animal.job = template.job;
        animal.job_kingdom = template.job_kingdom;
        animal.job_baby = template.job_baby;
        animal.decision_ids = Copy(template.decision_ids);

        ActorAsset ascended = AssetManager.actor_library.get(beast.AscendedAssetId)
            ?? AssetManager.actor_library.clone(beast.AscendedAssetId, orc.id);
        if (ascended == null) throw new InvalidOperationException("无法创建元婴化形单位");
        ConfigureShared(ascended, beast, orc, true);
        ascended.civ = true;
        ascended.auto_civ = true;
        ascended.unit_other = false;
        ascended.kingdom_id_civilization = orc.kingdom_id_civilization;
        ascended.kingdom_id_wild = orc.kingdom_id_wild;
        ascended.job = orc.job;
        ascended.job_kingdom = orc.job_kingdom;
        ascended.job_baby = orc.job_baby;
        ascended.decision_ids = Copy(orc.decision_ids);
        ascended.architecture_id = orc.architecture_id;
        ascended.banner_id = orc.banner_id;
        if (!named)
        {
            ActorAsset predator=AssetManager.actor_library.get(beast.AnimalTemplate=="piranha"?"piranha":"wolf")??template;
            foreach (ActorAsset form in new[] { animal, ascended })
            {
                form.civ = false;
                form.auto_civ = false;
                form.unit_other = true;
                form.kingdom_id_civilization = string.Empty;
                form.kingdom_id_wild = "mclsl_wild_beasts";
                form.job = predator.job;
                form.job_kingdom = predator.job_kingdom;
                form.job_baby = predator.job_baby;
                form.decision_ids = Copy(predator.decision_ids);
                form.default_subspecies_traits.RemoveAll(id => id == "prefrontal_cortex"
                    || id == "advanced_hippocampus" || id.StartsWith("reproduction_", StringComparison.Ordinal));
                form.base_stats["birth_rate"] = 0f;
                form.base_stats["offspring"] = 0f;
            }
            MclslBeastWorld.EnsureWildKingdom();
        }
    }

    private static void ConfigureShared(ActorAsset asset, MclslBeastDefinition beast,
        ActorAsset template, bool humanoid)
    {
        asset.name_locale = humanoid ? beast.AscendedAssetId : beast.BeastAssetId;
        asset.can_edit_traits = true;
        asset.can_have_subspecies = true;
        asset.can_evolve_into_new_species = false;
        asset.has_baby_form = false;
        asset.use_phenotypes = false;
        asset.has_advanced_textures = false;
        asset.skin_citizen_male = new[] { "male_1" };
        asset.skin_citizen_female = new[] { "female_1" };
        var traits = template.default_subspecies_traits == null
            ? new List<string>() : new List<string>(template.default_subspecies_traits);
        traits.RemoveAll(id => id.StartsWith("population_", StringComparison.Ordinal)
            || id.StartsWith("gestation_", StringComparison.Ordinal)
            || id == "high_fecundity");
        // A shared subspecies must be eligible for the native orc civilization
        // pipeline after metamorphosis. Actor.isSapient is gated by realm.
        if (!traits.Contains("prefrontal_cortex")) traits.Add("prefrontal_cortex");
        if (!traits.Contains("advanced_hippocampus")) traits.Add("advanced_hippocampus");
        if (!traits.Contains("reproduction_sexual")) traits.Add("reproduction_sexual");
        traits.Add("gestation_extremely_long");
        traits.Add(beast.PopulationTrait);
        asset.default_subspecies_traits = traits;
        asset.base_stats["birth_rate"] = 0.1f;
        asset.base_stats["offspring"] = 1f;
        string path = beast.ResourceFolder + "/" + (humanoid ? "Human" : "Beast") + "/";
        asset.base_stats["scale"] = MclslActorPixelResources.DisplayScale(path, humanoid ? 0.20f : 0.18f);
        asset.texture_id = asset.id;
        asset.texture_asset = new ActorTextureSubAsset(path, false);
        asset.texture_asset.texture_path_main = path + "main";
        Sprite first = MclslBeastAnimation.ConfigureAsset(asset, path);
        if (first == null)
            throw new InvalidOperationException("主贴图加载失败: " + path + "main/idle_0");
        asset.cached_sprite = first;
        asset._cached_sprite = first;
        AssetManager.actor_library.loadTexturesAndSprites(asset);
        asset.cached_sprite = first;
        asset._cached_sprite = first;
    }

    private static List<string> Copy(List<string> input)
        => input == null ? new List<string>() : new List<string>(input);
}
