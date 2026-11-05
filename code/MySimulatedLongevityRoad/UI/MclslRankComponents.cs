using System;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 榜单(通用榜 / 妖兽榜)共用的“单条增删后刷新可视行”逻辑。两处原先逐字相同，
/// 抽此处统一，避免再各写一份（含脏区间计算与视图标记）。
/// </summary>
internal static class MclslRankEntryChange
{
    internal static bool Apply(MclslOrderedIdIndex<MclslRankEntry> entries, MclslVirtualCardList rows,
        long id, bool included, MclslRankEntry entry)
    {
        int oldIndex = entries.IndexOf(id);
        if (included) entries.Upsert(id, entry); else entries.Remove(id);
        int newIndex = included ? entries.IndexOf(id) : -1;
        if (oldIndex < 0 && newIndex < 0) return false;
        (int changedStart, int changedEnd) = MclslOrderedChangeRange.Affected(oldIndex, newIndex);
        rows?.MarkDirty(changedStart, changedEnd);
        return true;
    }
}

internal sealed class MclslRankWindowUpdater : MonoBehaviour
{
    internal Action OnOpen;
    internal Action OnClose;
    internal Action OnUpdate;

    private void OnEnable() => OnOpen?.Invoke();
    private void OnDisable() => OnClose?.Invoke();
    private void Update() => OnUpdate?.Invoke();
}

internal sealed class MclslRankTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    internal string TooltipText;
    internal string TooltipDescription;
    private bool _hovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hovering || string.IsNullOrWhiteSpace(TooltipText)) return;
        _hovering = true;
        Tooltip.show(gameObject, "tip", new TooltipData
        {
            tip_name = MclslLocalizationBridge.RuntimeText(TooltipText),
            tip_description = MclslLocalizationBridge.RuntimeText(TooltipDescription ?? string.Empty)
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hovering) return;
        _hovering = false;
        Tooltip.hideTooltip();
    }

    private void OnDisable()
    {
        if (!_hovering) return;
        _hovering = false;
        Tooltip.hideTooltip();
    }
}

internal sealed class MclslRankRightClickHandler : MonoBehaviour, IPointerClickHandler
{
    internal Action OnRightClick;
    internal bool BlockRightClick = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        OnRightClick?.Invoke();
        if (BlockRightClick) eventData.Use();
    }
}

internal sealed class MclslRankCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Actor _actor;

    internal void Setup(MclslRankEntry item, int index, string sortValue)
    {
        _actor = item.Actor;
        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = index switch
            {
                0 => MclslUiTheme.RankRowFirst,
                1 => MclslUiTheme.RankRowSecond,
                2 => MclslUiTheme.RankRowThird,
                _ when index % 2 == 0 => MclslUiTheme.RankRowEven,
                _ => MclslUiTheme.RankRowOdd
            };
        }
        MclslRankTextFormat.SetText(transform, "RankText", (index + 1).ToString(CultureInfo.InvariantCulture), RankColor(index));
        MclslRankTextFormat.SetText(transform, "NameText", string.IsNullOrWhiteSpace(item.Name) ? "未名修士" : item.Name, index < 3 ? RankColor(index) : MclslUiTheme.RankTextPrimary);
        string detail = string.IsNullOrWhiteSpace(item.RootAttributes) ? item.RootText : item.RootAttributes;
        if (!string.IsNullOrWhiteSpace(item.ExtraText) && !string.Equals(detail, item.ExtraText, StringComparison.Ordinal))
            detail += " · " + item.ExtraText;
        MclslRankTextFormat.SetText(transform, "DetailText", detail, MclslUiTheme.RankTextMuted);
        MclslRankTextFormat.SetText(transform, "PowerText", sortValue, MclslUiTheme.RankAccentPrimary);
        MclslRankTextFormat.SetText(transform, "RightText", string.IsNullOrWhiteSpace(item.KingdomName) ? "无归属" : item.KingdomName, MclslUiTheme.RankAccentSecondary);
        MclslRankTextFormat.SetText(transform, "RealmText", string.IsNullOrWhiteSpace(item.RealmName) ? "未入道" : item.RealmName.Replace("新法·", ""), MclslUiTheme.RankTextPrimary);

        UiUnitAvatarElement avatar = GetComponentInChildren<UiUnitAvatarElement>(true);
        if (avatar != null)
        {
            bool valid = MclslActorAccessor.Alive(_actor);
            avatar.gameObject.SetActive(valid);
            if (valid)
            {
                avatar.show(_actor);
                if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
            }
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (MclslActorAccessor.Alive(_actor)) ActionLibrary.openUnitWindow(_actor);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MclslActorAccessor.Alive(_actor)) _actor.showTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData) => Tooltip.hideTooltip();

    private void OnDisable()
    {
        _actor = null;
        Tooltip.hideTooltip();
    }

    private static Color RankColor(int rank)
    {
        if (rank == 0) return MclslUiTheme.RankMedalFirst;
        if (rank == 1) return MclslUiTheme.RankMedalSecond;
        if (rank == 2) return MclslUiTheme.RankMedalThird;
        return MclslUiTheme.RankTextEmphasis;
    }
}

/// <summary>通用榜 / 妖兽榜共用的滚动区（Mask+ScrollRect）初始化，消除两处逐字重复。</summary>
internal static class MclslRankScrollSetup
{
    internal static ScrollRect ConfigureRankScroll(GameObject scrollObj, Color background)
    {
        Image bg = scrollObj.GetComponent<Image>();
        if (bg != null) bg.color = background;
        Mask mask = scrollObj.GetComponent<Mask>();
        if (mask != null) mask.showMaskGraphic = true;
        ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = false;
        scroll.scrollSensitivity = 22f;
        return scroll;
    }
}
