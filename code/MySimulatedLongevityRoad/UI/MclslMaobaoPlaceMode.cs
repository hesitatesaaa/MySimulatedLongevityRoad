using NeoModLoader.General;
using UnityEngine;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

/// <summary>猫宝“放”按钮使用鬼谷时光长河同款的地图 GodPower 放置模式。</summary>
internal static class MclslMaobaoPlaceMode
{
    internal const string PlacePowerId = "mclsl_maobao_place_actor";
    private static bool _registered;
    private static PowerButton _button;

    internal static void EnsureRegistered()
    {
        MclslMaobaoArchiveManager.Init();
        if (_registered && _button != null) return;
        if (AssetManager.powers == null) return;
        GodPower power = AssetManager.powers.get(PlacePowerId);
        if (power == null)
        {
            power = new GodPower
            {
                id = PlacePowerId,
                name = "mclsl.maobao.place_actor",
                click_action = MclslMaobaoArchiveManager.SpawnSavedActor,
                ignore_cursor_icon = true
            };
            AssetManager.powers.add(power);
        }
        else
        {
            power.click_action = MclslMaobaoArchiveManager.SpawnSavedActor;
            power.ignore_cursor_icon = true;
        }
        if (_button == null)
        {
            Sprite icon = SpriteTextureLoader.getSprite("ui/Icons/MaobaoEntrance");
            if (icon == null) icon = SpriteTextureLoader.getSprite("ui/Icons/MaobaoEntrance.png");
            _button = PowerButtonCreator.CreateGodPowerButton(PlacePowerId, icon);
            if (_button != null) _button.gameObject.SetActive(false);
        }
        _registered = _button != null;
    }

    internal static bool Begin(string actorId)
    {
        EnsureRegistered();
        if (_button == null || !MclslMaobaoArchiveManager.SelectActorToPlace(actorId)) return false;
        ScrollWindow.hideAllEvent();
        try { PowerButtonSelector.instance?.unselectAll(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslMaobaoPlaceMode-cs-1", "猫宝放置：取消原按钮选择失败: " + mclslEmptyCatchEx.Message); }
        try { PowerButtonSelector.instance?.clickPowerButton(_button); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslMaobaoPlaceMode-cs-2", "猫宝放置：模拟点击按钮失败: " + mclslEmptyCatchEx.Message); }
        try { PowerButtonSelector.instance?.setPower(_button); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslMaobaoPlaceMode-cs-3", "猫宝放置：切换能力按钮失败: " + mclslEmptyCatchEx.Message); }
        return true;
    }
}
