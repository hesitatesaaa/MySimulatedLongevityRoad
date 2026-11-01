using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.UI;

public sealed class ActorAsset {
    public string name_locale, Name;
    public bool Throws;
    public string getLocalizedName() => Throws ? throw new Exception("native") : Name;
}
public sealed class ActorTrait { public string id; }
namespace NeoModLoader.General {
    public static class LM {
        public static Dictionary<string,string> Values = new();
        public static HashSet<string> Throws = new();
        public static string Get(string key) => Throws.Contains(key) ? throw new Exception("locale") : Values.GetValueOrDefault(key, key);
    }
}
namespace MySimulatedLongevityRoad.Core {
    public static class MclslDiagnostics {
        public static int Errors;
        public static void Error(string key, string value) { Errors++; }
        public static void Fatal(string key, string value) { Errors++; }
        public static void Probe(string key, string value) { }
        public static void Log(string key, string value) { }
        public static void Warn(string key, string value) { }
        public static void Alert(string key, string value) { }
        public static void Notify(string key, string value) { }
    }
}
namespace MySimulatedLongevityRoad.Systems {
    public static class MclslLocalizationBridge {
        public static bool IsRuntimeKey(string key) => key.StartsWith("runtime:");
        public static bool TryResolveRuntimeKey(string key, out string text) { text=key=="runtime:known"?"运行时名称":null; return text!=null; }
    }
}
public sealed class Numbers {
    public int Count;
    public float Health;
    public double Unsupported=4;
    public int Property { get; set; }
    public int ReadOnly => 3;
    public float Bad { set { throw new InvalidOperationException("setter"); } }
}
public static class Program {
    static int Checks;
    static void Check(bool ok, string name) { if(!ok) throw new Exception(name); Checks++; }
    public static void Main() {
        Check(MclslStableHash.Positive79x43(null)==79,"stable Positive79x43 vector");
        Check(MclslStableHash.Positive79x43("")==79,"stable Positive79x43 vector");
        Check(MclslStableHash.Positive79x43("abc")==6464719,"stable Positive79x43 vector");
        Check(MclslStableHash.Positive79x43("合道功法|987654321")==1308108517,"stable Positive79x43 vector");
        Check(MclslStableHash.Positive79x43(new string('x',300))==1248520063,"stable Positive79x43 vector");
        Check(MclslStableHash.Positive23x37(null)==23,"stable Positive23x37 vector");
        Check(MclslStableHash.Positive23x37("")==23,"stable Positive23x37 vector");
        Check(MclslStableHash.Positive23x37("abc")==1301537,"stable Positive23x37 vector");
        Check(MclslStableHash.Positive23x37("合道功法|987654321")==1232906067,"stable Positive23x37 vector");
        Check(MclslStableHash.Positive23x37(new string('x',300))==2089051815,"stable Positive23x37 vector");
        Check(MclslStableHash.Positive83x47(null)==83,"stable Positive83x47 vector");
        Check(MclslStableHash.Positive83x47("")==83,"stable Positive83x47 vector");
        Check(MclslStableHash.Positive83x47("abc")==8836287,"stable Positive83x47 vector");
        Check(MclslStableHash.Positive83x47("合道功法|987654321")==1083846845,"stable Positive83x47 vector");
        Check(MclslStableHash.Positive83x47(new string('x',300))==2084276371,"stable Positive83x47 vector");
        var n=new Numbers();
        MclslReflectionNumbers.TrySetNumber(n,"count",12.9f,"test");
        MclslReflectionNumbers.TrySetNumber(n,"Health",3.75f,"test");
        MclslReflectionNumbers.TrySetNumber(n,"Property",9.8f,"test");
        Check(n.Count==12 && n.Health==3.75f && n.Property==9,"case-insensitive fields, float and truncated int property");
        MclslReflectionNumbers.TrySetNumber(n,"Unsupported",9,"test");
        MclslReflectionNumbers.TrySetNumber(n,"ReadOnly",9,"test");
        MclslReflectionNumbers.TrySetNumber(n,"Missing",9,"test");
        MclslReflectionNumbers.TrySetNumber(null,"Count",9,"test");
        Check(n.Unsupported==4 && n.ReadOnly==3,"unsupported, readonly, missing and null targets");
        MclslReflectionNumbers.TrySetNumber(n,"Bad",9,"test");
        Check(MclslDiagnostics.Errors==1,"throwing setter is diagnosed without escaping");
        // M2-C: accessor cached per (Type, member name) — repeated writes resolve only once.
        MclslReflectionNumbers.ResetCacheCounters();
        MclslReflectionNumbers.TrySetNumber(n,"Health",4.5f,"test");
        int factoryAfterFirst = MclslReflectionNumbers.FactoryCalls;
        MclslReflectionNumbers.TrySetNumber(n,"Health",5.5f,"test");
        Check(factoryAfterFirst==1 && MclslReflectionNumbers.FactoryCalls==1 && MclslReflectionNumbers.CacheCount==1,"reflection accessor cached (single resolve per type+name)");
        Check(n.Health==5.5f,"cached accessor still writes");
        MclslReflectionNumbers.TrySetNumber(n,"Missing",9,"test");
        Check(MclslReflectionNumbers.CacheCount==2,"missing member cached too");
        Check(MclslRankTextFormat.AssetDisplayName(null)=="未知生物","null asset fallback");
        var a=new ActorAsset { Name="原生名", name_locale="asset" };
        NeoModLoader.General.LM.Values["asset"]="翻译名";
        Check(MclslRankTextFormat.AssetDisplayName(a)=="原生名","native asset name has priority");
        a.Throws=true;
        Check(MclslRankTextFormat.AssetDisplayName(a)=="翻译名","native failure falls through to locale");
        NeoModLoader.General.LM.Throws.Add("asset");
        Check(MclslRankTextFormat.AssetDisplayName(a)=="未知生物","both lookups fail safely");
        Check(MclslRankTextFormat.TraitDisplayName(null)=="未知特质","null trait fallback");
        Check(MclslRankTextFormat.TraitDisplayName(new ActorTrait {id="runtime:known"})=="运行时名称","runtime translation priority");
        var t=new ActorTrait {id="gift"};
        NeoModLoader.General.LM.Values["trait_gift"]="特质名";
        NeoModLoader.General.LM.Values["gift"]="直接名";
        Check(MclslRankTextFormat.TraitDisplayName(t)=="特质名","prefixed locale priority");
        NeoModLoader.General.LM.Values["trait_gift"]="trait_gift";
        Check(MclslRankTextFormat.TraitDisplayName(t)=="直接名","unresolved prefixed key falls through");
        NeoModLoader.General.LM.Values["gift"]="runtime:unresolved";
        Check(MclslRankTextFormat.TraitDisplayName(t)=="未知特征","unresolved runtime key uses final fallback");

        Check(MclslStableHash.Positive43x53(null)==43,"null hash");
        Check(MclslStableHash.Positive43x53("")==43,"stable hash vector");
        Check(MclslStableHash.Positive43x53("abc")==6679477,"stable hash vector");
        Check(MclslStableHash.Positive43x53("world|x|95")==2095114913,"stable hash vector");
        Check(MclslStableHash.Positive43x53("overflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflowoverflow")==801105547,"stable hash vector");
        Check(MclslRankTextFormat.FormatPower(0)=="凡人","zero power");
        Check(MclslRankTextFormat.FormatPower(9999)==9999d.ToString("F0"),"power below unit boundary");
        Check(MclslRankTextFormat.FormatPower(10000)==1d.ToString("F2")+"万","ten-thousand boundary");
        Check(MclslRankTextFormat.FormatPower(100000000)==1d.ToString("F2")+"亿","hundred-million boundary");
        Check(MclslRankTextFormat.FormatPower(1e12)==1d.ToString("F2")+"万亿","trillion boundary");
        for(int i=0;i<10000;i++) {
            MclslStableHash.Positive43x53("warm");MclslStableHash.Positive79x43("warm");
            MclslStableHash.Positive23x37("warm");MclslStableHash.Positive83x47("warm");
        }
        long allocationBefore=GC.GetAllocatedBytesForCurrentThread();
        int hashSum=0;for(int i=0;i<100000;i++) {
            hashSum^=MclslStableHash.Positive43x53("stable");hashSum^=MclslStableHash.Positive79x43("stable");
            hashSum^=MclslStableHash.Positive23x37("stable");hashSum^=MclslStableHash.Positive83x47("stable");
        }
        long allocated=GC.GetAllocatedBytesForCurrentThread()-allocationBefore;
        Check(allocated==0,"shared hash allocation zero");GC.KeepAlive(hashSum);
        Console.WriteLine("PASS shared numeric and localization behavior: "+Checks+" checks");
    }
}
