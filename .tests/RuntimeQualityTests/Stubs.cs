// Only game/Unity boundaries are substituted. Scheduling, decoding, saving,
// target locking and the complete pending-spell loop come from production.
using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

internal struct Position
{
    public float x,y;
    public float sqrMagnitude=>x*x+y*y;
    public float magnitude=>MathF.Sqrt(sqrMagnitude);
    public static Position operator -(Position a,Position b)=>new(){x=a.x-b.x,y=a.y-b.y};
}
internal class BaseSimObject { }
internal class Actor : BaseSimObject
{
    public long Id;
    public float Health=1000000;
    public bool Beast;
    public BaseSimObject attack_target;
    public Position current_position;
    public object current_tile;
    public Kingdom kingdom;
    public Stats stats=new();
    public readonly Dictionary<string,string> Data=new();
    public void addStatusEffect(string name,float duration) { }
    public void restoreHealthPercent(float amount) { }
}
internal class Stats { public float get(string name)=>80; }
internal class Kingdom { public bool isEnemy(Kingdom other)=>this!=other; }
internal enum AttackType { Fire,Weapon,Divine }
internal static class Config { internal static bool paused; }
internal sealed class SaveCustomData
{
    internal readonly Dictionary<string,string> Values=new();
    public void get(string key,out string value,string fallback)=>value=Values.TryGetValue(key,out var v)?v:fallback;
    public void set(string key,string value)=>Values[key]=value;
}
internal sealed class MapStats { public SaveCustomData custom_data=new(); }
internal sealed class Map { public MapStats map_stats=new(); }
internal static class World { public static Map world=new(); }
internal static class Finder { public static List<Actor> getUnitsFromChunk(object tile,int count,float range,bool flag)=>new(); }

namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslWorldContext { internal static long Generation; }
    internal static class MclslFrameDeadline { internal static bool Expired; }
    internal static class MclslPerformanceProbe
    {
        internal static long Begin()=>0;
        internal static void End(string name,long value) { }
    }
    internal static class MclslDiagnostics
    {
        internal static int Errors;
        internal static void Error(string key,string message)=>Errors++;
    }
    internal static class MclslActorAccessor
    {
        internal static long Id(Actor actor)=>actor.Id;
        internal static bool Alive(Actor actor)=>actor!=null&&actor.Health>0;
        internal static string GetString(Actor actor,string key)=>actor.Data.TryGetValue(key,out var value)?value:"";
        internal static int GetInt(Actor actor,string key,int fallback)=>fallback;
        internal static void Set(Actor actor,string key,string value)=>actor.Data[key]=value;
    }
    internal static class MclslActorRegistry
    {
        internal static readonly Dictionary<long,Actor> Actors=new();
        internal static bool ResolveKnownOrWorld(long id,out Actor actor)=>Actors.TryGetValue(id,out actor);
    }
}
namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslBeastActorRegistration { internal static bool IsBeast(Actor actor)=>actor.Beast; }
}
namespace MySimulatedLongevityRoad.Data
{
    internal sealed class MclslSpellDefinition
    {
        internal string Id="S004",Law="火";
        internal int MaxTargets=1;
        internal float Power=1;
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslBeastLoot { internal static double Now; }
    internal static class MclslBeastCultivation { internal const string TalentKey="talent",TalentLevelKey="level"; }
    internal static class MclslSpellProgression { internal static float Strength(Actor a,string id)=>1; }
    internal static class MclslPhysiqueSystem { internal static float FireDamagePercent(Actor a)=>0; }
    internal static class MclslBeastItemEffects { internal static float ArtifactBonus(Actor a,string law)=>0; }
    internal static class MclslSpellEffect
    {
        internal static void Show(Actor caster,Actor target,string id,string phase,float duration) { }
    }
    internal static class MclslSpellTerrain { internal static void Queue(object tile,string id) { } }
    internal static class MclslSpellSystem
    {
        internal static readonly List<float> Hits=new();
        internal static Action OnDamage;
        internal static int RemainingMana=100;
        internal static void DeliverDamage(Actor caster,Actor target,float damage,AttackType type)
        {
            Hits.Add(damage);target.Health-=damage;OnDamage?.Invoke();
        }
    }
}
