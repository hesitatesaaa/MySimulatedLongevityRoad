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
        try { PowerButtonSelector.instance?.unselectAll(); } catch { }
        try { PowerButtonSelector.instance?.clickPowerButton(_button); } catch { }
        try { PowerButtonSelector.instance?.setPower(_button); } catch { }
        return true;
    }
}
