using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslBeastConsumableDefinition
{
    internal readonly string Id,Kind,Effect;
    internal readonly int Grade;
    internal MclslBeastConsumableDefinition(string id,string kind,string effect,int grade)
    {Id=id;Kind=kind;Effect=effect;Grade=grade;}
}

internal static class MclslBeastConsumables
{
    internal static readonly MclslBeastConsumableDefinition[] All=Build();
    private static readonly Dictionary<string,MclslBeastConsumableDefinition> ById=All.ToDictionary(x=>x.Id,StringComparer.Ordinal);
    private static MclslBeastConsumableDefinition[] Build()
    {
        var items=new List<MclslBeastConsumableDefinition>();
        foreach(var row in MclslBeastContent.Rows)
        {items.Add(new(row.PillId,"pill",row.PillEffect,row.Grade));items.Add(new(row.TalismanId,"talisman",row.TalismanEffect,row.Grade));}
        for(int grade=1;grade<=4;grade++)
        {items.Add(new("YD_common_"+grade,"pill","heal",grade));items.Add(new("YF_common_"+grade,"talisman","guard",grade));}
        return items.ToArray();
    }
    // Consume and purchase use one effect catalog. Only actual offers are demand candidates.
    internal static string[] SelectMarketNeeds(IEnumerable<string> offers,
        Func<MclslBeastConsumableDefinition,bool> useful,Func<string,int> owned,Func<string,int> price)
    {
        var available=offers.Distinct(StringComparer.Ordinal).Where(ById.ContainsKey).Select(id=>ById[id]).Where(useful);
        var satisfied=All.Where(x=>owned(x.Id)>0&&useful(x)).Select(x=>(x.Kind,x.Effect)).ToHashSet();
        var desired=new List<string>();
        foreach(var group in available.GroupBy(x=>(x.Kind,x.Effect)).OrderBy(x=>x.Key.Kind,StringComparer.Ordinal).ThenBy(x=>x.Key.Effect,StringComparer.Ordinal))
        {
            // A usable substitute already in the bag satisfies this effect's need.
            if(satisfied.Contains(group.Key))continue;
            desired.Add(group.OrderByDescending(x=>x.Grade).ThenBy(x=>price(x.Id)).ThenBy(x=>x.Id,StringComparer.Ordinal).First().Id);
        }
        return desired.ToArray();
    }
}
