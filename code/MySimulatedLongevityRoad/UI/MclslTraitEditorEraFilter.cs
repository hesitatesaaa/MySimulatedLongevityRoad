using System;
using System.Reflection;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 纪元（新法/仙道）在特质编辑器里的显隐过滤。
///
/// 生命周期由实例组件 <see cref="MclslTraitEditorLifecycle"/> 承担，绑定发生在非泛型的
/// 人物窗口打开完成之后，并且只覆盖本次打开的那个窗口里的编辑器。这里不再给
/// ActorTraitsEditor 挂补丁：它的 OnEnable 是继承来的泛型 TraitsEditor&lt;...&gt;.OnEnable，
/// 给它打补丁等于给所有特质编辑器打补丁，而且它本身没有 OnDisable。
/// </summary>
internal static class MclslTraitEditorEraFilter
{
    private const string NewLawRealmTitle = "新法境界";
    private const string AncientRealmTitle = "仙道境界";
    private const int MaxAuditEntries = 64;
    private const int MaxEmptyScanKeys = 64;
    private static readonly List<MclslTraitEditorLifecycle> Bound = new();
    private static readonly HashSet<string> EmptyScans = new();
    private static readonly List<string> AuditLog = new();
    private static bool _lastNewLaw;

    internal static int BoundCount => Bound.Count;
    internal static IReadOnlyList<string> BindAudit => AuditLog;
    internal static int EmptyScanCount => EmptyScans.Count;
    internal static string LastFailure { get; private set; }

    /// <summary>世界重置入口：解除全部实例绑定，并清掉跨世界的过滤状态。</summary>
    internal static void ClearRuntime()
    {
        ClearBinding();
        EmptyScans.Clear();
        AuditLog.Clear();
        _lastNewLaw = false;
        LastFailure = null;
    }

    /// <summary>世界重置或窗口关闭时调用；不依赖纪元变化。</summary>
    internal static void ClearBinding()
    {
        for (int i = 0; i < Bound.Count; i++)
        {
            MclslTraitEditorLifecycle life = Bound[i];
            if (life != null) life.ResetBinding();
        }
        Bound.Clear();
    }

    /// <summary>只清理该窗口内的绑定，其他窗口的编辑器不受影响。</summary>
    internal static void ReleaseWindow(ScrollWindow window)
    {
        if (window == null) return;
        for (int i = 0; i < Bound.Count; i++)
        {
            MclslTraitEditorLifecycle life = Bound[i];
            if (life == null || !life.OwnedBy(window)) continue;
            life.ResetBinding();
        }
        PruneInvalid();
    }

    /// <summary>
    /// 在非泛型 ScrollWindow 打开完成后调用：扫描本次打开的那个窗口（以及可选的相邻作用域），
    /// 为其内部的 ActorTraitsEditor 绑定实例组件。不假设编辑器一定在 UnitWindow 内——
    /// 独立挂在窗口内容下的编辑器同样覆盖；也不做任何全场景查找。
    /// </summary>
    internal static void BindToWindow(ScrollWindow window, Component scope = null)
    {
        if (window == null) return;
        int found = 0;
        found += BindInRoot(window, window);
        if (scope != null && !ReferenceEquals(scope.transform, window.transform)
            && !scope.transform.IsChildOf(window.transform))
            found += BindInRoot(scope, window);
        if (found == 0) NoteEmptyScan(window);
    }

    /// <summary>
    /// 没有编辑器的窗口极常见（背包、图鉴、排行……），这类记录按窗口类型记一次即可，
    /// 不能随着每次开窗无限增长。
    /// </summary>
    private static void NoteEmptyScan(ScrollWindow window)
    {
        string key = window.GetType().FullName + "|" + window.screen_id;
        if (!EmptyScans.Add(key)) return;
        if (EmptyScans.Count > MaxEmptyScanKeys) EmptyScans.Clear();
        AddAudit("scan-empty|window=" + key);
    }

    private static void AddAudit(string entry)
    {
        if (AuditLog.Count >= MaxAuditEntries) AuditLog.Clear();
        AuditLog.Add(entry);
    }

