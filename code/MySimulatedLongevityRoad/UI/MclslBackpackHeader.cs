using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslBackpackHeader
{
    internal static void Bind(UnitWindow window)
    {
        if (window == null) return;
        MclslBackpackHeaderDriver driver = window.GetComponent<MclslBackpackHeaderDriver>()
            ?? window.gameObject.AddComponent<MclslBackpackHeaderDriver>();
        driver.Bind(window);
    }

    internal static void RefreshSlots()
    {
        MclslBackpackHeaderDriver.Current?.RefreshSlots();
    }
}

/// <summary>Displays the six mod artifact slots in a private copy of the native character header.</summary>
public sealed class MclslBackpackHeaderDriver : MonoBehaviour
{
    private const string HeaderPath = "Background/Scroll View/Viewport/Header";
    private const string TabId = "MclslQiankunBagTab";
    private readonly List<Transform> _mainRows = new();
    private readonly List<GameObject> _slotIcons = new();
    private const float SlotSize = 18f;
    private UnitWindow _window;
    private Transform _original;
    private Transform _clone;
    private Transform _grid;
    private UiUnitAvatarElement _avatar;
    private NameInput _nameInput;
    private StatsIcon _ageIcon;
    private Actor _lastActor;
    private bool _showing;

    internal static MclslBackpackHeaderDriver Current { get; private set; }

    internal void Bind(UnitWindow window)
    {
        _window = window;
        Current = this;
        if (_clone != null) return;
        _original = window.transform.Find(HeaderPath);
        UnitEquipmentContainer equipment = _original?.GetComponentInChildren<UnitEquipmentContainer>(true);
        if (equipment == null || equipment._grid == null) return;

        GameObject clone = Instantiate(_original.gameObject, _original.parent);
        clone.SetActive(false);
        clone.name = "MclslQiankunHeader";
        clone.transform.SetSiblingIndex(_original.GetSiblingIndex() + 1);
        _clone = clone.transform;
        CollectHeaderRows(window);

        UnitEquipmentContainer clonedEquipment = clone.GetComponentInChildren<UnitEquipmentContainer>(true);
        _grid = clonedEquipment._grid;
        clonedEquipment.StopAllCoroutines();
        DestroyImmediate(clonedEquipment);
        for (int i = _grid.childCount - 1; i >= 0; i--)
        {
            Transform child = _grid.GetChild(i);
            if (child.name == "Title") continue;
            DestroyImmediate(child.gameObject);
        }

        _avatar = clone.GetComponentInChildren<UiUnitAvatarElement>(true);
        _nameInput = clone.GetComponentInChildren<NameInput>(true);
        _ageIcon = FindChild<StatsIcon>(_clone, "i_age");
        clone.SetActive(false);
        window.scroll_window?.tabs?.addTabShowCallback(OnTabShown);
        window.scroll_window?.tabs?.addTabHideCallback(OnTabHidden);
        ApplyTab(IsBackpackTab());
    }

    private bool IsBackpackTab() => _window?.scroll_window?.tabs?.getActiveTab()?.name == TabId;

    private void OnTabShown(WindowMetaTab tab) => ApplyTab(tab != null && tab.name == TabId);

    private void OnTabHidden() => ApplyTab(false);

    private void ApplyTab(bool selected)
    {
        if (_clone == null || _original == null) return;
        _showing = selected;
        _clone.gameObject.SetActive(selected);
        _original.gameObject.SetActive(!selected);
        _lastActor = null;
    }

    private void Update()
    {
        if (_window == null || _clone == null || !_window.gameObject.activeInHierarchy) return;
        bool selected = IsBackpackTab();
        if (selected != _showing) ApplyTab(selected);
        if (!selected)
        {
            if (_clone.gameObject.activeSelf) _clone.gameObject.SetActive(false);
            if (!_original.gameObject.activeSelf) _original.gameObject.SetActive(true);
            return;
        }
        if (!_clone.gameObject.activeSelf) _clone.gameObject.SetActive(true);
        if (_original.gameObject.activeSelf) _original.gameObject.SetActive(false);
        Actor actor = _window.actor;
        if (actor?.data == null || actor == _lastActor) return;
        _lastActor = actor;
        foreach (Transform row in _mainRows) ActivateChain(row);
        ActivateChain(_grid);
        if (_avatar != null) _avatar.show(actor);
        if (_nameInput != null) _nameInput.setText(actor.getName().Trim());
        if (_ageIcon != null) _ageIcon.setValue(actor.getAge());
        RefreshSlots();
    }

