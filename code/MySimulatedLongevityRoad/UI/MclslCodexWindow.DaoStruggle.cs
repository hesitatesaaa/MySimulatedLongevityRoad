using System;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private static string _daoStruggleRealmFilter = string.Empty;

    private void DrawDaoStruggle(MclslWorldRunState run)
    {
        DrawPageHeader("仙法不可同修", "天地变后，同修一法者越众，修行越滞。每年仅有少量受困法脉尝试改修，或在附近爆发法争。");
        int filteredCount = CountFilteredDaoLineages();
        GUILayout.BeginHorizontal();
        DrawOverviewPill("天地规则", run?.LawConflictEnabled == true ? "已降临" : "尚未降临", "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("受困功法", filteredCount.ToString(), "#FFD37A", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawInfoCard("同法压制", "#9CD7FF", () =>
        {
            GUILayout.BeginHorizontal(); DrawTag("人数均摊", "#A7E08A"); DrawTag("改修或法争", "#FFD37A"); DrawTag("死亡即减", "#CFC7B2"); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Label("同一功法存活修士越多，每名同修者修炼效率越低。系统不会全图寻找目标；每年只抽取少量法脉，尝试改修现有功法或触发附近原生战斗。");
        });

        GUILayout.BeginHorizontal();
        DaoRealmButton("全部", string.Empty);
        DaoRealmButton("炼气", MclslRealmIds.LianQi); DaoRealmButton("筑基", MclslRealmIds.ZhuJi); DaoRealmButton("金丹", MclslRealmIds.JinDan);
        DaoRealmButton("元婴", MclslRealmIds.YuanYing); DaoRealmButton("化神", MclslRealmIds.HuaShen); DaoRealmButton("合道", MclslRealmIds.HeDao);
        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal(); GUILayout.Space(6);

        if (filteredCount == 0) { DrawInfoCard("暂无同法之困", "#CFC7B2", () => GUILayout.Label("当前筛选下暂未出现多人同修一法的记录。")); return; }
        GUILayout.Label("<color=#FFD37A><b>同修人数最多的功法（按当前最高境界筛选）</b></color>");
        int shown = 0;
        for (int i = 0; i < _snapshot.DaoStruggleLineagesSorted.Count && shown < 20; i++)
        {
            MclslTechniqueLineageRecord lineage = _snapshot.DaoStruggleLineagesSorted[i];
            if (!DaoRealmMatches(lineage)) continue;
            DrawSameTechniqueCard(lineage); shown++;
        }
        DrawSameTechniqueRecentEvents();
    }

    private void DaoRealmButton(string label, string realm)
    {
        bool selected = string.Equals(_daoStruggleRealmFilter, realm, StringComparison.Ordinal);
        string text = selected ? "<color=#FFD37A><b>" + label + "</b></color>" : label;
        if (GUILayout.Button(text, GUILayout.Width(72))) _daoStruggleRealmFilter = realm;
    }
    private int CountFilteredDaoLineages() { int count=0; for(int i=0;i<_snapshot.DaoStruggleLineagesSorted.Count;i++) if(DaoRealmMatches(_snapshot.DaoStruggleLineagesSorted[i])) count++; return count; }
    private bool DaoRealmMatches(MclslTechniqueLineageRecord lineage) => lineage != null && (string.IsNullOrWhiteSpace(_daoStruggleRealmFilter) || string.Equals(lineage.PeakRealm, _daoStruggleRealmFilter, StringComparison.Ordinal));

    private void DrawSameTechniqueCard(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return; int count=Math.Max(1,lineage.CurrentPractitioners); int efficiency=Math.Max(1,(int)Math.Round(100f/count));
        DrawInfoCard("《"+Blank(lineage.Name)+"》",count>=10?"#FF8E8E":"#FFD37A",()=>
        {
            GUILayout.BeginHorizontal(); DrawTag("同修"+count+"人","#FFD37A"); DrawTag("效率"+efficiency+"%",count>=5?"#FF8E8E":"#A7E08A"); DrawTag("最高 "+MclslRealmIds.Display(lineage.PeakRealm),"#9CD7FF"); DrawTag("上限 "+MclslRealmIds.Display(AncientLineageDisplayMaxRealm(lineage)),"#B7A7FF"); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Label("法则：<color=#B7A7FF>"+ReplaceTags(lineage.LawTags)+"</color>");
        });
    }

    private void DrawSameTechniqueRecentEvents()
    {
        if (_snapshot.DaoStruggleEventsSorted.Count==0) return; GUILayout.Space(8); GUILayout.Label("<color=#D8A7FF><b>相关纪事</b></color>");
        int limit = Math.Min(8, _snapshot.DaoStruggleEventsSorted.Count);
        for (int i = 0; i < limit; i++)
        {
            MclslRunEventRecord e = _snapshot.DaoStruggleEventsSorted[i];
            if (e == null) continue;
            DrawInfoCard(e.Year + "年｜" + e.Title, "#D8A7FF", () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag("仙法不可同修", "#D8A7FF");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(e) && GUILayout.Button("定位", GUILayout.Width(62f)))
                    MclslEventLocator.Locate(e);
                GUILayout.EndHorizontal();
                GUILayout.Label(PlayerFacingEventBody(e.Body));
            });
        }
    }
}