    private static int BindInRoot(Component root, ScrollWindow window)
    {
        ActorTraitsEditor[] editors;
        try { editors = root.GetComponentsInChildren<ActorTraitsEditor>(true); }
        catch (Exception ex) { Fail("window-editor-scan", ex); return 0; }
        if (editors == null || editors.Length == 0) return 0;
        int bound = 0;
        for (int i = 0; i < editors.Length; i++) if (Bind(editors[i], window)) bound++;
        return bound;
    }

    internal static bool Bind(ActorTraitsEditor editor, ScrollWindow owner)
    {
        if (editor == null) return false;
        try
        {
            MclslTraitEditorLifecycle life = editor.GetComponent<MclslTraitEditorLifecycle>();
            if (life == null) life = editor.gameObject.AddComponent<MclslTraitEditorLifecycle>();
            life.Bind(editor, owner);
            Register(life);
            AddAudit("bound|editor=" + editor.GetType().FullName
                + "|window=" + (owner == null ? "none" : owner.GetType().FullName)
                + "|screen=" + (owner == null ? "none" : owner.screen_id)
                + "|insideWindow=" + (owner != null && editor.transform.IsChildOf(owner.transform)));
            return true;
        }
        catch (Exception ex)
        {
            Fail("bind-editor", ex);
            return false;
        }
    }

    internal static void Unregister(MclslTraitEditorLifecycle life)
    {
        if (life == null) return;
        Bound.Remove(life);
    }

    private static void Register(MclslTraitEditorLifecycle life)
    {
        if (life == null || Bound.Contains(life)) return;
        Bound.Add(life);
    }

