using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslBackpackHeader
{
    internal const string BagTabId = "MclslQiankunBagTab";
    internal const string MentorshipTabId = "MclslMentorshipTab";

    internal static void Bind(UnitWindow window)
    {
        if (window == null) return;
        MclslBackpackHeaderDriver driver = window.GetComponent<MclslBackpackHeaderDriver>()
            ?? window.gameObject.AddComponent<MclslBackpackHeaderDriver>();
        driver.Bind(window);
    }

    /// <summary>Bag and mentorship pages share one private copy of the native header.</summary>
    internal static void RegisterCustomTab(UnitWindow window, string tabName)
    {
        if (window == null || string.IsNullOrEmpty(tabName)) return;
        window.GetComponent<MclslBackpackHeaderDriver>()?.RegisterTab(tabName);
    }

    /// <summary>Refreshes the header that belongs to this exact window instance.</summary>
    internal static void RefreshSlots(UnitWindow window)
    {
        if (window == null) return;
        window.GetComponent<MclslBackpackHeaderDriver>()?.RefreshSlots();
    }
}

/// <summary>Displays the six mod artifact slots in a private copy of the native character header.</summary>
public sealed class MclslBackpackHeaderDriver : MonoBehaviour
{
    private const string HeaderPath = "Background/Scroll View/Viewport/Header";
    private const string CloneName = "MclslQiankunHeader";
    private const float SlotSize = 18f;
    private readonly List<Transform> _rowCopies = new();
    private readonly List<GameObject> _slotIcons = new();
    private readonly HashSet<string> _customTabs = new(StringComparer.Ordinal);
    private UnitWindow _window;
    private Transform _original;
    private Transform _clone;
    private Transform _grid;
    private UiUnitAvatarElement _avatar;
    private NameInput _nameInput;
    private StatsIcon _ageIcon;
    private CanvasGroup _originalGroup;
    private float _originalAlpha = 1f;
    private bool _originalInteractable = true;
    private bool _originalBlocksRaycasts = true;
    private long _lastActorId = long.MinValue;
    private long _lastRevision = long.MinValue;
    private float _lastAge = float.MinValue;
    private string _lastName;
    private long _slotsActorId = long.MinValue;
    private long _slotsRevision = long.MinValue;
    private string _slotsKey = string.Empty;
    private bool _showingCustom;
    private bool _callbacksHooked;

    internal void RegisterTab(string tabName)
    {
        if (!string.IsNullOrEmpty(tabName)) _customTabs.Add(tabName);
    }

    internal void Bind(UnitWindow window)
    {
        if (window == null) return;
        _window = window;
        _customTabs.Add(MclslBackpackHeader.BagTabId);
        _customTabs.Add(MclslBackpackHeader.MentorshipTabId);
        if (_clone == null && !BuildClone(window)) return;
        HookCallbacks();
        SyncState(force: true);
    }

