$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$s=Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs') -Raw -Encoding UTF8
$a=$s.Substring($s.IndexOf('    private static void AutoClaimLegacy()'))
$a=$a.Substring(0,$a.IndexOf('    internal static int LegacySelectionLimit()'))
$test=@'
using System;
using System.Linq;
using System.Collections.Generic;
enum TraitType {Positive,Negative,Other}
class ActorTrait:BaseTrait<ActorTrait> {public TraitType type;}
class BaseTrait<T>{public int rarity;}
class Library {public ActorTrait get(string id)=>new ActorTrait{type=id=="bad"?TraitType.Negative:TraitType.Positive,rarity=0};}
static class AssetManager {public static Library traits=new Library();}
class Legacy {public List<string> ClaimedChoices=new List<string>();public string Id="latest";}
class State {public List<Legacy> Legacies=new List<Legacy>();}
class Option {public string Id;public Option(string id){Id=id;}}
public class LegacyRegression {
 static State _state=new State();static bool _writeLocked;static bool host=true;
 static List<Option> options=new List<Option>();static HashSet<string> unavailable=new HashSet<string>();
 static object FindLivingHost()=>host?new object():null;
 static IEnumerable<Option> GetLegacyOptions(Legacy l)=>options;
 static bool TryClaimLegacyOption(string id,string option,out string message){message="";if(unavailable.Contains(option))return false;_state.Legacies[0].ClaimedChoices.Add(option);return true;}
 static void Check(bool value){if(!value)throw new Exception("legacy regression failed");}
 static void Setup(params string[] ids){_state.Legacies.Clear();_state.Legacies.Add(new Legacy());options=ids.Select(x=>new Option(x)).ToList();unavailable.Clear();host=true;_writeLocked=false;}
 public static string Run(){
 Setup("spirit_stones","world_soul","inverse_truth","technique","cultivation");AutoClaimLegacy();Check(string.Join(",",_state.Legacies[0].ClaimedChoices)=="cultivation,technique,inverse_truth");AutoClaimLegacy();Check(_state.Legacies[0].ClaimedChoices.Count==3);
 Setup("world_soul","cultivation","technique","golden_core");unavailable.Add("world_soul");AutoClaimLegacy();Check(string.Join(",",_state.Legacies[0].ClaimedChoices)=="cultivation,technique,golden_core");
 Setup("spirit_stones");AutoClaimLegacy();Check(_state.Legacies[0].ClaimedChoices.Count==1);
 Setup("cultivation","technique","inverse_truth");_state.Legacies[0].ClaimedChoices.Add("technique");AutoClaimLegacy();Check(_state.Legacies[0].ClaimedChoices.Count==3);
 Setup("trait:bad","trait:good");AutoClaimLegacy();Check(string.Join(",",_state.Legacies[0].ClaimedChoices)=="trait:good");
 Setup("cultivation");_writeLocked=true;AutoClaimLegacy();Check(_state.Legacies[0].ClaimedChoices.Count==0);_writeLocked=false;host=false;AutoClaimLegacy();Check(_state.Legacies[0].ClaimedChoices.Count==0);
 return "PASS: priority, prerequisites, fewer options, claimed slots, repeat, negative traits, write lock, absent host";
 }
'@
Add-Type -TypeDefinition ($test+"`n"+$a+"`n}")
[LegacyRegression]::Run()
if($s -notmatch 'MaxLegacies = 1' -or $s -notmatch 'return 3;' -or $s -notmatch 'actor.isSapient\(\)' -or $s -notmatch '!MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.IsBeast\(actor\)'){throw 'Latest legacy / host eligibility wiring missing'}
'PASS: latest-only normalization, fixed limit, sapience and beast exclusion wiring'
