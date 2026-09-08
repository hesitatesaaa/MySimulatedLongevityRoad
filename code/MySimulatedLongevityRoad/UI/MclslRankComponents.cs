using System;
using System.Globalization;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

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
            tip_name = TooltipText,
            tip_description = TooltipDescription ?? string.Empty
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
        SetText("RankText", (index + 1).ToString(CultureInfo.InvariantCulture), RankColor(index));
        SetText("NameText", string.IsNullOrWhiteSpace(item.Name) ? "未名修士" : item.Name, Color.white);
        SetText("PowerText", sortValue, new Color(0.4f, 0.8f, 1f));
        SetText("RightText", string.IsNullOrWhiteSpace(item.RootText) ? item.ExtraText : item.RootText, new Color(1f, 0.84f, 0f));
        SetText("RealmText", string.IsNullOrWhiteSpace(item.RealmName) ? "未入道" : item.RealmName.Replace("新法·", ""), new Color(1f, 0.6f, 0.2f));

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

    private void SetText(string childName, string value, Color color)
    {
        Text text = transform.Find(childName)?.GetComponent<Text>();
        if (text == null) return;
        text.text = value ?? string.Empty;
        text.color = color;
    }

    private static Color RankColor(int rank)
    {
        if (rank == 0) return new Color(1f, 0.84f, 0f);
        if (rank == 1) return new Color(0.75f, 0.75f, 0.75f);
        if (rank == 2) return new Color(0.8f, 0.5f, 0.2f);
        return Color.white;
    }
}
