using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslBeastContentRow
{
    internal readonly string Species, Material, Pill, PillEffect, Talisman, TalismanEffect, Artifact, ArtifactEffect;
    internal readonly int MinRealm, Grade;
    internal readonly MclslArtifactEquipmentSlot Slot;
    internal string MaterialId => Species == "zhuque" ? "M25" : "YM_" + Species;
    internal string PillId => "YD_" + Species;
    internal string TalismanId => "YF_" + Species;
    internal string ArtifactId => "YB_" + Species;
    internal MclslBeastContentRow(string species, int minRealm, string material, string pill, string pillEffect,
        string talisman, string talismanEffect, string artifact, MclslArtifactEquipmentSlot slot, string artifactEffect)
    {
        Species = species; MinRealm = minRealm; Grade = minRealm <= 1 ? 1 : Math.Min(4, minRealm);
        Material = material; Pill = pill; PillEffect = pillEffect; Talisman = talisman;
        TalismanEffect = talismanEffect; Artifact = artifact; Slot = slot; ArtifactEffect = artifactEffect;
    }
}

internal readonly struct MclslRecipeIngredient
{
    internal readonly string ItemId;
    internal readonly int Count;
    internal readonly string[] Alternatives;
    internal MclslRecipeIngredient(string id, int count,params string[] alternatives)
    { ItemId = id; Count = count; Alternatives=alternatives??Array.Empty<string>(); }
}

internal sealed class MclslRecipeDefinition
{
    internal readonly string Id, ProductId, Profession;
    internal readonly int Grade, OutputCount = 1;
    internal readonly MclslRecipeIngredient[] Ingredients;
    internal MclslRecipeDefinition(string product, int grade, string profession, params MclslRecipeIngredient[] ingredients)
    {
        Id = "recipe_" + product; ProductId = product; Grade = grade; Profession = profession;
        Ingredients = ingredients.GroupBy(x => x.ItemId, StringComparer.Ordinal)
            .Select(x => new MclslRecipeIngredient(x.Key, x.Sum(y => y.Count),x.SelectMany(y=>y.Alternatives).Distinct(StringComparer.Ordinal).ToArray())).ToArray();
    }
    internal MclslRecipeIngredient[] Resolve(Func<string,int> available,Func<string,string> canonical)
    {
        var used=new Dictionary<string,int>(StringComparer.Ordinal);
        foreach(var ingredient in Ingredients)
        {
            int remaining=ingredient.Count;
            foreach(string option in new[]{ingredient.ItemId}.Concat(ingredient.Alternatives))
            {
                string id=canonical(option);used.TryGetValue(id,out int prior);
                int take=Math.Min(remaining,Math.Max(0,available(id)-prior));
                if(take>0){used[id]=prior+take;remaining-=take;}
                if(remaining==0)break;
            }
            if(remaining>0)return null;
        }
        return used.Select(x=>new MclslRecipeIngredient(x.Key,x.Value)).ToArray();
    }
}

