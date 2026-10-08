using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private string _selectedFamilyId = string.Empty;
    private long _selectedFamilyMemberId;
    private int _familyPage;
    private int _familyMemberPage;
    private string _familyRealmFilter = MclslEventCatalog.All;
    private string _familyStoneAmount = "10";

    private static string FamilyCityName(MclslFamilyRecord family, long cityId)
    {
        if (World.world?.cities != null)
            foreach (City city in World.world.cities)
                if (city?.data?.id == cityId) return string.IsNullOrWhiteSpace(city.data.name) ? "未名城镇" : city.data.name;
        foreach (MclslFamilyMemberRecord member in family.Members)
            if (member != null && member.CityId == cityId
                && MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                && actor.city?.data?.id == cityId) return actor.city.data.name;
        return "城池 " + cityId;
    }

    private void DrawFamilyMemberControls(MclslFamilyRecord family, Actor actor)
    {
        DrawInfoCard("管理 · " + actor.getName(), "#FFD37A", () =>
        {
            GUILayout.Label("个人灵石：" + MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LM.Get("mclsl_family_set_head"), GUILayout.Width(92))) MclslFamilySystem.SetHead(family, MclslActorAccessor.Id(actor));
            if (GUILayout.Button(LM.Get("mclsl_family_set_focus"), GUILayout.Width(92))) MclslFamilySystem.SetFocus(family, MclslActorAccessor.Id(actor));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LM.Get("mclsl_family_stone_amount"), GUILayout.Width(78));
            _familyStoneAmount = GUILayout.TextField(_familyStoneAmount, 7, GUILayout.Width(70));
            int.TryParse(_familyStoneAmount, out int amount);
            if (GUILayout.Button(LM.Get("mclsl_family_deposit_stones"), GUILayout.Width(82)) && amount > 0)
                MclslFamilySystem.TransferStones(family, actor, amount, true);
            if (GUILayout.Button(LM.Get("mclsl_family_withdraw_stones"), GUILayout.Width(82)) && amount > 0)
                MclslFamilySystem.TransferStones(family, actor, amount, false);
            GUILayout.EndHorizontal();
            if (MclslBagSystem.IsLocked(actor)) { GUILayout.Label("背包不可写，物品调拨暂不可用。"); return; }
            MclslBagState bag = MclslBagSystem.Peek(actor);
            foreach (MclslOwnedItem item in bag.Items.Where(x => x != null
                && !MclslInventoryDataRules.IsEquipped(bag, x)).Take(12))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label((MclslItemCatalog.Get(item.ItemId)?.Name ?? "未知物品") + " ×" + item.Count);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(LM.Get("mclsl_family_deposit_item"), GUILayout.Width(88)))
                    MclslFamilySystem.TransferItem(family, actor, item.ItemId, true);
                GUILayout.EndHorizontal();
            }
        });
    }
}
