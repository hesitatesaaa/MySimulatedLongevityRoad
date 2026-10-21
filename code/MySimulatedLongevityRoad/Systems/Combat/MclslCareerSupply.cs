using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static class MclslCareerSupply
{
    internal static readonly MclslItemDefinition[] Items=Build();
    private static MclslItemDefinition[] Build()
    {
        var result=new List<MclslItemDefinition>();
        for(int tier=3;tier<=5;tier++)foreach(int career in new[]{3,4})
        {
            var skill=MclslCombatCareerCatalog.Skill(career,tier);
            result.Add(new MclslItemDefinition(skill.Item,
                MclslCombatCareerCatalog.Ranks[tier]+(career==3?"斗法灵符":"斗法阵盘"),"CareerSupply",tier-1,
                "职业高阶技能消耗；每次成功释放使用1份。制符师／炼器师可按品阶制作，每批6份。",
                career==3?"F01":"A01",tier==3?"A16":tier==4?"A17":"A18",160*(1<<(tier-3)),iconPath:skill.Icon));
        }
        return result.ToArray();
    }
    private static IEnumerable<MclslItemDefinition> Recipes(Actor actor)
    {
        string profession=MclslActorAccessor.GetString(actor,MclslActorDataKeys.Profession);
        int grade=MclslProfessionSystem.GetGrade(actor);
        return Items.Where(i=>i.Grade<=grade && (profession==MclslProfessionSystem.TalismanMaker&&i.Id.StartsWith("CP_Talisman_")
            || profession==MclslProfessionSystem.Refiner&&i.Id.StartsWith("CP_Array_")));
    }
    internal static void CraftAnnual(Actor actor,int year)
    {
        foreach(var recipe in Recipes(actor).OrderByDescending(i=>i.Grade))
        {
            var bag=MclslBagSystem.Read(actor);
            if(MclslBagSystem.Count(bag,recipe.Id)>=24||MclslBagSystem.Count(bag,recipe.IngredientA)<2||MclslBagSystem.Count(bag,recipe.IngredientB)<1)continue;
            MclslBagSystem.Remove(bag,recipe.IngredientA,2);MclslBagSystem.Remove(bag,recipe.IngredientB,1);MclslBagSystem.Write(actor,bag);
            MclslBagSystem.Add(actor,recipe.Id,6,year);break;
        }
    }
    internal static void AddReserves(Actor actor,Dictionary<string,int> reserves)
    {
        foreach(var recipe in Recipes(actor))
        {
            reserves.TryGetValue(recipe.IngredientA,out int a);reserves[recipe.IngredientA]=System.Math.Max(a,2);
            reserves.TryGetValue(recipe.IngredientB,out int b);reserves[recipe.IngredientB]=System.Math.Max(b,1);
        }
    }
    internal static int Reserve(Actor actor,MclslItemDefinition item)
    {
        if(!MclslCombatCareerSystem.Applicable(actor))return 0;
        var state=MclslCombatCareerSystem.Read(actor);
        return state.Career>=0 && Items.Contains(item) && item.Id==MclslCombatCareerCatalog.Skill(state.Career,item.Grade+1).Item
            && item.Grade+1<=state.Rank?12:0;
    }
    internal static void AddNeeds(Actor actor,MclslBagState bag,List<string> desired)
    {
        foreach(var item in Items)if(Reserve(actor,item)>MclslBagSystem.Count(bag,item.Id))desired.Add(item.Id);
    }
    internal static void GiveOpportunitySupply(Actor actor,int year)
    {
        if(!MclslCombatCareerSystem.Applicable(actor))return;
        var state=MclslCombatCareerSystem.Read(actor);
        if(state.Career is 3 or 4 &&state.Rank>=3)
            MclslBagSystem.Add(actor,MclslCombatCareerCatalog.Skill(state.Career,state.Rank).Item,2,year);
    }
}