internal static class MclslBeastContent
{
    internal static readonly MclslBeastContentRow[] Rows =
    {
        new("feiqiu",0,"浮空绒","轻身丹","wind","浮空符","speed","浮云履",MclslArtifactEquipmentSlot.Boots,"speed"),
        new("hongmao",0,"赤绒精血","赤血壮体丹","body","赤血战符","physical","赤绒护心甲",MclslArtifactEquipmentSlot.Armor,"physical_guard"),
        new("changliao",0,"长獠毒牙","清毒养脉丹","cleanse","獠牙破敌符","physical","獠牙刃",MclslArtifactEquipmentSlot.Weapon,"physical"),
        new("goushou",0,"犬首妖髓","镇邪丹","cleanse","辟邪符","cleanse","镇邪骨佩",MclslArtifactEquipmentSlot.Amulet,"dot_guard"),
        new("manman",0,"双生羽","双生回元丹","mana","比翼风符","speed","比翼羽环",MclslArtifactEquipmentSlot.Ring,"mana_regen"),
        new("dafeng",1,"风囊","巽风养息丹","wind","裂风符","wind","大风翼履",MclslArtifactEquipmentSlot.Boots,"风"),
        new("mo",1,"梦魄","安魂丹","soul","梦缚符","yin","吞梦珠",MclslArtifactEquipmentSlot.Amulet,"control_guard"),
        new("qingluan",1,"青鸾尾翎","青鸾回春丹","guard","青羽护身符","guard","青鸾羽冠",MclslArtifactEquipmentSlot.Helmet,"health_regen"),
        new("yingying",1,"阴影凝晶","玄阴定魄丹","soul","影缚符","yin","幽影戒",MclslArtifactEquipmentSlot.Ring,"阴"),
        new("qinglong",2,"青龙逆鳞","龙息归元丹","mana","苍龙水符","water","青龙御水佩",MclslArtifactEquipmentSlot.Amulet,"水"),
        new("huanglong",2,"黄龙地髓","地髓固脉丹","guard","镇岳符","guard","黄龙镇岳印",MclslArtifactEquipmentSlot.Weapon,"spell"),
        new("baihu",2,"白虎庚金牙","庚金炼体丹","body","虎煞金刃符","metal","白虎裂金剑",MclslArtifactEquipmentSlot.Weapon,"金"),
        new("xuanwu",2,"玄武甲片","玄甲护脉丹","guard","玄武御甲符","guard","玄武护身甲",MclslArtifactEquipmentSlot.Armor,"spell_guard"),
        new("xuanniao",2,"玄鸟玄羽","玄羽清魂丹","soul","玄鸟镇魂符","soul","玄鸟定魂冠",MclslArtifactEquipmentSlot.Helmet,"control_guard"),
        new("zhuque",3,"朱雀火羽","离火净体丹","cleanse","朱雀焚邪符","fire","朱雀焚天扇",MclslArtifactEquipmentSlot.Weapon,"火"),
        new("qilin",3,"麒麟瑞角","瑞麟培元丹","body","瑞麟护生符","guard","麒麟护命佩",MclslArtifactEquipmentSlot.Amulet,"health"),
        new("moqilin",3,"墨麟鳞心","墨麟凝神丹","soul","墨麟镇影符","yin","墨麟玄甲",MclslArtifactEquipmentSlot.Armor,"阴_guard"),
        new("fenghuang",3,"凤凰涅槃翎","涅槃回生丹","guard","凰火净障符","cleanse","凤凰续生佩",MclslArtifactEquipmentSlot.Amulet,"health_regen"),
        new("kunpeng",4,"鲲鹏翼骨","鲲鹏逍遥丹","wind","扶摇破空符","space","鲲鹏扶摇履",MclslArtifactEquipmentSlot.Boots,"空间"),
        new("liejiejing",4,"裂界鲸晶","裂界归元丹","mana","裂界震荡符","space_area","裂界鲸骨刃",MclslArtifactEquipmentSlot.Weapon,"空间"),
        new("tianshou",4,"兲兽山心","山心固元丹","body","兲岳镇御符","guard","兲岳镇世印",MclslArtifactEquipmentSlot.Weapon,"土")
    };
    internal static readonly Dictionary<string,MclslBeastContentRow> BySpecies = Rows.ToDictionary(x=>x.Species,StringComparer.Ordinal);
    internal static readonly string[] Herbs = { "", "A08", "A14", "A15", "A18" };
    internal static readonly string[] TierNames = { "", "黄", "玄", "地", "天" };
    internal static string Common(string kind, int grade) => "YM_" + kind + "_" + Math.Clamp(grade,1,4);
    internal static int MaterialGrade(int realm) => realm <= 1 ? 1 : Math.Clamp(realm,1,4);
    internal static float Bonus(int grade) => 0.05f + Math.Clamp(grade,1,4) * 0.05f;
    internal static readonly MclslItemDefinition[] Items = BuildItems();
    internal static readonly MclslRecipeDefinition[] Recipes = BuildRecipes();

