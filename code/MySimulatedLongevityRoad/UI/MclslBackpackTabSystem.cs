using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>Creates a native UnitWindow tab from the game's genealogy page layout.</summary>
internal static class MclslBackpackTabSystem
{
    private const string TabId = "MclslQiankunBagTab";
    private const string ContentId = "content_mclsl_qiankun_bag";
    private static readonly Dictionary<UnitWindow, WindowMetaTab> Tabs = new();
    private const string MentorshipTabId = "MclslMentorshipTab";
    private static readonly Dictionary<UnitWindow, WindowMetaTab> MentorshipTabs = new();

    internal static void Bind(UnitWindow window)
    {
        if (window?.actor?.data == null || window.scroll_window?.tabs == null) return;
        Transform tabsRoot = window.transform.Find("Background/Tabs");
        if (tabsRoot == null) return;
        WindowMetaTab existing = tabsRoot.Find(TabId)?.GetComponent<WindowMetaTab>();
        if (existing != null)
        {
            Tabs[window] = existing;
            MclslBackpackHeader.Bind(window);
            BindMentorship(window, tabsRoot);
            return;
        }

        UnitGenealogyElement template = window.transform.GetComponentInChildren<UnitGenealogyElement>(true);
        Transform contentParent = window.transform.Find("Background/Scroll View/Viewport/Content");
        WindowMetaTab genealogyTab = tabsRoot.Find("Genealogy")?.GetComponent<WindowMetaTab>();
        if (template == null || contentParent == null || genealogyTab == null) return;

        GameObject content = UnityEngine.Object.Instantiate(template, contentParent).gameObject;
        content.SetActive(false);
        content.name = ContentId;
        UnitGenealogyElement source = content.GetComponent<UnitGenealogyElement>();
        Transform equipment = source.transform_grandparents;
        Transform pills = source.transform_parents;
        Transform talismans = source.transform_siblings;
        Transform materials = source.transform_children;
        Image titleSexIcon = typeof(UnitGenealogyElement).GetField("_sex_icon", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as Image;
        UnityEngine.Object.DestroyImmediate(source);
        RemoveTemplateAvatars(content.transform);
        RemoveGenealogyExpanders(content.transform);

        MclslBackpackTabElement element = content.AddComponent<MclslBackpackTabElement>();
        element.Initialize(equipment, pills, talismans, materials);
        MclslLocalizationBridge.RegisterKey("mclsl_bag_artifact_section", "法宝");
        MclslLocalizationBridge.RegisterKey("mclsl_bag_pill_section", "丹药");
        MclslLocalizationBridge.RegisterKey("mclsl_bag_talisman_section", "符箓");
        MclslLocalizationBridge.RegisterKey("mclsl_bag_material_section", "材料");
        MclslLocalizationBridge.RegisterKey("mclsl_bag_book_section", "传承典籍");
        MclslLocalizationBridge.RegisterKey("mclsl_bag_spell_section", "法术");
        MclslLocalizationBridge.RegisterKey("mclsl_qiankun_tab", "乾坤袋");
        SetPageTitle(content.transform, "mclsl_qiankun_tab", "乾坤袋", "ui/Icons/QiankunBagEntrance");
        if (titleSexIcon != null)
        {
            titleSexIcon.sprite = SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance");
            titleSexIcon.preserveAspect = true;
        }
        SetSectionText(equipment, "mclsl_bag_artifact_section");
        SetSectionText(pills, "mclsl_bag_pill_section");
        SetSectionText(talismans, "mclsl_bag_talisman_section");
        SetSectionText(materials, "mclsl_bag_material_section");
        Transform books = CloneSection(materials, "MclslQiankunBooksSection");
        if (books != null)
        {
            element.SetBooksSection(books);
            SetSectionText(books, "mclsl_bag_book_section");
        }
        Transform spells = CloneSection(materials, "MclslQiankunSpellsSection");
        if (spells != null)
        {
            element.SetSpellsSection(spells);
            SetSectionText(spells, "mclsl_bag_spell_section");
        }
        VerticalLayoutGroup vertical = content.GetComponent<VerticalLayoutGroup>();
        if (vertical != null)
        {
            vertical.childControlHeight = true;
            vertical.childControlWidth = false;
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = false;
            vertical.spacing = 6f;
            vertical.childAlignment = TextAnchor.UpperCenter;
        }

        int insertAt = tabsRoot.Find("Genealogy").GetSiblingIndex();
        WindowMetaTab tab = UnityEngine.Object.Instantiate(genealogyTab, tabsRoot);
        tab.name = TabId;
        tab.tab_action = new WindowMetaTabEvent();
        tab.tab_action.AddListener(clicked => window.scroll_window.tabs.showTab(clicked));
        tab.transform.SetSiblingIndex(insertAt);
        tab.container = window.scroll_window.tabs;
        tab.tab_elements.RemoveAll(t => t.name.StartsWith("content_", StringComparison.OrdinalIgnoreCase));
        Sprite icon = SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance");
        if (icon != null)
        {
            PowerButton powerButton = tab.GetComponentInChildren<PowerButton>(true);
            if (powerButton?.icon != null) powerButton.icon.sprite = icon;
            else
            {
                Image img = tab.GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.gameObject.name.Contains("Icon", StringComparison.OrdinalIgnoreCase));
                if (img != null) img.sprite = icon;
            }
        }
        MclslLocalizationBridge.RegisterKey("mclsl_qiankun_tab_description", "查看法宝、丹药、符箓、材料、师门典籍与已学法术；点击物品执行使用或装备操作。");
        TipButton tip = tab.GetComponentInChildren<TipButton>(true);
        if (tip != null)
        {
            tip.textOnClick = "mclsl_qiankun_tab";
            tip.textOnClickDescription = "mclsl_qiankun_tab_description";
        }

        FieldInfo tabsField = typeof(WindowMetaTabButtonsContainer).GetField("_tabs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tabsField?.GetValue(window.scroll_window.tabs) is List<WindowMetaTab> tabs && !tabs.Contains(tab)) tabs.Add(tab);
        window.scroll_window.tabs.addTabContent(tab, content.transform);
        window.scroll_window.tabs.refillTabsWithContent();
        Tabs[window] = tab;
        MclslBackpackHeader.Bind(window);
        BindMentorship(window, tabsRoot);
    }

    internal static void Show(UnitWindow window)
    {
        Bind(window);
        if (window != null && Tabs.TryGetValue(window, out WindowMetaTab tab) && tab != null)
            window.scroll_window.tabs.showTab(tab);
    }

    private static void BindMentorship(UnitWindow window, Transform tabsRoot)
    {
        WindowMetaTab existing = tabsRoot.Find(MentorshipTabId)?.GetComponent<WindowMetaTab>();
        if (existing != null) { MentorshipTabs[window] = existing; return; }
        UnitGenealogyElement template = window.transform.GetComponentInChildren<UnitGenealogyElement>(true);
        Transform contentParent = window.transform.Find("Background/Scroll View/Viewport/Content");
        WindowMetaTab genealogyTab = tabsRoot.Find("Genealogy")?.GetComponent<WindowMetaTab>();
        if (template == null || contentParent == null || genealogyTab == null) return;
        GameObject content = UnityEngine.Object.Instantiate(template, contentParent).gameObject;
        content.name = "content_mclsl_mentorship";
        content.SetActive(false);
        UnitGenealogyElement source = content.GetComponent<UnitGenealogyElement>();
        UiUnitAvatarElement avatarPrefab = source.prefab_avatar;
        Transform students = source.transform_grandparents;
        Transform teacher = source.transform_parents;
        Transform lineage = source.transform_siblings;
        Transform books = source.transform_children;
        Image sexIcon = typeof(UnitGenealogyElement).GetField("_sex_icon", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as Image;
        UnityEngine.Object.DestroyImmediate(source);
        foreach (UiUnitAvatarElement avatar in content.GetComponentsInChildren<UiUnitAvatarElement>(true))
            if (avatar != avatarPrefab) UnityEngine.Object.DestroyImmediate(avatar.gameObject);
        RemoveGenealogyExpanders(content.transform);
        MclslMentorshipTabElement element = content.AddComponent<MclslMentorshipTabElement>();
        element.Initialize(avatarPrefab, sexIcon, ResolveContent(students), ResolveContent(teacher));
        MclslLocalizationBridge.RegisterKey("mclsl_mentor_teacher_section", "老师");
        MclslLocalizationBridge.RegisterKey("mclsl_mentor_students_section", "学生");
        MclslLocalizationBridge.RegisterKey("mclsl_mentorship_tab", "师承");
        SetPageTitle(content.transform, "mclsl_mentorship_tab", "师承", null);
        SetSectionText(students, "mclsl_mentor_students_section");
        SetSectionText(teacher, "mclsl_mentor_teacher_section");
        SectionRoot(lineage)?.gameObject.SetActive(false);
        SectionRoot(books)?.gameObject.SetActive(false);

        WindowMetaTab tab = UnityEngine.Object.Instantiate(genealogyTab, tabsRoot);
        tab.name = MentorshipTabId;
        tab.tab_action = new WindowMetaTabEvent();
        tab.tab_action.AddListener(clicked => window.scroll_window.tabs.showTab(clicked));
        tab.transform.SetSiblingIndex(tabsRoot.Find(TabId) != null ? tabsRoot.Find(TabId).GetSiblingIndex() + 1 : genealogyTab.transform.GetSiblingIndex());
        tab.container = window.scroll_window.tabs;
        tab.tab_elements.RemoveAll(t => t.name.StartsWith("content_", StringComparison.OrdinalIgnoreCase));
        Sprite icon = SpriteTextureLoader.getSprite("ui/Icons/icon_spellbook") ?? SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance");
        if (icon != null)
        {
            PowerButton power = tab.GetComponentInChildren<PowerButton>(true);
            if (power?.icon != null) power.icon.sprite = icon;
        }
        MclslLocalizationBridge.RegisterKey("mclsl_mentorship_tab_description", "查看旧法师父、弟子、功法传授和典籍进度。");
        TipButton tip = tab.GetComponentInChildren<TipButton>(true);
        if (tip != null) { tip.textOnClick = "mclsl_mentorship_tab"; tip.textOnClickDescription = "mclsl_mentorship_tab_description"; }
        FieldInfo tabsField = typeof(WindowMetaTabButtonsContainer).GetField("_tabs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tabsField?.GetValue(window.scroll_window.tabs) is List<WindowMetaTab> tabs && !tabs.Contains(tab)) tabs.Add(tab);
        window.scroll_window.tabs.addTabContent(tab, content.transform);
        window.scroll_window.tabs.refillTabsWithContent();
        MentorshipTabs[window] = tab;
    }

    private static Transform SectionRoot(Transform section)
    {
        if (section == null) return null;
        return section.name.StartsWith("bg_", StringComparison.OrdinalIgnoreCase) ? section : section.parent;
    }

    private static void SetPageTitle(Transform content, string key, string label, string iconPath)
    {
        Transform title = content.Find("tab_title_container_unit");
        if (title == null) return;
        LocalizedText localized = title.GetComponentInChildren<LocalizedText>(true);
        if (localized != null) localized.key = key;
        Text text = localized != null ? localized.GetComponent<Text>() : title.GetComponentInChildren<Text>(true);
        if (text != null) text.text = label;
        if (string.IsNullOrEmpty(iconPath)) return;
        Sprite sprite = SpriteTextureLoader.getSprite(iconPath);
        if (sprite == null) return;
        Image[] sideImages = title.GetComponentsInChildren<Image>(true)
            .Where(image => image.transform != title && image.sprite != null
                && image.rectTransform.rect.width <= 42f && image.rectTransform.rect.height <= 42f)
            .OrderBy(image => image.rectTransform.position.x).ToArray();
        if (sideImages.Length == 0) return;
        sideImages[0].sprite = sprite;
        sideImages[0].preserveAspect = true;
        if (sideImages.Length > 1)
        {
            sideImages[sideImages.Length - 1].sprite = sprite;
            sideImages[sideImages.Length - 1].preserveAspect = true;
        }
    }

    private static void RemoveTemplateAvatars(Transform root)
    {
        foreach (UiUnitAvatarElement avatar in root.GetComponentsInChildren<UiUnitAvatarElement>(true))
            UnityEngine.Object.DestroyImmediate(avatar.gameObject);
    }

    private static void RemoveGenealogyExpanders(Transform root)
    {
        foreach (UnfoldButton expander in root.GetComponentsInChildren<UnfoldButton>(true))
            UnityEngine.Object.DestroyImmediate(expander.gameObject);
    }

    private static Transform CloneSection(Transform templateContent, string name)
    {
        if (templateContent == null) return null;
        Transform root = SectionRoot(templateContent);
        if (root?.parent == null) return null;
        GameObject clone = UnityEngine.Object.Instantiate(root.gameObject, root.parent);
        clone.name = name;
        clone.transform.SetSiblingIndex(root.GetSiblingIndex() + 1);
        // Keep the original genealogy content transform: it already sits inside
        // the framed section and is positioned by the native layout.
        return templateContent == root ? clone.transform : clone.transform.Find(templateContent.name);
    }

    internal static Transform ResolveContent(Transform section)
    {
        if (section == null) return null;
        // Icons and mentor portraits must be children of the prefab's own
        // section content, not of a new grid anchored to the section frame.
        foreach (LayoutGroup old in section.GetComponents<LayoutGroup>())
            if (old is not GridLayoutGroup) UnityEngine.Object.DestroyImmediate(old);
        foreach (ContentSizeFitter fitter in section.GetComponents<ContentSizeFitter>())
            UnityEngine.Object.DestroyImmediate(fitter);
        GridLayoutGroup grid = section.GetComponent<GridLayoutGroup>() ?? section.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(28f, 28f);
        grid.spacing = new Vector2(4f, 4f);
        grid.padding = new RectOffset(2, 2, 2, 2);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 6;
        grid.childAlignment = TextAnchor.UpperLeft;
        return section;
    }

    internal static void SetSectionText(Transform reference, string key)
    {
        if (reference == null) return;
        Transform root = SectionRoot(reference);
        if (root == null) return;
        LocalizedText localized = root.GetComponentInChildren<LocalizedText>(true);
        if (localized != null) localized.key = key;
        else
        {
            Text text = root.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                localized = text.GetComponent<LocalizedText>() ?? text.gameObject.AddComponent<LocalizedText>();
                localized.key = key;
            }
        }
    }

    internal static void FitSection(Transform grid)
    {
        if (grid == null) return;
        int visible = 0;
        foreach (Transform child in grid)
            if (child.gameObject.activeSelf && child.GetComponent<LayoutElement>()?.ignoreLayout != true) visible++;
        int rows = Math.Max(1, (visible + 5) / 6);
        float height = Math.Max(82f, 34f + rows * 32f);
        RectTransform rect = grid as RectTransform;
        if (rect != null) rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rows * 32f + 4f);
        Transform root = SectionRoot(grid);
        if (root == null) return;
        LayoutElement layout = root.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        if (root.parent is RectTransform container) LayoutRebuilder.MarkLayoutForRebuild(container);
    }
}

internal sealed class MclslBackpackTabElement : UnitElement
{
    private Transform _equipment;
    private Transform _pills;
    private Transform _talismans;
    private Transform _materials;
    private Transform _books;
    private Transform _spells;
    private readonly List<GameObject> _icons = new();
    private string _selectedConsumableId = string.Empty;

