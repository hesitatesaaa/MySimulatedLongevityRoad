using System;
using System.Linq;
using System.Text;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static partial class MclslCombatCareerSystem
{
    internal static bool Applicable(Actor actor) => MclslActorAccessor.Alive(actor)
        && !MclslBeastActorRegistration.IsBeast(actor)
        && !actor.asset.id.StartsWith("mclsl_career_spirit_",StringComparison.Ordinal)
        && actor.isSapient() && MclslEligibility.IsCivilizedActor(actor);

    internal static int Roll(long id, int year, int salt)
    {
        unchecked
        {
            ulong n = (ulong)id ^ ((ulong)(uint)year << 32) ^ (uint)salt;
            n ^= n >> 30; n *= 0xbf58476d1ce4e5b9UL; n ^= n >> 27;
            n *= 0x94d049bb133111ebUL; n ^= n >> 31;
            return (int)(n % 10000UL);
        }
    }
    internal static int Rank(Actor actor) => Applicable(actor) ? Read(actor).Rank : 0;
    internal static int Career(Actor actor) => Applicable(actor) ? Read(actor).Career : -1;
    internal static bool HasPassive(Actor actor,int career,int tier)
    {
        if(!Applicable(actor)) return false;
        var state=Read(actor);
        return state.Career==career && state.Rank>=tier
            && MclslActorAccessor.IsCultivator(actor) && MclslRealmIds.Index(MclslActorAccessor.Realm(actor))>=tier-1;
    }
    // Combat-hit fast path: MclslCareerCombat.Passive used to call Career(actor) and
    // HasPassive, each re-running Applicable + Read and each re-reading the realm
    // string (IsCultivator reads it, then Realm reads it again) — so a single passive
    // probe did two Applicable visits, two cached-state reads and two realm reads.
    // This collapses it to one of each. The condition is identical: Applicable is
    // required, and because the career argument was always Career(actor) the
    // "state.Career==career" test was a tautology whenever Applicable held.
    internal static bool Passive(Actor actor,int tier)
    {
        if(!Applicable(actor)) return false;
        var state=Read(actor);
        if(state.Rank<tier) return false;
        string realm=MclslActorAccessor.Realm(actor);
        if(string.IsNullOrWhiteSpace(realm)) return false;
        return MclslRealmIds.Index(realm)>=tier-1;
    }
    internal static int TraitCareer(string id) => Array.FindIndex(MclslCombatCareerCatalog.Careers,x=>x.Trait==id);
    internal static bool IsPassiveTrait(string id) => id.StartsWith("MclslCombatPassive_",StringComparison.Ordinal);
    internal static void InitTraits()
    {
        foreach(var group in new[]{("MclslCombatCareers","战斗职业"),("MclslCombatPassives","职业被动")})
        {
            string key="trait_group_"+group.Item1;
            MclslLocalizationBridge.RegisterKey(key,group.Item2);
            AssetManager.trait_groups.add(new ActorTraitGroupAsset { id=group.Item1,name=key,color="#72D4BF" });
        }
        foreach(var career in MclslCombatCareerCatalog.Careers)
            RegisterTrait(new RegisterTraitArgs
            {
                Id = career.Trait,
                Name = career.Name,
                Icon = career.Icon,
                Group = "MclslCombatCareers",
                Grantable = true,
                Description = "主修"+career.Name+"；阶位、晋级条件与职业技能见右侧修士档案。可与生产职业并存。"
            });
        foreach(var skill in MclslCombatCareerCatalog.Skills.Where(x=>x.Passive))
            RegisterTrait(new RegisterTraitArgs { Id = skill.Trait, Name = skill.Name, Icon = skill.Icon, Group = "MclslCombatPassives", Grantable = false, Description = skill.Description });
    }
    private static void RegisterTrait(RegisterTraitArgs a)
    {
        string id = a.Id;
        string name = a.Name;
        string icon = a.Icon;
        string group = a.Group;
        bool grantable = a.Grantable;
        string description = a.Description;
        MclslLocalizationBridge.RegisterKey(id,name);
        MclslLocalizationBridge.RegisterKey("trait_"+id,name);
        MclslLocalizationBridge.RegisterKey(id+" Description",description);
        MclslTraitRegistration.RegisterTraitInfo(id,"斗法传承·"+name,description);
        AssetManager.traits.add(new ActorTrait { id=id,group_id=group,path_icon=icon,
            can_be_given=grantable,show_in_meta_editor=true,needs_to_be_explored=false,
            rate_birth=0,rate_inherit=0,rate_acquire_grow_up=0,base_stats=new BaseStats() });
    }
    internal static bool HandleAdded(Actor actor,string trait)
    {
        int career=TraitCareer(trait);
        if(career<0 && !IsPassiveTrait(trait)) return false;
        if(!Applicable(actor))
        {
            MclslTraitGrantRouter.SuppressRouting(()=>actor.removeTrait(trait));
            return true;
        }
        if(career>=0) SetCareer(actor,career,"玩家授予"); else SyncTraits(actor);
        return true;
    }
    internal static bool HandleRemoved(Actor actor,string trait)
    {
        int career=TraitCareer(trait);
        if(career<0 && !IsPassiveTrait(trait)) return false;
        if(career>=0)
        {
            var state=Read(actor);
            if(state.Career==career)
            {
                MclslCareerCombat.RemoveOwner(actor);
                state.Career=-1;state.Rank=0;state.Experience=0;state.TrainingYears=0;
                state.TrialProgress=0;state.Mastery=new[]{1,1,1,1,1};state.SkillExperience=new int[5];
                state.Cooldowns=new double[5];state.Source="";state.Opportunity="";
                state.OpportunityRate=0;state.OpportunityYear=-1;state.PreferredCareer=-1;
                Write(actor,state);
            }
        }
        SyncTraits(actor);return true;
    }
    internal static void SetCareer(Actor actor,int career,string source)
    {
        var state=Read(actor);
        if(IsBlocked(actor))return;
        if(state.Career!=career)
        {
            MclslCareerCombat.RemoveOwner(actor);
            state.TrialProgress=0;state.Mastery=new[]{1,1,1,1,1};state.SkillExperience=new int[5];
            state.Cooldowns=new double[5];
        }
        state.Career=career;state.Rank=Math.Max(1,state.Rank);state.Source=source;
        if(career==5&&state.ContractSchema<1)
        {
            // 灵契师首次就职：玩家授予则待选择，自然获得则四类等概率随机；已绑定的再授予不刷新。
            state.ContractSchema=1;
            if(state.ContractKind<0)
                state.ContractKind=source=="玩家授予"?-1
                    :MclslContractRules.RandomKind(Roll(MclslActorAccessor.Id(actor),0,0x5C57));
        }
        Write(actor,state);SyncTraits(actor);MclslWorldActorQuery.Track(actor);
    }
    internal static void SyncTraits(Actor actor)
    {
        var state=Read(actor);
        if(IsBlocked(actor))return;
        MclslTraitGrantRouter.SuppressRouting(()=>
        {
            foreach(var career in MclslCombatCareerCatalog.Careers)
            {
                bool wanted=state.Career==career.Index;
                if(wanted && !actor.hasTrait(career.Trait)) actor.addTrait(career.Trait);
                if(!wanted && actor.hasTrait(career.Trait)) actor.removeTrait(career.Trait);
            }
            int realm=MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
            foreach(var skill in MclslCombatCareerCatalog.Skills.Where(x=>x.Passive))
            {
                bool wanted=state.Career==skill.Career && state.Rank>=skill.Tier && realm>=skill.Tier-1;
                if(wanted && !actor.hasTrait(skill.Trait)) actor.addTrait(skill.Trait);
                if(!wanted && actor.hasTrait(skill.Trait)) actor.removeTrait(skill.Trait);
            }
        });
        if (state.Career >= 0) MclslCareerCombat.Track(actor);
    }
    internal static int Preferred(Actor actor)
    {
        long id=MclslActorAccessor.Id(actor);
        string roots=MclslActorAccessor.GetString(actor,MclslActorDataKeys.SpiritualRootAttributes);
        string technique=MclslActorAccessor.GetString(actor,MclslActorDataKeys.TechniqueName);
        string[] hints={"剑锋斩金","体拳山土","火水雷五行","符印篆","阵盘域","灵兽契木"};
        int best=0,score=-1;
        for(int c=0;c<6;c++)
        {
            int value=Roll(id,0,0x2840+c)%100;
            foreach(char ch in hints[c]) {if(roots.Contains(ch))value+=10;if(technique.Contains(ch))value+=20;}
            value+=MclslPhysiqueSystem.CareerAffinityScore(actor,c)/2;
            if(value>score){score=value;best=c;}
        }
        return best;
    }
    internal static void Offer(Actor actor,int year,string source,int rate,int career=-1)
    {
        if(!Applicable(actor))return;
        var state=Read(actor);if(IsBlocked(actor)||state.Career>=0)return;
        if(state.OpportunityYear<year-1 || rate>state.OpportunityRate)
        {
            state.OpportunityYear=year;state.OpportunityRate=rate;state.Opportunity=source;state.PreferredCareer=career;
            Write(actor,state);
        }
    }
    internal static void ProcessAnnual(Actor actor,int year)
    {
        if(!Applicable(actor))return;
        var state=Read(actor);if(IsBlocked(actor)||state.LastYear==year)return;
        state.LastYear=year;
        if(!actor.isAdult() || !MclslActorAccessor.IsCultivator(actor)){Write(actor,state);return;}
        if (long.TryParse(MclslActorAccessor.GetString(actor,MclslActorDataKeys.AncientMentorTeacherId),out long teacherId)
            && MclslActorRegistry.ResolveKnownOrWorld(teacherId,out Actor teacher)
            && Applicable(teacher) && Read(teacher).Career>=0)
            Offer(actor,year,"师承",1000,Read(teacher).Career);
        state.TrainingYears++;
        if(state.Career<0)
        {
            if(state.Potential && state.LastAdmissionYear!=year)
            {
                state.LastAdmissionYear=year;
                bool opportunity=state.OpportunityYear==year || state.OpportunityYear==year-1;
                int rate=opportunity?state.OpportunityRate:state.TrainingYears>=5?500:0;
                if(Roll(MclslActorAccessor.Id(actor),year,0x98341)<rate)
                {
                    int career=opportunity&&state.PreferredCareer>=0?state.PreferredCareer:Preferred(actor);
                    string source=opportunity?state.Opportunity:"五年训练·自悟";
                    Write(actor,state);SetCareer(actor,career,source);return;
                }
            }
        }
        else
        {
            int growth=MclslPhysiqueSystem.CareerGrowthPercent(actor,state.Career);
            if(year%20==0) state.Experience+=(int)Math.Ceiling(40*(1+growth/100f));
            int tier=Math.Min(state.Rank,MclslRealmIds.Index(MclslActorAccessor.Realm(actor))+1);
            if(tier>0&&year%20==0) GainSkillExperience(state,tier,(int)Math.Ceiling(8*(1+growth/100f)));
            Promote(actor,state);
        }
        Write(actor,state);SyncTraits(actor);
    }
    internal static void GainSkillExperience(MclslCombatCareerState state,int tier,int amount)
    {
        int i=tier-1;
        if(state.Mastery[i]>=10)return;
        state.SkillExperience[i]+=amount;
        int threshold=state.Mastery[i]<3?MclslCombatCareerCatalog.MasteryPoints[i]/2:12+4*state.Mastery[i]+2*i;
        if(state.SkillExperience[i]>=threshold){state.SkillExperience[i]-=threshold;state.Mastery[i]++;}
    }
    internal static void Promote(Actor actor,MclslCombatCareerState state)
    {
        if(state.Rank<1||state.Rank>=5)return;
        int next=state.Rank;
        if(state.Experience>=MclslCombatCareerCatalog.Experience[next]
            &&MclslRealmIds.Index(MclslActorAccessor.Realm(actor))>=next
            &&state.Mastery[next-1]>=3 &&state.TrialProgress>=MclslCombatCareerCatalog.TrialTargets[next-1])
            {state.Rank++;state.TrialProgress=0;}
    }
    internal static string Overview(Actor actor)
    {
        if(!Applicable(actor))return string.Empty;
        var s=Read(actor);var b=new StringBuilder();
        b.Append(MclslActorArchiveText.Section("斗法传承"));
        b.Append(MclslActorArchiveText.Line("先天潜质",s.Potential?MclslActorArchiveText.Highlight("具备","#A7E08A"):"不具备"));
        if(s.Career<0)
        {
            b.Append(MclslActorArchiveText.Line("战斗职业","尚未入职"));
            b.Append(MclslActorArchiveText.Line("入职","成年、入道、潜质与传承机缘"));
            b.Append(MclslActorArchiveText.Line("训练",s.TrainingYears+"年 / 自悟需5年"));
            return b.ToString();
        }
        b.Append(MclslActorArchiveText.Line("战斗职业",
            MclslActorArchiveText.Highlight(MclslCombatCareerCatalog.Careers[s.Career].Name+"·"+MclslCombatCareerCatalog.Ranks[s.Rank],"#F1D17A")));
        b.Append(MclslActorArchiveText.Line("来源",s.Source));
        int bdmg=MclslPhysiqueSystem.CareerDamagePercent(actor,s.Career);
        int bgro=MclslPhysiqueSystem.CareerGrowthPercent(actor,s.Career);
        if(bdmg!=0||bgro!=0)
        {
            string role=MclslPhysiqueSystem.CareerAffinityRole(actor,s.Career);
            b.Append(MclslActorArchiveText.Line("体质加成",MclslCombatCareerCatalog.Careers[s.Career].Name
                +(role.Length>0?"（"+role+"）":"")
                +" 伤害"+(bdmg>=0?"+":"")+bdmg+"% 成长"+(bgro>=0?"+":"")+bgro+"%"));
        }
        string affinity=MclslPhysiqueSystem.CareerAffinityText(actor);
        if(affinity.Length>0) b.Append(MclslActorArchiveText.Line("体质契合",affinity));
        b.Append(MclslActorArchiveText.Line("职业经验",s.Rank<5?s.Experience+" / "+MclslCombatCareerCatalog.Experience[s.Rank]:s.Experience.ToString()));
        if(s.Rank<5)
        {
            int i=s.Rank-1;
            b.Append(MclslActorArchiveText.Line("晋级境界",MclslActorArchiveText.Highlight(new[]{"炼气","筑基","金丹","元婴","化神"}[s.Rank],"#9CD7FF")));
            b.Append(MclslActorArchiveText.Line("前阶熟练",s.Mastery[i]+" / 3"));
            b.Append(MclslActorArchiveText.Line("本阶试炼",s.TrialProgress+" / "+MclslCombatCareerCatalog.TrialTargets[i]));
            var missing=new System.Collections.Generic.List<string>();
            if(s.Experience<MclslCombatCareerCatalog.Experience[s.Rank])missing.Add("职业经验");
            if(MclslRealmIds.Index(MclslActorAccessor.Realm(actor))<s.Rank)missing.Add("修炼境界");
            if(s.Mastery[i]<3)missing.Add("前阶技能熟练度");
            if(s.TrialProgress<MclslCombatCareerCatalog.TrialTargets[i])missing.Add("本职业试炼");
            b.Append(MclslActorArchiveText.Line("尚缺",missing.Count==0?MclslActorArchiveText.Highlight("可晋级","#A7E08A"):string.Join("、",missing)));
        }
        return b.ToString();
    }
    internal static void Clear() { States=new();MclslCareerCombat.Clear();MclslCareerSupply.ClearRuntime(); }
}

internal struct RegisterTraitArgs
{
    internal string Id;
    internal string Name;
    internal string Icon;
    internal string Group;
    internal bool Grantable;
    internal string Description;
}
