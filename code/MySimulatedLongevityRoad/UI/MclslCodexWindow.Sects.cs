using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawAncientSects(MclslWorldRunState run)
    {
        DrawPageHeader(LM.Get("mclsl_codex_ten_sects"), "各宗委托按修士境界分配；失败无报酬，且可能陨落。");
        DrawInfoCard("委托风险", "#FFB78A", () =>
        {
            for (int realm = 0; realm < MclslRealmIds.Ordered.Length; realm++)
            {
                (int fail, int death) = MclslFactionMissionSystem.RiskForRealm(realm);
                GUILayout.Label(MclslRealmIds.Display(MclslRealmIds.Ordered[realm]) + " · "
                    + MclslFactionMissionSystem.TierName(MclslFactionMissionSystem.TierForRealm(realm))
                    + "：失败 " + (fail / 100f).ToString("0.##") + "% · 失败后死亡 "
                    + (death / 100f).ToString("0.##") + "%");
            }
        });

        for (int i = 0; i < MclslFactionMissionSystem.AncientSects.Length; i++)
        {
            int sectIndex = i;
            var sect = MclslFactionMissionSystem.AncientSects[sectIndex];
            DrawInfoCard(sect.Name, "#A6D8D1", () =>
            {
                GUILayout.Label("宗门委托按境界分配，近期动向如下：");
                if (_snapshot.AncientSectRecentMissions.TryGetValue(sect.Id, out List<MclslFactionMissionRecord> recent)
                    && recent.Count > 0)
                {
                    GUILayout.Space(4);
                    foreach (MclslFactionMissionRecord record in recent)
                        DrawFactionMissionRow(record, "#A6D8D1");
                }
                else GUILayout.Label("近期无事入录。");
            });
        }
    }
}
