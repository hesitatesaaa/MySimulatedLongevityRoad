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
    internal int Mana => Tier switch { 1 => 18, 2 => 30, 3 => 55, 4 => 90, 5 => 150, _ => throw new ArgumentOutOfRangeException(nameof(Tier)) };
    internal float Cooldown => Tier switch { 1 => 8f, 2 => 12f, 3 => 18f, 4 => 26f, 5 => 40f, _ => throw new ArgumentOutOfRangeException(nameof(Tier)) };
    internal float Preparation => Career == 4 ? 0.2f + Tier * 0.15f
        : Tier == 5 ? 0.8f : Tier == 3 ? 0.35f : 0f;
    internal float Power => Tier switch { 1 => 1.05f, 2 => 1.25f, 3 => 1.55f, 4 => 1.8f, 5 => 2.3f, _ => throw new ArgumentOutOfRangeException(nameof(Tier)) };
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
    internal const string StateKey = "mclsl.combat_career.v2";
    internal static readonly string[] Ranks = { "未入门", "入门", "熟练", "精通", "大师", "宗师" };
    internal static readonly int[] Experience = { 0, 200, 500, 900, 1400 };
    internal static readonly int[] TrialTargets = { 3, 5, 8, 12 };
    internal static readonly int[] MasteryPoints = { 40, 60, 80, 100, 120 };
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
        new[]{"契印击","灵息灌注","御灵破势","守契回生","灵契共鸣"}
    };
    private static readonly string[][] PassiveNames =
    {
        new[]{"剑息","破隙","剑势","御锋","剑心"}, new[]{"淬体","卸劲","战意","不屈","武魄"},
        new[]{"引灵","元素印","相生","流转","法心"}, new[]{"凝符","符印","叠符","藏符","符心"},
        new[]{"识阵","阵息","阵眼","守势","阵心"}, new[]{"灵息相连","护主契纹","协同猎印","回息契纹","同心契"}
    };
    private static readonly string[][] ActiveDescriptions =
    {
        new[]{"凝聚剑气刺击单体敌人。","剑气回折追击；已受剑击的敌人承受追加伤害。","蓄力贯穿最多四名敌人，消耗剑势增强威力。","飞剑分别锁定有限敌人，连续追击。","消耗全部剑势，蓄力释放强力单体终结剑。"},
        new[]{"近战重拳，对当前敌人造成物理伤害。","崩岳重击，打断敌方职业蓄力。","震击周围敌人并短暂减速，交战积累战意。","凝聚护脉罡，短时减伤并恢复少量生命。","消耗战意，蓄力重击当前敌人及近处敌军。"},
        new[]{"发射灵焰，伤敌并附加火印。","寒潮伤敌减速并附加水印。","引雷轰击，消耗已有元素印产生组合爆发。","五气成障，短时防护并回转少量灵力。","消耗元素印，释放多目标元素终结。"},
        new[]{"以灵力绘制符文攻击单体敌人。","封脉符造成伤害并短时限制敌人，附加符印。","消耗灵符，引爆范围内敌人的符印。","消耗护命灵符，护持自身和附近受伤友方。","消耗高阶灵符，符群连续攻击有限敌人。"},
        new[]{"布下短时缚足阵，阵内敌人减速。","布下聚灵阵，为阵内正在交战的友方回灵。","消耗阵盘，建立持续伤害区域。","消耗阵盘，建立保护自身与友方的阵域。","消耗高阶阵盘，建立同时伤敌与护持友方的领域。"},
        new[]{"主人对单体敌人造成0.65倍伤害，并施加4秒契印；契灵攻击契印目标时伤害提高15%。","灌注灵息，契灵基础攻击提高20%，持续6秒。","契灵距敌人8格内时追加1.10倍单体伤害并减速20%，持续2秒；不附带普通攻击。","恢复契灵25%最大生命；契灵距主人6格内时，主人减伤15%，持续6秒。契灵生命低于65%或主人生命低于75%且契灵在附近时自动触发。","灵契共鸣8秒：契灵伤害提高35%，主人伤害提高10%；不治疗，不延长存续。"}
    };
    private static readonly string[][] PassiveDescriptions =
    {
        new[]{"剑道主动技能伤害提高10%。","连续剑击附加短时破甲。","有效命中积累剑势，最多五层。","飞剑冷却缩短15%。","满剑势的终结剑额外增强25%。"},
        new[]{"获得额外10%有效减伤。","受到有效攻击后触发短时卸劲。","受到或造成有效伤害积累战意，最多五层。","生命低于30%时获得额外20%减伤。","有效卸伤后下一次武道技能增强25%。"},
        new[]{"术法主动技能灵力消耗降低10%。","火、水、雷命中分别留下元素印。","消耗两种以上元素印时组合伤害增强20%。","交替施法后恢复少量灵力。","完成三元素循环后下一次终结术增强30%。"},
        new[]{"基础符术灵力消耗降低10%。","有效符术命中附加符印，最多五层。","封脉符可额外积累一层符印。","护命触发时延长防护两秒。","消耗三层以上符印时符群增强25%。"},
        new[]{"自身阵内控制持续时间增加20%。","阵法持续时间增加20%。","阵域优先集中攻击当前主敌。","阵内有效防护触发短时额外护持。","攻防阵域共同作用时提高阵法收益20%。"},
        new[]{"契灵最大生命提高20%。","契灵存活且距主人6格内时，主人受到的伤害降低8%。","主人与契灵在3秒内命中同一目标时追加0.15倍伤害；共享6秒冷却，且该追加伤害不会触发自身。","契灵接受职业治疗提高20%，鹿的契约治疗同样受益。","灵契共鸣的契灵增伤由35%提升至50%，主人仍为10%。"}
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
}

internal sealed class MclslCombatCareerState
{
    public int Version = 2;
    public bool Potential;
    public int Career = -1, Rank, Experience, TrainingYears, LastYear = -1, LastAdmissionYear = -1;
    public int OpportunityYear = -1, OpportunityRate;
    public string Opportunity = "", Source = "";
    public int PreferredCareer = -1;
    public int TrialProgress;
    public int[] Mastery = {1,1,1,1,1}, SkillExperience = new int[5];
    public int BattleYear = -1, BattleExperience;
    public double[] Cooldowns = new double[5];
    public double CooldownClock;
    // 灵契师契约（v2 增量字段，旧档缺失时保留这里的默认值）：
    // ContractKind 0狼1龟2隼3鹿；-1 表示尚未契约（新玩家待选择）。
    // ContractSchema 1 表示已由新逻辑确定语义；旧档为 0，需一次性迁移。
    // NextSummonAt 为游戏秒绝对时刻（读档时按 CooldownClock 重定位）；0 表示可立即召唤。
    public int ContractKind = -1;
    public int ContractSchema;
    public double NextSummonAt;
    public string ContractPetName = "";
}
