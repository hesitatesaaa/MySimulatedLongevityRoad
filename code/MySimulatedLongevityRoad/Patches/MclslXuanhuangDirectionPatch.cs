using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslXuanhuangDirectionPatch
{
    private sealed class Facing { internal int Value; internal AnimationFrameData Frame; }
    private sealed class Atlas
    {
        internal int Width,Height;
        internal Color32[] Pixels,Mask;
    }
    private static ConditionalWeakTable<Actor,Facing> Directions=new();
    private static readonly Dictionary<string,Texture2D> ColoredAtlases=new();
    private static readonly Dictionary<string,Sprite> ColoredFrames=new();
    private static readonly Dictionary<string,Atlas> Sources=new();
    private static readonly Dictionary<string,AnimationFrameData> FrameData=new();
    private static readonly HashSet<string> Colors=new();
    private static bool _ready;

    internal static int Apply(Harmony harmony)
    {
        Init();
        int count=MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.calculateMainSprite.xuanhuang-direction",
            AccessTools.Method(typeof(Actor),"calculateMainSprite"),null,
            AccessTools.Method(typeof(MclslXuanhuangDirectionPatch),nameof(AfterSprite)))?1:0;
        count+=MclslHarmonyPatchGuard.TryPatch(harmony,"ColorAsset.initColor.xuanhuang",
            AccessTools.Method(typeof(ColorAsset),"initColor"),null,
            AccessTools.Method(typeof(MclslXuanhuangDirectionPatch),nameof(AfterColor)))?1:0;
        count+=MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.getAnimationFrameData.xuanhuang",
            AccessTools.Method(typeof(Actor),"getAnimationFrameData"),null,
            AccessTools.Method(typeof(MclslXuanhuangDirectionPatch),nameof(AfterFrameData)))?1:0;
        count+=MclslHarmonyPatchGuard.TryPatch(harmony,"MapBox.preloadRenderedSprites.xuanhuang-colors",
            AccessTools.Method(typeof(MapBox),"preloadRenderedSprites"),
            AccessTools.Method(typeof(MclslXuanhuangDirectionPatch),nameof(BeforePreloadSprites)),null)?1:0;
        return count;
    }

    // Saved kingdoms need their colors before the loader asks for render frames,
    // which happens earlier than finishingUpLoading and its world-entry callback.
    private static void BeforePreloadSprites()=>PrepareWorldColors();

    private static string ColorKey(Color32 color)=>color.r+"-"+color.g+"-"+color.b;

    private static void Init()
    {
        if(!MclslRuntimeChanges.IsMainThread)throw new InvalidOperationException("Xuanhuang texture initialization requires the main thread.");
        string root=Path.Combine(MclslMod.I.GetDeclaration().FolderPath,"GameResources");
        foreach(string atlas in MclslLivingResources.Groups.Where(g=>g.Path.StartsWith("actors/species/civs/mclsl_xuanhuang",StringComparison.Ordinal)).Select(g=>g.Atlas).Distinct())
        {
            Color32[] Read(string suffix,out int width,out int height)
            {
                var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!ImageConversion.LoadImage(texture,File.ReadAllBytes(Path.Combine(root,atlas+suffix+".png")),false))
                    throw new InvalidDataException(atlas+suffix);
                width=texture.width;height=texture.height;
                Color32[] pixels=texture.GetPixels32();
                UnityEngine.Object.Destroy(texture);
                return pixels;
            }
            Color32[] pixels=Read("",out int width,out int height);
            Color32[] mask=Read("_robe_mask",out int maskWidth,out int maskHeight);
            if(width!=maskWidth||height!=maskHeight)throw new InvalidDataException("Robe mask dimensions: "+atlas);
            Sources.Add(atlas,new Atlas{Width=width,Height=height,Pixels=pixels,Mask=mask});
        }
        foreach(var group in MclslLivingResources.Groups.Where(g=>Sources.ContainsKey(g.Atlas)))
            foreach(var frame in group.Frames)
                FrameData.Add(group.Path+"/"+frame.Name,new AnimationFrameData {
                    id=frame.Name,sheet_path=group.Path,size_unit=new Vector2(frame.Width,frame.Height),
                    show_head=false,show_item=!group.Path.EndsWith("/child",StringComparison.Ordinal)
                        && !group.Path.Contains("_child"),
                    pos_item=new Vector2((frame.ItemX-frame.PivotX)/frame.PixelsPerUnit,
                        (frame.ItemY-frame.PivotY)/frame.PixelsPerUnit)
                });
        _ready=true;
        PrepareWorldColors();
    }

    private static void AfterColor(ColorAsset __instance)
    {
        if(_ready&&MclslRuntimeChanges.IsMainThread)PrepareColor(__instance.getColorMainSecond());
    }

    internal static void PrepareWorldColors()
    {
        foreach(var color in AssetManager.kingdom_colors_library.list)PrepareColor(color.getColorMainSecond());
        foreach(string id in new[]{MclslXuanhuangRegistration.Id,"nomads_"+MclslXuanhuangRegistration.Id})
            PrepareColor(AssetManager.kingdoms.get(id).default_kingdom_color.getColorMainSecond());
        string zombieKingdom=AssetManager.actor_library.get("zombie_"+MclslXuanhuangRegistration.Id).kingdom_id_wild;
        PrepareColor(AssetManager.kingdoms.get(zombieKingdom).default_kingdom_color.getColorMainSecond());
        if(World.world?.kingdoms!=null)
            foreach(var kingdom in World.world.kingdoms)PrepareColor(kingdom.getColor().getColorMainSecond());
    }

    private static void PrepareColor(Color32 nation)
    {
        if(!MclslRuntimeChanges.IsMainThread)throw new InvalidOperationException("Xuanhuang tinting requires the main thread.");
        string color=ColorKey(nation);
        if(!Colors.Add(color))return;
        foreach(var pair in Sources)
        {
            Atlas source=pair.Value;
            var pixels=(Color32[])source.Pixels.Clone();
            for(int i=0;i<pixels.Length;i++)
            {
                if(source.Mask[i].a==0)continue;
                float shade=source.Mask[i].r/190f;
                pixels[i]=new Color32((byte)Mathf.Clamp(nation.r*shade,0,255),
                    (byte)Mathf.Clamp(nation.g*shade,0,255),(byte)Mathf.Clamp(nation.b*shade,0,255),pixels[i].a);
            }
            var texture=new Texture2D(source.Width,source.Height,TextureFormat.RGBA32,false)
                {filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(pixels);texture.Apply(false,true);
            ColoredAtlases.Add(pair.Key+"/"+color,texture);
        }
        foreach(var group in MclslLivingResources.Groups.Where(g=>Sources.ContainsKey(g.Atlas)))
            foreach(var frame in group.Frames)
            {
                var sprite=Sprite.Create(ColoredAtlases[group.Atlas+"/"+color],
                    new Rect(frame.X,frame.Y,frame.Width,frame.Height),
                    new Vector2(frame.PivotX/frame.Width,frame.PivotY/frame.Height),frame.PixelsPerUnit);
                sprite.name=frame.Name;
                MclslSpriteOwnership.Register(sprite);
                ColoredFrames.Add(group.Path+"/"+color+"/"+frame.Name,sprite);
            }
    }

    internal static void Clear()
    {
        Directions=new();
        foreach(var sprite in ColoredFrames.Values)
        {MclslSpriteOwnership.Unregister(sprite);UnityEngine.Object.Destroy(sprite);}
        foreach(var texture in ColoredAtlases.Values)UnityEngine.Object.Destroy(texture);
        ColoredFrames.Clear();ColoredAtlases.Clear();
        Colors.Clear();
    }

    private static void AfterSprite(Actor __instance,ref Sprite __result)
    {
        if(!Supported(__instance) || __result==null)return;
        Vector2 delta=Vector2.zero;
        if(__instance.attack_target is Actor enemy && __instance.has_attack_target)
            delta=enemy.current_position-__instance.current_position;
        else if(__instance.is_moving)
            delta=__instance.next_step_position-__instance.current_position;
        Facing facing=Directions.GetOrCreateValue(__instance);
        if(delta.sqrMagnitude>0.01f)
            facing.Value=Math.Abs(delta.y)>Math.Abs(delta.x)*0.6f ? delta.y>0?2:1 : 0;
        string skin=__instance.isSexFemale()?"female_1":"male_1";
        string role=__instance.asset.unit_zombie?"zombie":__instance.isBaby()?"child":
            __instance.profession_asset?.profession_id switch
            {
                UnitProfession.King=>"king",UnitProfession.Leader=>"leader",
                UnitProfession.Warrior=>"warrior",_=>__instance.isPrettyOld()?"old":null
            };
        if(role!=null)skin+="_"+role;
        string direction=facing.Value==1?"_front":facing.Value==2?"_back":"";
        string path="actors/species/civs/mclsl_xuanhuang"+direction+"/"+skin;
        Color32 nation=(__instance.kingdom==null
            ? AssetManager.kingdoms.get("nomads_"+MclslXuanhuangRegistration.Id).default_kingdom_color
            : __instance.kingdom.getColor()).getColorMainSecond();
        facing.Frame=FrameData[path+"/"+__result.name];
        __instance.frame_data=facing.Frame;
        __result=ColoredFrames[path+"/"+ColorKey(nation)+"/"+__result.name];
    }

    private static bool Supported(Actor actor)=>actor.asset.id==MclslXuanhuangRegistration.Id
        || actor.asset.id=="zombie_"+MclslXuanhuangRegistration.Id;

    private static void AfterFrameData(Actor __instance,ref AnimationFrameData __result)
    {
        if(Supported(__instance) && Directions.TryGetValue(__instance,out var facing) && facing.Frame!=null)
            __result=facing.Frame;
    }
}
