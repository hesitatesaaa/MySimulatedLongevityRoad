using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslBeastTalentDefinition
{
    internal readonly string Species, Name, Description, Motif;
    internal readonly float Power, Cooldown, Radius;
    internal readonly int Mana;
    internal string Id => "BT_"+Species;
    internal string Icon => "ui/BeastTalents/"+Id;
    internal string Law=>Species switch {
        "kunpeng" or "dafeng" or "manman" or "feiqiu"=>"风",
        "qinglong"=>"水", "huanglong" or "xuanwu" or "tianshou"=>"土",
        "zhuque" or "fenghuang"=>"火", "baihu"=>"金", "qilin" or "qingluan"=>"木",
        "moqilin" or "mo" or "yingying" or "goushou"=>"阴", "liejiejing"=>"空间", _=>"physical"};
    internal MclslBeastTalentDefinition(string species,string name,string description,string motif,
        float power=1.3f,float cooldown=12,int mana=24,float radius=7)
    {Species=species;Name=name;Description=description;Motif=motif;Power=power;Cooldown=cooldown;Mana=mana;Radius=radius;}
}

internal static class MclslBeastTalentCatalog
{
    internal static readonly MclslBeastTalentDefinition[] All =
    {
        new("kunpeng","扶摇垂天","前方扇形风压攻击最多6敌，离自身越近推力越强。","巨翼、弧形风墙",1.4f,14,44,12),
        new("qinglong","苍龙御澜","水流依次攻击最多3敌，后续伤害为前一击的75%。","龙首水珠、盘龙水流",1.5f,12,34,9),
        new("huanglong","地脉龙震","目标脚下蓄势0.7秒后地震；内圈高伤，外圈减速35%，持续3秒。","地脉亮纹、破土龙影",1.6f,14,34,7),
        new("zhuque","离火焚邪","火羽直线穿透最多6敌，命中后4秒持续灼烧。","朱红羽阵、命中焰纹",1.3f,12,36,11),
        new("baihu","庚金虎煞","近身爪击降低护甲20点，持续12秒；对自身已破甲的目标伤害增加50%。","虎牙、交错金爪",1.8f,8,28,4),
        new("qilin","瑞麟护生","治疗自身及附近生命比例最低的一只友方妖兽，各恢复最大生命的12%。","瑞角、生机连光",0,20,36,9),
        new("moqilin","墨麟蚀光","目标位置留下6秒墨域；域内敌人造成的伤害降低25%。","墨鳞暗日、蚀光波纹",0.6f,18,38,7),
        new("xuanwu","龟蛇玄甲","获得最大生命18%的护盾，持续8秒；护盾耗尽时向最多6名近敌反震。","龟甲护罩、蛇影反震",1.1f,22,38,6),
        new("fenghuang","涅火回生","生命低于35%时自动净化灼烧、毒伤，6秒内恢复最大生命的24%。不是复活。","焰心、火茧展羽",0,45,44,0),
        new("dafeng","九霄风牢","目标附近生成4秒风牢，每秒牵引并伤害最多6敌。","风眼、旋转羽刃",0.45f,16,30,6),
        new("manman","比翼同心","羽击一敌；附近存在同族友方蛮蛮时，0.25秒后追加一次协同羽击。","双羽结、双鸟翼影",1.1f,10,18,8),
        new("xuanniao","玄音夺魄","音波伤敌，并削减目标当前灵力的15%。","鸟影、暗金声环",1.3f,14,30,8),
        new("qingluan","青羽净障","自身受到减速或束缚时净化，4秒内移速增加25%，免疫再次施加减速和束缚。","青羽净环",0,20,24,0),
        new("changliao","獠毒噬骨","毒牙咬击，留下6秒獠毒；再次咬中自身獠毒目标时引爆剩余毒伤。","毒牙、骨纹毒痕",1.15f,4,16,4),
        new("mo","吞梦入寐","令一敌沉睡最多2秒，受伤即醒；成功入梦时恢复自身最大灵力的8%。","梦珠、回流梦雾",0,16,24,7),
        new("hongmao","赤血狂鬃","消耗当前生命的5%（不致死），6秒内普攻伤害增加30%。","赤鬃、背脊血气",0,18,16,0),
        new("feiqiu","浮空震荡","膨胀震退最多6名近敌，4秒内受到的伤害降低25%。","膨胀绒球、圆形气浪",0.9f,14,18,5),
        new("yingying","逐影猎印","给一敌留下6秒猎印，自身实际伤害的25%积为影伤，到期追加结算。","影瞳、收拢影爪",0.7f,16,26,8),
        new("goushou","犬祟封生","咒印伤敌，6秒内目标获得的治疗降低50%。","犬首咒牙、暗红封印",1.2f,14,24,7),
        new("liejiejing","裂界鲸鸣","目标区域震击，0.8秒后在原处发生第二次裂爆，各攻击最多6敌。","鲸影、聚裂空间纹",1.1f,18,46,8),
        new("tianshou","镇天重压","自身周围展开6秒重压领域，敌人移速降低30%、攻击间隔增加30%。","重甲镇印、下压波纹",0.5f,22,48,9)
    };
    private static readonly Dictionary<string,MclslBeastTalentDefinition> BySpecies=All.ToDictionary(x=>x.Species,StringComparer.Ordinal);
    internal static MclslBeastTalentDefinition ForSpecies(string species)
        =>species!=null&&BySpecies.TryGetValue(species,out var value)?value:null;
    internal static MclslBeastTalentDefinition ForId(string id)
        =>id!=null&&id.StartsWith("BT_",StringComparison.Ordinal)?ForSpecies(id.Substring(3)):null;
}
