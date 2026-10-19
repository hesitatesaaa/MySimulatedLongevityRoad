using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastCultivation
{
    internal static void Ensure(Actor actor)
    {
        if (!MclslBeastActorRegistration.IsOrdinary(actor)) return;
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < 0)
        {
            var beast=MclslBeastCatalog.ForAsset(actor.asset.id);
            int realm=MclslBeastContent.BySpecies[beast.Id].MinRealm;
            SetRealm(actor,MclslRealmIds.Ordered[realm],MclslRuntime.CurrentYear());
        }
    }

    internal static void SetRealm(Actor actor,string realm,int year)
    {
        if (!MclslBeastActorRegistration.IsOrdinary(actor) || MclslRealmIds.Index(realm)<0) return;
        float oldMaximum=actor.getMaxHealth();
        float fraction=oldMaximum>0?actor.getHealth()/oldMaximum:1;
        MclslActorAccessor.Set(actor,MclslActorDataKeys.CultivationSystem,MclslCultivationSystemIds.AncientLaw);
        MclslCultivationStateTransitions.TrySetRealm(actor,realm,year,"妖兽本能修炼");
        MclslCultivationGrowthSystem.Reconcile(actor,realm,true);
        MclslBeastActorRegistration.ReconcileForm(actor);
        MclslActorAccessor.ApplyDisplayName(actor,realm);
        actor.updateStats();
        actor.data.health=Math.Max(1,(int)Math.Min(int.MaxValue,actor.getMaxHealth()*Math.Clamp(fraction,0.001f,1f)));
        MclslActorAccessor.Set(actor,MclslActorDataKeys.ManaCurrent,MclslSpellSystem.MaxMana(actor));
    }

    internal static void TickAnnual(Actor actor,int year)
    {
        Ensure(actor);
        if (!MclslActorAccessor.Alive(actor) || MclslActorAccessor.GetInt(actor,"mclsl.beast.growth_year",-1)>=year) return;
        MclslActorAccessor.Set(actor,"mclsl.beast.growth_year",year);
        if (MclslLongevityRules.TryExpireAtAnnualLimit(actor,year)) return;
        string realm=MclslActorAccessor.Realm(actor);
        int index=MclslRealmIds.Index(realm);
        if(index<0 || index>=6) return;
        int aptitude=Math.Clamp(MclslActorAccessor.GetInt(actor,MclslActorDataKeys.Aptitude,60),1,100);
        var gift=MclslAptitudeGiftCatalog.ForAptitude(aptitude);
        float gain=gift.AnnualTrueEssence*MclslCultivationGrowthSystem.RealmGainScale(realm);
        MclslCultivationGrowthSystem.GrantTrueEssence(actor,realm,gain,year,true,applySameLaw:false);
        MclslActorAccessor.Set(actor,MclslActorDataKeys.LastCultivationYear,year);
        int threshold=MclslRealmProgress.NextRealmMinimum(realm,true);
        // The human new-law HeDao route uses world souls. Beasts use the ancient
        // essence threshold; their final bottleneck uses the same span again.
        if(index==5) threshold=MclslRealmProgress.AncientHeDaoEntryMinimum*2;
        if(threshold<=0 || MclslCultivationGrowthSystem.CurrentTrueEssence(actor)<threshold) return;
        int chance=Math.Clamp(24+aptitude/4-(index+1)*8+MclslWorldStateModifierSystem.BreakthroughStabilityBonus(year),8,100);
        if(MclslWorldEpochSystem.IsNewLawActive(year)) chance=Math.Max(3,chance/2);
        if(MclslBeastWorld.Roll(MclslActorAccessor.Id(actor)+"|growth|"+year,100)<chance)
            SetRealm(actor,MclslRealmIds.Ordered[index+1],year);
        else
        {
            float loss=index==0?14:index==1?20:index==2?28:35;
            MclslCultivationGrowthSystem.ApplyProgressSetback(actor,realm,loss,true);
            if(index==5) MclslCultivationGrowthSystem.SetTrueEssence(actor,realm,
                Math.Max(MclslRealmProgress.AncientHeDaoEntryMinimum,threshold-(int)(threshold*0.35f)),true,false);
            MclslActorAccessor.Set(actor,MclslActorDataKeys.LastBreakthroughResult,"妖兽本能突破未成");
        }
    }
    internal static string Talent(Actor actor)
    {
        var beast=MclslBeastCatalog.ForAsset(actor?.asset?.id);
        return beast?.Id switch
        {
            "zhuque" or "fenghuang"=>"S004", "xuanwu" or "tianshou"=>"S005",
            "qinglong"=>"S013", "yingying" or "mo" or "moqilin"=>"S008",
            "dafeng" or "manman" or "qingluan"=>"S015", "kunpeng" or "liejiejing"=>"S022",
            _=>"S001"
        };
    }
}
