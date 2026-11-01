using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
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
    private static readonly FieldInfo[] TemplateFields =
    {
        TemplateField("_mana"), TemplateField("_stamina"), TemplateField("_hunger"), TemplateField("_happiness")
    };
    private static ConditionalWeakTable<Transform, MclslManaBarBinding> Bindings = new();
    private static FieldInfo TemplateField(string name) => typeof(UnitBarsElement).GetField(name,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    internal static void ClearRuntime() => Bindings = new();

    internal static void UpdateSelected(SelectedUnitTab tab, Actor actor)
    {
        if (tab?._bar_health == null || !tab.gameObject.activeInHierarchy) return;
        StatBar template = tab._bar_health;
        MclslManaBarBinding binding = Update(template.transform.parent, template, actor, false);
        if (binding == null) return;
        int targetIndex = template.transform.GetSiblingIndex() + 1;
        if (binding.transform.GetSiblingIndex() != targetIndex) binding.transform.SetSiblingIndex(targetIndex);
    }

    internal static void UpdateUnit(UnitBarsElement element)
    {
        if (element == null || !element.gameObject.activeInHierarchy) return;
        StatBar template = null;
        for (int i = 0; i < TemplateFields.Length && template == null; i++)
            template = TemplateFields[i]?.GetValue(element) as StatBar;
        MclslManaBarBinding binding = Update(element.transform, template, element.actor, true);
        if (binding != null && binding.transform.GetSiblingIndex() != 0) binding.transform.SetAsFirstSibling();
    }

    private static MclslManaBarBinding Update(Transform parent, StatBar template, Actor actor, bool compact)
    {
        if (parent == null || template == null || !parent.gameObject.activeInHierarchy) return null;
        Bindings.TryGetValue(parent, out MclslManaBarBinding binding);
        long id = MclslActorAccessor.Alive(actor) ? MclslActorAccessor.Id(actor) : 0;
        float now = Time.unscaledTime;
        if (binding != null && binding.ActorId == id && now < binding.NextRefresh) return binding.Bar.gameObject.activeSelf ? binding : null;
        int maximum = id <= 0 ? 0 : MclslSpellSystem.MaxMana(actor);
        if (maximum <= 0)
        {
            if (binding != null) { binding.Bar.gameObject.SetActive(false); binding.ActorId = id; binding.NextRefresh = now + 0.1f; }
            return null;
        }
        if (binding == null)
        {
            StatBar bar = parent.Find(BarName)?.GetComponent<StatBar>();
            if (bar == null)
            {
                bar = UnityEngine.Object.Instantiate(template, parent, false);
                bar.name = BarName;
                Image fill = bar.bar?.GetComponent<Image>();
                if (fill != null) fill.color = new Color(0.19f, 0.72f, 0.95f, 1f);
                Image icon = bar.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null) { icon.sprite = SpriteTextureLoader.getSprite("ui/Icons/LingLi"); icon.gameObject.SetActive(icon.sprite != null); }
            }
            binding = bar.GetComponent<MclslManaBarBinding>() ?? bar.gameObject.AddComponent<MclslManaBarBinding>();
            binding.Bar = bar;
            binding.Tips = bar.GetComponentsInChildren<TipButton>(true);
            Bindings.Remove(parent);
            Bindings.Add(parent, binding);
            MclslLocalizationBridge.RegisterKey(TitleKey, "灵力");
            MclslLocalizationBridge.RegisterKey(DescriptionKey, "灵力是施展法术的能量来源，上限随境界提升。");
        }
        binding.Bar.gameObject.SetActive(true);
        int current = MclslSpellSystem.CurrentMana(actor);
        if (binding.ActorId != id || binding.Current != current || binding.Maximum != maximum)
        {
            binding.Bar.setBar(current, maximum, "/" + maximum, false, false, true, compact ? 0.3f : 0.25f);
            string description = MclslLocalizationBridge.RuntimeText("当前灵力：" + current + "\n最大灵力：" + maximum
                + "\n\n" + LocalizedTextManager.getText(DescriptionKey));
            for (int i = 0; i < binding.Tips.Length; i++)
            {
                TipButton tip = binding.Tips[i];
                if (tip == null) continue;
                tip.textOnClick = TitleKey; tip.textOnClickDescription = description;
                tip.text_description_2 = string.Empty; tip.clickAction = null;
                tip.setHoverAction(tip.showTooltipDefault, false);
            }
            binding.Current = current; binding.Maximum = maximum;
        }
        binding.ActorId = id; binding.NextRefresh = now + 0.1f;
        return binding;
    }
}

internal sealed class MclslManaBarBinding : MonoBehaviour
{
    internal long ActorId;
    internal int Current = -1, Maximum = -1;
    internal float NextRefresh;
    internal StatBar Bar;
    internal TipButton[] Tips = Array.Empty<TipButton>();
    private void OnDisable() { ActorId = 0; NextRefresh = 0; }
}
