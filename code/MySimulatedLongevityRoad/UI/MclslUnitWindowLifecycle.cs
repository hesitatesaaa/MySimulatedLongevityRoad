using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Patches;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 人物窗口的生命周期保护。补丁只挂在具体的非泛型类型上：
/// UnitWindow.OnEnable、WindowMetaTabButtonsContainer.startTabAction、
/// ScrollWindow.showWindow 的 "unit" 入口与 setActive 打开完成、NeuronsOverview，
/// 以及两个页面协程的包装。
///
/// 早期版本还给泛型 TraitsContainer&lt;TTrait,TTraitButton&gt; 的 sortTraits 和
/// loadActiveTraits 打全局补丁。泛型方法体被所有实例化共享，这类补丁会把国家、文化、
/// 宗教、物种等全部特质容器一并卷进人物窗口的判断；崩溃日志的致命调用落在
/// loadActiveTraits 迭代器阶段，这条路径因此是首要排查假设，但未经证实为唯一根因。
/// 无论如何，泛型全局补丁都不该承担具体窗口的职责，所以这里彻底不再碰泛型容器：
/// 人物有效性只在具体窗口入口检查，特质容器只通过所属窗口被只读观察
/// （见 <see cref="ContainerWindowType"/>），不被改写。
/// </summary>
internal static class MclslUnitWindowLifecycle
{
    private static readonly List<string> PatchLog = new();
    private static readonly List<string> WindowLog = new();
    private static Exception _firstException;

    /// <summary>挂载后的实际目标与 owner，形如 verified|prefix|目标|owner=...|patch=...</summary>
    internal static IReadOnlyList<string> PatchAudit => PatchLog;
    /// <summary>每次窗口有效性检查的类型、是否确有 UnitWindow、人物 id。</summary>
    internal static IReadOnlyList<string> ObservedWindows => WindowLog;
    internal static string FirstException => _firstException == null
        ? null
        : _firstException.GetType().Name + ": " + _firstException.Message;

    internal static bool Valid(Actor actor) => actor?.data != null && actor.asset != null && actor.isAlive();

    internal static int Apply(Harmony harmony)
    {
        PatchLog.Clear();
        WindowLog.Clear();
        _firstException = null;

        int count = 0;
        count += Patch(harmony, typeof(WindowMetaTabButtonsContainer), "startTabAction",
            new[] { typeof(WindowMetaTab), typeof(bool) }, prefix: nameof(BeforeTab), postfix: nameof(AfterTab));
        // OnEnable 只做失效拦截（prefix）；编辑器绑定统一放到真正的打开路径
        // （ScrollWindow.setActive 打开完成 / 页签激活）之后，避免窗口未打开就绑定。
        count += Patch(harmony, typeof(UnitWindow), "OnEnable", Type.EmptyTypes,
            prefix: nameof(BeforeWindow));
        count += Patch(harmony, typeof(ScrollWindow), "showWindow",
            new[] { typeof(string), typeof(bool), typeof(bool) }, prefix: nameof(BeforeShow));
        // 所有窗口（含没有页签的独立窗口）唯一的激活路径：
        // show/hide 都经由 setActive，pActive 为真即“本次窗口已打开”，
        // 其内部 gameObject.SetActive(true) 已完成原生 OnEnable。
        count += Patch(harmony, typeof(ScrollWindow), "setActive",
            new[] { typeof(bool), typeof(string), typeof(string), typeof(bool), typeof(bool) },
            postfix: nameof(AfterWindowActive));
        count += Patch(harmony, typeof(NeuronsOverview), "OnEnable", Type.EmptyTypes, prefix: nameof(BeforeMind));
        count += Patch(harmony, typeof(NeuronsOverview), "checkActorDecisions", Type.EmptyTypes, prefix: nameof(BeforeMind));
        // 页面协程：迭代器体在创建之后才运行，必须在创建处把结果包一层。
        count += Patch(harmony, typeof(UnitGenealogyElement), "showContent", Type.EmptyTypes, postfix: nameof(AfterCoroutine));
        count += Patch(harmony, typeof(UnitStatsElement), "showContent", Type.EmptyTypes, postfix: nameof(AfterCoroutine));
        return count;
    }

    private static int Patch(Harmony harmony, Type type, string method, Type[] args, string prefix = null, string postfix = null)
    {
        MethodInfo original = AccessTools.Method(type, method, args);
        MethodInfo prefixMethod = string.IsNullOrEmpty(prefix) ? null : AccessTools.Method(typeof(MclslUnitWindowLifecycle), prefix);
        MethodInfo postfixMethod = string.IsNullOrEmpty(postfix) ? null : AccessTools.Method(typeof(MclslUnitWindowLifecycle), postfix);
        string key = "unit-lifecycle:" + type.Name + "." + method;
        if (!MclslHarmonyPatchGuard.TryPatch(harmony, key, original, prefixMethod, postfixMethod)) return 0;
        Record(key, original, prefixMethod, postfixMethod);
        return 1;
    }