    /// <summary>
    /// 每帧入口，也是 <see cref="_lastNewLaw"/> 的唯一写入点。失效引用清理无条件先做，
    /// 不挂在纪元变化后面；只有真正的纪元翻转才重新过滤，绝不每帧全场景查找。
    /// </summary>
    internal static void RefreshVisibleEditors()
    {
        PruneInvalid();
        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear());
        bool changed = newLaw != _lastNewLaw;
        // 必须先记录，再由各实例读取自身纪元状态，避免某个实例的 Apply 回写全局状态。
        _lastNewLaw = newLaw;
        if (Bound.Count == 0 || !changed) return;
        for (int i = 0; i < Bound.Count; i++)
        {
            MclslTraitEditorLifecycle life = Bound[i];
            if (life != null) life.MarkEpochPending(newLaw);
        }
    }

    /// <summary>清理已销毁或失去归属的绑定；与纪元是否变化无关，每次调用都执行。</summary>
    internal static int PruneInvalid()
    {
        int removed = 0;
        for (int i = Bound.Count - 1; i >= 0; i--)
        {
            MclslTraitEditorLifecycle life = Bound[i];
            if (life == null) { Bound.RemoveAt(i); removed++; continue; }
            if (life.IsUsable) continue;
            life.ResetBinding();
            Bound.RemoveAt(i);
            removed++;
        }
        return removed;
    }

    /// <summary>
    /// 把当前纪元的显隐规则应用到某个编辑器上；幂等，可重复调用。
    /// 纯读取：绝不回写 <see cref="_lastNewLaw"/>。否则本实例先 Apply 会把全局状态
    /// 推到新纪元，导致其他已打开编辑器在 RefreshVisibleEditors 里被判为"无变化"而漏刷新。
    /// 各实例的纪元记录由 <see cref="MclslTraitEditorLifecycle"/> 自己保存。
    /// </summary>
    internal static bool Apply(ActorTraitsEditor editor)
    {
        if (editor == null) return false;
        int year = MclslRuntime.CurrentYear();
        bool showNewLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        try
        {
            MclslTraitRegistration.RefreshRealmTraitVisibility(year, true);
            ApplyRealmGroupVisibility(editor, showNewLaw);
            ApplyRealmButtonVisibility(editor, year);
            return true;
        }
        catch (Exception ex) { Fail("apply-editor", ex); return false; }
    }

    private static void Fail(string key, Exception exception)
    {
        LastFailure ??= key + ": " + exception.GetType().Name + ": " + exception.Message;
        MclslDiagnostics.Error("trait-editor-" + key, "特质编辑器纪元过滤失败: " + exception.Message);
    }

    /// <summary>面板是否已经完成原生按钮构建：未完成时不得过滤。</summary>
    internal static bool NativeButtonsReady(ActorTraitsEditor editor)
    {
        if (editor == null) return false;
        try
        {
            if (!editor.gameObject.activeInHierarchy) return false;
            if (!editor._groups_initialized) return false;
            return editor.all_augmentation_buttons != null;
        }
        catch (Exception ex) { Fail("native-ready", ex); return false; }
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

/// <summary>
/// 单个 ActorTraitsEditor 的实例级生命周期：绑定 -> 等原生按钮初始化 -> 过滤 ->
/// 关闭/销毁时注销。重开时通过 OnEnable 重新挂起过滤；不做每帧全局查找。
/// </summary>
internal sealed class MclslTraitEditorLifecycle : MonoBehaviour
{
    internal ActorTraitsEditor Editor { get; private set; }
    internal ScrollWindow Owner { get; private set; }

    /// <summary>绑定仍然有效：编辑器存在、组件未被销毁、仍挂在场景里。</summary>
    internal bool IsUsable => Editor != null && Owner != null && this != null && !_destroyed && Editor.gameObject != null;

    /// <summary>只有同一个打开窗口的绑定才由该窗口负责清理。</summary>
    internal bool OwnedBy(ScrollWindow window)
        => window != null && Owner != null && ReferenceEquals(Owner, window);

    private bool _pending;
    private bool _initSeen;
    private bool _destroyed;
    private int _waitedFrames;
    /// <summary>本实例上次成功过滤时使用的纪元；null 表示尚未过滤过。</summary>
    private bool? _appliedNewLaw;

    internal bool? AppliedNewLaw => _appliedNewLaw;

    internal void Bind(ActorTraitsEditor editor, ScrollWindow owner)
    {
        Editor = editor;
        Owner = owner;
        _destroyed = false;
        // 重新绑定等于重新打开：必须按当前纪元过滤，不能沿用上一个绑定的结果。
        _appliedNewLaw = null;
        RequestFilter();
    }

    internal void ResetBinding()
    {
        _pending = false;
        _initSeen = false;
        _waitedFrames = 0;
        _appliedNewLaw = null;
        Owner = null;
    }

    /// <summary>纪元翻转：按新纪元重新过滤一次，但不清除绑定。</summary>
    internal void MarkEpochPending(bool newLaw)
    {
        if (Editor == null) return;
        if (_appliedNewLaw == newLaw) return;
        RequestFilter();
    }

    private void RequestFilter()
    {
        _pending = true;
        _initSeen = false;
        _waitedFrames = 0;
    }

    private void OnEnable()
    {
        if (Editor == null || Owner == null) return;
        // 重开该页签：按当前纪元重新过滤，不等纪元变化，也不复用旧结果。
        MclslTraitEditorEraFilter.Bind(Editor, Owner);
    }

    private void OnDisable()
    {
        _pending = false;
        _initSeen = false;
        _waitedFrames = 0;
        MclslTraitEditorEraFilter.Unregister(this);
    }

    private void OnDestroy()
    {
        _destroyed = true;
        _pending = false;
        MclslTraitEditorEraFilter.Unregister(this);
        Editor = null;
        Owner = null;
    }

    private void LateUpdate()
    {
        if (!_pending || Editor == null) return;
        if (!MclslTraitEditorEraFilter.NativeButtonsReady(Editor))
        {
            // 原生按钮还没构建完：等，但不无限等，超出上限就放弃本次过滤并留痕。
            if (++_waitedFrames > 240)
            {
                _pending = false;
                MclslDiagnostics.Error("trait-editor-init-timeout", "特质编辑器原生按钮初始化超时，本次未过滤。");
            }
            return;
        }
        if (!_initSeen)
        {
            // 本帧刚看到初始化完成，下一帧再过滤，保证在原按钮初始化之后。
            _initSeen = true;
            return;
        }
        _pending = false;
        if (!MclslTraitEditorEraFilter.Apply(Editor)) return;
        // 只有真正应用成功才记录本实例的纪元，供下次翻转比较。
        _appliedNewLaw = MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear());
    }
}
