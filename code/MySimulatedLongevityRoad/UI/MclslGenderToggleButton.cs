using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslGenderToggleButton
{
    private const string ButtonName = "MclslGenderToggle";

    internal static void Refresh(UnitWindow window)
    {
        if (window?.actor?.data == null) return;
        Transform avatar = SafeAvatarTransform(window);
        if (avatar == null) return;

        GameObject buttonObject = EnsureButtonObject(avatar);
        Image image = buttonObject.GetComponent<Image>();
        Button button = buttonObject.GetComponent<Button>();
        if (image == null || button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Toggle(window));
        RefreshImage(window.actor, image, button);
    }

    private static Transform SafeAvatarTransform(UnitWindow window)
    {
        try { return window?._avatar_element?.transform; }
        catch { return null; }
    }

    private static GameObject EnsureButtonObject(Transform avatar)
    {
        Transform existing = avatar.Find(ButtonName);
        GameObject buttonObject = existing?.gameObject;
        if (buttonObject == null)
        {
            buttonObject = new GameObject(ButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(avatar, false);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(17.5f, -16.8f);
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(16f, 16f);

        Image image = buttonObject.GetComponent<Image>();
        image.raycastTarget = true;

        TipButton tip = buttonObject.GetComponent<TipButton>() ?? buttonObject.AddComponent<TipButton>();
        tip.textOnClick = "切换性别";
        tip.textOnClickDescription = "点击切换此生物的原生性别，并刷新头像显示。";
        return buttonObject;
    }

    private static void Toggle(UnitWindow window)
    {
        Actor actor = window?.actor;
        if (actor?.data == null) return;
        if (actor.data.sex == ActorSex.None) return;

        actor.data.sex = actor.data.sex == ActorSex.Male ? ActorSex.Female : ActorSex.Male;
        try { actor.clearSprites(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslGenderToggleButton-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslGenderToggleButton.cs #1: " + mclslEmptyCatchEx.Message); }
        try { window._avatar_element?.show(actor); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslGenderToggleButton-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslGenderToggleButton.cs #2: " + mclslEmptyCatchEx.Message); }
        Refresh(window);
        try { MclslActorInfoPanel.Refresh(window); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslGenderToggleButton-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslGenderToggleButton.cs #3: " + mclslEmptyCatchEx.Message); }
    }

    private static void RefreshImage(Actor actor, Image image, Button button)
    {
        ActorSex sex = actor?.data?.sex ?? ActorSex.None;
        if (sex == ActorSex.None)
        {
            image.enabled = false;
            button.interactable = false;
            return;
        }

        image.enabled = true;
        button.interactable = true;
        image.sprite = SpriteTextureLoader.getSprite(sex == ActorSex.Male
            ? "ui/Icons/GenderMale"
            : "ui/Icons/GenderFemale");
        image.color = Color.white;
    }
}
