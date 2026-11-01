using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastTalentFrames
{
    internal static bool Enabled=true;
    private static readonly Dictionary<string,Sprite[]> Cache=new(StringComparer.Ordinal);
    internal static Sprite Get(string id,int frame,string phase)
    {
        if(MclslBeastTalentCatalog.ForId(id)==null)return null;
        if(!Cache.TryGetValue(id,out var frames))
        {
            Texture2D texture=SpriteTextureLoader.getSprite("effects/BeastTalents/"+id)?.texture;
            if(texture==null||texture.width!=768||texture.height!=1152)
                throw new InvalidOperationException("本命神通专属图集缺失或尺寸错误："+id);
            texture.filterMode=FilterMode.Bilinear;frames=new Sprite[24];Cache.Add(id,frames);
            for(int i=0;i<24;i++)
            {
                frames[i]=Sprite.Create(texture,new Rect(i%4*192,(5-i/4)*192,192,192),new Vector2(0.5f,0.5f),192);
                frames[i].name=id+"_"+i;MclslSpriteOwnership.Register(frames[i]);
            }
        }
        int offset=phase=="cast"?0:phase is "hit"?16:phase is "cancel"?20:8;
        return frames[offset+Math.Clamp(frame,0,offset>=16?3:7)];
    }
    internal static void Clear()
    {
        foreach(var frames in Cache.Values)foreach(var sprite in frames)
        {MclslSpriteOwnership.Unregister(sprite);UnityEngine.Object.Destroy(sprite);}
        Cache.Clear();
    }
}
