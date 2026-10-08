using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        internal int AttackStart = -1000;
        internal int HitStart = -1000;
        internal int X;
        internal int Y;
        internal int LastMove = -1000;
        internal bool PositionKnown;
        internal Sprite RenderSprite;
        internal byte Mode = byte.MaxValue;
        internal int StateStart;
    }

    private sealed class Frames
    {
        internal Sprite[] Idle;
        internal Sprite[] Walk;
        internal Sprite[] Attack;
        internal Sprite[] Hit;
        internal Sprite[] Death;
        internal int LastLoad;
        internal bool Valid;
    }

    private static ConditionalWeakTable<Actor, Clock> Clocks = new();
    private static readonly Dictionary<string, Frames> Sets = new(StringComparer.Ordinal);

    internal static void Forget(Actor actor)
    {
        if (actor != null) Clocks.Remove(actor);
    }

    internal static void Clear()
    {
        Clocks = new ConditionalWeakTable<Actor, Clock>();
        Sets.Clear();
        MclslBeastDeathVisualSystem.Clear();
    }

    internal static void MarkAttack(Actor actor)
    {
        if (!IsSupported(actor)) return;
        Clock clock = Clocks.GetOrCreateValue(actor);
        clock.AttackStart = Time.frameCount;
        clock.Mode = byte.MaxValue;
    }

    internal static void MarkHit(Actor actor)
    {
        if (!IsSupported(actor)) return;
        Clock clock = Clocks.GetOrCreateValue(actor);
        clock.HitStart = Time.frameCount;
        clock.Mode = byte.MaxValue;
    }

    internal static void MarkDeath(Actor actor)
    {
        if (!IsSupported(actor)) return;
        string path = PathFor(actor.asset.id);
        Frames frames = GetFrames(path);
        if (frames?.Death == null || frames.Death.Length == 0 || frames.Death[0] == null) return;
        Vector2 position = actor.data == null ? Vector2.zero : new Vector2(actor.data.x, actor.data.y);
        MclslBeastDeathVisualSystem.Play(position, actor.asset.base_stats["scale"], frames.Death);
    }

    internal static bool TryGetRenderSprite(Actor actor, out Sprite sprite)
    {
        sprite = null;
        if (!IsSupported(actor)) return false;
        sprite = GetSprite(actor);
        string path = PathFor(actor.asset.id);
        return path != null && Sets.TryGetValue(path, out Frames frames)
            && frames.Valid && sprite != null;
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
        if (path == null) return actor.asset.cached_sprite;
        int now = Time.frameCount;
        Frames frames = GetFrames(path);
        if (frames == null || !frames.Valid) return actor.asset.cached_sprite;
        Clock clock = Clocks.GetOrCreateValue(actor);
        Sprite[] selected;
        byte mode;
        int speed;
        if (!actor.isAlive()) return frames.Death[frames.Death.Length - 1];
        if (now - clock.HitStart < 20)
        {
            selected = frames.Hit;
            mode = 3;
            speed = 5;
        }
        else if (now - clock.AttackStart < 36)
        {
            selected = frames.Attack;
            mode = 2;
            speed = 6;
        }
        else if (IsMoving(actor, clock, now))
        {
            selected = frames.Walk;
            mode = 1;
            speed = 10;
        }
        else
        {
            selected = frames.Idle;
            mode = 0;
            speed = 14;
        }
        if (clock.Mode != mode)
        {
            clock.Mode = mode;
            clock.StateStart = now;
        }
        int index = Math.Max(0, (now - clock.StateStart) / speed);
        if (mode < 2) index %= selected.Length;
        else index = Math.Min(selected.Length - 1, index);
        Sprite sprite = selected[index];
        return sprite;
    }

    internal static Sprite LoadSprite(string path)
    {
        Sprite sprite = SpriteTextureLoader.getSprite(path)
            ?? SpriteTextureLoader.getSprite("GameResources/" + path);
        if (sprite?.texture != null)
        {
            sprite.texture.filterMode = FilterMode.Point;
            sprite.texture.wrapMode = TextureWrapMode.Clamp;
        }
        return sprite;
    }

    private static bool IsMoving(Actor actor, Clock clock, int now)
    {
        if (actor?.data == null) return false;
        int x = actor.data.x, y = actor.data.y;
        bool moving = actor.is_moving;
        try { moving |= actor.isUsingPath(); } catch { }
        if (clock.PositionKnown && (clock.X != x || clock.Y != y)) moving = true;
        if (moving) clock.LastMove = now;
        clock.X = x;
        clock.Y = y;
        clock.PositionKnown = true;
        return moving || now - clock.LastMove <= 12;
    }

    private static Sprite[] Load(string path, string action, int count)
    {
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++)
            frames[i] = LoadSprite(path + "/main/" + action + "_" + i);
        return frames;
    }

    private static Frames GetFrames(string path)
    {
        if (path == null) return null;
        int now = Time.frameCount;
        if (Sets.TryGetValue(path, out Frames existing)
            && (existing.Valid || now - existing.LastLoad < 120)) return existing;
        Frames frames = new Frames
        {
            Idle = Load(path, "idle", 4), Walk = Load(path, "walk", 4),
            Attack = Load(path, "attack", 6), Hit = Load(path, "hit", 4),
            Death = Load(path, "death", 4), LastLoad = now
        };
        frames.Valid = Complete(frames.Idle) && Complete(frames.Walk)
            && Complete(frames.Attack) && Complete(frames.Hit) && Complete(frames.Death);
        Sets[path] = frames;
        if (!frames.Valid)
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("actor-animation-asset",
                "动画资源未加载完整: " + path + "/main/{idle,walk,attack,hit,death}_N");
        return frames;
    }

    private static bool Complete(Sprite[] frames)
    {
        foreach (Sprite frame in frames) if (frame == null) return false;
        return true;
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
