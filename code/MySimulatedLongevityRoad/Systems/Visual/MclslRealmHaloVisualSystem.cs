using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

internal static class MclslRealmHaloVisualSystem
{
    private const float BackZOffset = -0.05f;
    private const int FrameStep = 3;
    private const float FrameIntervalSeconds = 0.08f;
    private const int VisibleScanCadenceFrames = 10;
    private const int CleanupIntervalFrames = 45;
    private const int StaleFrameThreshold = 24;

    private static readonly Dictionary<string, Sprite[]> FramesByFolder = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, HaloEntry> EntriesByActorId = new(64);
    private static readonly List<long> CleanupBuffer = new(64);
    private static int _lastCleanupFrame = -1;
    private static int _lastVisibleScanFrame = -1;

    internal static bool BeginRenderFrame(int frame)
    {
        bool shouldScan = _lastVisibleScanFrame < 0
            || frame - _lastVisibleScanFrame >= VisibleScanCadenceFrames;
        if (shouldScan) _lastVisibleScanFrame = frame;
        return shouldScan;
    }

    internal static void ObserveVisibleActor(Actor actor, Vector3 position, int frame)
    {
        if (!TryResolveProfile(actor, out long actorId, out HaloProfile profile)) return;

        Sprite[] frames = GetFrames(profile.Folder);
        if (frames == null || frames.Length == 0) return;

        HaloEntry entry = GetOrCreateEntry(actor, actorId, profile, position, frames);
        if (entry == null) return;
        entry.Actor = actor;
        entry.LastSeenFrame = frame;
        entry.LastRenderPosition = position;

        if (!string.Equals(entry.Folder, profile.Folder, StringComparison.Ordinal)
            || !ReferenceEquals(entry.BoundFrames, frames))
        {
            if (MclslWorldSpriteRenderLayer.TryBindLoopAnimation(
                    entry.Effect,
                    frames,
                    FrameIntervalSeconds,
                    (int)(actorId % frames.Length)))
            {
                entry.Folder = profile.Folder;
                entry.BoundFrames = frames;
                DisableNativeAnimator(entry.Effect);
            }
        }
    }

    internal static void EndRenderFrame(int frame, int lodLevel)
    {
        UpdateExistingEntries(frame, lodLevel);
        if (_lastCleanupFrame < 0 || frame - _lastCleanupFrame >= CleanupIntervalFrames)
        {
            Cleanup(frame, false);
        }
    }

    internal static void Clear()
    {
        foreach (HaloEntry entry in EntriesByActorId.Values) DestroyEntry(entry);
        EntriesByActorId.Clear();
        CleanupBuffer.Clear();
        FramesByFolder.Clear();
        _lastCleanupFrame = -1;
        _lastVisibleScanFrame = -1;
        MclslWorldSpriteRenderLayer.ResetActorLayerCache();
    }

    private static void UpdateExistingEntries(int frame, int lodLevel)
    {
        foreach (HaloEntry entry in EntriesByActorId.Values)
        {
            if (entry == null || entry.Transform == null || entry.Renderer == null || entry.Actor?.data == null)
            {
                continue;
            }

            if (!TryResolveProfile(entry.Actor, out long actorId, out HaloProfile profile)
                || !string.Equals(entry.Folder, profile.Folder, StringComparison.Ordinal))
            {
                if (entry.GameObject != null) entry.GameObject.SetActive(false);
                continue;
            }

            if (frame - entry.LastSeenFrame > VisibleScanCadenceFrames + 2)
            {
                if (entry.GameObject != null) entry.GameObject.SetActive(false);
                continue;
            }

            Sprite[] frames = entry.BoundFrames;
            if (frames == null || frames.Length == 0) continue;

            bool updatePosition = lodLevel < 2 || ((frame + (int)(actorId & 1L)) & 1) == 0;
            Vector3 position = entry.LastRenderPosition;
            if (updatePosition)
            {
                TryGetActorPosition(entry.Actor, ref position);
                entry.LastRenderPosition = position;
                entry.Transform.position = AnchorPosition(position, profile);
            }

            int spriteIndex;
            if (lodLevel >= 2)
            {
                spriteIndex = Math.Abs((int)(actorId % frames.Length));
            }
            else
            {
                int animationStride = lodLevel == 1 ? 3 : 1;
                spriteIndex = Math.Abs((frame / (FrameStep * animationStride)) + (int)(actorId % frames.Length)) % frames.Length;
            }

            Sprite sprite = frames[spriteIndex];
            if (!ReferenceEquals(entry.Renderer.sprite, sprite)) entry.Renderer.sprite = sprite;
            entry.Renderer.color = Color.white;
            entry.Transform.localScale = new Vector3(profile.Scale, profile.Scale, 1f);
            if (entry.GameObject != null) entry.GameObject.SetActive(true);
        }
    }

