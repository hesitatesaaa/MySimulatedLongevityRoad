using System;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslBeastRules
{
    internal static readonly System.Collections.Generic.HashSet<string> CommonSpecies=new(StringComparer.Ordinal)
        {"feiqiu","hongmao","changliao","goushou","manman","dafeng","mo","qingluan"};
    internal static readonly int[] Weights={256,128,64,32,16,4,1};
    // Fantasy GetSpawnCount: normal/elite bases 8/4, half-to-full range,
    // scale 1 + (level-1)*0.5. Use their midpoint plus one boss.
    internal static readonly int[] Counts={10,15,19,24,28,33,37};
    // Common species are judged once per twenty world years as two whole-map
    // pools: one land pool and one sea pool, land first then sea, each an
    // independent 40% (400000 per million) roll. Rare species every ten years
    // keep their existing multiplier.
    internal const int CommonPoolChancePerMillion = 400000;
    internal static readonly MclslBeastSurface[] CommonPoolOrder = { MclslBeastSurface.Land, MclslBeastSurface.Water };
    internal static bool PercentPass(int hash,int chancePerMillion)
        => chancePerMillion>0 && (uint)hash%1000000u<(uint)chancePerMillion;
    // One successful pool roll spawns 8..20 beasts, capped by the remaining
    // natural budget; roll is a uniform 0..12 from MclslBeastWorld.Roll.
    internal static int CommonPoolAmount(int roll,int remaining)
        => remaining<=0?0:Math.Min(remaining,8+Math.Clamp(roll,0,12));
    // Rare-species probabilities retain their existing multiplier.
    // Does not apply to waves, player placement or species-variant selection.
    internal static int NaturalChance(int chance)=>(int)(Math.Clamp((long)chance,0,1000000)*80/10000);
    // Natural cadence, independent of the per-judgment probability above:
    // common species are judged once per twenty world years, rare species once
    // per ten. Years that are not a judgment year are skipped without being
    // back-filled, and each judgment year is rolled at most once.
    internal static bool CommonNaturalYear(int year)=>year>0 && year%20==0;
    internal static bool RareNaturalYear(int year)=>year>0 && year%10==0;
    internal static bool ShouldPrepareNatural(int year,int lastNaturalYear,bool pendingEmpty)
        => pendingEmpty && year>0 && lastNaturalYear<year
            && (CommonNaturalYear(year)||RareNaturalYear(year));
    internal static int WeightedLevel(int maximum,int roll)
    {
        maximum=Math.Clamp(maximum,1,7);int total=0;
        for(int i=0;i<maximum;i++)total+=Weights[i];
        if(roll<0||roll>=total)throw new ArgumentOutOfRangeException(nameof(roll));
        for(int i=0;i<maximum;i++){if(roll<Weights[i])return i+1;roll-=Weights[i];}
        throw new InvalidOperationException("Invalid weighted level");
    }
    internal static int Role(int cursor,int count)
    {
        if(cursor<0||cursor>=count)throw new ArgumentOutOfRangeException(nameof(cursor));
        return cursor==0?2:cursor<=(int)Math.Ceiling((count-1)*0.25)?1:0;
    }
    internal static int PlannedCount(int level,float multiplier,int limit,int alive,int reserved,int worldTiles=65536,int waveLimit=64)
    {
        int available=Math.Max(0,limit-alive-reserved);
        float mapScale=Math.Clamp((float)Math.Sqrt(Math.Max(1,worldTiles)/65536f),0.5f,1.5f);
        return Math.Min(Math.Min(available,Math.Clamp(waveLimit,1,256)),Math.Max(1,(int)Math.Round(Counts[Math.Clamp(level,1,7)-1]*Math.Clamp(multiplier,0.1f,10f)*mapScale)));
    }
}
