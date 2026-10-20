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
    internal static int NaturalChance(int chance)=>(int)(Math.Clamp((long)chance,0,1000000)*40/100);
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