    internal void Initialize(Transform equipment, Transform pills, Transform talismans, Transform materials)
    {
        _equipment = MclslBackpackTabSystem.ResolveContent(equipment);
        _pills = MclslBackpackTabSystem.ResolveContent(pills);
        _talismans = MclslBackpackTabSystem.ResolveContent(talismans);
        _materials = MclslBackpackTabSystem.ResolveContent(materials);
    }

    internal void SetBooksSection(Transform section) => _books = MclslBackpackTabSystem.ResolveContent(section);
    internal void SetSpellsSection(Transform section) => _spells = MclslBackpackTabSystem.ResolveContent(section);

    public override void Awake() { base.Awake(); }
    public override void OnEnable() { base.OnEnable(); }

    public override IEnumerator showContent()
    {
        ClearIcons();
        if (actor?.data == null) yield break;
        if (MclslBagSystem.IsLocked(actor))
        {
            MclslBackpackTabSystem.SetSectionText(_materials, "mclsl_qiankun_corrupt_warning");
            yield break;
        }
        MclslBackpackTabSystem.SetSectionText(_materials, "mclsl_bag_material_section");
        MclslBagState bag = MclslBagSystem.Read(actor);
        LoadEquipmentAndArtifacts(bag);
        LoadItems(bag, "Pill", _pills, true);
        LoadItems(bag, "Talisman", _talismans, true);
        LoadItems(bag, null, _materials, false);
        LoadBooks(bag);
        LoadSpells(bag);
        MclslBackpackTabSystem.FitSection(_equipment);
        MclslBackpackTabSystem.FitSection(_pills);
        MclslBackpackTabSystem.FitSection(_talismans);
        MclslBackpackTabSystem.FitSection(_materials);
        MclslBackpackTabSystem.FitSection(_books);
        MclslBackpackTabSystem.FitSection(_spells);
        MclslBackpackHeader.RefreshSlots();
        yield break;
    }

