using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslCombatCareerDefinition
{
    internal readonly int Index;
    internal readonly string Id, Name, Motif;
    internal string Trait => "MclslCombatCareer_" + Id;
    internal string Icon => "trait/" + Trait;
    internal MclslCombatCareerDefinition(int index, string id, string name, string motif)
    { Index = index; Id = id; Name = name; Motif = motif; }
}

internal sealed class MclslCareerSkill
{
    internal readonly int Career, Tier;
    internal readonly bool Passive;
    internal readonly string Name, Description;
    internal string Id => "CP_" + MclslCombatCareerCatalog.Careers[Career].Id + "_" + Tier + (Passive ? "P" : "A");
    internal string Trait => "MclslCombatPassive_" + MclslCombatCareerCatalog.Careers[Career].Id + "_" + Tier;
    internal string Icon => Passive ? "trait/" + Trait : "ui/CareerSkills/" + Id;
    internal int Mana => new[] { 18, 30, 55, 90, 150 }[Tier - 1];
    internal float Cooldown => new[] { 8f, 12f, 18f, 26f, 40f }[Tier - 1];
    internal float Preparation => Career == 4 ? 0.2f + Tier * 0.15f
        : Tier == 5 ? 0.8f : Tier == 3 ? 0.35f : 0f;
    internal float Power => new[] { 1.05f, 1.25f, 1.55f, 1.8f, 2.3f }[Tier - 1];
    internal int Targets => Career == 0 && Tier == 5 ? 1
        : Career == 4 ? 2 + Tier : Tier >= 3 && Career != 1 ? Tier + 1 : 1;
    internal string Item => Career == 3 && Tier >= 3 ? "CP_Talisman_" + Tier
        : Career == 4 && Tier >= 3 ? "CP_Array_" + Tier : string.Empty;
    internal string Law => Career switch { 0 => "金", 1 => "physical", 2 => Tier switch { 1 => "火", 2 => "水", 3 => "雷", _ => "空间" }, 3 => "阳", 4 => "土", _ => "木" };
    internal MclslCareerSkill(int career, int tier, bool passive, string name, string description)
    { Career = career; Tier = tier; Passive = passive; Name = name; Description = description; }
}