    /// <summary>
    /// Copies the header while it still sits under an inactive container, so no cloned
    /// component can run Awake/OnEnable first. The native equipment behaviour is removed
    /// there, before the copy is attached and shown.
    /// </summary>
    private bool BuildClone(UnitWindow window)
    {
        Transform original = window.transform.Find(HeaderPath);
        if (original == null) return false;
        UnitEquipmentContainer sourceEquipment = original.GetComponentInChildren<UnitEquipmentContainer>(true);
        if (sourceEquipment == null || sourceEquipment._grid == null) return false;

        GameObject staging = new("MclslHeaderStaging");
        staging.transform.SetParent(window.transform, false);
        staging.SetActive(false);
        Transform clone = null;
        try
        {
            GameObject copy = Instantiate(original.gameObject, staging.transform);
            copy.name = CloneName;
            copy.SetActive(false);

            UnitEquipmentContainer clonedEquipment = copy.GetComponentInChildren<UnitEquipmentContainer>(true);
            Transform grid = clonedEquipment != null ? clonedEquipment._grid : null;
            if (clonedEquipment != null)
            {
                clonedEquipment.StopAllCoroutines();
                DestroyImmediate(clonedEquipment);
            }
            if (grid == null) return false;
            for (int i = grid.childCount - 1; i >= 0; i--)
            {
                Transform child = grid.GetChild(i);
                if (child == null || child.name == "Title") continue;
                DestroyImmediate(child.gameObject);
            }
            _grid = grid;
            _avatar = copy.GetComponentInChildren<UiUnitAvatarElement>(true);
            _nameInput = copy.GetComponentInChildren<NameInput>(true);
            _ageIcon = FindChild<StatsIcon>(copy.transform, "i_age");

            _rowCopies.Clear();
            CollectHeaderRows(original, copy.transform);

            Transform parent = original.parent;
            copy.transform.SetParent(parent, false);
            copy.transform.localPosition = original.localPosition;
            copy.transform.localRotation = original.localRotation;
            copy.transform.localScale = original.localScale;
            if (parent != null)
                copy.transform.SetSiblingIndex(Mathf.Min(original.GetSiblingIndex() + 1, parent.childCount - 1));
            clone = copy.transform;
        }
        finally
        {
            Destroy(staging);
        }

        if (clone == null) return false;
        _original = original;
        _clone = clone;
        // Hide the native header non-destructively (alpha only) so whichever native page
        // becomes active again keeps full control of its own rows' active state.
        _originalGroup = original.GetComponent<CanvasGroup>();
        if (_originalGroup == null) _originalGroup = original.gameObject.AddComponent<CanvasGroup>();
        _originalAlpha = _originalGroup.alpha;
        _originalInteractable = _originalGroup.interactable;
        _originalBlocksRaycasts = _originalGroup.blocksRaycasts;
        _slotsActorId = long.MinValue;
        _slotsRevision = long.MinValue;
        _slotsKey = string.Empty;
        return true;
    }

    private bool IsCustomTab()
    {
        WindowMetaTab active = _window?.scroll_window?.tabs?.getActiveTab();
        if (active == null || !active.gameObject.activeSelf) return false;
        string name = active.name;
        if (string.IsNullOrEmpty(name)) return false;
        return name == MclslBackpackHeader.BagTabId
            || name == MclslBackpackHeader.MentorshipTabId
            || _customTabs.Contains(name);
    }

    /// <summary>
    /// Hides the native header without touching its active state: only the CanvasGroup is
    /// faded. Native tab_elements activation on the way back is therefore never overridden.
    /// </summary>
    private void HideOriginalVisual()
    {
        if (_originalGroup == null) return;
        _originalGroup.alpha = 0f;
        _originalGroup.interactable = false;
        _originalGroup.blocksRaycasts = false;
    }

    private void RestoreOriginalVisual()
    {
        if (_originalGroup == null) return;
        _originalGroup.alpha = _originalAlpha;
        _originalGroup.interactable = _originalInteractable;
        _originalGroup.blocksRaycasts = _originalBlocksRaycasts;
    }

    private void HookCallbacks()
    {
        if (_callbacksHooked) return;
        WindowMetaTabButtonsContainer tabs = _window?.scroll_window?.tabs;
        if (tabs == null) return;
        tabs.addTabShowCallback(OnTabShown);
        tabs.addTabHideCallback(OnTabHidden);
        _callbacksHooked = true;
    }

    private void UnhookCallbacks()
    {
        if (!_callbacksHooked) return;
        _callbacksHooked = false;
        WindowMetaTabButtonsContainer tabs = _window?.scroll_window?.tabs;
        if (tabs == null) return;
        tabs.removeTabShowCallback(OnTabShown);
        tabs.removeTabHideCallback(OnTabHidden);
    }

    private void OnTabShown(WindowMetaTab tab) => SyncState(force: true);

    private void OnTabHidden() => SyncState(force: false);

    private void SyncState(bool force)
    {
        if (_window == null || _clone == null || _original == null) return;
        if (!_window.gameObject.activeInHierarchy) return;
        bool custom = IsCustomTab();
        if (force || custom != _showingCustom) ApplyPanel(custom);
        if (!custom) return;
        if (!_clone.gameObject.activeSelf) _clone.gameObject.SetActive(true);
        RefreshActor(force: false);
    }