    public override void clear()
    {
        ClearIcons();
        base.clear();
    }

    private void LoadEquipmentAndArtifacts(MclslBagState bag)
    {
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item?.Category != "Artifact") continue;
            bool equipped = MclslArtifactSystem.EquippedArtifactId(actor, item.EquipmentSlot) == item.Id;
            string recipe = "\n制作配方：" + MclslItemCatalog.IngredientDisplayName(item.IngredientA)
                + " + " + MclslItemCatalog.IngredientDisplayName(item.IngredientB);
            AddIcon(_equipment, item.IconPath, item.Name + (equipped ? " · 已装备" : " · " + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot)),
                item.EffectText + "\n品阶 " + Grade(item.Grade) + "\n槽位 " + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot)
                    + "\n耐久 " + owned.Durability + "%" + recipe + "\n点击装备或更换。",
                () =>
                {
                    if (equipped) MclslArtifactSystem.TryUnequip(actor, item.EquipmentSlot);
                    else MclslArtifactSystem.TryEquip(actor, owned.InstanceId, replaceExisting: true);
                    Refresh();
                });
        }
    }

    private void LoadItems(MclslBagState bag, string category, Transform parent, bool usable)
    {
        if (parent == null) return;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item == null || (category == null
                    ? item.Category is not ("Plant" or "SpiritObject" or "TalismanMaterial" or "Material")
                    : item.Category != category)) continue;
            string title = item.Name + (owned.Count > 1 ? " ×" + owned.Count : string.Empty);
            string description = item.EffectText + "\n数量 " + owned.Count;
            if (!usable && item.MaterialTier != MclslMaterialTier.None)
                description += "\n材料品阶：" + MaterialGrade(item.MaterialTier)
                    + "\n来源：" + MaterialSources(item.MaterialSources)
                    + (item.PreferredHabitat == MclslMaterialHabitat.None ? string.Empty : "\n偏好环境：" + MaterialHabitat(item.PreferredHabitat));
            if (usable) AddConsumableIcon(parent, item, title, description);
            else AddIcon(parent, item.IconPath, title, description, null);
        }
    }

    private static string MaterialGrade(MclslMaterialTier tier) => tier switch
    {
        MclslMaterialTier.Huang => "黄级",
        MclslMaterialTier.Xuan => "玄级",
        MclslMaterialTier.Di => "地级",
        MclslMaterialTier.Tian => "天级",
        _ => "未定级"
    };

    private static string MaterialSources(MclslMaterialSource sources)
    {
        List<string> values = new();
        if ((sources & MclslMaterialSource.AnnualActivity) != 0) values.Add("日常修炼");
        if ((sources & MclslMaterialSource.Ruin) != 0) values.Add("遗迹探索");
        if ((sources & MclslMaterialSource.Breakthrough) != 0) values.Add("境界突破");
        if ((sources & MclslMaterialSource.Opportunity) != 0) values.Add("机缘事件");
        if ((sources & MclslMaterialSource.Faction) != 0) values.Add("仙盟势力");
        return values.Count == 0 ? "暂无来源" : string.Join("、", values);
    }

    private static string MaterialHabitat(MclslMaterialHabitat habitat)
    {
        List<string> values = new();
        if ((habitat & MclslMaterialHabitat.Woodland) != 0) values.Add("林地");
        if ((habitat & MclslMaterialHabitat.Water) != 0) values.Add("水域");
        if ((habitat & MclslMaterialHabitat.Mountain) != 0) values.Add("山地");
        return string.Join("、", values);
    }

    private void AddConsumableIcon(Transform parent, MclslItemDefinition item, string title, string description)
    {
        GameObject go = new("mclsl_backpack_consumable", typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.preserveAspect = true;
        image.sprite = SpriteTextureLoader.getSprite(item.IconPath) ?? SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance");
        TipButton tip = go.GetComponent<TipButton>();
        tip.textOnClick = MclslLocalizationBridge.RuntimeText(title);
        tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(description + "\n选中后点击图标下方的“使用”按钮。");
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            _selectedConsumableId = item.Id;
            UpdateConsumableSelection();
        });

        GameObject action = new("mclsl_backpack_use", typeof(RectTransform), typeof(Image), typeof(Button));
        action.transform.SetParent(go.transform, false);
        RectTransform actionRect = action.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.08f, 0.02f);
        actionRect.anchorMax = new Vector2(0.92f, 0.38f);
        actionRect.offsetMin = Vector2.zero;
        actionRect.offsetMax = Vector2.zero;
        Image actionImage = action.GetComponent<Image>();
        actionImage.color = new Color(0.10f, 0.10f, 0.10f, 0.92f);
        Button actionButton = action.GetComponent<Button>();
        actionButton.targetGraphic = actionImage;
        actionButton.transition = Selectable.Transition.ColorTint;
        GameObject labelObject = new("label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(action.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 9;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = "使用";
        label.raycastTarget = false;
        actionButton.onClick.AddListener(() =>
        {
            if (MclslItemUseSystem.TryUse(actor, item.Id))
            {
                tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText("已使用一份" + item.Name + "。\n" + item.EffectText);
                Refresh();
            }
            else
            {
                string reason = MclslItemUseSystem.ManualUseFailureReason(actor, item.Id);
                tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(reason + "\n" + description);
                label.text = "条件不足";
            }
        });
        _icons.Add(go);
    }

    private void LoadBooks(MclslBagState bag)
    {
        if (_books == null) return;
        foreach (MclslMentorshipBook book in bag.Books ?? new List<MclslMentorshipBook>())
            AddIcon(_books, "ui/Icons/QiankunBagEntrance", book.TechniqueName,
                "师父：" + book.TeacherName + "\n收徒年份：" + book.StartYear + "\n参悟进度：" + book.Progress + "%", null);
    }

    private void LoadSpells(MclslBagState bag)
    {
        if (_spells == null) return;
        foreach (MclslSpellDefinition spell in MclslSpellSystem.Known(actor))
            AddIcon(_spells, spell.IconPath, spell.Name,
                spell.Description + "\n灵力消耗：" + spell.ManaCost + "\n冷却：" + spell.Cooldown + " 秒", null);
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item?.Category != "SpellScroll") continue;
            AddIcon(_spells, item.IconPath, item.Name + " ×" + owned.Count,
                item.EffectText + "\n点击研习；需要达到对应境界。", () =>
                {
                    if (MclslSpellSystem.TryStudyScroll(actor, item.Id)) Refresh();
                });
        }
    }

    private void AddIcon(Transform parent, string iconPath, string title, string description, Action clicked)
    {
        if (parent == null) return;
        GameObject go = new("mclsl_backpack_icon", typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.preserveAspect = true;
        image.sprite = SpriteTextureLoader.getSprite(iconPath) ?? SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance");
        Button button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        if (clicked != null) button.onClick.AddListener(() => clicked());
        TipButton tip = go.GetComponent<TipButton>();
        tip.textOnClick = MclslLocalizationBridge.RuntimeText(title);
        tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(description);
        _icons.Add(go);
    }

    private void Refresh()
    {
        if (isActiveAndEnabled) StartCoroutine(showContent());
    }

    internal void RefreshVisible() => Refresh();

    private void ClearIcons()
    {
        foreach (GameObject icon in _icons)
        {
            if (icon == null) continue;
            icon.SetActive(false);
            icon.transform.SetParent(null, false);
            UnityEngine.Object.Destroy(icon);
        }
        _icons.Clear();
        _selectedConsumableId = string.Empty;
    }

    private void UpdateConsumableSelection()
    {
        foreach (GameObject icon in _icons)
        {
            if (icon == null || icon.name != "mclsl_backpack_consumable") continue;
            TipButton tip = icon.GetComponent<TipButton>();
            Image image = icon.GetComponent<Image>();
            bool selected = tip != null && tip.textOnClick.StartsWith(
                MclslItemCatalog.Get(_selectedConsumableId)?.Name ?? "\0", StringComparison.Ordinal);
            if (image != null) image.color = selected ? new Color(1f, 0.82f, 0.48f, 1f) : Color.white;
        }
    }

    private static string Grade(int grade) => grade switch
    {
        0 => "凡阶",
        1 => "黄级",
        2 => "玄级",
        3 => "地级",
        4 => "天级",
        _ => "未定品阶"
    };
}