    private static MclslItemDefinition[] BuildItems()
    {
        List<MclslItemDefinition> items = new();
        string[] kinds = { "blood","bone","skin","core" }, names = { "妖血","兽骨","兽皮","妖丹" };
        for(int grade=1;grade<=4;grade++)
        {
            for(int i=0;i<kinds.Length;i++) items.Add(Material(Common(kinds[i],grade),TierNames[grade]+"阶"+names[i],grade));
            items.Add(Product("YD_common_"+grade,TierNames[grade]+"阶兽血回元丹","Pill",grade,"heal",Common("blood",grade),Herbs[grade]));
            items.Add(Product("YF_common_"+grade,TierNames[grade]+"阶兽皮护身符","Talisman",grade,"guard",Common("skin",grade),"F01"));
            items.Add(Artifact("YB_common_"+grade,TierNames[grade]+"阶兽骨灵剑",grade,MclslArtifactEquipmentSlot.Weapon,"",Common("bone",grade)));
        }
        foreach(var row in Rows)
        {
            if(row.Species!="zhuque") items.Add(Material(row.MaterialId,row.Material,row.Grade));
            items.Add(Product(row.PillId,row.Pill,"Pill",row.Grade,row.PillEffect,row.MaterialId,Common("blood",row.Grade)));
            items.Add(Product(row.TalismanId,row.Talisman,"Talisman",row.Grade,row.TalismanEffect,row.MaterialId,"F01"));
            items.Add(Artifact(row.ArtifactId,row.Artifact,row.Grade,row.Slot,row.ArtifactEffect,row.MaterialId));
        }
        return items.ToArray();
    }
    private static MclslItemDefinition Material(string id,string name,int grade)
        => new(id,name,"Material",0,"击杀妖兽获得的"+name+"。"+(id.Contains("_core_",StringComparison.Ordinal)?"可等量替代同阶妖血用于丹药和符箓配方。"
            :id.Contains("_blood_",StringComparison.Ordinal)?"用于炼丹与制符。":id.Contains("_bone_",StringComparison.Ordinal)?"用于炼器。":id.Contains("_skin_",StringComparison.Ordinal)?"用于绘制护身符。":"用于炼丹、制符与炼器。"),"","",6*(1<<(grade-1)),
            (MclslMaterialTier)grade,MclslMaterialSource.BeastKill);
    private static MclslItemDefinition Product(string id,string name,string category,int grade,string effect,string a,string b)
        => new(id,name,category,grade,Describe(category,grade,effect),a,b,(category=="Pill"?20:12)*(int)Math.Pow(3,grade-1));
    internal static string Describe(string category,int grade,string effect)
    {
        int bonus=(int)Math.Round(Bonus(grade)*100), duration=15+grade*5;
        if(category=="Pill")
        {
            string recovery=effect is "mana" or "soul" ? "恢复"+(10+grade*10)+"%最大灵力；" : "恢复"+(15+grade*10)+"%最大生命；";
            string buff=effect switch { "body"=>"最大生命+"+bonus+"%", "mana"=>"灵力恢复速度+"+bonus+"%", "wind"=>"移速+"+bonus+"%",
                "guard"=>"所受伤害减少"+bonus+"%", "soul"=>"清除普通控制，后续控制时间减少"+bonus+"%",
                "cleanse"=>"清除普通中毒与灼烧，持续伤害减少"+bonus+"%", _=>"" };
            return recovery+buff+(buff.Length>0?"，持续"+duration+"秒。":"")+"同类型取最高效果，共享30秒冷却。模组扩展配方。";
        }
        return effect switch
        {
            "guard"=>"受到伤害减少"+bonus+"%，持续"+duration+"秒。",
            "speed"=>"移动速度+"+(10+grade*5)+"%，持续"+duration+"秒。",
            "soul"=>"控制持续时间减少"+(15+grade*5)+"%，持续"+duration+"秒。",
            "cleanse"=>"解除普通中毒与灼烧，持续伤害减少"+(15+grade*5)+"%，持续"+duration+"秒。",
            _=>"攻击倍率"+(0.5f+grade*0.3f).ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"；"+EffectName(effect)+"；最多"+(effect=="space_area"?3:effect=="wind"?2:1)+"敌。共享10秒符箓冷却。"
        };
    }
    internal static string EffectName(string effect)=>effect switch
    {
        "physical"=>"物理伤害","physical_guard"=>"物理伤害减免","dot_guard"=>"持续伤害减免","mana_regen"=>"灵力自然恢复速度",
        "control_guard"=>"控制抗性","health_regen"=>"生命自然恢复速度","spell"=>"法术伤害","spell_guard"=>"法术伤害减免",
        "阴_guard"=>"阴系伤害减免","speed"=>"移动速度","health"=>"最大生命","fire"=>"火焰灼烧","water"=>"水系伤害与减速",
        "wind"=>"风系伤害","yin"=>"阴系伤害与束缚","metal"=>"金系伤害","space"=>"空间伤害","space_area"=>"空间震荡",_=>effect+"系伤害"
    };
    private static MclslItemDefinition Artifact(string id,string name,int grade,MclslArtifactEquipmentSlot slot,string effect,string ingredient)
    {
        int[] damage={0,200,750,2200,5500},health={0,150,500,2000,6000},armor={0,10,20,35,50};
        float[] multiplier={0,0.6f,2.5f,8,16};
        List<(string,float)> stats=new();
        switch(slot)
        {
            case MclslArtifactEquipmentSlot.Weapon: stats.Add(("damage",damage[grade])); stats.Add(("multiplier_damage",multiplier[grade]));break;
            case MclslArtifactEquipmentSlot.Armor: stats.Add(("health",health[grade]));stats.Add(("armor",armor[grade]));break;
            case MclslArtifactEquipmentSlot.Helmet: stats.Add(("health",health[grade]/2f));stats.Add(("armor",armor[grade]/2f));break;
            case MclslArtifactEquipmentSlot.Boots: stats.Add(("health",health[grade]/2f));stats.Add(("multiplier_speed",Bonus(grade)));break;
            case MclslArtifactEquipmentSlot.Ring: stats.Add(("health",health[grade]/2f));break;
            case MclslArtifactEquipmentSlot.Amulet: stats.Add(("health",health[grade]));stats.Add(("multiplier_health",Bonus(grade)));break;
        }
        if(effect is "speed" or "health") stats.Add((effect=="speed"?"multiplier_speed":"multiplier_health",Bonus(grade)));
        // Slot attributes and the signature ability can use the same native
        // key. Merge before registration, which assigns rather than adds.
        var combined=stats.GroupBy(x=>x.Item1,StringComparer.Ordinal).Select(x=>(x.Key,x.Sum(y=>y.Item2))).ToArray();
        return new(id,name,"Artifact",grade,effect.Length==0?"兽骨炼制的基础灵剑。":"模组扩展法宝；"+EffectName(effect)+"专属增益"+(int)Math.Round(Bonus(grade)*100)+"%。同类专属效果合计上限40%。",ingredient,"M0"+grade,30*(int)Math.Pow(3,grade-1),equipmentSlot:slot,nativeStats:combined);
    }
    private static MclslRecipeDefinition[] BuildRecipes()
    {
        List<MclslRecipeDefinition> recipes=new();
        for(int g=1;g<=4;g++)
        {
            recipes.Add(new("YD_common_"+g,g,"alchemist",new(Common("blood",g),2,Common("core",g)),new(Herbs[g],1)));
            recipes.Add(new("YF_common_"+g,g,"talisman",new(Common("skin",g),1),new(Common("blood",g),1,Common("core",g)),new("F01",1)));
            recipes.Add(new("YB_common_"+g,g,"refiner",new(Common("bone",g),3),new("M0"+g,1)));
        }
        foreach(var r in Rows)
        {
            recipes.Add(new(r.PillId,r.Grade,"alchemist",new(r.MaterialId,1),new(Common("blood",r.Grade),1,Common("core",r.Grade)),new(Herbs[r.Grade],1)));
            recipes.Add(new(r.TalismanId,r.Grade,"talisman",new(r.MaterialId,1),new(Common("blood",r.Grade),1,Common("core",r.Grade)),new("F01",1)));
            recipes.Add(new(r.ArtifactId,r.Grade,"refiner",new(r.MaterialId,2),new(Common("bone",r.Grade),2),new("M0"+r.Grade,1)));
        }
        return recipes.ToArray();
    }
}

internal static class MclslRecipeCatalog
{
    private static readonly Dictionary<string,MclslRecipeDefinition> Recipes = Build();
    private static Dictionary<string,MclslRecipeDefinition> Build()
    {
        var result=MclslBeastContent.Recipes.ToDictionary(x=>x.ProductId,StringComparer.Ordinal);
        foreach(var item in MclslItemCatalog.All)
            if(!result.ContainsKey(item.Id) && !string.IsNullOrEmpty(item.IngredientA) && !string.IsNullOrEmpty(item.IngredientB))
                result.Add(item.Id,new(item.Id,item.Grade,item.Category=="Pill"?"alchemist":item.Category=="Talisman"?"talisman":"refiner",new(item.IngredientA,1),new(item.IngredientB,1)));
        return result;
    }
    internal static MclslRecipeDefinition Get(string product) => product!=null && Recipes.TryGetValue(product,out var recipe)?recipe:null;
    internal static string Describe(string product) => Get(product) is {} recipe
        ? string.Join(" + ",recipe.Ingredients.Select(x=>string.Join("／",new[]{x.ItemId}.Concat(x.Alternatives).Select(MclslItemCatalog.IngredientDisplayName))+"×"+x.Count)) : "无制作配方";
}
