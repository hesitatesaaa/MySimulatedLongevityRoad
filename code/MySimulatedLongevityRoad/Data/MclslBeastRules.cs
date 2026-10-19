using System;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslBeastRules
{
    internal static readonly System.Collections.Generic.HashSet<string> CommonSpecies=new(StringComparer.Ordinal)
        {"feiqiu","hongmao","changliao","goushou","manman","dafeng","mo","qingluan"};
    internal static readonly int[] Weights={256,128,64,32,16,4,1};
    internal static readonly int[] Counts={24,36,48,64,80,96,112};
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
    internal static int PlannedCount(int level,float multiplier,int limit,int alive,int reserved)
    {
        int available=Math.Max(0,limit-alive-reserved);
        return Math.Min(available,Math.Max(1,(int)(Counts[Math.Clamp(level,1,7)-1]*Math.Clamp(multiplier,0.1f,10f))));
    }
}
