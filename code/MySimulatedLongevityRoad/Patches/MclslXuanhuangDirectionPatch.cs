using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslXuanhuangDirectionPatch
{
    private sealed class Facing { internal int Value; }
    private static ConditionalWeakTable<Actor,Facing> Directions=new();
    private static readonly Dictionary<string,Texture2D> ColoredAtlases=new();
    private static readonly Dictionary<string,Sprite> ColoredFrames=new();

    internal static int Apply(Harmony harmony)
        =>MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.calculateMainSprite.xuanhuang-direction",
            AccessTools.Method(typeof(Actor),"calculateMainSprite"),null,
            AccessTools.Method(typeof(MclslXuanhuangDirectionPatch),nameof(AfterSprite)))?1:0;

    internal static void Clear()
    {
        Directions=new();
        foreach(Sprite sprite in ColoredFrames.Values)UnityEngine.Object.Destroy(sprite);
        foreach(Texture2D texture in ColoredAtlases.Values)UnityEngine.Object.Destroy(texture);
        ColoredFrames.Clear();ColoredAtlases.Clear();
    }

    private static void AfterSprite(Actor __instance,ref Sprite __result)
    {
        if(__instance.asset.id!=MclslXuanhuangRegistration.Id || __result==null)return;
        Vector2 delta=Vector2.zero;
        if(__instance.attack_target is Actor enemy && __instance.has_attack_target)
            delta=enemy.current_position-__instance.current_position;
        else if(__instance.is_moving)
            delta=__instance.next_step_position-__instance.current_position;
        Facing facing=Directions.GetOrCreateValue(__instance);
        if(delta.sqrMagnitude>0.01f)
            facing.Value=Math.Abs(delta.y)>Math.Abs(delta.x)*0.6f ? delta.y>0?2:1 : 0;
        string group=__instance.asset.texture_asset.getUnitTexturePath(__instance);
        string skin=group.Substring(group.LastIndexOf('/')+1);
        string direction=facing.Value==1?"_front":facing.Value==2?"_back":"";
        if(facing.Value!=0)
        {
            string path="actors/species/civs/mclsl_xuanhuang"+direction+"/"+skin;
            __result=SpriteTextureLoader._cached_sprites[path+"/"+__result.name];
        }
        Color32 nation=(Color32)__instance.kingdom.getColor().getColorMainSecond();
        string atlas=(skin=="female_1"?"female_1":"male_1")+direction;
        string colorKey=atlas+"/"+nation.r+"-"+nation.g+"-"+nation.b;
        string frameKey=colorKey+"/"+__result.name;
        if(!ColoredFrames.TryGetValue(frameKey,out Sprite tinted))
        {
            if(!ColoredAtlases.TryGetValue(colorKey,out Texture2D texture))
            {
                string source=Path.Combine(MclslMod.I.GetDeclaration().FolderPath,
                    "GameResources","actors","Xuanhuang",atlas+".png");
                texture=new Texture2D(2,2,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
                ImageConversion.LoadImage(texture,File.ReadAllBytes(source),false);
                Color32[] pixels=texture.GetPixels32();
                bool female=skin=="female_1";
                for(int i=0;i<pixels.Length;i++)
                {
                    Color32 p=pixels[i];
                    bool robe=female
                        ? p.g>p.r*1.25f && p.g>p.b*1.18f && p.r<135
                        : p.r>p.g*1.5f && p.r>p.b*1.4f && p.g<125;
                    if(!robe)continue;
                    float shade=female?p.g/115f:p.r/190f;
                    pixels[i]=new Color32(
                        (byte)Mathf.Clamp(nation.r*shade,0,255),
                        (byte)Mathf.Clamp(nation.g*shade,0,255),
                        (byte)Mathf.Clamp(nation.b*shade,0,255),p.a);
                }
                texture.SetPixels32(pixels);texture.Apply(false,true);
                ColoredAtlases[colorKey]=texture;
            }
            Rect rect=__result.rect;
            tinted=Sprite.Create(texture,rect,
                new Vector2(__result.pivot.x/rect.width,__result.pivot.y/rect.height),
                __result.pixelsPerUnit);
            tinted.name=__result.name;
            ColoredFrames[frameKey]=tinted;
        }
        __result=tinted;
    }
}
