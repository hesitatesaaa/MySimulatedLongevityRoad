$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$pet=Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Combat/MclslCareerCombat.cs') -Raw -Encoding UTF8
$boat=Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Patches/MclslXuanhuangDirectionPatch.cs') -Raw -Encoding UTF8
$petMethod=[regex]::Match($pet,'(?s)    private static Sprite ContractPetSprite\(Actor pet\).*?\n    }').Value
$boatMethod=[regex]::Match($boat,'(?s)    private static void KeepBoatScale\(Actor __instance\).*?\n    }').Value
$state=Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslActorAnimationState.cs') -Raw -Encoding UTF8
$state=$state.Replace('namespace MySimulatedLongevityRoad.Systems;','')
$stubs=@'
class Sprite {public int Id;public Sprite(int id){Id=id;}}
class Clip {public Sprite[] frames;public Clip(int n,int start){frames=new Sprite[n];for(int i=0;i<n;i++)frames[i]=new Sprite(start+i);}}
class Animations {public Clip walking=new Clip(8,10),idle=new Clip(4,0);}
class Stats {public float Scale;public float get(string key)=>Scale;public float this[string key]{get=>Scale;set=>Scale=value;}}
class Asset {public bool is_boat;public string id="pet";public Stats base_stats=new Stats();public float animation_walk_speed=.125f,animation_idle_speed=.25f;}
class Actor {public Asset asset=new Asset();public Stats stats=new Stats();public float target_scale,current;public bool is_moving,alive=true;public Animations animation_container=new Animations();public bool isAlive()=>alive;public void checkAnimationContainer(){}public void setActorScale(float scale){current=scale;}}
static class MclslRuntime {public static double DisplayClock;}
static class MclslXuanhuangRegistration {public const string Id="mclsl_xuanhuang";}
public class PetBoatRegression {
 class PetAnimation {internal readonly MclslActorAnimationState State=new MclslActorAnimationState();}
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Actor,PetAnimation> PetAnimations=new System.Runtime.CompilerServices.ConditionalWeakTable<Actor,PetAnimation>();
 static void Check(bool b,string s){if(!b)throw new Exception(s);}
 public static string Run(){
 var pet=new Actor();MclslRuntime.DisplayClock=0;Check(ContractPetSprite(pet).Id==0,"idle first");
 MclslRuntime.DisplayClock=.5;Check(ContractPetSprite(pet).Id==2,"idle advances");Check(ContractPetSprite(pet).Id==2,"paused clock freezes");
 pet.is_moving=true;Check(ContractPetSprite(pet).Id==10,"move starts from first");MclslRuntime.DisplayClock=.75;Check(ContractPetSprite(pet).Id==12,"walk advances");
 MclslRuntime.DisplayClock=1.5;Check(ContractPetSprite(pet).Id==10,"walk loops at eight");pet.is_moving=false;Check(ContractPetSprite(pet).Id==0,"stop resets to idle");
 MclslRuntime.DisplayClock=0;Check(ContractPetSprite(pet).Id==0,"world clock reset");
 foreach(string id in new[]{"boat_fishing","boat_transport_human","boat_trading_human"}){var b=new Actor();b.asset.id="mclsl_xuanhuang_"+id;b.asset.is_boat=true;b.asset.base_stats.Scale=.2125f;b.stats.Scale=4;b.target_scale=4;b.current=4;KeepBoatScale(b);Check(b.current==.2125f&&b.target_scale==.2125f&&b.stats.Scale==.2125f,"fixed boat size");KeepBoatScale(b);Check(b.current==.2125f,"no cumulative shrinking");}
 var vanilla=new Actor();vanilla.asset.id="boat_fishing";vanilla.asset.is_boat=true;vanilla.current=3;KeepBoatScale(vanilla);Check(vanilla.current==3,"native boat unaffected");
 return "PASS: idle/walk transitions, 4/8 frame progression, pause/reset, three boat types, repeat, native exclusion";
 }
'@
Add-Type -TypeDefinition ($state+"`n"+$stubs+"`n"+$petMethod+"`n"+$boatMethod+"`n}")
[PetBoatRegression]::Run()
