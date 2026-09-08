using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static partial class MclslWorldSoulActorRegistration
{
    internal const string ActorAssetId = "mclsl_world_soul_avatar";
    internal const string ActorLocaleKey = "MclslWorldSoulSpecies";
    private const string RootFolder = "Souls";
    private const string ActorIconId = "TianDiZhiPo";
    private const string ActorColorHex = "#D0B067";
    private static bool _initialized;
    private static readonly Dictionary<string, Sprite> TintedSpriteCache = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SoulSpriteSet> SpriteSets = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, int> AttackUntilFrame = new();
    private static readonly Dictionary<long, int> NextPulseFrame = new();
    private static readonly Dictionary<long, int> AttackScanCursor = new();
    private static readonly Dictionary<long, int> LastSpriteX = new();
    private static readonly Dictionary<long, int> LastSpriteY = new();
    private static readonly Dictionary<long, int> LastMovementFrame = new();
    private static readonly Dictionary<string, SoulActorDefinition> DefinitionsByFolder = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SoulActorDefinition> DefinitionsByActorId = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SoulActorDefinition> DefinitionsBySoulId = new(StringComparer.Ordinal);
    private static readonly string[] WorldSoulSubspeciesTraitIds =
    {
        "pure",
        "hyper_intelligence",
        "long_lifespan",
        "hovering",
        "heat_resistance",
        "cold_resistance",
        "photosynthetic_skin"
    };
    private static readonly string[] WorldSoulForbiddenSubspeciesTraitIds =
    {
        "reproduction_sexual",
        "reproduction_asexual",
        "reproduction_vegetative",
        "reproduction_strategy_oviparity",
        "reproduction_strategy_viviparity",
        "gestation_short",
        "gestation_moderate",
        "gestation_long",
        "gestation_extremely_long",
        "egg_orb",
        "diet_carnivore",
        "stomach",
        "accelerated_healing",
        "regeneration",
        "regenerate",
        "evil",
        "one_eyed",
        "eyepatch",
        "crippled",
        "disabled",
        "madness",
        "plague",
        "infected"
    };
    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            ActorAsset asset = AssetManager.actor_library.get(ActorAssetId);
            if (asset == null)
            {
                asset = AssetManager.actor_library.clone(ActorAssetId, "$mob$");
            }
            ConfigureAsset(asset, string.Empty);
            RegisterSoulDefinitions();
            SoulActorDefinition[] definitions = MclslWorldSoulActorDefinitions.All;
            for (int i = 0; i < definitions.Length; i++)
                RegisterSoulAsset(definitions[i]);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路] 天地之魄载体注册失败，已停止使用原生敌对单位兜底: " + ex.Message);
        }
    }

    internal static string ActorAssetIdFor(MclslWorldSoulRecord soul)
    {
        SoulActorDefinition definition = ResolveDefinition(soul);
        return definition?.ActorId ?? ActorAssetId;
    }

    internal static string FolderForSoul(MclslWorldSoulRecord soul)
    {
        SoulActorDefinition definition = ResolveDefinition(soul);
        return definition?.Folder ?? string.Empty;
    }

    internal static void ClearRuntime()
    {
        AttackUntilFrame.Clear();
        NextPulseFrame.Clear();
        AttackScanCursor.Clear();
        PendingTerrainTasks.Clear();
        LastSpriteX.Clear();
        LastSpriteY.Clear();
        LastMovementFrame.Clear();
    }

    internal static void ForgetActorRuntime(long actorId)
    {
        if (actorId <= 0L) return;
        AttackUntilFrame.Remove(actorId);
        NextPulseFrame.Remove(actorId);
        AttackScanCursor.Remove(actorId);
        LastSpriteX.Remove(actorId);
        LastSpriteY.Remove(actorId);
        LastMovementFrame.Remove(actorId);
    }

    internal static bool IsWorldSoulAssetId(string actorAssetId)
    {
        RegisterSoulDefinitions();
        return string.Equals(actorAssetId, ActorAssetId, StringComparison.Ordinal)
            || (!string.IsNullOrWhiteSpace(actorAssetId) && DefinitionsByActorId.ContainsKey(actorAssetId));
    }

    internal static bool IsWorldSoulSubspecies(Subspecies subspecies)
    {
        try
        {
            return subspecies != null && IsWorldSoulAssetId(subspecies.getActorAsset()?.id);
        }
        catch
        {
            return false;
        }
    }

    internal static void NormalizeSubspecies(Subspecies subspecies)
    {
        if (!IsWorldSoulSubspecies(subspecies)) return;
        bool changed = false;
        try
        {
            if (!string.Equals(subspecies.data?.name, "天地之魄", StringComparison.Ordinal))
            {
                subspecies.data.name = "天地之魄";
                subspecies.data.custom_name = true;
                changed = true;
            }
            SetDataStringIfExists(subspecies.data, "天地之魄", "localized_name", "localizedName", "name_locale", "nameLocale", "customName");
            SetDataBoolIfExists(subspecies.data, true, "custom_name", "customName");
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #1: " + mclslEmptyCatchEx.Message); }

        for (int i = 0; i < WorldSoulSubspeciesTraitIds.Length; i++)
        {
            string traitId = WorldSoulSubspeciesTraitIds[i];
            try
            {
                if (!subspecies.hasTrait(traitId) && AssetManager.subspecies_traits?.get(traitId) != null)
                {
                    subspecies.addTrait(traitId, true);
                    changed = true;
                }
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #2: " + mclslEmptyCatchEx.Message); }
        }

        for (int i = 0; i < WorldSoulForbiddenSubspeciesTraitIds.Length; i++)
        {
            string traitId = WorldSoulForbiddenSubspeciesTraitIds[i];
            try
            {
                if (subspecies.hasTrait(traitId))
                {
                    subspecies.removeTrait(traitId);
                    changed = true;
                }
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #3: " + mclslEmptyCatchEx.Message); }
        }

        if (changed)
        {
            try { subspecies.forceRecalcBaseStats(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #4: " + mclslEmptyCatchEx.Message); }
        }
    }

    internal static Sprite TryGetBannerSprite()
    {
        try
        {
            return WorldSoulIconSprite()
                ?? AssetManager.actor_library?.get(ActorAssetId)?.getSpriteIcon();
        }
        catch
        {
            return null;
        }
    }

    private static void RegisterSoulDefinitions()
    {
        if (DefinitionsByFolder.Count > 0) return;
        SoulActorDefinition[] definitions = MclslWorldSoulActorDefinitions.All;
        for (int i = 0; i < definitions.Length; i++)
        {
            SoulActorDefinition definition = definitions[i];
            DefinitionsByFolder[definition.Folder] = definition;
            DefinitionsByActorId[definition.ActorId] = definition;
            DefinitionsBySoulId[definition.SoulId] = definition;
        }
    }

    private static void RegisterSoulAsset(SoulActorDefinition definition)
    {
        if (definition == null) return;
        try
        {
            ActorAsset asset = AssetManager.actor_library.get(definition.ActorId);
            if (asset == null)
            {
                ActorAsset template = AssetManager.actor_library.get(ActorAssetId);
                if (template == null) return;
                asset = AssetManager.actor_library.clone(definition.ActorId, ActorAssetId);
            }
            ConfigureAsset(asset, definition.Folder);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路] 天地之魄载体 " + definition.ActorId + " 注册失败: " + ex.Message);
        }
    }

    private static void ConfigureAsset(ActorAsset asset, string folder)
    {
        if (asset == null) return;
        try
        {
            asset.name_locale = ActorLocaleKey;
            asset.icon = ActorIconId;
            asset.show_icon_inspect_window_id = ActorIconId;
            asset.color_hex = ActorColorHex;
            asset.color = Toolbox.makeColor(ActorColorHex);
            asset.use_phenotypes = false;
            asset.unit_other = true;
            asset.has_advanced_textures = false;
            asset.has_baby_form = false;
            asset.can_evolve_into_new_species = false;
            SetAssetBoolIfExists(asset, false, "needs_food", "need_food", "needFood", "can_get_hungry", "canGetHungry", "can_sleep", "canSleep", "can_marry", "canMarry", "can_reproduce", "canReproduce", "can_have_children", "canHaveChildren", "can_have_baby", "canHaveBaby");
            SetAssetBoolIfExists(asset, false, "can_rotate", "canRotate", "rotate_sprite", "rotateSprite", "rotate_on_move", "rotateOnMove", "rotate_on_movement", "rotateOnMovement", "use_rotation", "useRotation", "draw_directional", "drawDirectional", "can_flip", "canFlip", "flip_sprite", "flipSprite", "draw_equipment", "drawEquipment", "draw_weapons", "drawWeapons", "use_items", "useItems", "take_items", "takeItems", "can_use_items", "canUseItems", "can_have_equipment", "canHaveEquipment", "can_pickup_items", "canPickupItems", "can_wear_equipment", "canWearEquipment");
            SetAssetNumberIfExists(asset, 0f, "rotation", "default_rotation", "defaultRotation", "angle", "sprite_angle", "spriteAngle");
            SetAssetBoolIfExists(asset, true, "unit_other", "is_monster", "isMonster");
            TrySetActorSize(asset);
            asset.show_icon_inspect_window = true;
            asset.animation_idle = new[] { "Idle_01", "Idle_02", "Idle_03", "Idle_04", "Idle_05", "Idle_06" };
            asset.animation_walk = asset.animation_idle;
            asset.animation_idle_speed = 0.12f;
            asset.animation_walk_speed = 0.12f;
            asset.animation_speed_based_on_walk_speed = false;
            asset.check_flip = delegate { return false; };
            asset.job = new[] { "random_move" };
            string actorPath = ActorPath(folder);
            asset.texture_asset = new ActorTextureSubAsset(actorPath, false);
            asset._cached_sprite = FallbackSprite(folder);
            asset.cached_sprite = asset._cached_sprite;
            asset.get_override_sprite = GetWorldSoulSprite;
            asset.has_override_sprite = true;
            asset.base_stats["scale"] = 0.2f;
            asset.base_stats["size"] = 0.4f;
            ApplyDefaultSubspeciesTraits(asset);
            AssetManager.actor_library.loadTexturesAndSprites(asset);
            Sprite boundSprite = FallbackSprite(folder);
            if (boundSprite != null)
            {
                asset._cached_sprite = boundSprite;
                asset.cached_sprite = boundSprite;
            }
            else
            {
                asset._cached_sprite = asset._cached_sprite ?? FallbackSprite(folder);
                asset.cached_sprite = asset._cached_sprite;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路] 天地之魄载体贴图绑定失败，将使用天地之魄图标兜底: " + ex.Message);
        }
    }

    private static void ApplyDefaultSubspeciesTraits(ActorAsset asset)
    {
        if (asset == null) return;
        try { asset.default_subspecies_traits = new List<string>(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #5: " + mclslEmptyCatchEx.Message); }
        for (int i = 0; i < WorldSoulSubspeciesTraitIds.Length; i++)
        {
            string traitId = WorldSoulSubspeciesTraitIds[i];
            try
            {
                if (AssetManager.subspecies_traits?.get(traitId) != null)
                    asset.addSubspeciesTrait(traitId);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #6: " + mclslEmptyCatchEx.Message); }
        }
    }

    internal static void NormalizeActor(Actor actor)
    {
        if (actor?.data == null) return;
        try { NormalizeSubspecies(actor.subspecies); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #7: " + mclslEmptyCatchEx.Message); }
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            for (int i = 0; i < WorldSoulForbiddenSubspeciesTraitIds.Length; i++)
            {
                string traitId = WorldSoulForbiddenSubspeciesTraitIds[i];
                try { if (!string.IsNullOrWhiteSpace(traitId) && actor.hasTrait(traitId)) actor.removeTrait(traitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #8: " + mclslEmptyCatchEx.Message); }
            }
        });
        TryClearInventory(actor);
        ResetActorOrientation(actor);
    }


    internal static bool IsBlockedControlStatus(string statusId)
    {
        if (string.IsNullOrWhiteSpace(statusId)) return false;
        string id = statusId.Trim().ToLowerInvariant();
        return id == "surprised" || id == "stunned" || id == "stun" || id == "sleeping" || id == "sleep"
            || id == "shocked" || id == "shock" || id == "frozen" || id == "freeze" || id == "dazed" || id == "daze";
    }

    private static void ResetActorOrientation(Actor actor)
    {
        if (actor == null) return;
        SetNumberIfExists(actor, 0f, "rotation", "default_rotation", "defaultRotation", "angle", "sprite_angle", "spriteAngle", "z_rotation", "zRotation");
        SetNumberIfExists(actor.data, 0f, "rotation", "default_rotation", "defaultRotation", "angle", "sprite_angle", "spriteAngle", "z_rotation", "zRotation");
    }

    private static void SetNumberIfExists(object target, float value, params string[] names)
    {
        if (target == null || names == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = target.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    if (field.FieldType == typeof(float)) field.SetValue(target, value);
                    else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                    else if (field.FieldType == typeof(double)) field.SetValue(target, (double)value);
                    continue;
                }
                PropertyInfo property = type.GetProperty(name, flags);
                if (property?.CanWrite != true) continue;
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
                else if (property.PropertyType == typeof(double)) property.SetValue(target, (double)value);
            }
            catch { }
        }
    }

    private static Sprite WorldSoulIconSprite()
    {
        return LoadSprite("ui/Icons/" + ActorIconId)
            ?? LoadSprite("trait/" + ActorIconId);
    }

    internal static bool IsWorldSoulActor(Actor actor)
    {
        if (actor?.data == null) return false;
        try
        {
            if (IsWorldSoulAssetId(actor.asset?.id)) return true;
            if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return true;
            return actor.hasTrait(MclslTraitRegistration.WorldSoulEntityTraitId);
        }
        catch
        {
            return false;
        }
    }

    internal static bool TryGetRenderSprite(Actor actor, out Sprite sprite)
    {
        sprite = null;
        if (!IsWorldSoulActor(actor)) return false;
        sprite = GetWorldSoulSprite(actor) ?? FallbackSprite(FolderForActor(actor));
        return sprite != null && !string.IsNullOrWhiteSpace(sprite.name);
    }

    internal static void UpdateFrameData(Actor actor, Sprite sprite)
    {
        if (actor == null || sprite == null) return;
        ResetActorOrientation(actor);
        try
        {
            actor.checkAnimationContainer();
            if (actor.animation_container?.dict_frame_data == null) return;
            if (!string.IsNullOrWhiteSpace(sprite.name) && actor.animation_container.dict_frame_data.TryGetValue(sprite.name, out actor.frame_data))
                return;
            foreach (AnimationFrameData frameData in actor.animation_container.dict_frame_data.Values)
            {
                if (frameData == null) continue;
                actor.frame_data = frameData;
                return;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-9", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #9: " + mclslEmptyCatchEx.Message); }
    }

    private static Sprite GetWorldSoulSprite(Actor actor)
    {
        string folder = FolderForActor(actor);
        SoulSpriteSet set = GetSpriteSet(folder);
        if (actor?.data == null) return FirstFrame(set.Idle) ?? FallbackSprite(folder);
        if (!MclslActorAccessor.Alive(actor))
            return FrameForActor(actor, set, set.Death.Length > 0 ? set.Death : set.Idle, 5) ?? FallbackSprite(folder);

        if (IsActivelyAttacking(actor))
        {
            TryPulseAttack(actor, set);
            return FrameForActor(actor, set, set.Attack.Length > 0 ? set.Attack : set.Idle, 4) ?? FallbackSprite(folder);
        }
        long actorId = ActorId(actor);
        if (actorId > 0 && AttackUntilFrame.TryGetValue(actorId, out int attackUntil) && Time.frameCount <= attackUntil)
            return FrameForActor(actor, set, set.Attack.Length > 0 ? set.Attack : set.Idle, 4) ?? FallbackSprite(folder);
        if (IsMovingForSprite(actor))
            return FrameForActor(actor, set, set.Run.Length > 0 ? set.Run : set.Idle, 6) ?? FallbackSprite(folder);
        return FrameForActor(actor, set, set.Idle, 8) ?? FallbackSprite(folder);
    }

    private static void SetAssetBoolIfExists(ActorAsset asset, bool value, params string[] names)
    {
        if (asset == null || names == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = asset.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(asset, value);
                    continue;
                }
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null && property.CanWrite && property.PropertyType == typeof(bool))
                    property.SetValue(asset, value);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-10", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #10: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static void SetAssetNumberIfExists(ActorAsset asset, float value, params string[] names)
    {
        if (asset == null || names == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = asset.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    if (field.FieldType == typeof(float)) field.SetValue(asset, value);
                    else if (field.FieldType == typeof(int)) field.SetValue(asset, (int)value);
                    continue;
                }
                PropertyInfo property = type.GetProperty(name, flags);
                if (property?.CanWrite == true)
                {
                    if (property.PropertyType == typeof(float)) property.SetValue(asset, value);
                    else if (property.PropertyType == typeof(int)) property.SetValue(asset, (int)value);
                }
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-11", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #11: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static void SetDataStringIfExists(object data, string value, params string[] names)
    {
        if (data == null || names == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = data.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null && field.FieldType == typeof(string)) { field.SetValue(data, value); continue; }
                PropertyInfo property = type.GetProperty(name, flags);
                if (property?.CanWrite == true && property.PropertyType == typeof(string)) property.SetValue(data, value);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-12", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #12: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static void SetDataBoolIfExists(object data, bool value, params string[] names)
    {
        if (data == null || names == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = data.GetType();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null && field.FieldType == typeof(bool)) { field.SetValue(data, value); continue; }
                PropertyInfo property = type.GetProperty(name, flags);
                if (property?.CanWrite == true && property.PropertyType == typeof(bool)) property.SetValue(data, value);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-13", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #13: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static void TryClearInventory(Actor actor)
    {
        if (actor == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            object equipment = actor.GetType().GetField("equipment", flags)?.GetValue(actor)
                ?? actor.GetType().GetProperty("equipment", flags)?.GetValue(actor);
            if (equipment == null) return;
            MethodInfo clear = equipment.GetType().GetMethod("clear", flags) ?? equipment.GetType().GetMethod("Clear", flags);
            clear?.Invoke(equipment, null);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-14", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #14: " + mclslEmptyCatchEx.Message); }
    }

    private static void TrySetActorSize(ActorAsset asset)
    {
        if (asset == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = asset.GetType().GetField("actor_size", flags);
            if (field == null || !field.FieldType.IsEnum) return;
            object value = Enum.Parse(field.FieldType, "S13_Human");
            field.SetValue(asset, value);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-15", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #15: " + mclslEmptyCatchEx.Message); }
    }

    private static bool IsMovingForSprite(Actor actor)
    {
        long actorId = ActorId(actor);
        if (actorId <= 0) return false;

        int x = SafeX(actor);
        int y = SafeY(actor);
        int frame = Time.frameCount;
        bool hasLastX = LastSpriteX.TryGetValue(actorId, out int lastX);
        bool hasLastY = LastSpriteY.TryGetValue(actorId, out int lastY);
        bool hasLast = hasLastX && hasLastY;
        LastSpriteX[actorId] = x;
        LastSpriteY[actorId] = y;

        if (hasLast && (lastX != x || lastY != y))
            LastMovementFrame[actorId] = frame;

        return LastMovementFrame.TryGetValue(actorId, out int movedFrame) && frame - movedFrame <= 12;
    }

    private static Sprite Frame(Sprite[] frames, int ticksPerFrame)
    {
        if (frames == null || frames.Length == 0) return FallbackSprite(string.Empty);
        int index = Math.Abs(Time.frameCount / Math.Max(1, ticksPerFrame)) % frames.Length;
        return frames[index] ?? FallbackSprite(string.Empty);
    }

    private static Sprite FirstFrame(Sprite[] frames)
    {
        if (frames == null) return null;
        for (int i = 0; i < frames.Length; i++)
            if (frames[i] != null) return frames[i];
        return null;
    }

    private static Sprite FrameForActor(Actor actor, SoulSpriteSet set, Sprite[] frames, int ticksPerFrame)
    {
        Sprite sprite = Frame(frames, ticksPerFrame);
        return set.UsesGenericFallback ? TintFrame(actor, sprite) : sprite;
    }

    private static Sprite FallbackSprite(string folder)
    {
        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(folder))
        {
            sprite = FirstFrame(GetSpriteSet(folder).Idle);
            if (sprite != null) return sprite;
        }
        sprite = WorldSoulIconSprite();
        if (sprite != null) return sprite;
        return null;
    }

    private static Sprite RegisteredActorSprite(Actor actor)
    {
        try
        {
            if (actor == null || !IsWorldSoulAssetId(actor.asset?.id)) return null;
            Sprite sprite = actor?.asset?._cached_sprite ?? actor?.asset?.cached_sprite ?? actor?.asset?.getSpriteIcon();
            if (sprite != null
                && !string.IsNullOrWhiteSpace(sprite.name)
                && !sprite.name.Contains("iconDemon", StringComparison.OrdinalIgnoreCase)
                && !sprite.name.Contains("skeleton", StringComparison.OrdinalIgnoreCase))
            {
                return sprite;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-16", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #16: " + mclslEmptyCatchEx.Message); }
        return null;
    }

    private static Sprite TintFrame(Actor actor, Sprite sprite)
    {
        if (sprite == null || actor?.data == null) return sprite;
        string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return sprite;

        MclslWorldSoulRecord soul = FindWorldSoulById(soulId);
        Color tint = SoulTint(soulId, soul?.LawTags ?? string.Empty, soul?.Quality ?? 3);
        string cacheKey = sprite.GetInstanceID() + "|" + soulId + "|" + ColorKey(tint);
        if (TintedSpriteCache.TryGetValue(cacheKey, out Sprite cached)) return cached;

        Sprite tinted = CreateTintedSprite(sprite, tint);
        if (tinted == null) return sprite;
        TintedSpriteCache[cacheKey] = tinted;
        return tinted;
    }

    private static Sprite CreateTintedSprite(Sprite source, Color tint)
    {
        try
        {
            Texture2D sourceTexture = source.texture;
            if (sourceTexture == null) return source;
            Rect textureRect = source.textureRect;
            int x = Mathf.RoundToInt(textureRect.x);
            int y = Mathf.RoundToInt(textureRect.y);
            int width = Mathf.RoundToInt(textureRect.width);
            int height = Mathf.RoundToInt(textureRect.height);
            if (width <= 0 || height <= 0) return source;

            Color[] pixels = sourceTexture.GetPixels(x, y, width, height);
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                if (pixel.a <= 0.01f) continue;
                float light = Mathf.Clamp01((pixel.r + pixel.g + pixel.b) / 3f);
                pixel.r = Mathf.Clamp01(pixel.r * 0.38f + tint.r * light * 0.92f + tint.r * 0.08f);
                pixel.g = Mathf.Clamp01(pixel.g * 0.38f + tint.g * light * 0.92f + tint.g * 0.08f);
                pixel.b = Mathf.Clamp01(pixel.b * 0.38f + tint.b * light * 0.92f + tint.b * 0.08f);
                pixels[i] = pixel;
            }

            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "mclsl_world_soul_tint_" + ColorKey(tint)
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            Vector2 pivot = new(source.pivot.x / source.rect.width, source.pivot.y / source.rect.height);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = source.name + "_mclsl_tinted_" + ColorKey(tint);
            return sprite;
        }
        catch
        {
            return source;
        }
    }

    private static Color SoulTint(string soulId, string lawTags, int quality)
    {
        List<Color> colors = new();
        string tags = lawTags ?? string.Empty;
        AddColorIf(tags, colors, new[] { "火", "炎", "焚", "余烬", "毁灭" }, new Color(1.00f, 0.30f, 0.12f));
        AddColorIf(tags, colors, new[] { "雷", "霆", "电" }, new Color(0.45f, 0.62f, 1.00f));
        AddColorIf(tags, colors, new[] { "木", "生命", "生机", "重生" }, new Color(0.20f, 0.95f, 0.48f));
        AddColorIf(tags, colors, new[] { "水", "冰", "寒", "霜" }, new Color(0.30f, 0.88f, 1.00f));
        AddColorIf(tags, colors, new[] { "金", "秩序", "惩戒" }, new Color(1.00f, 0.78f, 0.18f));
        AddColorIf(tags, colors, new[] { "土", "山", "稳定" }, new Color(0.76f, 0.58f, 0.30f));
        AddColorIf(tags, colors, new[] { "风", "速度" }, new Color(0.42f, 1.00f, 0.82f));
        AddColorIf(tags, colors, new[] { "杀", "终结", "戮" }, new Color(0.92f, 0.12f, 0.32f));
        AddColorIf(tags, colors, new[] { "影", "暗", "隐匿", "阴" }, new Color(0.50f, 0.28f, 0.95f));
        AddColorIf(tags, colors, new[] { "阳", "光" }, new Color(1.00f, 0.94f, 0.45f));
        AddColorIf(tags, colors, new[] { "空间", "迁跃", "虚空" }, new Color(0.62f, 0.45f, 1.00f));

        Color baseColor = colors.Count == 0 ? HashColor(soulId) : Average(colors);
        float boost = Math.Clamp((quality - 2) * 0.06f, 0f, 0.18f);
        return new Color(
            Mathf.Clamp01(baseColor.r + boost),
            Mathf.Clamp01(baseColor.g + boost),
            Mathf.Clamp01(baseColor.b + boost),
            1f);
    }

    private static void AddColorIf(string text, List<Color> colors, string[] keys, Color color)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(keys[i]) && text.Contains(keys[i], StringComparison.Ordinal))
            {
                colors.Add(color);
                return;
            }
        }
    }

    private static Color Average(List<Color> colors)
    {
        if (colors == null || colors.Count == 0) return Color.white;
        float r = 0f;
        float g = 0f;
        float b = 0f;
        for (int i = 0; i < colors.Count; i++)
        {
            r += colors[i].r;
            g += colors[i].g;
            b += colors[i].b;
        }
        return new Color(r / colors.Count, g / colors.Count, b / colors.Count, 1f);
    }

    private static Color HashColor(string seed)
    {
        int hash = StableHash(seed);
        float hue = (hash % 360) / 360f;
        return Color.HSVToRGB(hue, 0.68f, 1.00f);
    }

    private static string ColorKey(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255f);
        int g = Mathf.RoundToInt(color.g * 255f);
        int b = Mathf.RoundToInt(color.b * 255f);
        return r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }

    private static SoulSpriteSet GetSpriteSet(string folder)
    {
        folder ??= string.Empty;
        if (SpriteSets.TryGetValue(folder, out SoulSpriteSet cached)) return cached;

        Sprite[] idle = LoadActionFrames(folder, new[] { "idle", "breathing", "Idle" }, 128);
        Sprite[] run = LoadActionFrames(folder, new[] { "run", "walk", "Walk" }, 128);
        Sprite[] attack = LoadActionFrames(folder, new[] { "attack", "BasicAtk" }, 128);
        Sprite[] death = LoadActionFrames(folder, new[] { "death", "Dead" }, 128);
        bool genericFallback = false;
        if (folder.Length > 0 && idle.Length == 0 && run.Length == 0 && attack.Length == 0 && death.Length == 0)
        {
            SoulSpriteSet generic = GetSpriteSet(string.Empty);
            SoulSpriteSet fallback = new(generic.Idle, generic.Run, generic.Attack, generic.Death, true);
            SpriteSets[folder] = fallback;
            return fallback;
        }
        Sprite[] resolvedIdle;
        if (idle.Length > 0) resolvedIdle = idle;
        else if (run.Length > 0) resolvedIdle = run;
        else if (attack.Length > 0) resolvedIdle = attack;
        else resolvedIdle = folder.Length > 0 ? GetSpriteSet(string.Empty).Idle : Array.Empty<Sprite>();

        Sprite[] resolvedRun = run.Length > 0 ? run : resolvedIdle;
        Sprite[] resolvedAttack = attack.Length > 0 ? attack : resolvedIdle;
        Sprite[] resolvedDeath = death.Length > 0 ? death : resolvedIdle;
        SoulSpriteSet set = new(resolvedIdle, resolvedRun, resolvedAttack, resolvedDeath, genericFallback);
        SpriteSets[folder] = set;
        return set;
    }

    private static Sprite[] LoadFrames(string folder, string animation, string prefix, int count)
    {
        List<Sprite> frames = new();
        string basePath = ActorPath(folder) + animation + "/";
        for (int i = 1; i <= count; i++)
        {
            Sprite sprite = LoadFirstExistingSprite(
                basePath + prefix + "_" + i.ToString("00"),
                basePath + prefix + "_" + i,
                basePath + i.ToString("00"),
                basePath + i);
            if (sprite != null) frames.Add(sprite);
        }
        return frames.ToArray();
    }

    private static Sprite[] LoadActionFrames(string folder, string[] actions, int count)
    {
        if (actions == null || actions.Length == 0) return Array.Empty<Sprite>();
        for (int i = 0; i < actions.Length; i++)
        {
            Sprite[] flat = LoadFlatActionFrames(folder, actions[i], count);
            if (flat.Length > 0) return flat;

            Sprite[] legacy = LoadFrames(folder, actions[i], actions[i], count);
            if (legacy.Length > 0) return legacy;
        }
        return Array.Empty<Sprite>();
    }

    private static Sprite[] LoadFlatActionFrames(string folder, string action, int count)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(action)) return Array.Empty<Sprite>();
        List<Sprite> frames = new();
        string basePath = ActorPath(folder);
        string lower = action.ToLowerInvariant();
        string upper = char.ToUpperInvariant(lower[0]) + lower.Substring(1);
        for (int i = 0; i < count; i++)
        {
            Sprite sprite = LoadFirstExistingSprite(
                basePath + folder + "_" + lower + "_" + i.ToString("000"),
                basePath + folder + "_" + lower + "_" + i.ToString("00"),
                basePath + folder + "_" + lower + "_" + i,
                basePath + folder + "_" + upper + "_" + i.ToString("000"),
                basePath + folder + "_" + upper + "_" + i.ToString("00"),
                basePath + folder + "_" + upper + "_" + i);
            if (sprite != null) frames.Add(sprite);
            else if (i > 0 && frames.Count > 0) break;
        }
        return frames.ToArray();
    }

    private static Sprite LoadFirstExistingSprite(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            Sprite sprite = LoadSprite(paths[i]);
            if (sprite != null) return sprite;
        }
        return null;
    }

    private static Sprite LoadSprite(string path)
    {
        try
        {
            return SpriteTextureLoader.getSprite(path)
                ?? SpriteTextureLoader.getSprite("GameResources/" + path)
                ?? Resources.Load<Sprite>("GameResources/" + path)
                ?? Resources.Load<Sprite>(path);
        }
        catch
        {
            return null;
        }
    }

    private static bool MclslWorldSoulActorRegistrationMarker(Actor actor)
    {
        try { return !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty)); }
        catch { return false; }
    }

    private static int SafeX(Actor actor)
    {
        try { return actor?.data?.x ?? 0; }
        catch { return 0; }
    }

    private static int SafeY(Actor actor)
    {
        try { return actor?.data?.y ?? 0; }
        catch { return 0; }
    }

    private static long ActorId(Actor actor)
    {
        try { return actor?.data?.id ?? 0L; }
        catch { return 0L; }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }

    private static string ActorPath(string folder)
    {
        return string.IsNullOrWhiteSpace(folder)
            ? "actors/" + RootFolder + "/"
            : "actors/" + RootFolder + "/" + folder + "/";
    }

    private static string FolderForActor(Actor actor)
    {
        try
        {
            string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
            if (!string.IsNullOrWhiteSpace(soulId))
            {
                MclslWorldSoulRecord soul = FindWorldSoulById(soulId);
                string folder = FolderForSoul(soul);
                if (!string.IsNullOrWhiteSpace(folder)) return folder;
                if (DefinitionsBySoulId.TryGetValue(soulId, out SoulActorDefinition fromId)) return fromId.Folder;
            }

            string actorAssetId = actor?.asset?.id ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(actorAssetId) && DefinitionsByActorId.TryGetValue(actorAssetId, out SoulActorDefinition fromAsset))
                return fromAsset.Folder;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslWorldSoulActorRegistration-cs-17", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslWorldSoulActorRegistration.cs #17: " + mclslEmptyCatchEx.Message); }
        return string.Empty;
    }

    private static SoulActorDefinition ResolveDefinition(MclslWorldSoulRecord soul)
    {
        RegisterSoulDefinitions();
        if (soul == null) return null;
        if (!string.IsNullOrWhiteSpace(soul.Id) && DefinitionsBySoulId.TryGetValue(soul.Id, out SoulActorDefinition exact))
            return exact;

        string tags = (soul.LawTags ?? string.Empty) + "," + (soul.Name ?? string.Empty);
        SoulActorDefinition best = null;
        int bestScore = 0;
        SoulActorDefinition[] definitions = MclslWorldSoulActorDefinitions.All;
        for (int i = 0; i < definitions.Length; i++)
        {
            SoulActorDefinition definition = definitions[i];
            int score = definition.MatchScore(tags);
            if (score > bestScore)
            {
                bestScore = score;
                best = definition;
            }
        }
        return best;
    }

    private static MclslWorldSoulRecord FindWorldSoulById(string soulId)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || string.IsNullOrWhiteSpace(soulId)) return null;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul != null && soul.Id == soulId) return soul;
        }
        return null;
    }

}
