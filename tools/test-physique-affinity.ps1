$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$physique = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Traits/MclslPhysiqueSystem.cs') -Raw -Encoding UTF8
$career = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Combat/MclslCombatCareerSystem.cs') -Raw -Encoding UTF8
function Slice($text, $start, $end) {
    $a = $text.IndexOf($start, [StringComparison]::Ordinal)
    $b = $text.IndexOf($end, $a, [StringComparison]::Ordinal)
    if ($a -lt 0 -or $b -lt 0) { throw "Missing source boundary: $start / $end" }
    $text.Substring($a, $b - $a)
}
$definitions = Slice $physique '    private sealed class Physique' '    private static readonly Dictionary<string, Physique> ById'
$affinities = Slice $physique '    private readonly struct CareerAffinity' '    internal static string CareerAffinityRole'
$preferred = Slice $career '    internal static int Preferred(Actor actor)' '    internal static void Offer('
$legacyPreferred = $preferred.Replace('Preferred(Actor actor)', 'LegacyPreferred(Actor actor)').Replace('        uint physiqueMask=MclslPhysiqueSystem.ReadPhysiqueMask(actor);', '').Replace('CareerAffinityScore(physiqueMask,c)', 'CareerAffinityScore(actor,c)')
$roll = Slice $career '    internal static int Roll(' '    internal static int Rank('
$source = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
public sealed class Actor {
    public object data = new object();
    public long Id;
    public string Roots = "", Technique = "";
    public uint Mask;
    public long Calls;
    public bool hasTrait(string id) { Calls++; return (Mask & (1u << MclslPhysiqueSystem.Index(id))) != 0; }
}
static class MclslActorDataKeys { public const string SpiritualRootAttributes="roots", TechniqueName="technique"; }
static class MclslActorAccessor {
    public static long Id(Actor actor) { return actor.Id; }
    public static string GetString(Actor actor,string key) { return key=="roots"?actor.Roots:actor.Technique; }
}
static class MclslPhysiqueSystem {
$definitions
    private static readonly Dictionary<string,Physique> ById = BuildIndex();
    private static Dictionary<string,Physique> BuildIndex() {
        var result=new Dictionary<string,Physique>(); foreach(var p in Definitions)result.Add(p.Id,p);return result;
    }
    public static int Index(string id) { for(int i=0;i<Definitions.Length;i++)if(Definitions[i].Id==id)return i;throw new Exception(id); }
    public static int Count { get { return Definitions.Length; } }
$affinities
}
static class Career {
$roll
$preferred
$legacyPreferred
}
public static class PhysiqueRegression {
    static void Check(bool ok,string message) { if(!ok)throw new Exception(message); }
    public static string Run() {
        int n=MclslPhysiqueSystem.Count;
        Check(n==17,"Update exhaustive test for changed physique catalog");
        var actor=new Actor();
        Check(MclslPhysiqueSystem.ReadPhysiqueMask(null)==0,"null actor");
        actor.data=null;Check(MclslPhysiqueSystem.ReadPhysiqueMask(actor)==0,"null data");actor.data=new object();
        for(uint mask=0;mask<(1u<<n);mask++) {
            actor.Mask=mask; actor.Calls=0;
            uint snapshot=MclslPhysiqueSystem.ReadPhysiqueMask(actor);
            Check(snapshot==mask && actor.Calls==n,"snapshot / scan count");
            for(int c=0;c<6;c++) {
                long before=actor.Calls;
                int score=MclslPhysiqueSystem.CareerAffinityScore(snapshot,c);
                Check(actor.Calls==before,"snapshot score reads actor");
                Check(score==MclslPhysiqueSystem.CareerAffinityScore(actor,c),"score mismatch "+mask+" / "+c);
            }
        }
        string[] hints={"","剑锋斩金","体拳山土","火水雷五行","符印篆","阵盘域","灵兽契木","剑火阵木"};
        int ties=0;
        for(int i=0;i<2048;i++) {
            actor.Id=i;actor.Mask=(uint)(i*7919)&((1u<<n)-1);actor.Roots=hints[i%hints.Length];actor.Technique=hints[(i/8)%hints.Length];
            actor.Calls=0;int actual=Career.Preferred(actor);Check(actor.Calls==n,"Preferred scan count");
            Check(actual==Career.LegacyPreferred(actor),"Preferred mismatch");
            int best=-1,first=-1,winners=0;
            string[] careerHints={"剑锋斩金","体拳山土","火水雷五行","符印篆","阵盘域","灵兽契木"};
            for(int c=0;c<6;c++) {
                int value=Career.Roll(actor.Id,0,0x2840+c)%100;
                foreach(char ch in careerHints[c]){if(actor.Roots.Contains(ch))value+=10;if(actor.Technique.Contains(ch))value+=20;}
                value+=MclslPhysiqueSystem.CareerAffinityScore(actor,c)/2;
                if(value>best){best=value;first=c;winners=1;}else if(value==best)winners++;
            }
            Check(actual==first,"first maximum must win");if(winners>1)ties++;
        }
        Check(ties>0,"tie scenarios must be exercised");
        actor.Mask=0;Check(MclslPhysiqueSystem.ReadPhysiqueMask(actor)==0,"remove traits");
        actor.Mask=1;Check(MclslPhysiqueSystem.ReadPhysiqueMask(actor)==1,"grant trait");
        Check(MclslPhysiqueSystem.ReadPhysiqueMask(new Actor())==0,"actor isolation");
        // Both paths run in the same process, alternate order after warming up.
        for(int i=0;i<1000;i++){Career.Preferred(actor);Career.LegacyPreferred(actor);}
        var oldTimes=new double[7];var newTimes=new double[7];long oldCalls=0,newCalls=0;
        for(int round=0;round<7;round++)for(int order=0;order<2;order++) {
            bool optimized=((round+order)&1)==0;actor.Calls=0;var timer=Stopwatch.StartNew();
            for(int i=0;i<10000;i++){actor.Mask=(uint)(i*7919)&((1u<<n)-1);if(optimized)Career.Preferred(actor);else Career.LegacyPreferred(actor);}
            timer.Stop();if(optimized){newTimes[round]=timer.Elapsed.TotalMilliseconds;newCalls=actor.Calls;}else{oldTimes[round]=timer.Elapsed.TotalMilliseconds;oldCalls=actor.Calls;}
        }
        Array.Sort(oldTimes);Array.Sort(newTimes);
        return "PASS: 131072 combinations x 6 careers; 2048 Preferred cases (ties="+ties+"); null/change/isolation; median ms per 10000 old="+oldTimes[3].ToString("F2")+" new="+newTimes[3].ToString("F2")+" trait checks old="+oldCalls+" new="+newCalls;
    }
}
"@
Add-Type -TypeDefinition $source
[PhysiqueRegression]::Run()
