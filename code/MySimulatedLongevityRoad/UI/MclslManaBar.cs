using System;
using System.Reflection;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslManaBar
{
    private const string BarName = "MclslLingLiBar";
    private const string TitleKey = "mclsl_lingli";
    private const string DescriptionKey = "mclsl_lingli_description";

    internal static void UpdateSelected(SelectedUnitTab tab, Actor actor)
    {
        if (tab?._bar_health == null) return;
        StatBar template = tab._bar_health;
        if (!Update(template.transform.parent, template, actor, false)) return;
        Transform bar = template.transform.parent.Find(BarName);
        int targetIndex = template.transform.GetSiblingIndex() + 1;
        if (bar != null && bar.GetSiblingIndex() != targetIndex) bar.SetSiblingIndex(targetIndex);
    }

    internal static void UpdateUnit(UnitBarsElement element)
    {
        if (element == null) return;
        StatBar template = null;
        foreach (string field in new[] { "_mana", "_stamina", "_hunger", "_happiness" })
        {
            template = typeof(UnitBarsElement).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(element) as StatBar;
            if (template != null) break;
        }
        if (!Update(element.transform, template, element.actor, true)) return;
        Transform bar = element.transform.Find(BarName);
        if (bar != null && bar.GetSiblingIndex() != 0) bar.SetAsFirstSibling();
    }

    private static bool Update(Transform parent, StatBar template, Actor actor, bool compact)
    {
        if (parent == null || template == null) return false;
        Transform existing = parent.Find(BarName);
        StatBar bar = existing?.GetComponent<StatBar>();
        MclslManaBarBinding binding = bar?.GetComponent<MclslManaBarBinding>();

        // SelectedUnitTab.showStatBars runs every frame. The view is a snapshot
        // taken when it opens or changes actor; the mana model still regenerates.
        if (bar != null && binding != null && ReferenceEquals(binding.Actor, actor)
            && bar.gameObject.activeInHierarchy) return true;

        int maximum = MclslSpellSystem.MaxMana(actor);
        if (maximum <= 0)
        {
            if (bar != null) bar.gameObject.SetActive(false);
            return false;
        }

        if (bar == null)
        {
            bar = UnityEngine.Object.Instantiate(template, parent, false);
            bar.name = BarName;
            Image fill = bar.bar?.GetComponent<Image>();
            if (fill != null) fill.color = new Color(0.19f, 0.72f, 0.95f, 1f);
            Transform icon = bar.transform.Find("Icon");
            Image iconImage = icon?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = SpriteTextureLoader.getSprite("ui/Icons/LingLi");
                iconImage.gameObject.SetActive(iconImage.sprite != null);
            }
        }

        if (binding == null) binding = bar.gameObject.AddComponent<MclslManaBarBinding>();
        bar.gameObject.SetActive(true);
        int current = MclslSpellSystem.CurrentMana(actor);
        bar.setBar(current, maximum, "/" + maximum, false, false, true, compact ? 0.3f : 0.25f);
        SetTooltip(bar, current, maximum);
        binding.Actor = actor;
        return true;
    }

    private static void SetTooltip(StatBar bar, int current, int maximum)
    {
        MclslLocalizationBridge.RegisterKey(TitleKey, "灵力");
        MclslLocalizationBridge.RegisterKey(DescriptionKey, "灵力是施展法术的能量来源，上限随境界提升。");
        string description = "当前灵力：" + current + "\n最大灵力：" + maximum
            + "\n\n" + LocalizedTextManager.getText(DescriptionKey);
        foreach (TipButton tip in bar.GetComponentsInChildren<TipButton>(true))
        {
            tip.textOnClick = TitleKey;
            tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(description);
            tip.text_description_2 = string.Empty;
            tip.clickAction = null;
            tip.setHoverAction(tip.showTooltipDefault, false);
        }
    }
}

internal sealed class MclslManaBarBinding : MonoBehaviour
{
    internal Actor Actor;

    private void OnDisable() => Actor = null;
}
