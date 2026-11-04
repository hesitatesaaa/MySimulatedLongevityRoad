using System;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static partial class MclslCombatCareerSystem
{
    private sealed class Cache
    {
        internal string Raw;
        internal MclslCombatCareerState State;
        internal bool Blocked;
    }
    private static ConditionalWeakTable<Actor, Cache> States = new();

    internal static MclslCombatCareerState Read(Actor actor)
    {
        Cache cache=States.GetOrCreateValue(actor);
        string raw=MclslActorAccessor.GetString(actor,MclslCombatCareerCatalog.StateKey);
        if(cache.State!=null && cache.Raw==raw)return cache.State;
        try
        {
            var state=raw.Length==0
                ? new MclslCombatCareerState {Potential=Roll(MclslActorAccessor.Id(actor),0,0x621981)<4000}
                : JsonConvert.DeserializeObject<MclslCombatCareerState>(raw);
            state.ContractPetName??=string.Empty;
            MclslCareerStateValidation.Validate(state);
            if(raw.Length>0)
            {
                double now=MclslSpellCasting.Now;
                for(int i=0;i<5;i++)
                {
                    state.Cooldowns[i]=now+Math.Max(0,state.Cooldowns[i]-state.CooldownClock);
                    MclslCareerStateValidation.Finite(state.Cooldowns[i]);
                }
                state.NextSummonAt=MclslContractRules.Rebase(state.NextSummonAt,state.CooldownClock,now);
                MclslCareerStateValidation.Finite(state.NextSummonAt);
            }
            cache.Raw=raw;cache.State=state;cache.Blocked=false;
            if(raw.Length==0)Write(actor,state);
        }
        catch(Exception ex)
        {
            // An independent disabled state per actor; raw data stays untouched.
            cache.Raw=raw;cache.State=new MclslCombatCareerState();cache.Blocked=true;
            MclslDiagnostics.Error("career-state-load-"+MclslActorAccessor.Id(actor),ex.ToString());
        }
        return cache.State;
    }
    private static bool IsBlocked(Actor actor)=>States.TryGetValue(actor,out var cache)&&cache.Blocked;

    internal static void Write(Actor actor,MclslCombatCareerState state)
    {
        Cache cache=States.GetOrCreateValue(actor);
        if(cache.Blocked)return;
        state.CooldownClock=MclslSpellCasting.Now;
        string raw=JsonConvert.SerializeObject(state);
        cache.Raw=raw;cache.State=state;
        MclslActorAccessor.Set(actor,MclslCombatCareerCatalog.StateKey,raw);
    }
}