internal sealed class MclslMentorshipTabElement : UnitElement
{
    private UiUnitAvatarElement _avatarPrefab;
    private Image _sexIcon;
    private Transform _students;
    private Transform _teacher;
    private ObjectPoolGenericMono<UiUnitAvatarElement> _studentPool;
    private ObjectPoolGenericMono<UiUnitAvatarElement> _teacherPool;

    internal void Initialize(UiUnitAvatarElement avatarPrefab, Image sexIcon, Transform students, Transform teacher)
    {
        _avatarPrefab = avatarPrefab;
        _sexIcon = sexIcon;
        _students = students;
        _teacher = teacher;
        if (_avatarPrefab != null) _avatarPrefab.gameObject.SetActive(false);
    }

    public override void Awake() { base.Awake(); }
    public override void OnEnable() { base.OnEnable(); }

    public override IEnumerator showContent()
    {
        _studentPool?.clear(true);
        _teacherPool?.clear(true);
        if (actor?.data == null) yield break;
        if (_sexIcon != null)
            _sexIcon.sprite = actor.asset.inspect_sex
                ? SpriteTextureLoader.getSprite(actor.isSexMale() ? "ui/icons/IconMale" : "ui/icons/IconFemale")
                : SpriteTextureLoader.getSprite("ui/Icons/icon_spellbook");
        if (_avatarPrefab == null) yield break;
        _studentPool ??= new ObjectPoolGenericMono<UiUnitAvatarElement>(_avatarPrefab, _students);
        _teacherPool ??= new ObjectPoolGenericMono<UiUnitAvatarElement>(_avatarPrefab, _teacher);
        List<Actor> students = MclslAncientMentorshipSystem.GetLiveStudents(actor);
        foreach (Actor student in students)
            ShowAvatar(_studentPool, student, BuildDescription(student, student));
        if (MclslAncientMentorshipSystem.TryGetTeacher(actor, out Actor teacher))
            ShowAvatar(_teacherPool, teacher, BuildDescription(teacher, actor));
        MclslBackpackTabSystem.FitSection(_students);
        MclslBackpackTabSystem.FitSection(_teacher);
        yield break;
    }