    // 挂载成功后回读 Harmony 的真实记录，自检依据“补丁目标与 owner 是这些”，
    // 而不是“没有失败日志”。
    private static void Record(string key, MethodInfo original, MethodInfo prefix, MethodInfo postfix)
    {
        string target = original == null ? "missing" : original.DeclaringType?.FullName + "::" + original.Name;
        if (original == null)
        {
            PatchLog.Add("unverified|" + key + "|" + target);
            return;
        }
        HarmonyLib.Patches info;
        try { info = Harmony.GetPatchInfo(original); }
        catch (Exception ex) { RecordException(key, ex); PatchLog.Add("unreadable|" + key + "|" + target); return; }
        if (info == null)
        {
            PatchLog.Add("unverified|" + key + "|" + target);
            return;
        }
        string owner = MclslMod.HarmonyId;
        Verify(info.Prefixes, prefix, owner, "prefix", key, target);
        Verify(info.Postfixes, postfix, owner, "postfix", key, target);
    }

    private static void Verify(IEnumerable<Patch> patches, MethodInfo expected, string owner, string kind, string key, string target)
    {
        if (expected == null) return;
        bool owned = false;
        bool matched = false;
        if (patches != null)
        {
            foreach (Patch patch in patches)
            {
                if (patch == null || !string.Equals(patch.owner, owner, StringComparison.Ordinal)) continue;
                owned = true;
                if (patch.PatchMethod == expected) matched = true;
            }
        }
        string patchName = expected.DeclaringType?.Name + "." + expected.Name;
        bool ok = owned && matched;
        PatchLog.Add((ok ? "verified" : "mismatch") + "|" + kind + "|" + target + "|owner=" + owner + "|patch=" + patchName);
        if (!ok)
            MclslDiagnostics.Error("unit-lifecycle-audit-" + key,
                "补丁回读与预期不一致: " + kind + " " + target + " owner=" + owner + " patch=" + patchName);
    }

    /// <summary>
    /// 自检用：列出本模组打在泛型特质容器/编辑器上的补丁目标。健康状态必须为空，
    /// 否则说明又给泛型容器加了全局补丁。
    /// </summary>
    internal static List<string> GenericContainerPatchViolations()
    {
        var violations = new List<string>();
        foreach (MethodBase patched in Harmony.GetAllPatchedMethods())
        {
            Type declaring = patched?.DeclaringType;
            if (declaring == null || !declaring.IsGenericType) continue;
            string name = declaring.Name;
            if (!name.StartsWith("TraitsContainer`", StringComparison.Ordinal)
                && !name.StartsWith("TraitsEditor`", StringComparison.Ordinal)) continue;
            HarmonyLib.Patches info = Harmony.GetPatchInfo(patched);
            if (info == null || !OwnedByMclsl(info)) continue;
            violations.Add(declaring + "::" + patched.Name);
        }
        return violations;
    }

    private static bool OwnedByMclsl(HarmonyLib.Patches info)
        => Owns(info.Prefixes) || Owns(info.Postfixes) || Owns(info.Finalizers) || Owns(info.Transpilers);

    private static bool Owns(IEnumerable<Patch> patches)
    {
        if (patches == null) return false;
        foreach (Patch patch in patches)
            if (patch != null && string.Equals(patch.owner, MclslMod.HarmonyId, StringComparison.Ordinal)) return true;
        return false;
    }

    internal static void RecordException(string context, Exception exception)
    {
        if (exception == null) return;
        _firstException ??= exception;
        MclslDiagnostics.Error("unit-window-exception-" + context, context + " 异常: " + exception.Message);
    }

    internal static bool BeforeWindow(UnitWindow __instance) => Prepare(__instance);

    internal static bool BeforeShow(string __0) => __0 != "unit" || Valid(SelectedUnit.unit);

    /// <summary>
    /// 本次窗口打开完成（pActive 为真）后，只在这个窗口实例内绑定特质编辑器。
    /// 不依赖页签、不限定 UnitWindow：独立打开的编辑器窗口同样覆盖。
    /// 关闭（pActive 为假）时只释放本窗口的绑定。
    /// </summary>
    internal static void AfterWindowActive(ScrollWindow __instance, bool __0)
    {
        if (__instance == null) return;
        if (!__0) { MclslTraitEditorEraFilter.ReleaseWindow(__instance); return; }
        MclslTraitEditorEraFilter.BindToWindow(__instance);
    }

