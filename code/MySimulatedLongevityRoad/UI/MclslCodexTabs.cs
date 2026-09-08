using System;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslCodexTabCatalog
{
    private static readonly MclslCodexTab[] NewLawTabs =
    {
        T("天下总览", "ui/Icons/XuanHuangXianLu"),
        T("境界资源", "trait/realm_7"),
        T("原生诸国", "ui/Icons/WanXianMeng"),
        T("元婴洞天", "ui/Icons/DongTian"),
        T("天地之变", "ui/Icons/TianDiZhiBian"),
        T("天地之魄", "ui/Icons/TianDiZhiPo"),
        T("天地之理", "ui/Icons/TianDiZhiLi"),
        T("宗门遗迹", "ui/Icons/ZongMenYiJi"),
        T("秘境", "ui/Icons/ZongMenYiJi"),
        T("仙法不可同修", "ui/Icons/TianDiZhiLi"),
        T("修士生死", "ui/Icons/SiWang"),
        T("还真轮回", "ui/Icons/HuanZhen"),
        T("世界纪事", "ui/Icons/XuanHuangXianLu")
    };

    private static readonly MclslCodexTab[] AncientTabs =
    {
        T("仙道总览", "ui/Icons/XuanHuangXianLu"),
        T("仙道修行", "trait/realm_5"),
        T("仙师授法", "trait/gifts_6"),
        T("仙道破境", "trait/realm_3"),
        T("心境劫数", "ui/Icons/XinJing"),
        T("原生诸国", "ui/Icons/WanXianMeng"),
        T("灵机灾变", "ui/Icons/TianDiZhiBian"),
        T("秘境", "ui/Icons/ZongMenYiJi"),
        T("遗府", "ui/Icons/ZongMenYiJi"),
        T("天地观悟", "ui/Icons/TianDiZhiPo"),
        T("仙道纪事", "ui/Icons/XuanHuangXianLu")
    };

    internal static MclslCodexTab[] ForEpoch(string epoch)
    {
        return string.Equals(epoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal)
            ? AncientTabs
            : NewLawTabs;
    }

    private static MclslCodexTab T(string title, string iconPath) => new(title, iconPath);
}

internal sealed class MclslCodexTab
{
    internal readonly string Title;
    internal readonly string IconPath;

    internal MclslCodexTab(string title, string iconPath)
    {
        Title = title ?? string.Empty;
        IconPath = iconPath ?? string.Empty;
    }
}
