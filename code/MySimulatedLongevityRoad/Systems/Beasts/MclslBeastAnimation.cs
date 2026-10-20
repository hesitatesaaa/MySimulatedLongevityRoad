using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.Systems.Visual;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastAnimation
{
    private sealed class Clock
    {
        internal readonly MclslActorAnimationState State = new();
        internal int X;
        internal int Y;
        internal double LastMove = double.NegativeInfinity;
        internal double LastNow = double.NegativeInfinity;
        internal bool PositionKnown;
        internal Sprite RenderSprite;
        internal bool DeathPlayed;
        internal MclslBeastDeathVisualSystem.Pose DeathPose;
    }

    internal sealed class Clip
    {
        internal readonly MclslAnimationClipData Data;
        internal readonly bool Complete;
        private readonly string _path;
        internal Clip(string path, MclslAnimationClipData data)
        {
            _path = path;
            Data = data;
            Complete = data?.Names.Length > 0;
            MclslActorAnimationCatalog.Form form = MclslActorAnimationCatalog.Get(path);
            if (form?.NativeResourcePath == null)
                throw new InvalidOperationException("Current actor resource group is missing: " + path);
            for (int i = 0; i < (data?.Names.Length ?? 0); i++)
            {
                string resource = path + "/main/" + data.Names[i];
                MclslNativeActorFrames.Register(resource, form);
            }
        }
        internal double Duration => Data?.Duration ?? 0;
        internal Sprite At(double elapsed)
        {
            if (!Complete) return null;
            int index = Data.FrameIndex(elapsed);
            return MclslNativeActorFrames.Request(_path + "/main/" + Data.Names[index]);
        }
    }

    private sealed class Frames
    {
        internal readonly Clip[] Actions = new Clip[5];
        internal Sprite Fallback;
        internal bool Valid;
    }

    private static ConditionalWeakTable<Actor, Clock> Clocks = new();
    private static readonly Dictionary<string, Frames> Sets = new(StringComparer.Ordinal);

    internal static void Forget(Actor actor)
    {
        if (actor == null) return;
        Clocks.Remove(actor);
        MclslBeastDeathVisualSystem.Forget(actor);
    }

    internal static void Clear()
    {
        Clocks = new ConditionalWeakTable<Actor, Clock>();
        MclslActorPresentation.ClearRuntime();
        // Immutable shared resources survive world loads without duplicate textures.
        MclslActorAnimationTime.Clear();
        MclslBeastDeathVisualSystem.Clear();
        MclslNativeActorFrames.Clear();
    }

    internal static void MarkAttack(Actor actor)
    {
        if (!IsSupported(actor)) return;
        MclslActorAnimationTime.Sample();
        Clock clock = Clocks.GetOrCreateValue(actor);
        clock.State.Attack(MclslActorAnimationTime.Now);
    }

    internal static void MarkHit(Actor actor)
    {
        if (!IsSupported(actor)) return;
        MclslActorAnimationTime.Sample();
        Clock clock = Clocks.GetOrCreateValue(actor);
        double duration = GetFrames(PathFor(actor.asset.id))?.Actions[(int)MclslActorAction.Hit].Duration ?? 0;
        clock.State.Hit(MclslActorAnimationTime.Now, duration);
    }

    internal static MclslBeastDeathVisualSystem.Pose CaptureDeathPose(Actor actor)
        => IsSupported(actor) ? MclslBeastDeathVisualSystem.Capture(actor) : default;

    internal static void PrepareDeath(Actor actor)
    {
        MclslActorAnimationTime.Sample();
        if (IsSupported(actor)) Clocks.GetOrCreateValue(actor).DeathPose = CaptureDeathPose(actor);
    }

    internal static void MarkDeath(Actor actor)
    {
        if (IsSupported(actor)) MarkDeath(actor, Clocks.GetOrCreateValue(actor).DeathPose);
    }

    internal static void MarkDeath(Actor actor, MclslBeastDeathVisualSystem.Pose pose)
    {
        if (!IsSupported(actor) || actor.isAlive() || !pose.Valid) return;
        MclslActorAnimationTime.Sample();
        Clock clock = Clocks.GetOrCreateValue(actor);
        if (clock.DeathPlayed) return;
        Clip clip = GetFrames(PathFor(actor.asset.id))?.Actions[(int)MclslActorAction.Death];
        if (clip?.Complete == true)
            clock.DeathPlayed = MclslBeastDeathVisualSystem.Play(actor, pose, clip,
                GetFrames(PathFor(actor.asset.id))?.Fallback);
    }

    internal static bool TryGetRenderSprite(Actor actor, out Sprite sprite)
    {
        sprite = null;
        if (!IsSupported(actor)) return false;
        sprite = GetSprite(actor);
        return sprite != null;
    }

    internal static void UpdateFrameData(Actor actor, Sprite sprite)
    {
        if (actor == null || sprite == null) return;
        Clock clock = Clocks.GetOrCreateValue(actor);
        if (clock.RenderSprite == sprite) return;
        clock.RenderSprite = sprite;
        try
        {
            actor.checkAnimationContainer();
            if (actor.animation_container?.dict_frame_data == null) return;
            if (actor.animation_container.dict_frame_data.TryGetValue(sprite.name, out actor.frame_data)) return;
            foreach (AnimationFrameData data in actor.animation_container.dict_frame_data.Values)
            {
                if (data == null) continue;
                actor.frame_data = data;
                return;
            }
        }
        catch (Exception ex)
        {
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("beast-frame-data", ex.Message);
        }
    }

    internal static Sprite GetSprite(Actor actor)
    {
        if (!IsSupported(actor)) return actor?.asset?.cached_sprite;
        string path = PathFor(actor.asset.id);
        if (path == null) return null;
        Frames frames = GetFrames(path);
        if (frames == null) return null;
        Sprite fallback = frames.Fallback;
        if (!frames.Valid) return fallback;
        Clock clock = Clocks.GetOrCreateValue(actor);
        if (!actor.isAlive() && clock.DeathPlayed) return MclslActorPixelResources.TransparentSprite;
        if (!actor.isAlive()) return frames.Actions[(int)MclslActorAction.Death].At(double.PositiveInfinity) ?? fallback;
        double now = MclslActorAnimationTime.Now;
        MclslActorAction action = clock.State.Select(now, IsMoving(actor, clock, now),
            frames.Actions[(int)MclslActorAction.Attack].Duration,
            frames.Actions[(int)MclslActorAction.Hit].Duration, out double elapsed);
        return frames.Actions[(int)action].At(elapsed) ?? fallback;
    }

    internal static Sprite ConfigureAsset(ActorAsset asset, string formPath)
    {
        string iconId = asset.id.EndsWith("_yuan", StringComparison.Ordinal) ? asset.id.Substring(0, asset.id.Length - 5) : asset.id;
        Sprite icon = MclslActorPixelResources.PlacementIcon(iconId);
        if (icon == null) throw new InvalidOperationException("Current actor icon is missing: " + iconId);
        asset.icon = asset.id;
        asset.show_icon_inspect_window_id = asset.id;
        SpriteTextureLoader._cached_sprites["ui/Icons/" + asset.id] = icon;
        // Whole-body authored frames have no separate phenotype/head palette.
        // Bypass both skin recoloring and its cached atlas of earlier frames.
        asset.need_colored_sprite = false;
        // Registration is on the main thread. Warm this sprite before native
        // Parallel.For can request it to hide a corpse beneath the death visual.
        _ = MclslActorPixelResources.TransparentSprite;
        Frames frames = GetFrames(formPath.TrimEnd('/'));
        Clip idle = frames.Actions[(int)MclslActorAction.Idle], walk = frames.Actions[(int)MclslActorAction.Walk];
        List<string> availableIdle = new();
        availableIdle.AddRange(idle.Data.Names);
        asset.animation_idle = availableIdle.ToArray();
        asset.animation_walk = walk.Complete ? walk.Data.Names : asset.animation_idle;
        asset.animation_swim = asset.animation_walk;
        asset.animation_idle_speed = (float)Math.Max(0.001d, idle.Duration > 0 ? idle.Duration / Math.Max(1, idle.Data.Names.Length) : 0.14d);
        asset.animation_walk_speed = walk.Complete
            ? (float)(walk.Duration / asset.animation_walk.Length) : asset.animation_idle_speed;
        asset.animation_swim_speed = asset.animation_walk_speed;
        asset.animation_speed_based_on_walk_speed = false;
        asset.disable_jump_animation = true;
        asset.get_override_sprite = GetSprite;
        asset.has_override_sprite = true;
        MclslActorPresentation.Configure(asset, frames.Fallback);
        return frames.Fallback;
    }

    private static bool IsMoving(Actor actor, Clock clock, double now)
    {
        if (actor?.data == null) return false;
        if (now < clock.LastNow) { clock.PositionKnown = false; clock.LastMove = double.NegativeInfinity; }
        clock.LastNow = now;
        int x = actor.data.x, y = actor.data.y;
        bool moving = actor.is_moving;
        try { moving |= actor.isUsingPath(); } catch { }
        if (clock.PositionKnown && (clock.X != x || clock.Y != y)) moving = true;
        if (moving) clock.LastMove = now;
        clock.X = x;
        clock.Y = y;
        clock.PositionKnown = true;
        return moving || now - clock.LastMove <= 0.2d;
    }

    private static Frames GetFrames(string path)
    {
        if (path == null) return null;
        path = path.TrimEnd('/');
        if (Sets.TryGetValue(path, out Frames existing)) return existing;
        Frames frames = new() { Valid = true };
        for (int i = 0; i < frames.Actions.Length; i++)
        {
            Clip clip = new(path, MclslActorAnimationCatalog.Clip(path, MclslActorAnimationCatalog.ActionNames[i]));
            frames.Actions[i] = clip;
            frames.Valid &= clip.Complete;
        }
        if (frames.Actions[0].Data?.Names.Length > 0)
            frames.Fallback = MclslNativeActorFrames.Initial(path + "/main/" + frames.Actions[0].Data.Names[0]);
        Sets[path] = frames;
        if (!frames.Valid)
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("actor-animation-asset",
                "动画资源未加载完整: " + path + "/main/{idle,walk,attack,hit,death}_N");
        return frames;
    }

    private static bool IsSupported(Actor actor)
        => actor?.asset != null && (MclslBeastCatalog.ForAsset(actor.asset.id) != null
            || MclslNamedCharacterRegistration.IsNamedAsset(actor.asset.id)
            || MclslImmortalActorRegistration.IsImmortal(actor));

    private static string PathFor(string assetId)
    {
        MclslBeastDefinition beast = MclslBeastCatalog.ForAsset(assetId);
        if (beast != null)
            return beast.ResourceFolder + "/"
                + (assetId == beast.AscendedAssetId ? "Human" : "Beast");
        if (assetId == MclslImmortalActorRegistration.BaiId) return "actors/Immortals/Bai";
        if (assetId == MclslImmortalActorRegistration.ChuanfaId) return "actors/Immortals/Chuanfa";
        string namedFolder = MclslNamedCharacterRegistration.FolderFor(assetId);
        return namedFolder == null ? null : "actors/Named/" + namedFolder;
    }
}