    internal static bool BeforeTab(WindowMetaTabButtonsContainer __instance)
    {
        UnitWindow window = ResolveWindow(__instance);
        if (window != null) return Prepare(window);
        return true;
    }

    // 页签内容刚被激活，原生 OnEnable 已完成。绑定对象是真正打开的那个 ScrollWindow，
    // 并通过该窗口自身的作用域扫描编辑器：不限定 UnitWindow，独立的 ActorTraitsEditor 也覆盖。
    internal static void AfterTab(WindowMetaTabButtonsContainer __instance)
    {
        if (__instance == null) return;
        ScrollWindow opened = __instance._scroll_window ?? ResolveWindow(__instance)?.scroll_window;
        UnitWindow window = ResolveWindow(__instance);
        if (opened != null) { MclslTraitEditorEraFilter.BindToWindow(opened); return; }
        if (window != null) BindOpenedWindow(window.scroll_window, window);
    }

    internal static bool BeforeMind(NeuronsOverview __instance)
    {
        UnitWindow window = ResolveWindow(__instance);
        return window != null && Prepare(window);
    }

    internal static void AfterCoroutine(MonoBehaviour __instance, ref IEnumerator __result)
    {
        UnitWindow window = ResolveWindow(__instance);
        if (window != null && __result != null)
            __result = GuardCoroutine(__result, window, window.actor);
    }

    private static void BindOpenedWindow(ScrollWindow opened, Component scope)
    {
        if (opened == null) return;
        MclslTraitEditorEraFilter.BindToWindow(opened, scope);
    }

    private static UnitWindow ResolveWindow(Component component)
        => component == null ? null : component.GetComponentInParent<UnitWindow>();

    // 迭代器体运行在 Harmony 构造期之后，只能在迭代过程中检查。
    // 嵌套迭代器在父级挂起期间也需要同样保护。异常只记录首个、随后继续抛出，不吞掉。
    private static IEnumerator GuardCoroutine(IEnumerator work, UnitWindow window, Actor expected)
    {
        try
        {
            while (window != null && Valid(expected) && ReferenceEquals(window.actor, expected))
            {
                object current;
                try
                {
                    if (!work.MoveNext()) yield break;
                    current = work.Current;
                }
                catch (Exception ex)
                {
                    RecordException("page-coroutine", ex);
                    throw;
                }
                yield return current is IEnumerator nested ? GuardCoroutine(nested, window, expected) : current;
            }
        }
        finally { (work as IDisposable)?.Dispose(); }
    }

    private static bool Prepare(UnitWindow window)
    {
        if (window == null) return true;
        bool valid = Valid(window.actor);
        RecordWindow(window, valid);
        if (!valid)
        {
            CloseInvalid(window);
            return false;
        }
        MclslUnitSelectionWatcher watcher = window.GetComponent<MclslUnitSelectionWatcher>();
        if (watcher == null) watcher = window.gameObject.AddComponent<MclslUnitSelectionWatcher>();
        watcher.Window = window;
        return true;
    }

    private static void RecordWindow(UnitWindow window, bool valid)
    {
        if (WindowLog.Count >= 64) WindowLog.Clear();
        WindowLog.Add("window=" + window.GetType().FullName
            + "|isUnitWindow=" + (window is UnitWindow)
            + "|valid=" + valid
            + "|actor=" + (window.actor?.data == null ? 0L : MclslActorAccessor.Id(window.actor)));
    }

    /// <summary>
    /// 只读观察：该特质容器归属哪个窗口。用于测试断言容器确实属于人物窗口，
    /// 而不是通过给泛型容器打补丁来"保护"。
    /// </summary>
    internal static string ContainerWindowType(object container)
    {
        if (container is not TraitsContainer<ActorTrait, ActorTraitButton> traits) return "not-actor-traits-container";
        ITraitWindow<ActorTrait, ActorTraitButton> owner = traits._trait_window;
        return owner == null ? "unbound" : owner.GetType().FullName;
    }

    internal static void StopPageWork(UnitWindow window)
    {
        if (window == null) return;
        foreach (MonoBehaviour component in window.GetComponentsInChildren<MonoBehaviour>(true))
            if (component != null) component.StopAllCoroutines();
    }

    /// <summary>
    /// 只停仍绑在旧人物身上的页面元素，不碰其他人物的协程。
    /// 切换瞬间新人物可能已经由 showStatsRows/showInfo 等路径先行启动过工作，
    /// 对整个窗口 StopAllCoroutines 会把它一起杀掉，所以这里按元素持有的 actor 过滤。
    /// </summary>
    private static int StopPageWorkFor(UnitWindow window, Actor previous)
    {
        if (window == null || previous == null) return 0;
        int stopped = 0;
        foreach (UnitElement element in window.GetComponentsInChildren<UnitElement>(true))
        {
            if (element == null || !ReferenceEquals(element.actor, previous)) continue;
            element.StopAllCoroutines();
            stopped++;
        }
        return stopped;
    }