    /// <summary>
    /// Shows the private copy on the mod pages, then hands the native header back to the
    /// rules of whichever native page becomes active again.
    /// </summary>
    private void ApplyPanel(bool custom)
    {
        if (_clone == null || _original == null) return;
        if (custom)
        {
            bool wasShown = _showingCustom && _clone.gameObject.activeSelf;
            _showingCustom = true;
            HideOriginalVisual();
            if (!_clone.gameObject.activeSelf) _clone.gameObject.SetActive(true);
            // The mod pages have no native rule for these rows, so the copy shows the
            // shared top status itself. Native pages are never touched here.
            ShowSharedRows();
            // Re-entering the copy after it was hidden must re-show the avatar: the cloned
            // avatar clears itself on OnDisable. Only the actor/name/age cache is reset, so
            // the shared equipment grid is not needlessly rebuilt on every page switch.
            if (!wasShown) InvalidateActorCache();
            return;
        }

        if (_clone.gameObject.activeSelf) _clone.gameObject.SetActive(false);
        if (_showingCustom)
        {
            _showingCustom = false;
            // Restore only the fade; native showTab already re-applied its own row rules.
            RestoreOriginalVisual();
        }
        InvalidateActorCache();
    }

    private void InvalidateActorCache()
    {
        _lastActorId = long.MinValue;
        _lastRevision = long.MinValue;
        _lastAge = float.MinValue;
        _lastName = null;
    }

