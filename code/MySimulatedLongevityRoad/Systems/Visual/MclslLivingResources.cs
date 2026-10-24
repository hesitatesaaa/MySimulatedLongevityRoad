using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslLivingResources
{
    internal sealed class Frame
    {
        public string Name;
        public int X,Y,Width,Height;
        public float PivotX,PivotY,PixelsPerUnit;
        public float ItemX,ItemY;
    }
    internal sealed class Group
    {
        public string Path,Atlas;
        public Frame[] Frames;
    }
    internal sealed class Manifest
    {
        public Group[] Groups;
    }
    internal static Group[] Groups { get; private set; }
    internal static void Init()
    {
        string file=System.IO.Path.Combine(MclslMod.I.GetDeclaration().FolderPath,"resources","living-resources.json");
        Groups=JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(file)).Groups;
        foreach(var group in Groups)
        {
            Texture2D texture=SpriteTextureLoader.getSprite(group.Atlas).texture;
            texture.filterMode=FilterMode.Point;
            var sprites=new List<Sprite>();
            foreach(var f in group.Frames)
            {
                Sprite sprite=Sprite.Create(texture,new Rect(f.X,f.Y,f.Width,f.Height),
                    new Vector2(f.PivotX/f.Width,f.PivotY/f.Height),f.PixelsPerUnit);
                sprite.name=f.Name;sprites.Add(sprite);
                MclslSpriteOwnership.Register(sprite);
                SpriteTextureLoader._cached_sprites[group.Path+"/"+f.Name]=sprite;
            }
            SpriteTextureLoader._cached_sprite_list[group.Path]=sprites.ToArray();
        }
    }
}

internal static class MclslCareerEffectFrames
{
    private static readonly Dictionary<string,Sprite[]> Frames=new();
    private static readonly Dictionary<string,Sprite> Passives=new();
    internal static Sprite Get(string id,int frame,string phase)
    {
        if(id.EndsWith("P"))
        {
            if(!Passives.TryGetValue(id,out var passive))
            {
                foreach(var skill in Data.MclslCombatCareerCatalog.Skills)
                    if(skill.Id==id)
                    {
                        Sprite source=SpriteTextureLoader.getSprite(skill.Icon);
                        passive=Sprite.Create(source.texture,source.rect,new Vector2(0.5f,0.5f),256);
                        Passives[id]=passive;
                        break;
                    }
            }
            return passive;
        }
        if(!Frames.TryGetValue(id,out var sprites))
        {
            Texture2D texture=SpriteTextureLoader.getSprite("effects/Careers/"+id).texture;
            texture.filterMode=FilterMode.Point;
            sprites=new Sprite[24];
            int size=texture.width/4;
            for(int i=0;i<24;i++)sprites[i]=Sprite.Create(texture,
                new Rect(i%4*size,(5-i/4)*size,size,size),new Vector2(0.5f,0.5f),size);
            Frames[id]=sprites;
        }
        int offset=phase=="cast"?0:phase=="travel"||phase=="area"?8:phase=="cancel"?20:16;
        int count=offset>=16?4:8;
        return sprites[offset+Mathf.Clamp(frame,0,count-1)];
    }
    internal static void Clear()
    {
        foreach(var group in Frames.Values)foreach(var sprite in group)Object.Destroy(sprite);
        foreach(var sprite in Passives.Values)Object.Destroy(sprite);
        Frames.Clear();Passives.Clear();
    }
}
