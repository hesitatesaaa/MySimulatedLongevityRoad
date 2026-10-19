using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Systems.Combat;
using UnityEngine;

public class BaseSimObject { }
public enum AttackType { Fire, Weapon, Divine }
public class Actor : BaseSimObject
{
    public long Id;public object data=new();public bool Alive=true,Path=true;public string Realm="r4";
    public float Health=100,Maximum=100;public Stats stats=new();public Kingdom kingdom=new();
    public BaseSimObject attack_target;public WorldTile current_tile=new();public Vector2 current_position;
    public Dictionary<string,string> Data=new();public HashSet<string> Traits=new();public Dictionary<string,float> Statuses=new();
    public float getHealth()=>Health;public float getMaxHealth()=>Maximum;
    public void restoreHealthPercent(float f)=>Health=Math.Min(Maximum,Health+Maximum*f);
    public bool hasTrait(string t)=>Traits.Contains(t);public void addTrait(string t)=>Traits.Add(t);public void removeTrait(string t)=>Traits.Remove(t);
    public void addStatusEffect(string s,float t)=>Statuses[s]=t;
    public void addForce(float a,float b,float c,bool d,bool e){}
    public void stopMovement(){}public void setCurrentTilePosition(WorldTile t)=>current_tile=t;
}
public class Stats { public float get(string s)=>10; }
public class Kingdom { public int Team;public bool isEnemy(Kingdom k)=>Team!=k.Team; }
public struct TilePos{public int x,y;public TilePos(int a,int b){x=a;y=b;}}
public class WorldTile { public Vector2 current_position;public TilePos pos;public Vector3 posV3;public TileType Type=new();public void setBurned(int n)=>MapAction.Burns++; }
public class TileType { public bool liquid,block;public string id="soil"; }
public class MapStats { public SaveCustomData custom_data=new(); }
public class SaveCustomData
{
    public Dictionary<string,string> Values=new();public void get(string k,out string v,string fallback)=>v=Values.GetValueOrDefault(k,fallback);
    public void set(string k,string v)=>Values[k]=v;
}
public class MapBox { public static int width=100,height=100;public MapStats map_stats=new();public WorldTile GetTileSimple(int x,int y)=>new(){pos=new(x,y)}; }
public class TerraformOptions { public int damage;public bool damage_buildings,destroy_buildings,make_ruins,remove_roads,remove_trees_fully,add_burned; }
public static class MapAction
{
    public static List<(int X,int Y)> Damaged=new();public static int Burns;public static bool Safe=true;
    public static bool checkTileDamageGaiaCovenant(WorldTile t,bool d)=>true;
    public static void decreaseTile(WorldTile t,bool b,TerraformOptions o)
    {Damaged.Add((t.pos.x,t.pos.y));Safe&=o.damage==0&&o.damage_buildings&&o.destroy_buildings;}
}
public class World : MapBox { public static World world=new();public double Time;public double getCurWorldTime()=>Time; }
public static class Config { public static bool paused; }
public static class Finder { public static Actor[] Nearby=Array.Empty<Actor>();public static IEnumerable<Actor> getUnitsFromChunk(WorldTile t,int n,float f,bool b)=>Nearby; }

