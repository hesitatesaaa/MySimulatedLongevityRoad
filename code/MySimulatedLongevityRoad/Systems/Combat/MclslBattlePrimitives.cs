using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Systems.Visual;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

// Runtime pixel quads, not additional artwork. Every quad shares the 96-effect budget.
internal static class MclslBattlePrimitives
{
    private sealed class Stroke
    {
        internal GameObject Object;
        internal SpriteRenderer Renderer;
        internal Vector3 Start,End;
        internal Vector2 Size;
        internal double Born;
        internal float Duration,Angle;
        internal Color Color;
    }
    private static readonly List<Stroke> Live=new();
    private static readonly Stack<Stroke> Pool=new();
    private static Sprite _pixel;
    private static Sprite Pixel
    {
        get
        {
            if(_pixel!=null)return _pixel;
            var texture=new Texture2D(1,1,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,hideFlags=HideFlags.DontSave};
            texture.SetPixel(0,0,Color.white);texture.Apply(false,true);
            return _pixel=Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
        }
    }
    private static void Add(Vector3 start,Vector3 end,Vector2 size,float angle,Color color,float duration)
    {
        if(!MclslBattleVisuals.Visible(start)||!MclslSpellEffect.TryReserveExternal())return;
        var s=Pool.Count>0?Pool.Pop():new Stroke();
        if(s.Object==null&&!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(start,1,3,Pixel,out s.Object,out s.Renderer))
        {MclslSpellEffect.ReleaseExternal();return;}
        s.Start=start;s.End=end;s.Size=size;s.Angle=angle;s.Color=color;s.Duration=duration;s.Born=MclslSpellCasting.Now;
        s.Renderer.sprite=Pixel;s.Object.SetActive(true);Live.Add(s);Render(s);
    }
    private static void Ring(Vector3 center,float radius,Color color,float duration)
    {
        for(int i=0;i<8;i++)
        {
            float angle=i*Mathf.PI/4;
            Vector3 point=center+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*radius;
            Add(point,point,new Vector2(radius*.65f,.1f),angle*Mathf.Rad2Deg+90,color,duration);
        }
    }
    internal static void Begin(Actor caster,Actor target,string id,float duration)
    {
        if(!MclslSpellEffect.Enabled)return;
        Vector3 start=caster.current_position,end=target==null?start:(Vector3)target.current_position;
        Color qi=ColorFor(caster,id);float windup=MclslBattleRules.Windup(id);
        if(MclslBattleRules.IsArea(id)||id=="C013")Ring(id=="C013"?start:end,id=="C013"?1.1f:2.5f,qi,windup);
        if(id=="C004")
        {
            Add(end+Vector3.up*6,end,new Vector2(.18f,4),0,new Color(.9f,.95f,1,.9f),duration);
            Add(end+Vector3.up*6.5f,end+Vector3.up*.5f,new Vector2(1.2f,.16f),0,qi,duration);
            return;
        }
        if(id is "C002" or "C006" or "C010" or "C015")Ring(start,.8f,qi,duration);
        if(id is "C001" or "C003" or "C005" or "C007" || MclslBattleRules.IsBeam(id))
        {
            Vector3 d=end-start;float angle=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg;
            int strokes=id is "C003" or "C007"?3:1;
            for(int i=0;i<strokes;i++)
                Add(Vector3.Lerp(start,end,.5f)+Vector3.up*(i-1)*.3f,
                    Vector3.Lerp(start,end,.5f)+Vector3.up*(1-i)*.3f,
                    new Vector2(Math.Max(.5f,Math.Min(8,d.magnitude)),.08f),angle+(i%2==0?18:-18),qi,duration);
        }
    }
    private static Color ColorFor(Actor actor,string id)
    {
        string law=id=="C009"?MclslActorAccessor.GetString(actor,Data.MclslActorDataKeys.SpiritualRootPrimary):
            MclslSpellSystem.TryGet(id,out var spell)?spell.Law:"金";
        return LawColor(law);
    }
    internal static Color LawColor(string law)=>law switch
        {
            "木"=>new Color(.3f,1,.65f,.8f),"水"=>new Color(.35f,.75f,1,.8f),"火"=>new Color(1,.4f,.15f,.8f),
            "土"=>new Color(.85f,.65f,.3f,.8f),"雷"=>new Color(.75f,.5f,1,.85f),"阴"=>new Color(.55f,.45f,.85f,.8f),
            _=>new Color(.9f,.95f,1,.85f)
        };
    private static void Render(Stroke s)
    {
        float t=Mathf.Clamp01((float)((MclslSpellCasting.Now-s.Born)/s.Duration));
        // A brief visual hold gives impact weight without pausing simulation or the camera.
        float motion=t>.55f&&t<.62f?.55f:t;
        s.Object.transform.position=Vector3.Lerp(s.Start,s.End,motion)+new Vector3(0,.05f,-.01f);
        s.Object.transform.localScale=new Vector3(s.Size.x,s.Size.y,1);
        s.Object.transform.rotation=Quaternion.Euler(0,0,s.Angle);
        var color=s.Color;color.a*=1-t;s.Renderer.color=color;
    }
    internal static void Tick()
    {
        for(int i=Live.Count-1;i>=0;i--)
        {
            var s=Live[i];
            if(!MclslSpellEffect.Enabled||MclslSpellCasting.Now>=s.Born+s.Duration)
            {s.Object.SetActive(false);Live.RemoveAt(i);Pool.Push(s);MclslSpellEffect.ReleaseExternal();}
            else Render(s);
        }
    }
    internal static void Clear()
    {
        foreach(var s in Live){if(s.Object!=null)UnityEngine.Object.Destroy(s.Object);MclslSpellEffect.ReleaseExternal();}
        foreach(var s in Pool)if(s.Object!=null)UnityEngine.Object.Destroy(s.Object);
        Live.Clear();Pool.Clear();
        if(_pixel!=null){UnityEngine.Object.Destroy(_pixel.texture);UnityEngine.Object.Destroy(_pixel);_pixel=null;}
    }
}
