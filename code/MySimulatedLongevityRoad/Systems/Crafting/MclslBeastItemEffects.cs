using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastItemEffects
{
    private static readonly Dictionary<string,(string Kind,string Effect,int Grade)> Definitions=Build();
    private static Dictionary<string,(string,string,int)> Build()
    {
        var definitions=new Dictionary<string,(string,string,int)>(StringComparer.Ordinal);
        foreach(var item in MclslBeastConsumables.All)
            definitions.Add(item.Id,(item.Kind,item.Effect,item.Grade));
        return definitions;
    }
    internal static bool IsConsumable(string id)=>id!=null&&Definitions.ContainsKey(id);
    private static string Status(string kind,string effect,int grade)=>"mclsl_beast_"+kind+"_"+effect+"_"+grade;
    internal static void RegisterStatuses()
    {
        foreach(var control in new[]{("mclsl_beast_water_slow",-0.3f,"S013"),("mclsl_beast_yin_bind",-0.5f,"S008")})
            if(AssetManager.status.get(control.Item1)==null)
            {
                var asset=new StatusAsset { id=control.Item1,duration=3,base_stats=new BaseStats(),can_be_cured=true,path_icon="ui/Spells/"+control.Item3,
                    locale_id=control.Item3,locale_description=control.Item3+" Description" };
                MclslItemUseSystem.TrySetStat(asset.base_stats,"multiplier_speed",control.Item2);AssetManager.status.add(asset);
            }
        foreach(var definition in Definitions)
        {
            var (kind,effect,grade)=definition.Value;
            if(kind=="pill"&&effect=="heal" || kind=="talisman"&&effect is not ("guard" or "speed" or "soul" or "cleanse"))continue;
            string id=Status(kind,effect,grade);if(AssetManager.status.get(id)!=null)continue;
            var status=new StatusAsset { id=id,duration=15+grade*5,base_stats=new BaseStats(),path_icon=MclslItemCatalog.Get(definition.Key).IconPath,
                locale_id=definition.Key,locale_description=definition.Key+" Description" };
            if(kind=="pill" && effect=="body")MclslItemUseSystem.TrySetStat(status.base_stats,"multiplier_health",MclslBeastContent.Bonus(grade));
            if(effect is "wind" or "speed")MclslItemUseSystem.TrySetStat(status.base_stats,"multiplier_speed",kind=="pill"?MclslBeastContent.Bonus(grade):0.1f+grade*0.05f);
            AssetManager.status.add(status);
        }
    }
    private static int ActiveGrade(Actor actor,string kind,string effect)
    {for(int grade=4;grade>=1;grade--)if(actor.hasStatus(Status(kind,effect,grade)))return grade;return 0;}
    private static bool Cooldown(Actor actor,string key,int seconds)
        =>double.TryParse(MclslActorAccessor.GetString(actor,key),NumberStyles.Float,CultureInfo.InvariantCulture,out double due)&&MclslSpellCasting.Now<due;
    internal static bool CanUse(Actor actor,MclslItemDefinition item,MclslConsumableUseTrigger trigger)
    {
        if(!Definitions.TryGetValue(item.Id,out var definition)||MclslBeastActorRegistration.IsOrdinary(actor))return false;
        var (kind,effect,grade)=definition;
        if(trigger is MclslConsumableUseTrigger.Breakthrough or MclslConsumableUseTrigger.Cultivation or MclslConsumableUseTrigger.Emergency)return false;
        if(kind=="pill")
        {
            string key=effect is "mana" or "soul"?"mclsl.mana_restore.last_time":"mclsl.beast.pill_due";
            if(effect is "mana" or "soul")
            {
                if(double.TryParse(MclslActorAccessor.GetString(actor,key),NumberStyles.Float,CultureInfo.InvariantCulture,out double last)&&MclslSpellCasting.Now-last<30)return false;
            }
            else if(Cooldown(actor,key,30))return false;
            bool needsRecovery=effect is "mana" or "soul"?MclslSpellSystem.CurrentMana(actor)<MclslSpellSystem.MaxMana(actor)
                :actor.getHealth()<actor.getMaxHealth();
            if(trigger!=MclslConsumableUseTrigger.Manual)
                return effect is "mana" or "soul"?MclslSpellSystem.CurrentMana(actor)<MclslSpellSystem.MaxMana(actor)*0.5f:actor.getHealth()<actor.getMaxHealth()*0.5f;
            return needsRecovery || effect!="heal"&&ActiveGrade(actor,kind,effect)<grade;
        }
        if(Cooldown(actor,"mclsl.beast.talisman_due",10))return false;
        if(effect is "guard" or "speed" or "soul" or "cleanse")return ActiveGrade(actor,kind,effect)<grade;
        return actor.attack_target is Actor target && MclslCombatTargets.IsEnemy(actor,target);
    }
    internal static bool Use(Actor actor,MclslItemDefinition item)
    {
        var (kind,effect,grade)=Definitions[item.Id];
        if(kind=="talisman" && effect is not ("guard" or "speed" or "soul" or "cleanse"))
        {if(!MclslSpellCasting.QueueTalisman(actor,effect,grade))return false;}
        else
        {
            if(effect is "soul" or "cleanse")Cleanse(actor,effect=="soul");
            if(effect!="heal" && ActiveGrade(actor,kind,effect)<grade)
            {
                for(int g=1;g<=4;g++)if(actor.hasStatus(Status(kind,effect,g)))actor.finishStatusEffect(Status(kind,effect,g));
                actor.addStatusEffect(Status(kind,effect,grade),15+grade*5);actor.updateStats();
            }
            if(kind=="pill")
            {
                if(effect is "mana" or "soul")MclslSpellSystem.RestoreMana(actor,0.1f+grade*0.1f);
                else actor.restoreHealthPercent(0.15f+grade*0.1f);
            }
        }
        if(kind=="pill" && effect is "mana" or "soul")MclslActorAccessor.Set(actor,"mclsl.mana_restore.last_time",MclslSpellCasting.Now.ToString("R",CultureInfo.InvariantCulture));
        else MclslActorAccessor.Set(actor,kind=="pill"?"mclsl.beast.pill_due":"mclsl.beast.talisman_due",(MclslSpellCasting.Now+(kind=="pill"?30:10)).ToString("R",CultureInfo.InvariantCulture));
        return true;
    }
    private static void Cleanse(Actor actor,bool control)
    {
        foreach(var id in control?new[]{"stunned","frozen","mclsl_beast_yin_bind","mclsl_beast_water_slow","mclsl_spell_yin_bind","mclsl_spell_all_bind","mclsl_spell_shock","mclsl_spell_frost_slow","mclsl_spell_vine_bind","mclsl_spell_soul_slow","mclsl_spell_domain_slow","mclsl_spell_origin_slow"}
            :new[]{"poisoned","poison","burning","weakness"})if(actor.hasStatus(id))actor.finishStatusEffect(id);
    }
    internal static float ArtifactBonus(Actor actor,string effect)
    {
        if(actor==null||MclslBeastActorRegistration.IsOrdinary(actor)||!MclslArtifactSystem.HasEquippedArtifact(actor))return 0;
        float value=0;
        foreach(var slot in MclslArtifactSystem.EquipmentSlots)
        {
            string item=MclslArtifactSystem.EquippedArtifactId(actor,slot);
            if(!item.StartsWith("YB_",StringComparison.Ordinal))continue;
            if(MclslBeastContent.BySpecies.TryGetValue(item.Substring(3),out var row)&&row.ArtifactEffect==effect)value+=MclslBeastContent.Bonus(row.Grade);
            if(effect=="mana"&&slot==MclslArtifactEquipmentSlot.Ring)value+=MclslBeastContent.Bonus(MclslItemCatalog.Get(item).Grade);
        }
        return Math.Min(0.4f,value);
    }
    internal static float ManaRecoveryBonus(Actor actor)=>ArtifactBonus(actor,"mana_regen")+MclslBeastContent.Bonus(ActiveGrade(actor,"pill","mana"))* (ActiveGrade(actor,"pill","mana")>0?1:0);
    internal static float ControlFactor(Actor actor)
    {
        int pill=ActiveGrade(actor,"pill","soul"),talisman=ActiveGrade(actor,"talisman","soul");
        float bonus=Math.Max(pill==0?0:MclslBeastContent.Bonus(pill),talisman==0?0:0.15f+talisman*0.05f);
        return 1-Math.Min(0.75f,bonus+ArtifactBonus(actor,"control_guard"));
    }
    internal static float DamageFactor(Actor actor,AttackType type)
    {
        if(MclslBeastActorRegistration.IsOrdinary(actor))return 1;
        int pill=ActiveGrade(actor,"pill","guard"),talisman=ActiveGrade(actor,"talisman","guard");
        float reduction=Math.Max(pill==0?0:MclslBeastContent.Bonus(pill),talisman==0?0:MclslBeastContent.Bonus(talisman));
        string effect=MclslSpellCasting.DamageLaw=="physical"||!MclslSpellSystem.IsApplyingSpellDamage&&type==AttackType.Weapon?"physical_guard":"spell_guard";
        reduction+=ArtifactBonus(actor,effect);
        if(MclslSpellCasting.DamageLaw=="阴")reduction+=ArtifactBonus(actor,"阴_guard");
        if(MclslSpellCasting.DotDamage)
        {
            int pc=ActiveGrade(actor,"pill","cleanse"),tc=ActiveGrade(actor,"talisman","cleanse");
            reduction+=Math.Max(pc==0?0:MclslBeastContent.Bonus(pc),tc==0?0:0.15f+tc*0.05f)+ArtifactBonus(actor,"dot_guard");
        }
        return 1-Math.Min(0.85f,reduction);
    }
    internal static bool TryAuto(Actor actor,MclslConsumableUseTrigger trigger,bool includeTalisman)
    {
        if(!MclslActorAccessor.Alive(actor)||MclslBeastActorRegistration.IsOrdinary(actor))return false;
        var items=MclslBagSystem.Peek(actor).Items.Where(x=>x.Count>0&&IsConsumable(x.ItemId))
            .OrderByDescending(x=>MclslItemCatalog.Get(x.ItemId).Grade).ThenBy(x=>x.ItemId,StringComparer.Ordinal).ToArray();
        foreach(var held in items)
        {
            var item=MclslItemCatalog.Get(held.ItemId);
            if(item.Category=="Talisman"&&!includeTalisman)continue;
            if(CanUse(actor,item,trigger)&&MclslItemUseSystem.TryUseBeastAutomatic(actor,item.Id,trigger))return true;
        }
        return false;
    }
}