    internal void RefreshSlots()
    {
        if (_grid == null || _window?.actor?.data == null || !IsBackpackTab()) return;
        foreach (GameObject icon in _slotIcons)
        {
            if (icon == null) continue;
            icon.SetActive(false);
            icon.transform.SetParent(null, false);
            Destroy(icon);
        }
        _slotIcons.Clear();
        Actor actor = _window.actor;
        foreach (MclslArtifactEquipmentSlot slot in MclslArtifactSystem.EquipmentSlots)
        {
            MclslOwnedItem owned = MclslArtifactSystem.EquippedArtifact(actor, slot);
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item == null) continue;
            GameObject icon = new("MclslSlot_" + slot, typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button), typeof(TipButton));
            icon.transform.SetParent(_grid, false);
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(SlotSize, SlotSize);
            LayoutElement layout = icon.GetComponent<LayoutElement>();
            layout.preferredWidth = SlotSize;
            layout.preferredHeight = SlotSize;
            Image image = icon.GetComponent<Image>();
            image.sprite = SpriteTextureLoader.getSprite(item.IconPath);
            image.preserveAspect = true;
            image.color = Color.white;
            Button button = icon.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            MclslArtifactEquipmentSlot selectedSlot = slot;
            button.onClick.AddListener(() =>
            {
                if (!MclslArtifactSystem.TryUnequip(actor, selectedSlot)) return;
                RefreshSlots();
                _window.GetComponentInChildren<MclslBackpackTabElement>(true)?.RefreshVisible();
            });
            TipButton tip = icon.GetComponent<TipButton>();
            tip.textOnClick = MclslLocalizationBridge.RuntimeText(item.Name);
            tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(item.EffectText + "\n耐久 " + owned.Durability + "%\n点击卸下。");
            _slotIcons.Add(icon);
        }
    }

    private void CollectHeaderRows(UnitWindow window)
    {
        WindowMetaTab main = window.scroll_window?.tabs?.tab_default;
        if (main?.tab_elements == null) return;
        foreach (Transform row in main.tab_elements)
        {
            if (row == null || (row != _original && !row.IsChildOf(_original))) continue;
            Stack<int> path = new();
            Transform node = row;
            while (node != _original)
            {
                path.Push(node.GetSiblingIndex());
                node = node.parent;
            }
            Transform mapped = _clone;
            while (path.Count > 0 && mapped != null)
            {
                int index = path.Pop();
                mapped = index < mapped.childCount ? mapped.GetChild(index) : null;
            }
            if (mapped != null) _mainRows.Add(mapped);
        }
    }

    private void ActivateChain(Transform row)
    {
        for (Transform current = row; current != null && current != _clone.parent; current = current.parent)
        {
            current.gameObject.SetActive(true);
            if (current == _clone) break;
        }
    }

    private static T FindChild<T>(Transform root, string name) where T : Component
    {
        foreach (T component in root.GetComponentsInChildren<T>(true))
            if (component.name == name) return component;
        return null;
    }

    private void OnDisable()
    {
        if (_original != null) _original.gameObject.SetActive(true);
        if (_clone != null) _clone.gameObject.SetActive(false);
        _showing = false;
        _lastActor = null;
    }

    private void OnDestroy()
    {
        if (_window?.scroll_window?.tabs != null)
        {
            _window.scroll_window.tabs.removeTabShowCallback(OnTabShown);
            _window.scroll_window.tabs.removeTabHideCallback(OnTabHidden);
        }
        if (Current == this) Current = null;
    }
}