namespace UnityEngine
{
    public struct Vector2
    {
        public float x,y;public Vector2(float a,float b){x=a;y=b;}public float sqrMagnitude=>x*x+y*y;public float magnitude=>MathF.Sqrt(sqrMagnitude);
        public Vector2 normalized=>magnitude>0?new(x/magnitude,y/magnitude):new();
        public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.x+b.x,a.y+b.y);
        public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.x-b.x,a.y-b.y);
        public static Vector2 operator *(Vector2 a,float b)=>new(a.x*b,a.y*b);
        public static implicit operator Vector3(Vector2 a)=>new(a.x,a.y,0);
    }
    public struct Vector3
    {
        public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;}
        public static Vector3 one=>new(1,1,1);public static Vector3 operator +(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator *(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);
    }
    public struct Color { public Color(float a,float b,float c,float d){} }
    public class Transform { public Vector3 position,localScale; }
    public class GameObject { public Transform transform=new(); }
    public class Object { public static void Destroy(object o){} }
    public class Sprite {}
    public class SpriteRenderer { public Sprite sprite;public Color color;public bool flipX; }
    public static class Mathf { public static float Cos(float v)=>MathF.Cos(v);public static float Sin(float v)=>MathF.Sin(v); }
    public static class Time{public static int frameCount;}
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslDiagnostics { internal static bool Enabled=false;internal static List<string> Errors=new();internal static void Error(string k,string s)=>Errors.Add(k);internal static void Throttle(string s,int f,int t,string m){} }
    internal static class MclslRuntimeWorkBudget {internal static int ScaleCount(int a,int b)=>a;}
    internal static class MclslRuntime { internal static int CurrentYear()=>10; }
}
namespace MySimulatedLongevityRoad.Data
{
    internal static class MclslRealmIds { internal static int Index(string r)=>int.TryParse(r?.TrimStart('r'),out int i)?i:-1; }
    internal static class MclslActorDataKeys { internal const string TechniqueId="tech",AncientSectOrigin="sect",Aptitude="apt",SpiritualRootPrimary="root",ManaCurrent="mana"; }
    internal class MclslTechniqueDefinition { internal string Name="五行";internal string[] LawPool={"火"}; }
    internal static class MclslCultivationCatalog { internal static MclslTechniqueDefinition Technique(string s)=>new(); }
}
namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslBeastActorRegistration { internal static bool IsOrdinary(Actor a)=>false; }
}
namespace MySimulatedLongevityRoad.Systems.Visual
{
    internal static class MclslWorldSpriteRenderLayer
    {
        internal static bool TryCreateStaticWorldSprite(Vector3 p,float s,int l,Sprite sprite,out GameObject o,out SpriteRenderer r)
        {o=new();r=new();return true;}
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslActorAccessor
    {
        internal static long Id(Actor a)=>a?.Id??0;internal static bool Alive(Actor a)=>a!=null&&a.Alive&&a.Health>0;
        internal static string Realm(Actor a)=>a?.Realm??"";
        internal static bool IsCultivator(Actor a)=>a?.Path==true;internal static bool HasCultivationPath(Actor a)=>a?.Path==true;
        internal static string GetString(Actor a,string k,string fallback="")=>a?.Data.GetValueOrDefault(k,fallback)??fallback;
        internal static int GetInt(Actor a,string k,int fallback=0)=>int.TryParse(GetString(a,k),out int n)?n:fallback;
        internal static void Set(Actor a,string k,string v)=>a.Data[k]=v;
        internal static void Set(Actor a,string k,int v)=>a.Data[k]=v.ToString();
    }
    internal static class MclslActorRegistry
    {internal static Dictionary<long,Actor> Actors=new();internal static bool ResolveKnownOrWorld(long id,out Actor a)=>Actors.TryGetValue(id,out a);}
    internal static class MclslBeastLoot { internal static double Now=>World.world.Time; }
    internal static class MclslPhysiqueSystem { internal static float FireDamagePercent(Actor a)=>0; }
    internal static class MclslBeastItemEffects { internal static float ArtifactBonus(Actor a,string s)=>0; }
    internal static class MclslTraitGrantRouter { internal static void SuppressRouting(Action a)=>a(); }
    internal static class MclslSpiritualRootSystem { internal static string[] RootAttributes(Actor a)=>new[]{"火"}; }
    internal sealed class MclslSpellDefinition
    {internal string Id,Law="金",Name;internal int MinRealm,ManaCost=10,MaxTargets=1;internal float Power=1,Cooldown=4;}
    internal sealed class TestSpellCatalog:Dictionary<string,MclslSpellDefinition>
    {public new IEnumerator<MclslSpellDefinition> GetEnumerator()=>Values.GetEnumerator();}
    internal static partial class MclslSpellSystem
    {
        internal static TestSpellCatalog All=new();internal static Dictionary<long,HashSet<string>> Learned=new();
        internal static Action<Actor,Actor,float> DamageCallback;internal static Action SelfCallback;
        internal static List<(long Target,float Damage)> Hits=new();internal static int SelfCalls;
        internal static bool TryGet(string id,out MclslSpellDefinition s)=>All.TryGetValue(id??"",out s);
        internal static bool Knows(Actor a,string id)=>Learned.TryGetValue(a.Id,out var known)&&known.Contains(id);
        internal static bool Learn(Actor a,string id)
        {
            if(!MclslBattleProfessionSystem.CanLearn(a,id))return false;
            if(!Learned.TryGetValue(a.Id,out var known))Learned[a.Id]=known=new();
            if(!known.Add(id))return false;MclslBattleProfessionSystem.OnLearned(a,id);return true;
        }
        private static Dictionary<string,MclslSpellDefinition> ById=>All;
        private static ConditionalWeakTable<Actor,RuntimeState> _runtime=new();
        internal static int CurrentMana(Actor a)=>MclslActorAccessor.GetInt(a,"mana",1000);
        private static void PersistManaTime(Actor a){}
        internal static double Cooldown(Actor a,string id)=>GetRuntime(a).NextCast.GetValueOrDefault(id);
        internal static bool CastPaid(Actor a,Actor b,string id)
        {if(!MclslSpellCasting.Queue(a,b,All[id]))return false;CommitCast(a,GetRuntime(a),All[id],CurrentMana(a),b,b.current_tile);return true;}
        internal static float LinkedDamage(Actor a,MclslSpellDefinition s)=>10;
        internal static void DeliverDamage(Actor a,Actor target,float d,AttackType t){Hits.Add((target.Id,d));DamageCallback?.Invoke(a,target,d);}
        internal static void ApplySelfEffect(Actor a,string id){SelfCalls++;SelfCallback?.Invoke();}
        internal static void TryCastCombat(Actor a){}
    }
    internal static class MclslSpellProgression
    {
        internal static Dictionary<string,int> Levels=new();
        internal static int Level(Actor a,string id)=>Levels.GetValueOrDefault(a.Id+"|"+id,1);
        internal static float Strength(Actor a,string s)=>1;
        internal static void RecordCast(Actor a,MclslSpellDefinition s,int y){}
    }
    internal static class MclslSpellEffect
    {
        internal static bool Enabled=true;internal static int Reserved;
        internal static bool Show(Actor a,Actor b,string s,string phase=null,float duration=0,Color? tint=null)=>true;
        internal static bool TryReserveExternal(){Reserved++;return true;}internal static void ReleaseExternal()=>Reserved--;
    }
    internal static class MclslActorAnimationCatalog
    {
        internal sealed class Form {internal float Scale=1;}
        internal sealed class ClipData {internal string[] Names={"walk_0"};internal int FrameIndex(double t)=>0;}
        internal static Form Get(string p)=>new();internal static ClipData Clip(string p,string a)=>new();
    }
    internal static class MclslNativeActorFrames {internal static bool Available;internal static Sprite Request(string p)=>Available?new():null;}
}
namespace MySimulatedLongevityRoad.Systems.Combat
{
    internal static class MclslBattlePrimitives {internal static Color LawColor(string s)=>new();}
    internal static class MclslBattleVisuals
    {internal static bool VisibleFlag;internal static void Begin(Actor a,Actor b,string s,float t){}internal static void BeastRush(Actor a,Actor b,Sprite s,float f){}internal static bool Visible(Vector3 p)=>VisibleFlag;}
}
