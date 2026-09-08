using System;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

public sealed class MclslRankCard : MonoBehaviour
{
    private Text _rankText;
    private UiUnitAvatarElement _avatarElement;
    private Image _avatarImage;
    private Text _nameText;
    private Text _rootText;
    private Text _powerText;
    private Text _realmText;
    private Text _extraText;
    private Actor _actor;
    private long _actorId;
    private static GameObject _prefab;
    private const float CardWidth = 210f;
    private const float CardHeight = 38f;

    internal static GameObject CreatePrefab()
    {
        if (_prefab != null) return _prefab;

        GameObject cardObj = new("MclslRankCard", typeof(Button));
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(CardWidth, CardHeight);
        Image bg = cardObj.AddComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;

        Text rankText = CreateText(cardObj.transform, "RankText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-5f, 0f), new Vector2(26f, 26f), 12, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0f));
        UiUnitAvatarElement avatarElement = CreateAvatarElement(cardObj.transform);

        Text nameText = CreateText(cardObj.transform, "NameText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(64f, 12f), new Vector2(120f, 18f), 7, TextAnchor.MiddleLeft, Color.white);
        Text rootText = CreateText(cardObj.transform, "RootText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(175f, 12f), new Vector2(40f, 18f), 7, TextAnchor.MiddleLeft, Color.yellow);
        Text powerText = CreateText(cardObj.transform, "PowerText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(64f, -6f), new Vector2(75f, 18f), 8, TextAnchor.MiddleLeft, new Color(1f, 0.3f, 0.3f));
        Text realmText = CreateText(cardObj.transform, "RealmText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(115f, -6f), new Vector2(50f, 18f), 8, TextAnchor.MiddleLeft, new Color(0.6f, 0.9f, 1f));
        Text extraText = CreateText(cardObj.transform, "ExtraText", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(175f, -6f), new Vector2(40f, 18f), 8, TextAnchor.MiddleLeft, new Color(1f, 0.8f, 0f));

        ((Selectable)cardObj.GetComponent<Button>()).targetGraphic = bg;
        MclslRankCard card = cardObj.AddComponent<MclslRankCard>();
        card._rankText = rankText;
        card._avatarElement = avatarElement;
        card._avatarImage = avatarElement == null ? cardObj.transform.Find("Avatar")?.GetComponent<Image>() : null;
        card._nameText = nameText;
        card._rootText = rootText;
        card._powerText = powerText;
        card._realmText = realmText;
        card._extraText = extraText;
        cardObj.SetActive(false);
        _prefab = cardObj;
        return _prefab;
    }

    internal void Setup(MclslRankEntry entry, int rank)
    {
        _actor = entry?.Actor;
        _actorId = entry?.ActorId ?? 0L;
        EnsureRefs();
        if (entry == null) return;

        if (_rankText != null)
        {
            _rankText.text = (rank + 1).ToString();
            _rankText.color = rank switch
            {
                0 => new Color(1f, 0.84f, 0f),
                1 => new Color(0.75f, 0.75f, 0.75f),
                2 => new Color(0.8f, 0.5f, 0.2f),
                _ => Color.white
            };
        }
        if (_avatarElement != null && entry.Actor != null)
        {
            _avatarElement.show(entry.Actor);
            if (_avatarElement.kingdomBanner != null) _avatarElement.kingdomBanner.gameObject.SetActive(false);
            if (_avatarElement.clanBanner != null) _avatarElement.clanBanner.gameObject.SetActive(false);
        }
        else if (_avatarImage != null && entry.Actor?.asset != null)
        {
            _avatarImage.sprite = entry.Actor._last_colored_sprite ?? entry.Actor.asset.getSpriteIcon();
        }

        if (_nameText != null) _nameText.text = Short(entry.Name, 12);
        if (_rootText != null) _rootText.text = Short(entry.RootText, 4);
        if (_powerText != null) _powerText.text = FormatPower(entry.Power);
        if (_realmText != null) _realmText.text = Short(entry.RealmName, 5);
        if (_extraText != null) _extraText.text = Short(entry.ExtraText, 4);

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(new UnityAction(OnClick));
        }
    }

    private static Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject obj = new(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = obj.AddComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        return text;
    }

    private static UiUnitAvatarElement CreateAvatarElement(Transform parent)
    {
        UiUnitAvatarElement prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
        if (prefab != null)
        {
            UiUnitAvatarElement avatar = Instantiate(prefab, parent);
            RectTransform rect = avatar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(30f, 0f);
            rect.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            avatar.show_banner_kingdom = false;
            avatar.show_banner_clan = false;
            if (avatar.kingdomBanner != null) avatar.kingdomBanner.gameObject.SetActive(false);
            if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
            return avatar;
        }

        GameObject avatarObj = new("Avatar");
        avatarObj.transform.SetParent(parent, false);
        RectTransform avatarRect = avatarObj.AddComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 0.5f);
        avatarRect.anchorMax = new Vector2(0f, 0.5f);
        avatarRect.pivot = new Vector2(0f, 0.5f);
        avatarRect.anchoredPosition = new Vector2(30f, 0f);
        avatarRect.sizeDelta = new Vector2(30f, 30f);
        avatarObj.AddComponent<Image>().color = Color.white;
        return null;
    }

    private void EnsureRefs()
    {
        _rankText ??= transform.Find("RankText")?.GetComponent<Text>();
        _avatarElement ??= GetComponentInChildren<UiUnitAvatarElement>();
        _avatarImage ??= transform.Find("Avatar")?.GetComponent<Image>();
        _nameText ??= transform.Find("NameText")?.GetComponent<Text>();
        _rootText ??= transform.Find("RootText")?.GetComponent<Text>();
        _powerText ??= transform.Find("PowerText")?.GetComponent<Text>();
        _realmText ??= transform.Find("RealmText")?.GetComponent<Text>();
        _extraText ??= transform.Find("ExtraText")?.GetComponent<Text>();
    }

    private void OnClick()
    {
        Actor target = FindAliveActorById(_actorId) ?? _actor;
        if (target?.data == null) return;
        try { ActionLibrary.openUnitWindow(target); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankCard-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankCard.cs #1: " + mclslEmptyCatchEx.Message); }
    }

    private void OnDisable()
    {
        _actor = null;
        _actorId = 0L;
    }

    private static Actor FindAliveActorById(long actorId)
    {
        if (actorId <= 0L) return null;
        return MclslCultivatorCandidateIndex.Resolve(actorId, out Actor actor) ? actor : null;
    }

    private static string FormatPower(double power)
    {
        if (power <= 0d) return "凡人";
        if (power >= 1000000000000d) return (power / 1000000000000d).ToString("F2") + "万亿";
        if (power >= 100000000d) return (power / 100000000d).ToString("F2") + "亿";
        if (power >= 10000d) return (power / 10000d).ToString("F2") + "万";
        return power.ToString("F0");
    }

    private static string Short(string value, int length)
    {
        if (string.IsNullOrWhiteSpace(value)) return "无";
        string text = value.Trim();
        return text.Length <= length ? text : text.Substring(0, length);
    }
}
