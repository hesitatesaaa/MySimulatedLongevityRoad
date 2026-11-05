using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static class MclslCareerStateValidation
{
    internal static void LoadBattles(string raw,Dictionary<long,MclslCareerBattleState> battles,
        Dictionary<long,long> summons,HashSet<long> active,double now)
    {
        var records=JsonConvert.DeserializeObject<List<MclslCareerBattleState>>(raw);
        if(records==null)throw new InvalidOperationException("Missing career battle list");
        foreach(var battle in records)
        {
            Rebase(battle,now);
            if(battles.ContainsKey(battle.Owner))throw new InvalidOperationException("Duplicate career owner");
            battles.Add(battle.Owner,battle);active.Add(battle.Owner);
            foreach(var summon in battle.Summons)
            {
                if(summons.ContainsKey(summon.Unit))throw new InvalidOperationException("Duplicate career summon");
                summons.Add(summon.Unit,battle.Owner);
            }
        }
    }
    internal static void Finite(double value)
    {
        if(double.IsNaN(value)||double.IsInfinity(value))throw new InvalidOperationException("Non-finite career state");
    }
    internal static void Validate(MclslCombatCareerState s)
    {
        if(s==null||s.Version!=2||s.Career < -1||s.Career>=6||s.Rank<0||s.Rank>5
            ||s.PreferredCareer < -1||s.PreferredCareer>=6
            ||s.Mastery==null||s.Mastery.Length!=5||s.SkillExperience==null||s.SkillExperience.Length!=5
            ||s.Cooldowns==null||s.Cooldowns.Length!=5||s.Opportunity==null||s.Source==null
            ||s.ContractKind < -1||s.ContractKind > 3||s.ContractSchema < 0||s.ContractSchema > 1)
            throw new InvalidOperationException("Invalid career state");
        Finite(s.CooldownClock);Finite(s.NextSummonAt);
        for(int i=0;i<5;i++)
        {
            if(s.Mastery[i]<1||s.Mastery[i]>10||s.SkillExperience[i]<0)
                throw new InvalidOperationException("Invalid career skill state");
            Finite(s.Cooldowns[i]);
        }
    }
    internal static void Rebase(MclslCareerBattleState b,double now)
    {
        if(b==null||b.Version!=2||b.Owner<=0||b.Career<0||b.Career>=6||b.Marks==null||b.Zones==null||b.Summons==null)
            throw new InvalidOperationException("Invalid career battle state");
        Finite(now);Finite(b.Clock);
        double shift=now-b.Clock;Finite(shift);
        if(b.LastAction!=0)b.LastAction=Shift(b.LastAction,shift);
        b.NextDecision=Shift(b.NextDecision,shift);
        b.GuardUntil=Shift(b.GuardUntil,shift);b.ResonanceUntil=Shift(b.ResonanceUntil,shift);
        b.UnloadUntil=Shift(b.UnloadUntil,shift);b.NextPassiveEffect=Shift(b.NextPassiveEffect,shift);b.NextAudio=Shift(b.NextAudio,shift);
        // 0 表示“未开始/立即”，不能被 shift 变成未来时刻。
        if(b.PetBuffUntil!=0)b.PetBuffUntil=Shift(b.PetBuffUntil,shift);
        if(b.Cast!=null)
        {
            if(b.Cast.Tier<1||b.Cast.Tier>5||b.Cast.Phase<0||b.Cast.Phase>1||b.Cast.Target<=0)
                throw new InvalidOperationException("Invalid career cast");
            b.Cast.Due=Shift(b.Cast.Due,shift);
        }
        foreach(var pair in b.Marks)
        {
            var m=pair.Value;
            if(pair.Key<=0||m==null)throw new InvalidOperationException("Invalid career mark");
            m.Expires=Shift(m.Expires,shift);m.OwnerHit=Shift(m.OwnerHit,shift);
            m.SpiritHit=Shift(m.SpiritHit,shift);m.NextAssist=Shift(m.NextAssist,shift);
            if(m.ContractMark!=0)m.ContractMark=Shift(m.ContractMark,shift);
        }
        foreach(var z in b.Zones)
        {
            if(z==null||z.Tier<1||z.Tier>5)throw new InvalidOperationException("Invalid career zone");
            z.Expires=Shift(z.Expires,shift);z.Pulse=Shift(z.Pulse,shift);
        }
        foreach(var s in b.Summons)
        {
            if(s==null||s.Unit<=0||s.Kind<0||s.Kind>3)throw new InvalidOperationException("Invalid career summon");
            s.Pulse=Shift(s.Pulse,shift);
            if(s.NextAttack!=0)s.NextAttack=Shift(s.NextAttack,shift);
            if(s.NextHeal!=0)s.NextHeal=Shift(s.NextHeal,shift);
        }
        b.Clock=now;
    }
    private static double Shift(double value,double shift)
    {
        Finite(value);value+=shift;Finite(value);return value;
    }
}
