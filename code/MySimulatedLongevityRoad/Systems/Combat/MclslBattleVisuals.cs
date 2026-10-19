using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems.Visual;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

// Choreography adapted from CombatGods Motion/Clones/Strikes: only copies move.
// The native actor transform, physics and animation ownership are never changed.
internal static class MclslBattleVisuals
{
    private sealed class Ghost
    {
        internal GameObject Object;
        internal SpriteRenderer Renderer;
        internal Actor Owner, Target;
        internal Vector3 Start, End, Scale;
        internal double Born;
        internal float Duration;
        internal string Skill;
    }
    private static readonly List<Ghost> Live = new();
    private static readonly Stack<Ghost> Pool = new();
    internal static void Begin(Actor caster, Actor target, string skillId, float totalDuration)
    {
        if (!MclslActorAccessor.Alive(caster) || !MclslSpellEffect.Enabled) return;
        MclslBattlePrimitives.Begin(caster,target,skillId,totalDuration);
        if(skillId is not ("C001" or "C003" or "C005" or "C007" or "C008" or "C010" or "C016"))return;
        if (Live.Count >= 24 || !Visible(caster.current_position)) return;
        Sprite sprite = MclslBeastAnimation.GetSprite(caster) ?? caster.asset?.cached_sprite;
        if (sprite == null || !MclslSpellEffect.TryReserveExternal()) return;
        Ghost ghost = Pool.Count > 0 ? Pool.Pop() : new Ghost();
        if (ghost.Object == null && !MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(caster.current_position, 1, 2, sprite, out ghost.Object, out ghost.Renderer))
        { MclslSpellEffect.ReleaseExternal(); return; }
        var pose = MclslBeastDeathVisualSystem.Capture(caster);
        ghost.Owner = caster; ghost.Target = target; ghost.Skill = skillId;
        ghost.Start = pose.Position; ghost.End = target == null ? pose.Position : (Vector3)target.current_position;
        ghost.Scale = pose.Valid?pose.Scale:Vector3.one; ghost.Born = MclslSpellCasting.Now;
        ghost.Duration = Mathf.Clamp(totalDuration, .12f, 2f);
        ghost.Renderer.sprite = sprite; ghost.Renderer.flipX = pose.Flip;
        ghost.Object.SetActive(true); Live.Add(ghost); TickGhost(ghost);
    }
    internal static void BeastRush(Actor owner,Actor target,Sprite sprite,float scale)
    {
        int before=Live.Count;Begin(owner,target,"C016",.65f);
        if(Live.Count==before)return;
        Ghost ghost=Live[Live.Count-1];ghost.Renderer.sprite=sprite;ghost.Scale=Vector3.one*scale;
    }
    internal static bool Visible(Vector3 position)
    {
        Camera camera = Camera.main;
        if (camera == null) return true;
        Vector3 p = camera.WorldToViewportPoint(position);
        return p.x >= -.1f && p.x <= 1.1f && p.y >= -.1f && p.y <= 1.1f && p.z >= camera.nearClipPlane && p.z <= camera.farClipPlane;
    }
    private static void TickGhost(Ghost g)
    {
        float p = Mathf.Clamp01((float)((MclslSpellCasting.Now - g.Born) / g.Duration));
        float beat = Mathf.Sin(p * Mathf.PI);
        Vector3 delta = g.End - g.Start;
        Vector3 direction = delta.sqrMagnitude > .001f ? delta.normalized : Vector3.right;
        Vector3 position = g.Start;
        float rotation = 0, scale = 1;
        switch (g.Skill)
        {
            case "C001": case "C005": position += direction * beat * Math.Min(3, delta.magnitude); rotation = -20 * direction.x * beat; break;
            case "C010": position -= direction * beat * 2; break;
            case "C003": position = Vector3.Lerp(g.Start, g.End, p); position.y += Mathf.Sin(p * 6 * Mathf.PI) * .7f; rotation = Mathf.Sin(p * 6 * Mathf.PI) * 35; break;
            case "C016": position = Vector3.Lerp(g.Start,g.End,p);position.y+=Mathf.Sin(p*Mathf.PI)*.8f;break;
            case "C007": position += direction * beat; position.y += Mathf.Sin(p * Mathf.PI) * 2; rotation = p < .65f ? -30 * beat : 35 * beat; break;
            case "C004": position = g.End + Vector3.up * (1 - p) * 5; scale = 1.5f; rotation = -80; break;
            case "C008": scale = 1 + beat * 2; position.y += beat; rotation = -20 * beat; break;
            default: scale = 1 + beat * .15f; break;
        }
        g.Object.transform.position = position;
        g.Object.transform.localScale = g.Scale * scale;
        g.Object.transform.rotation = Quaternion.Euler(0, 0, rotation);
        g.Renderer.color = new Color(.45f, .85f, 1f, .45f * (1 - p));
    }
    internal static void Tick()
    {
        MclslBattlePrimitives.Tick();
        for (int i = Live.Count - 1; i >= 0; i--)
        {
            Ghost g = Live[i];
            if (!MclslSpellEffect.Enabled || !MclslActorAccessor.Alive(g.Owner) || MclslSpellCasting.Now >= g.Born + g.Duration)
            {
                g.Object.SetActive(false); g.Owner = g.Target = null;
                Live.RemoveAt(i); Pool.Push(g); MclslSpellEffect.ReleaseExternal();
            }
            else TickGhost(g);
        }
    }
    internal static void Clear()
    {
        MclslBattlePrimitives.Clear();
        foreach (Ghost g in Live) { if (g.Object != null) UnityEngine.Object.Destroy(g.Object); MclslSpellEffect.ReleaseExternal(); }
        foreach (Ghost g in Pool) if (g.Object != null) UnityEngine.Object.Destroy(g.Object);
        Live.Clear(); Pool.Clear();
    }
}
