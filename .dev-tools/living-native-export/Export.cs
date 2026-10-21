using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NeoModLoader.api;
using Newtonsoft.Json;
using UnityEngine;

public sealed class LivingNativeExport : BasicMod<LivingNativeExport>
{
    protected override void OnModLoad()
    {
        string folder=Environment.GetCommandLineArgs().SkipWhile(x=>x!="--living-export").Skip(1).First();
        Directory.CreateDirectory(folder);
        ActorAsset human=AssetManager.actor_library.get("human");
        var paths=new HashSet<string>();
        foreach(var field in typeof(ActorTextureSubAsset).GetFields(BindingFlags.Instance|BindingFlags.Public))
            if(field.FieldType==typeof(string) && field.GetValue(human.texture_asset) is string p && p.StartsWith("actors/"))paths.Add(p);
        paths.Add(human.texture_asset.texture_path_base);
        var skins=new Dictionary<string,object>();
        foreach(var field in typeof(ActorAsset).GetFields(BindingFlags.Instance|BindingFlags.Public))
            if(field.Name.Contains("skin")||field.Name.Contains("phenotype"))
            {skins[field.Name]=field.GetValue(human);if(field.GetValue(human) is string[] values)foreach(string v in values)paths.Add(human.texture_asset.texture_path_base+v);}
        var buildings=new List<object>();
        foreach(var pair in human.architecture_asset.building_ids_for_construction)
        {
            BuildingAsset b=AssetManager.buildings.get(pair.Value);
            string path=string.IsNullOrEmpty(b.sprite_path)?b.main_path+b.id:b.sprite_path;
            paths.Add(path);
            buildings.Add(new{order=pair.Key,id=b.id,path,upgrade=b.upgrade_to,previous=b.upgraded_from});
        }
        var boats=new[]{human.architecture_asset.actor_asset_id_boat_fishing,human.architecture_asset.actor_asset_id_trading,human.architecture_asset.actor_asset_id_transport};
        foreach(string boat in boats)paths.Add("actors/boats/"+boat);
        var resources=new List<object>();
        foreach(string path in paths.OrderBy(x=>x))
        {
            Sprite[] sprites=SpriteTextureLoader.getSpriteList(path);
            resources.Add(new{path,frames=sprites.Select(s=>new{name=s.name,x=s.rect.x,y=s.rect.y,width=s.rect.width,height=s.rect.height,pivotX=s.pivot.x,pivotY=s.pivot.y,pixelsPerUnit=s.pixelsPerUnit}).ToArray()});
        }
        File.WriteAllText(Path.Combine(folder,"native-human-resources.json"),JsonConvert.SerializeObject(new{skins,buildings,boats,resources},Formatting.Indented));
        var samples=SpriteTextureLoader.getSpriteList("actors/species/civs/human/male_1")
            .Where(s=>s.name.StartsWith("walk_")||s.name.StartsWith("swim_")).ToArray();
        foreach(Sprite sprite in samples)
        {
            var source=sprite.texture;
            RenderTexture rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(source,rt);
            RenderTexture old=RenderTexture.active;RenderTexture.active=rt;
            var pixels=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);
            pixels.ReadPixels(new Rect(0,0,source.width,source.height),0,0);pixels.Apply();
            RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
            var cropped=new Texture2D((int)sprite.rect.width,(int)sprite.rect.height,TextureFormat.RGBA32,false);
            cropped.SetPixels(pixels.GetPixels((int)sprite.rect.x,(int)sprite.rect.y,(int)sprite.rect.width,(int)sprite.rect.height));cropped.Apply();
            File.WriteAllBytes(Path.Combine(folder,"human_"+sprite.name+".png"),cropped.EncodeToPNG());
            Destroy(pixels);Destroy(cropped);
        }
        File.WriteAllText(Path.Combine(folder,"human-animation-fields.json"),JsonConvert.SerializeObject(
            typeof(ActorAsset).GetFields(BindingFlags.Instance|BindingFlags.Public)
                .Where(f=>f.Name.Contains("animation")||f.Name.Contains("texture"))
                .Select(f=>new{name=f.Name,value=f.GetValue(human)?.ToString()}),Formatting.Indented));
        Application.Quit();
    }
}