internal static class MclslCombatCareerCatalog
{
    internal const string StateKey = "mclsl.combat_career.v1";
    internal static readonly string[] Ranks = { "未入门", "入门", "熟练", "精通", "大师", "宗师" };
    internal static readonly int[] Experience = { 0, 200, 500, 900, 1400 };
    internal static readonly int[] Trials = { 10, 25, 50, 80 };
    internal static readonly MclslCombatCareerDefinition[] Careers =
    {
        new(0,"sword","剑修","灵剑与剑气"), new(1,"martial","武修","铁拳与山岳"),
        new(2,"elemental","术修","五行灵焰"), new(3,"sigil","符修","符纸与符印"),
        new(4,"array","阵修","阵盘与阵眼"), new(5,"spirit","灵契师","灵兽与契纹")
    };
    private static readonly string[][] ActiveNames =
    {
        new[]{"凝锋刺","回锋斩","贯虹剑","分光剑","天外一剑"},
        new[]{"震山拳","崩岳击","撼地踏","护脉罡","镇岳破"},
        new[]{"灵焰矢","寒潮缚","雷火引","五气障","归元劫"},
        new[]{"灵符击","封脉符","连爆符","护命符","万符诏"},
        new[]{"缚足阵","聚灵阵","杀伐阵","护山阵","乾坤阵"},
        new[]{"唤灵狼","唤岩龟","唤雷隼","唤灵鹿","灵契共鸣"}
    };
    private static readonly string[][] PassiveNames =
    {
        new[]{"剑息","破隙","剑势","御锋","剑心"}, new[]{"淬体","卸劲","战意","不屈","武魄"},
        new[]{"引灵","元素印","相生","流转","法心"}, new[]{"凝符","符印","叠符","藏符","符心"},
        new[]{"识阵","阵息","阵眼","守势","阵心"}, new[]{"灵息相连","护主契纹","协同猎印","回息契纹","三灵同心"}
    };
    private static readonly string[][] ActiveDescriptions =
    {
        new[]{"凝聚剑气刺击单体敌人。","剑气回折追击；已受剑击的敌人承受追加伤害。","蓄力贯穿最多四名敌人，消耗剑势增强威力。","飞剑分别锁定有限敌人，连续追击。","消耗全部剑势，蓄力释放强力单体终结剑。"},
        new[]{"近战重拳，对当前敌人造成物理伤害。","崩岳重击，打断敌方职业蓄力。","震击周围敌人并短暂减速，交战积累战意。","凝聚护脉罡，短时减伤并恢复少量生命。","消耗战意，蓄力重击当前敌人及近处敌军。"},
        new[]{"发射灵焰，伤敌并附加火印。","寒潮伤敌减速并附加水印。","引雷轰击，消耗已有元素印产生组合爆发。","五气成障，短时防护并回转少量灵力。","消耗元素印，释放多目标元素终结。"},
        new[]{"以灵力绘制符文攻击单体敌人。","封脉符造成伤害并短时限制敌人，附加符印。","消耗灵符，引爆范围内敌人的符印。","消耗护命灵符，护持自身和附近受伤友方。","消耗高阶灵符，符群连续攻击有限敌人。"},
        new[]{"布下短时缚足阵，阵内敌人减速。","布下聚灵阵，为阵内正在交战的友方回灵。","消耗阵盘，建立持续伤害区域。","消耗阵盘，建立保护自身与友方的阵域。","消耗高阶阵盘，建立同时伤敌与护持友方的领域。"},
        new[]{"召唤短时近战灵狼协助攻击。","召唤岩龟护主，降低主人受到的伤害。","召唤远程雷隼，与主人合击同一敌人。","召唤灵鹿，为主人及交战友方提供治疗。","强化现存灵兽，延长剩余时间并提高协作收益。"}
    };
    private static readonly string[][] PassiveDescriptions =
    {
        new[]{"剑道主动技能伤害提高10%。","连续剑击附加短时破甲。","有效命中积累剑势，最多五层。","飞剑冷却缩短15%。","满剑势的终结剑额外增强25%。"},
        new[]{"获得额外10%有效减伤。","受到有效攻击后触发短时卸劲。","受到或造成有效伤害积累战意，最多五层。","生命低于30%时获得额外20%减伤。","有效卸伤后下一次武道技能增强25%。"},
        new[]{"术法主动技能灵力消耗降低10%。","火、水、雷命中分别留下元素印。","消耗两种以上元素印时组合伤害增强20%。","交替施法后恢复少量灵力。","完成三元素循环后下一次终结术增强30%。"},
        new[]{"基础符术灵力消耗降低10%。","有效符术命中附加符印，最多五层。","封脉符可额外积累一层符印。","护命触发时延长防护两秒。","消耗三层以上符印时符群增强25%。"},
        new[]{"自身阵内控制持续时间增加20%。","阵法持续时间增加20%。","阵域优先集中攻击当前主敌。","阵内有效防护触发短时额外护持。","攻防阵域共同作用时提高阵法收益20%。"},
        new[]{"召唤灵兽持续时间增加20%。","岩龟护主触发时给予短时额外防护。","主人与灵兽命中同一敌人时触发协同追击。","灵鹿有效治疗提高20%。","三类灵兽同时存在时，灵契共鸣额外增强25%。"}
    };
    internal static readonly MclslCareerSkill[] Skills = Build();
    private static MclslCareerSkill[] Build()
    {
        var result = new List<MclslCareerSkill>(60);
        for (int c = 0; c < 6; c++) for (int tier = 1; tier <= 5; tier++)
        {
            result.Add(new(c,tier,false,ActiveNames[c][tier-1],ActiveDescriptions[c][tier-1]));
            result.Add(new(c,tier,true,PassiveNames[c][tier-1],PassiveDescriptions[c][tier-1]));
        }
        return result.ToArray();
    }
    internal static MclslCareerSkill Skill(int career,int tier,bool passive=false)
        => Skills[career*10+(tier-1)*2+(passive?1:0)];
    internal static string TrialDescription(int career, int promotion) => (career,promotion) switch
    {
        (0,0)=>"有效剑击",(0,1)=>"连续剑击与破隙",(0,2)=>"剑势与贯虹命中",(0,3)=>"飞剑协同",
        (1,0)=>"近战重击",(1,1)=>"卸伤与打断",(1,2)=>"战意爆发",(1,3)=>"低生命防护与反击",
        (2,0)=>"元素命中",(2,1)=>"双元素连携",(2,2)=>"元素组合爆发",(2,3)=>"防护与元素循环",
        (3,0)=>"有效符击",(3,1)=>"封脉与叠印",(3,2)=>"符印引爆",(3,3)=>"护命救险与符术协同",
        (4,0)=>"阵内控敌",(4,1)=>"聚灵支援",(4,2)=>"阵内合击",(4,3)=>"护阵救险",
        (5,0)=>"灵狼参战",(5,1)=>"岩龟护主",(5,2)=>"灵兽合击",_=>"灵鹿有效支援"
    };
}

internal sealed class MclslCombatCareerState
{
    public int Version = 1;
    public bool Potential;
    public int Career = -1, Rank, Experience, TrainingYears, LastYear = -1, LastAdmissionYear = -1;
    public int OpportunityYear = -1, OpportunityRate;
    public string Opportunity = "", Source = "";
    public int PreferredCareer = -1;
    public int[] Trials = new int[4], Mastery = {1,1,1,1,1}, SkillExperience = new int[5];
    public int BattleYear = -1, BattleExperience;
    public double[] Cooldowns = new double[5];
    public double CooldownClock;
}
