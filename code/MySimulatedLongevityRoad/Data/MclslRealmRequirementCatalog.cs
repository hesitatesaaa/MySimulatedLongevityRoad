using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslRealmRequirementDefinition
{
    internal string RealmId { get; set; } = string.Empty;
    internal string IconPath { get; set; } = string.Empty;
    internal string Motto { get; set; } = string.Empty;
    internal string CoreObject { get; set; } = string.Empty;
    internal string EntryRequirement { get; set; } = string.Empty;
    internal string BreakthroughRequirement { get; set; } = string.Empty;
    internal string WorldInteraction { get; set; } = string.Empty;
    internal string FailureRisk { get; set; } = string.Empty;
    internal string CurrentImplementation { get; set; } = string.Empty;
}

internal static class MclslRealmRequirementCatalog
{
    internal static readonly IReadOnlyList<MclslRealmRequirementDefinition> Requirements = new[]
    {
        R(
            MclslRealmIds.LianQi,
            "trait/realm_1",
            "吸天地之灵，以御其气",
            "感气、真元、仙凡瘴、功法",
            "角色获得修行资格后先进入感气，不会直接成为炼气修士；真元达到800才正式踏入炼气。玩家手动赋予炼气时会补足最低800真元。",
            "炼气以800真元为起点，继续修至4000圆满后，才会寻得筑基奇物机缘。",
            "感气只积累入门真元；炼气阶段以祛瘴、引灵和凝聚天地之根为主，不直接争夺高境世界资源。",
            "灵根纯度、心境、功法感悟和年度进度不足会拉长感气及炼气停留时间。",
            "已实现：感气一至十成、800真元入炼气、玄黄炼心决、洗瘴池、功法池、灵根、心境和年度修炼。"),
        R(
            MclslRealmIds.ZhuJi,
            "trait/realm_2",
            "假天地之奇，以筑道基",
            "筑基奇物、噬物法、道基",
            "炼气真元达到筑基最低要求4000，并遇到筑基奇物。真元4000是下境门槛，不是炼气真元上限。",
            "以噬物法吞服天地奇物于丹田，天地之根扎根其上；模组中遇得奇物后即筑道基，不再额外设置筑基失败。",
            "筑基奇物先分人之奇、地之奇、天之奇。人之奇分上中下品；地之奇重完整度与规则规模；天之奇极罕见，可支撑后续大道。",
            "未遇合适奇物会停留；奇物品质影响后续法则容量、金丹成色和稳定性。",
            "已实现：动态筑基奇物、人地天分类、人之奇品阶、地之奇完整度、天之奇规则强度与来历记录。"),
        R(
            MclslRealmIds.JinDan,
            "trait/realm_3",
            "窥天地之法，以炼金丹",
            "自身法则、金丹成色",
            "筑基真元达到金丹最低要求24000，已拥有筑基奇物和主修功法。真元达到门槛后仍可继续积累。",
            "由功法与道基决定金丹法则；仙法不可同修只按同修人数降低真元增长，不设置法位，也不会额外封锁结丹。",
            "金丹是修士真正属于自己的法则凝聚，决定后续可争夺的洞天类型；多法提高上限但降低协调与纯度。",
            "法则协调低会影响洞天适配；若失去道基根本，金丹成色和稳定也会受损。",
            "已实现：功法法则池 + 奇物法则生成金丹法则、纯度、协调。"),
        R(
            MclslRealmIds.YuanYing,
            "trait/realm_4",
            "夺天地之精，以成元婴",
            "洞天法域及天地之精",
            "金丹真元达到元婴最低要求120000，并拥有金丹法则、纯度和协调。",
            "在本世洞天中寻找法则适配目标，同年与其他金丹争夺天地之精；心境、洞天残图和金丹质量共同影响争夺与炼化。",
            "以洞天形成自身法域以代替元婴；洞天附着于原生世界，元婴及以上人数受当前洞天数量限制。",
            "匹配不足会无果；炼化失败会损伤洞天完整度并延迟突破。后续可继续补“洞天不灭修士不死”的死亡减免表现。",
            "已实现：世界洞天、洞天席位上限、适配争夺、天地之精、洞天余精和完整度消耗。"),
        R(
            MclslRealmIds.HuaShen,
            "trait/realm_5",
            "抽天地之髓，以得其神",
            "天地之变及天地之髓",
            "元婴真元达到化神最低要求600000，拥有个人元婴洞天与天地之精，并等到可抽取的天地之变显化。",
            "伪装自身洞天为玄黄界本体环境，骗过天之髓本能的赋法行为，抽走本应出现在世界上的一丝法则；心境影响承受与反噬风险。",
            "天地之变来自历史锚点、世界周期、遗迹崩毁或原生灾厄死亡信号，只以原生地形扰动表现；化神及以上最多20人。",
            "抽髓失败可能退转进度、受创，严重时死于天地反噬。",
            "已实现：天地之变生成、化神席位上限、抽髓争夺、失败反噬、灾厄死亡触发天地之变。"),
        R(
            MclslRealmIds.HeDao,
            "trait/realm_6",
            "祭天地之魄，以身合道",
            "天地之魄及天职",
            "新法化神通过祭炼天地之魄合道，不使用普通真元门槛；旧法化神则须将累计真元修至2400000，再以自身大道尝试合道。",
            "天地之魄是天道法则的具象化身，显化为敌对实体；凡俗生灵或修士承接并祭炼魄核即可立地合道。前六境根基只计算祭魄适配与合道稳定，不决定能否合道。",
            "合道者继承天职并接近天人合一。魄内除修仙界法则外还附着构筑世界的基石力量，后续天职履行决定逆理根基。",
            "低境或凡人也可能一步合道；根基不足不会阻止合道，但会提高后续天职反噬与稳定代价。",
            "已实现：十属性天地之魄显化、十魄席位上限、祭魄合道、合道记录、天职反噬和魄位复归。"),
        R(
            MclslRealmIds.ChangSheng,
            "trait/realm_7",
            "逆天地之理，以证长生",
            "天地之理及逆理工程",
            "合道者承接天职后积累履职进度，依据根基、心境、稳定、还真、死亡、终局等世界事实触及天地之理。",
            "合道后经历寻找天地之理、熟悉天地之理、掌控天地之理、逆转天地之理；围绕特定天地之理完成逆理工程，进度满100后唯一替换为长生境界。",
            "长生不是数值强化，而是成道之理取代原本天地之理，化作世界的一部分；本世最多5名长生天尊。",
            "逆理失败会引发天地修正、长生遗痕崩解或规则反噬。",
            "已实现：预生成逆理、长生席位上限、逆理数据、面板、进度、长生达成公告、已逆之理的年度世界影响。")
    };

    internal static MclslRealmRequirementDefinition Get(string realmId)
    {
        foreach (MclslRealmRequirementDefinition requirement in Requirements)
            if (requirement.RealmId == realmId) return requirement;
        return Requirements[0];
    }

    private static MclslRealmRequirementDefinition R(string realm, string icon, string motto, string core, string entry, string breakthrough, string world, string risk, string implementation) => new()
    {
        RealmId = realm,
        IconPath = icon,
        Motto = motto,
        CoreObject = core,
        EntryRequirement = entry,
        BreakthroughRequirement = breakthrough,
        WorldInteraction = world,
        FailureRisk = risk,
        CurrentImplementation = implementation
    };
}