    public override void clear()
    {
        _studentPool?.clear(true);
        _teacherPool?.clear(true);
        if (_avatarPrefab != null) _avatarPrefab.gameObject.SetActive(false);
        base.clear();
    }

    private static string BuildDescription(Actor source, Actor recipient)
    {
        string description = "境界：" + MclslRealmIds.Display(MclslActorAccessor.Realm(source))
            + "\n功法：" + MclslActorAccessor.GetString(source, MclslActorDataKeys.TechniqueName, "未传功法")
            + "\n功法上限：" + MclslRealmIds.Display(MclslTechniqueRealmLimit.MaxRealm(source))
            + "\n法脉：" + MclslActorAccessor.GetInt(source, MclslActorDataKeys.AncientLineageStrength);
        MclslBagState bag = MclslBagSystem.Peek(recipient);
        foreach (MclslMentorshipBook book in bag.Books ?? new List<MclslMentorshipBook>())
            description += "\n" + book.TechniqueName + " · " + book.StartYear + "年收徒 · 参悟" + book.Progress + "%";
        return description;
    }

    private static void ShowAvatar(ObjectPoolGenericMono<UiUnitAvatarElement> pool, Actor related, string description)
    {
        UiUnitAvatarElement avatar = pool.getNext();
        avatar.show_banner_kingdom = false;
        avatar.show_banner_clan = false;
        avatar.show(related);
        TipButton tip = avatar.GetComponent<TipButton>() ?? avatar.gameObject.AddComponent<TipButton>();
        tip.textOnClick = MclslLocalizationBridge.RuntimeText(MclslActorAccessor.DisplayName(related));
        tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(description);
    }
}