    private static bool TryResolveProfile(Actor actor, out long actorId, out HaloProfile profile)
    {
        actorId = 0L;
        profile = default;
        if (!MclslActorAccessor.Alive(actor)) return false;

        actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return false;

        string realm = ResolveRealmForHalo(actor);
        if (string.Equals(realm, MclslRealmIds.HuaShen, StringComparison.Ordinal))
        {
            profile = new HaloProfile("HuaShen", 0.00020f, 0.11f);
            return true;
        }

        if (string.Equals(realm, MclslRealmIds.HeDao, StringComparison.Ordinal))
        {
            profile = new HaloProfile("HeDao", 0.00022f, 0.12f);
            return true;
        }

        if (!string.Equals(realm, MclslRealmIds.ChangSheng, StringComparison.Ordinal)) return false;
        int taishangProgress = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0);
        profile = taishangProgress >= 100
            ? new HaloProfile("TaiShang", 0.00026f, 0.14f)
            : new HaloProfile("ChangSheng", 0.00024f, 0.13f);
        return true;
    }

    private static string ResolveRealmForHalo(Actor actor)
    {
        string realm = MclslActorAccessor.Realm(actor);
        if (!string.IsNullOrWhiteSpace(realm)) return realm;

        foreach (string candidate in MclslRealmIds.Ordered)
        {
            string newLawTrait = MclslTraitRegistration.TraitIdForRealm(candidate);
            string ancientTrait = MclslTraitRegistration.LegacyTraitIdForRealm(candidate);
            try
            {
                if (!string.IsNullOrWhiteSpace(newLawTrait) && actor.hasTrait(newLawTrait)) realm = candidate;
                if (!string.IsNullOrWhiteSpace(ancientTrait) && actor.hasTrait(ancientTrait)) realm = candidate;
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #1: " + mclslEmptyCatchEx.Message); }
        }

        return realm ?? string.Empty;
    }

    private static HaloEntry GetOrCreateEntry(
        Actor actor,
        long actorId,
        HaloProfile profile,
        Vector3 position,
        Sprite[] frames)
    {
        if (EntriesByActorId.TryGetValue(actorId, out HaloEntry entry)
            && entry != null
            && entry.Transform != null
            && entry.Renderer != null)
        {
            return entry;
        }

        try
        {
            if (!MclslWorldSpriteRenderLayer.TrySpawnActorBackEffect(
                    AnchorPosition2D(position, profile),
                    profile.Scale,
                    -1,
                    out BaseEffect effect,
                    out SpriteRenderer renderer))
            {
                return null;
            }

            Component component = effect as Component;
            if (component == null)
            {
                effect.kill();
                return null;
            }

            component.gameObject.name = "mclsl_realm_halo_" + actorId;
            component.gameObject.hideFlags = HideFlags.DontSave;
            if (!MclslWorldSpriteRenderLayer.TryBindLoopAnimation(
                    effect,
                    frames,
                    FrameIntervalSeconds,
                    (int)(actorId % frames.Length)))
            {
                effect.kill();
                return null;
            }
            DisableNativeAnimator(effect);

            renderer.color = Color.white;
            entry = new HaloEntry(actor, effect, component.gameObject, component.transform, renderer, profile.Folder, frames, position);
            EntriesByActorId[actorId] = entry;
            return entry;
        }
        catch
        {
            return null;
        }
    }

    private static Vector3 AnchorPosition(Vector3 actorPosition, HaloProfile profile)
    {
        return new Vector3(actorPosition.x, actorPosition.y + profile.YOffset, actorPosition.z + BackZOffset);
    }

    private static Vector2 AnchorPosition2D(Vector3 actorPosition, HaloProfile profile)
    {
        return new Vector2(actorPosition.x, actorPosition.y + profile.YOffset);
    }

    private static Sprite[] GetFrames(string folder)
    {
        if (FramesByFolder.TryGetValue(folder, out Sprite[] cached)) return cached;

        List<Sprite> frames = new(24);
        for (int i = 1; i <= 64; i++)
        {
            Sprite sprite = LoadSprite("effects/halo/" + folder + "/" + i)
                ?? LoadSprite("effects/halo/" + folder + "/" + i + ".png");
            if (sprite != null)
            {
                frames.Add(sprite);
                continue;
            }

            if (frames.Count > 0) break;
        }

        Sprite[] result = frames.Count > 0 ? frames.ToArray() : Array.Empty<Sprite>();
        FramesByFolder[folder] = result;
        return result;
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

    private static void TryGetActorPosition(Actor actor, ref Vector3 position)
    {
        try
        {
            position = actor.current_position;
            return;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #2: " + mclslEmptyCatchEx.Message); }

        try
        {
            WorldTile tile = ((BaseSimObject)actor).current_tile;
            if (tile != null) position = tile.posV3;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #3: " + mclslEmptyCatchEx.Message); }
    }

    private static void Cleanup(int frame, bool force)
    {
        _lastCleanupFrame = frame;
        CleanupBuffer.Clear();
        foreach (KeyValuePair<long, HaloEntry> pair in EntriesByActorId)
        {
            HaloEntry entry = pair.Value;
            bool remove = force
                || entry == null
                || entry.Actor?.data == null
                || !entry.Actor.isAlive()
                || frame - entry.LastSeenFrame > StaleFrameThreshold;
            if (remove) CleanupBuffer.Add(pair.Key);
        }

        for (int i = 0; i < CleanupBuffer.Count; i++)
        {
            long actorId = CleanupBuffer[i];
            if (EntriesByActorId.TryGetValue(actorId, out HaloEntry entry)) DestroyEntry(entry);
            EntriesByActorId.Remove(actorId);
        }
        CleanupBuffer.Clear();
    }

    private static void DestroyEntry(HaloEntry entry)
    {
        try { entry?.Effect?.kill(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #4: " + mclslEmptyCatchEx.Message); }
        try
        {
            if (entry?.GameObject != null) UnityEngine.Object.Destroy(entry.GameObject);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #5: " + mclslEmptyCatchEx.Message); }
    }

    private static void DisableNativeAnimator(BaseEffect effect)
    {
        try
        {
            Component component = effect as Component;
            SpriteAnimation spriteAnimation = component != null ? component.GetComponent<SpriteAnimation>() : null;
            if (spriteAnimation != null)
            {
                spriteAnimation.isOn = false;
                if (spriteAnimation is Behaviour spriteAnimationBehaviour)
                    spriteAnimationBehaviour.enabled = false;
            }
            Behaviour animator = component != null ? component.GetComponent("SpriteAnimator") as Behaviour : null;
            if (animator != null) animator.enabled = false;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslRealmHaloVisualSystem-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslRealmHaloVisualSystem.cs #6: " + mclslEmptyCatchEx.Message); }
    }

    private readonly struct HaloProfile
    {
        internal HaloProfile(string folder, float scale, float yOffset)
        {
            Folder = folder;
            Scale = scale;
            YOffset = yOffset;
        }

        internal string Folder { get; }
        internal float Scale { get; }
        internal float YOffset { get; }
    }

    private sealed class HaloEntry
    {
        internal HaloEntry(
            Actor actor,
            BaseEffect effect,
            GameObject gameObject,
            Transform transform,
            SpriteRenderer renderer,
            string folder,
            Sprite[] frames,
            Vector3 position)
        {
            Actor = actor;
            Effect = effect;
            GameObject = gameObject;
            Transform = transform;
            Renderer = renderer;
            Folder = folder;
            BoundFrames = frames;
            LastRenderPosition = position;
            LastSeenFrame = Time.frameCount;
        }

        internal Actor Actor { get; set; }
        internal BaseEffect Effect { get; }
        internal GameObject GameObject { get; }
        internal Transform Transform { get; }
        internal SpriteRenderer Renderer { get; }
        internal string Folder { get; set; }
        internal Sprite[] BoundFrames { get; set; }
        internal Vector3 LastRenderPosition { get; set; }
        internal int LastSeenFrame { get; set; }
    }
}
