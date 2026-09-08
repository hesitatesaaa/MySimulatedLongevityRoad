using System;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslTraitEditorEraFilter
{
    private const string NewLawRealmTitle = "新法境界";
    private const string AncientRealmTitle = "仙道境界";
    private const int ActiveEditorRefreshIntervalFrames = 30;
    private static int _lastActiveEditorRefreshFrame = -9999;

    internal static void Apply(ActorTraitsEditor editor)
    {
        if (editor == null) return;

        int year = MclslRuntime.CurrentYear();
        bool showNewLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        MclslTraitRegistration.RefreshRealmTraitVisibility(year, true);

        ApplyRealmGroupVisibility(editor, showNewLaw);
        ApplyRealmButtonVisibility(editor, year);
    }

    internal static void RefreshActiveEditorsThrottled(int frameCount)
    {
        if (frameCount - _lastActiveEditorRefreshFrame < ActiveEditorRefreshIntervalFrames) return;
        _lastActiveEditorRefreshFrame = frameCount;
        ActorTraitsEditor[] editors;
        try { editors = Resources.FindObjectsOfTypeAll<ActorTraitsEditor>(); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("trait-editor-find-active-editors", "查找特质编辑器失败: " + ex.Message);
            return;
        }
        if (editors == null || editors.Length == 0) return;
        for (int i = 0; i < editors.Length; i++)
        {
            ActorTraitsEditor editor = editors[i];
            if (editor == null || !editor.gameObject.activeInHierarchy) continue;
            Apply(editor);
        }
    }

    private static void ApplyRealmButtonVisibility(ActorTraitsEditor editor, int year)
    {
        ActorTraitEditorButton[] buttons = editor.GetComponentsInChildren<ActorTraitEditorButton>(true);
        if (buttons == null || buttons.Length == 0) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            ActorTraitEditorButton button = buttons[i];
            ActorTrait trait = TryGetTrait(button);
            if (trait == null || !MclslTraitRegistration.IsEraRealmTrait(trait.id)) continue;

            bool shouldShow = MclslTraitRegistration.ShouldShowRealmTraitInEditor(trait.id, year);
            GameObject obj = button.gameObject;
            if (obj != null && obj.activeSelf != shouldShow)
                obj.SetActive(shouldShow);
        }
    }

    private static void ApplyRealmGroupVisibility(ActorTraitsEditor editor, bool showNewLaw)
    {
        Transform editorRoot = editor.transform;
        GameObject newLawRoot = FindRealmGroupRootFromButtons(editorRoot, true);
        GameObject ancientRoot = FindRealmGroupRootFromButtons(editorRoot, false);

        if (newLawRoot != null && newLawRoot != ancientRoot && newLawRoot.activeSelf != showNewLaw)
            newLawRoot.SetActive(showNewLaw);
        if (ancientRoot != null && ancientRoot != newLawRoot && ancientRoot.activeSelf == showNewLaw)
            ancientRoot.SetActive(!showNewLaw);

        ApplyRealmTitleVisibilityFallback(editor, showNewLaw);
    }

    private static void ApplyRealmTitleVisibilityFallback(ActorTraitsEditor editor, bool showNewLaw)
    {
        Text[] texts = editor.GetComponentsInChildren<Text>(true);
        if (texts == null || texts.Length == 0) return;

        Transform editorRoot = editor.transform;
        for (int i = 0; i < texts.Length; i++)
        {
            Text text = texts[i];
            string value = text == null ? string.Empty : (text.text ?? string.Empty).Trim();
            bool isNewLawTitle = value.Contains(NewLawRealmTitle);
            bool isAncientTitle = value.Contains(AncientRealmTitle);
            if (!isNewLawTitle && !isAncientTitle) continue;

            bool shouldShow = isNewLawTitle ? showNewLaw : !showNewLaw;
            GameObject groupRoot = FindRealmGroupRoot(editorRoot, text.transform, isNewLawTitle);
            if (groupRoot != null && groupRoot.activeSelf != shouldShow)
                groupRoot.SetActive(shouldShow);
            else if (groupRoot == null && text.gameObject != null && text.gameObject.activeSelf != shouldShow)
                text.gameObject.SetActive(shouldShow);
        }
    }

    private static GameObject FindRealmGroupRootFromButtons(Transform editorRoot, bool newLawGroup)
    {
        if (editorRoot == null) return null;
        ActorTraitEditorButton[] buttons = editorRoot.GetComponentsInChildren<ActorTraitEditorButton>(true);
        if (buttons == null || buttons.Length == 0) return null;

        for (int i = 0; i < buttons.Length; i++)
        {
            ActorTrait trait = TryGetTrait(buttons[i]);
            if (trait == null) continue;
            bool isTarget = newLawGroup
                ? MclslTraitRegistration.IsNewLawRealmTrait(trait.id)
                : MclslTraitRegistration.IsAncientRealmTrait(trait.id);
            if (!isTarget) continue;

            GameObject root = FindRealmGroupRoot(editorRoot, buttons[i].transform, newLawGroup);
            if (root != null) return root;
        }

        return null;
    }

    private static GameObject FindRealmGroupRoot(Transform editorRoot, Transform start, bool newLawGroup)
    {
        Transform current = start;
        GameObject best = null;
        int guard = 0;
        while (current != null && guard++ < 16)
        {
            CountRealmButtons(current, out int newLawCount, out int ancientCount);
            bool containsTargetOnly = newLawGroup
                ? newLawCount > 0 && ancientCount == 0
                : ancientCount > 0 && newLawCount == 0;
            if (containsTargetOnly)
                best = current.gameObject;

            if (current == editorRoot || (newLawCount > 0 && ancientCount > 0))
                break;
            current = current.parent;
        }
        return best;
    }

    private static void CountRealmButtons(Transform root, out int newLawCount, out int ancientCount)
    {
        newLawCount = 0;
        ancientCount = 0;
        if (root == null) return;

        ActorTraitEditorButton[] buttons = root.GetComponentsInChildren<ActorTraitEditorButton>(true);
        if (buttons == null || buttons.Length == 0) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            ActorTrait trait = TryGetTrait(buttons[i]);
            if (trait == null) continue;
            if (MclslTraitRegistration.IsNewLawRealmTrait(trait.id)) newLawCount++;
            else if (MclslTraitRegistration.IsAncientRealmTrait(trait.id)) ancientCount++;
        }
    }

    private static ActorTrait TryGetTrait(ActorTraitEditorButton button)
    {
        if (button == null) return null;
        ActorTrait trait = TryResolveTrait(button);
        if (trait != null) return trait;
        try { trait = button.augmentation_button?.getElementAsset(); }
        catch { trait = null; }
        if (trait != null) return trait;
        trait = TryResolveTrait(button.augmentation_button);
        if (trait != null) return trait;
        return TryResolveRealmTraitFromHierarchy(button.transform);
    }

    private static ActorTrait TryResolveRealmTraitFromHierarchy(Transform root)
    {
        if (root == null) return null;
        Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
        if (nodes == null || nodes.Length == 0) return null;

        for (int i = 0; i < nodes.Length; i++)
        {
            Transform node = nodes[i];
            ActorTrait trait = TryResolveKnownRealmTraitId(node?.name);
            if (trait != null) return trait;
        }

        return null;
    }

    private static ActorTrait TryResolveKnownRealmTraitId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        for (int i = 0; i < MclslRealmIds.Ordered.Length; i++)
        {
            string realm = MclslRealmIds.Ordered[i];
            ActorTrait trait = TryResolveKnownTraitId(value, MclslTraitRegistration.TraitIdForRealm(realm));
            if (trait != null) return trait;
            trait = TryResolveKnownTraitId(value, MclslTraitRegistration.LegacyTraitIdForRealm(realm));
            if (trait != null) return trait;
        }
        return null;
    }

    private static ActorTrait TryResolveKnownTraitId(string value, string traitId)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(traitId)) return null;
        if (value.IndexOf(traitId, StringComparison.Ordinal) < 0) return null;
        try { return AssetManager.traits.get(traitId); }
        catch { return null; }
    }

    private static ActorTrait TryResolveTrait(object source)
    {
        if (source == null) return null;
        if (source is ActorTrait direct) return direct;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = source.GetType();
        string[] memberNames =
        {
            "trait", "actor_trait", "actorTrait", "asset", "element_asset", "elementAsset", "pAsset", "m_asset"
        };

        for (int i = 0; i < memberNames.Length; i++)
        {
            string name = memberNames[i];
            try
            {
                FieldInfo field = type.GetField(name, flags);
                ActorTrait trait = CoerceTrait(field?.GetValue(source));
                if (trait != null) return trait;
            }
            catch (Exception ex) { MclslDiagnostics.Error("trait-editor-read-field-" + name, "读取特质按钮字段失败: " + ex.Message); }

            try
            {
                PropertyInfo property = type.GetProperty(name, flags);
                ActorTrait trait = CoerceTrait(property?.GetValue(source));
                if (trait != null) return trait;
            }
            catch (Exception ex) { MclslDiagnostics.Error("trait-editor-read-property-" + name, "读取特质按钮属性失败: " + ex.Message); }
        }

        string[] methodNames = { "getElementAsset", "getAsset", "getTrait", "GetTrait" };
        for (int i = 0; i < methodNames.Length; i++)
        {
            try
            {
                MethodInfo method = type.GetMethod(methodNames[i], flags, null, Type.EmptyTypes, null);
                ActorTrait trait = CoerceTrait(method?.Invoke(source, null));
                if (trait != null) return trait;
            }
            catch (Exception ex) { MclslDiagnostics.Error("trait-editor-read-method-" + methodNames[i], "读取特质按钮方法失败: " + ex.Message); }
        }

        return CoerceTrait(source);
    }

    private static ActorTrait CoerceTrait(object value)
    {
        if (value == null) return null;
        if (value is ActorTrait trait) return trait;

        string id = null;
        try
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            Type type = value.GetType();
            id = type.GetField("id", flags)?.GetValue(value) as string
                ?? type.GetProperty("id", flags)?.GetValue(value) as string;
        }
        catch (Exception ex) { MclslDiagnostics.Error("trait-editor-read-trait-id", "读取特质 ID 失败: " + ex.Message); }

        if (string.IsNullOrWhiteSpace(id)) return null;
        try { return AssetManager.traits.get(id); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("trait-editor-resolve-trait", "解析特质 ID 失败: " + ex.Message);
            return null;
        }
    }
}