    /// <summary>
    /// 仅判断"当前活动标签"的内容是否已全部指向该人物。
    ///
    /// 只认活动标签自己的 tab_elements，不看其他未激活标签，也不看窗口里的辅助元素，
    /// 因此"别处的元素恰好已是新人物"不会被误判成当前标签已就绪。
    /// 判定条件保守：取不到活动标签、标签内没有任何 UnitElement、或有任一元素仍指向
    /// 别的人物，都返回 false。宁可多重放一次（reloadActiveTab 只重跑当前标签的
    /// doAction），也不把部分内容误判为已完成而漏刷新。
    /// </summary>
    private static bool ActiveTabReadyFor(UnitWindow window, Actor actor)
    {
        if (window == null || actor == null) return false;
        WindowMetaTabButtonsContainer tabs = window.scroll_window?.tabs;
        if (tabs == null) return false;
        WindowMetaTab active = tabs.getActiveTab();
        if (active == null || active.tab_elements == null) return false;

        int examined = 0;
        for (int i = 0; i < active.tab_elements.Count; i++)
        {
            Transform root = active.tab_elements[i];
            if (root == null) continue;
            foreach (UnitElement element in root.GetComponentsInChildren<UnitElement>(true))
            {
                if (element == null) continue;
                examined++;
                if (!ReferenceEquals(element.actor, actor)) return false;
            }
        }
        return examined > 0;
    }

    /// <summary>人物死亡或对象失效：取消旧页面任务并清理状态，然后关闭窗口。</summary>
    internal static void CloseInvalid(UnitWindow window)
    {
        if (window == null) return;
        StopPageWork(window);
        MclslTraitEditorEraFilter.ReleaseWindow(window.scroll_window);
        MclslActorInfoPanel.OnWindowClosed(window);
        window.scroll_window?.hide("right", false);
        window.gameObject.SetActive(false);
        MclslDiagnostics.Once("unit-window-invalid-selection", "人物选择已失效，请重新选择单位。");
    }

    /// <summary>
    /// 切换人物。旧协程依靠 GuardCoroutine 的人物令牌自行失效，这里只额外停掉仍绑在
    /// 旧人物身上的元素；重放用 reloadActiveTab（仅重跑当前标签的 doAction），
    /// 不用 initialTabAction（后者在 _tab_last 为空时会跑 tab_default），
    /// 并且仅当前活动标签已整体指向新人物时才跳过重放。
    /// </summary>
    private static void SwitchPage(UnitWindow window, Actor previous, Actor current)
    {
        StopPageWorkFor(window, previous);
        MclslTraitEditorEraFilter.ReleaseWindow(window.scroll_window);
        MclslActorInfoPanel.OnWindowClosed(window);
        bool ready = ActiveTabReadyFor(window, current);
        if (!ready) window.scroll_window?.tabs?.reloadActiveTab();
        RecordSwitch(window, previous, current, ready);
        BindOpenedWindow(window.scroll_window, window);
        MclslActorInfoPanel.Refresh(window, true, true);
    }

    private static void RecordSwitch(UnitWindow window, Actor previous, Actor current, bool skippedReplay)
    {
        if (WindowLog.Count >= 64) WindowLog.Clear();
        WindowLog.Add("switch|from=" + (previous?.data == null ? 0L : MclslActorAccessor.Id(previous))
            + "|to=" + (current?.data == null ? 0L : MclslActorAccessor.Id(current))
            + "|replay=" + (skippedReplay ? "skipped-active-tab-ready" : "reloadActiveTab")
            + "|window=" + window.GetType().FullName);
    }

    internal sealed class MclslUnitSelectionWatcher : MonoBehaviour
    {
        internal UnitWindow Window;
        private Actor _previous;
        private bool _observed;

        private void OnDisable()
        {
            _previous = null;
            _observed = false;
        }

        private void LateUpdate()
        {
            if (Window == null) return;
            Actor current = Window.actor;
            if (!Valid(current))
            {
                CloseInvalid(Window);
                return;
            }
            if (_observed && ReferenceEquals(_previous, current)) return;
            // 首次观察到这个窗口只记录基线：正常打开有效人物不额外重放初始化。
            bool switched = _observed;
            Actor previous = _previous;
            _previous = current;
            _observed = true;
            if (!switched) return;
            SwitchPage(Window, previous, current);
        }
    }
}