    /// <summary>
    /// Refreshes the shared top rows for the current actor. The header is refreshed in
    /// place and never rebuilds the mentorship list; the equipment grid is only rebuilt
    /// when the actor or the bag revision actually changes.
    /// </summary>
    private void RefreshActor(bool force)
    {
        Actor actor = _window?.actor;
        if (actor?.data == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        long revision = MclslBagSystem.Revision(actor);
        float age = actor.getAge();
        string name = (actor.getName() ?? string.Empty).Trim();
        bool actorChanged = force || actorId != _lastActorId;
        bool ageChanged = force || age != _lastAge;
        bool nameChanged = force || !string.Equals(name, _lastName, StringComparison.Ordinal);
        bool revisionChanged = force || revision != _lastRevision;
        if (!actorChanged && !ageChanged && !nameChanged && !revisionChanged) return;
        _lastActorId = actorId;
        _lastRevision = revision;
        _lastAge = age;
        _lastName = name;

        ShowSharedRows();
        if (_avatar != null && actorChanged) _avatar.show(actor);
        if (_nameInput != null && (actorChanged || nameChanged)) _nameInput.setText(name);
        if (_ageIcon != null)
        {
            _ageIcon.gameObject.SetActive(true);
            if (actorChanged || ageChanged) _ageIcon.setValue(age);
        }
        // RefreshSlots has its own idempotence key, so calling it here cannot thrash the grid.
        if (actorChanged || revisionChanged) RefreshSlots();
    }

    private void Update()
    {
        if (_window == null || _clone == null || !_window.gameObject.activeInHierarchy) return;
        bool custom = IsCustomTab();
        if (custom != _showingCustom) ApplyPanel(custom);
        if (!custom) return;
        if (!_clone.gameObject.activeSelf) _clone.gameObject.SetActive(true);
        RefreshActor(force: false);
    }

    internal void RefreshSlots()
    {
        if (_grid == null || _clone == null || !_clone.gameObject.activeSelf) return;
        Actor actor = _window?.actor;
        if (actor?.data == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        long revision = MclslBagSystem.Revision(actor);
        string signature = SlotSignature(actor);
        if (actorId == _slotsActorId && revision == _slotsRevision
            && string.Equals(signature, _slotsKey, StringComparison.Ordinal)) return;
        _slotsActorId = actorId;
        _slotsRevision = revision;
        _slotsKey = signature;
        _lastRevision = revision;

        ClearSlotIcons();
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
                Actor current = _window?.actor;
                if (current?.data == null || !MclslArtifactSystem.TryUnequip(current, selectedSlot)) return;
                _lastRevision = MclslBagSystem.Revision(current);
                RefreshSlots();
                _window.GetComponentInChildren<MclslBackpackTabElement>(true)?.RefreshVisible();
            });
            TipButton tip = icon.GetComponent<TipButton>();
            tip.textOnClick = MclslLocalizationBridge.RuntimeText(item.Name);
            tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(item.EffectText + "\n耐久 " + owned.Durability + "%\n点击卸下。");
            _slotIcons.Add(icon);
        }
    }

    /// <summary>Cheap key of what the private equipment grid would display.</summary>
    private static string SlotSignature(Actor actor)
    {
        string key = string.Empty;
        foreach (MclslArtifactEquipmentSlot slot in MclslArtifactSystem.EquipmentSlots)
        {
            MclslOwnedItem owned = MclslArtifactSystem.EquippedArtifact(actor, slot);
            key += owned == null
                ? slot + "=-;"
                : slot + "=" + owned.InstanceId + ":" + owned.ItemId + ":" + owned.Durability + ";";
        }
        return key;
    }

    private void ClearSlotIcons()
    {
        foreach (GameObject icon in _slotIcons)
        {
            if (icon == null) continue;
            icon.SetActive(false);
            Destroy(icon);
        }
        _slotIcons.Clear();
    }

    /// <summary>
    /// Shows the copy's own rows: the top status rows the default page owns, plus the
    /// avatar, name, age and the mod backpack area. The native header itself is never
    /// re-activated here, so native pages keep their own rules.
    /// </summary>
    private void ShowSharedRows()
    {
        for (int i = 0; i < _rowCopies.Count; i++)
        {
            Transform copy = _rowCopies[i];
            if (copy == null) continue;
            ActivateChain(copy);
        }
        ActivateChain(_grid);
        if (_avatar != null) ActivateChain(_avatar.transform);
        if (_nameInput != null) ActivateChain(_nameInput.transform);
        if (_ageIcon != null)
        {
            ActivateChain(_ageIcon.transform);
            _ageIcon.gameObject.SetActive(true);
        }
        if (_clone != null && !_clone.gameObject.activeSelf) _clone.gameObject.SetActive(true);
    }

    private void CollectHeaderRows(Transform original, Transform copy)
    {
        WindowMetaTab main = _window?.scroll_window?.tabs?.tab_default;
        if (main?.tab_elements == null) return;
        foreach (Transform row in main.tab_elements)
        {
            if (row == null || (row != original && !row.IsChildOf(original))) continue;
            Stack<int> path = new();
            Transform node = row;
            while (node != null && node != original)
            {
                path.Push(node.GetSiblingIndex());
                node = node.parent;
            }
            if (node != original) continue;
            Transform mapped = copy;
            while (path.Count > 0 && mapped != null)
            {
                int index = path.Pop();
                mapped = index < mapped.childCount ? mapped.GetChild(index) : null;
            }
            if (mapped == null || _rowCopies.Contains(mapped)) continue;
            _rowCopies.Add(mapped);
        }
    }

    private void ActivateChain(Transform row)
    {
        for (Transform current = row; current != null; current = current.parent)
        {
            current.gameObject.SetActive(true);
            if (current == _clone) return;
        }
    }

    private static T FindChild<T>(Transform root, string name) where T : Component
    {
        foreach (T component in root.GetComponentsInChildren<T>(true))
            if (component.name == name) return component;
        return null;
    }

    private void OnEnable()
    {
        if (_window == null) return;
        HookCallbacks();
        InvalidateActorCache();
        SyncState(force: true);
    }

    private void OnDisable()
    {
        if (_clone != null) _clone.gameObject.SetActive(false);
        // Closing the window must not leave the native header faded out behind the copy.
        RestoreOriginalVisual();
        _showingCustom = false;
        InvalidateActorCache();
    }

    private void OnDestroy()
    {
        UnhookCallbacks();
        // If the window is destroyed while its header is faded, put the native CanvasGroup
        // back to its captured values. activeSelf of the root/rows is never forced here.
        RestoreOriginalVisual();
        ClearSlotIcons();
        _rowCopies.Clear();
        _customTabs.Clear();
        _slotsActorId = long.MinValue;
        _slotsRevision = long.MinValue;
        _slotsKey = string.Empty;
    }
}
