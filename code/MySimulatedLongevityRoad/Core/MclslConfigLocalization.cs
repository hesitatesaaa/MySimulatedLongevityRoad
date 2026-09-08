using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslConfigLocalization
{
    private static bool _initialized;

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        Register("MCLSL_config_enable_death_announcements", "启用修士陨落公告", "高境修士死亡时发布玄黄界公告。");
        Register("MCLSL_config_enable_breakthrough_failure_announcements", "显示破境失败记录", "控制仙道破境失败是否写入玄黄仙录；金丹以下默认不入录，收藏角色除外。");
        Register("MCLSL_config_enable_yuanying_breakthrough_announcements", "启用元婴突破公告", "角色成就元婴时发布玄黄界公告；关闭后仍写入玄黄仙录。");
        Register("MCLSL_config_enable_huashen_breakthrough_announcements", "启用化神突破公告", "角色成就化神时发布玄黄界公告；关闭后仍写入玄黄仙录。");
        Register("MCLSL_config_enable_world_soul_announcements", "启用天地之魄公告", "天地之魄显化、陨灭或离去时发布公告。");
        Register("MCLSL_config_enable_ruin_death_announcements", "启用遗迹/遗府死亡公告", "控制修士死于宗门遗迹、遗府或秘境探索时是否显示顶部公告；关闭后仍写入玄黄仙录。");
        Register("MCLSL_config_death_popup_min_realm", "陨落公告最低境界", "最低只能设为金丹；炼气与筑基死亡绝不弹出顶部公告。");
        Register("MCLSL_config_enable_world_adventures", "启用遗迹游历", "允许修士通过遗迹和秘境获得机缘。");
        Register("MCLSL_config_enable_faction_commissions", "显示仙盟五老委托公告", "只控制万仙盟与五老会委托是否写入世界纪事；委托本身始终运行。");
        Register("MCLSL_config_enable_ancient_lineage_announcements", "显示仙道传承公告", "控制仙道法脉兴盛等传承消息是否写入公告与纪事；传承数据本身仍会记录。");
        Register("MCLSL_config_enable_pioneer_announcements", "显示新法初传提示", "新法开始萌芽时显示一次顶部提示；关闭后仍可在玄黄仙录查看时代状态。");
        Register("MCLSL_config_enable_minor_world_announcements", "显示日常天地异象", "显示灵气汇聚、地火涌动、星石坠落等日常异象提示；关闭后事件与奖励仍正常发生。");
        Register("MCLSL_config_enable_ruin_birth_announcements", "显示遗迹秘境显世提示", "遗府、秘境与宗门遗迹出现时显示顶部提示；关闭后仍会进入玄黄仙录并可正常探索。");
        Register("MCLSL_config_enable_resource_birth_announcements", "显示天地资源出现提示", "洞天与天地异变出现时显示顶部提示；元婴、化神突破公告仍由各自开关控制。");
        Register("MCLSL_config_enable_faction_policy_announcements", "显示仙盟五老施政提示", "万仙盟或五老会改变施政方略时显示顶部提示；势力影响与委托仍正常结算。");
        Register("MCLSL_config_enable_survival_announcements", "显示洞天避劫提示", "元婴借洞天避过死劫时显示顶部提示；关闭后保命与洞天损耗仍正常生效。");
        Register("MCLSL_config_enable_huanzhen", "启用还真轮回", "开启本世轮回与档案封存逻辑。");
        Register("MCLSL_config_huanzhen_anchor_interval", "还真锚点间隔", "调整还真锚点生成的年份间隔。");
        Register("MCLSL_config_huanzhen_safety_gap", "还真安全间隔", "限制连续还真触发的最短间隔。");
        Register("MCLSL_config_transmission_year", "传法变世年份", "控制仙道纪元持续年数；仙道纪元会按此年份自动分段。");
        Register("MCLSL_config_auto_collect_yuanying", "元婴自动收藏", "角色成就元婴时自动加入收藏。");
        Register("MCLSL_config_auto_collect_huashen", "化神自动收藏", "角色成就化神时自动加入收藏。");
        Register("MCLSL_config_auto_collect_hedao", "合道自动收藏", "角色成就合道时自动加入收藏。");
        Register("MCLSL_config_auto_collect_changsheng", "长生自动收藏", "角色证得长生时自动加入收藏。");
        Register("MCLSL_config_auto_collect_pure_root", "纯一灵根自动收藏", "角色获得纯一灵根时自动加入收藏。");
        Register("MCLSL_config_auto_collect_heaven_root", "天赐灵根自动收藏", "角色获得天赐灵根时自动加入收藏。");
    }

    private static void Register(string key, string name, string description)
    {
        MclslLocalizationBridge.RegisterKey(key, name);
        MclslLocalizationBridge.RegisterKey(key + " Description", description);
    }
}
