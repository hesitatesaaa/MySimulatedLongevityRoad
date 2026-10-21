using System;
using System.Reflection;
using System.Collections;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Patches;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslUnitWindowLifecycle
{
    internal static bool Valid(Actor actor) => actor?.data != null && actor.asset != null && actor.isAlive();

    internal static int Apply(Harmony harmony)
    {
        int count = 0;
        count += Patch(harmony, typeof(WindowMetaTabButtonsContainer), "startTabAction",
            new[] { typeof(WindowMetaTab), typeof(bool) }, nameof(BeforeTab));
        count += Patch(harmony, typeof(UnitWindow), "OnEnable", Type.EmptyTypes, nameof(BeforeWindow));
        count += Patch(harmony, typeof(ScrollWindow), "showWindow", new[] { typeof(string), typeof(bool), typeof(bool) }, nameof(BeforeShow));
        count += Patch(harmony, typeof(NeuronsOverview), "OnEnable", Type.EmptyTypes, nameof(BeforeMind));
        count += Patch(harmony, typeof(NeuronsOverview), "checkActorDecisions", Type.EmptyTypes, nameof(BeforeMind));
        count += Patch(harmony, typeof(TraitsContainer<ActorTrait, ActorTraitButton>), "sortTraits", Type.EmptyTypes, nameof(BeforeTraitsSort));
        count += Wrap(harmony, typeof(UnitGenealogyElement), "showContent", Type.EmptyTypes);
        count += Wrap(harmony, typeof(UnitStatsElement), "showContent", Type.EmptyTypes);
        count += Wrap(harmony, typeof(TraitsContainer<ActorTrait, ActorTraitButton>), "loadActiveTraits", new[] { typeof(bool) });
        return count;
    }

    private static int Patch(Harmony harmony, Type type, string method, Type[] args, string prefix)
        => MclslHarmonyPatchGuard.TryPatch(harmony, "unit-lifecycle:" + type.Name + "." + method,
            AccessTools.Method(type, method, args), AccessTools.Method(typeof(MclslUnitWindowLifecycle), prefix)) ? 1 : 0;

    private static int Wrap(Harmony harmony, Type type, string method, Type[] args)
        => MclslHarmonyPatchGuard.TryPatch(harmony, "unit-coroutine:" + type.Name + "." + method,
            AccessTools.Method(type, method, args), postfix: AccessTools.Method(typeof(MclslUnitWindowLifecycle), nameof(AfterCoroutine))) ? 1 : 0;

    private static bool BeforeTraitsSort(MonoBehaviour __instance)
    {
        UnitWindow window = __instance.GetComponentInParent<UnitWindow>();
        return window == null || Prepare(window);
    }

    private static void AfterCoroutine(MonoBehaviour __instance, ref IEnumerator __result)
    {
        UnitWindow window = __instance.GetComponentInParent<UnitWindow>();
        if (window != null && __result != null)
            __result = GuardCoroutine(__result, window, window.actor);
    }

    // Iterator bodies run later, outside Harmony's creation-time finalizer.
    // Nested iterators need the same guard while their parent is suspended.
    private static IEnumerator GuardCoroutine(IEnumerator work, UnitWindow window, Actor expected)
    {
        try
        {
            while (window != null && Valid(expected) && ReferenceEquals(window.actor, expected))
            {
                if (!work.MoveNext()) yield break;
                yield return work.Current is IEnumerator nested ? GuardCoroutine(nested, window, expected) : work.Current;
            }
        }
        finally { (work as IDisposable)?.Dispose(); }
    }

    private static bool BeforeTab(WindowMetaTabButtonsContainer __instance)
    {
        UnitWindow window = __instance.GetComponentInParent<UnitWindow>();
        if (window == null) return true;
        return Prepare(window);
    }

    private static bool BeforeWindow(UnitWindow __instance) => Prepare(__instance);

    private static bool BeforeShow(string __0) => __0 != "unit" || Valid(SelectedUnit.unit);

    private static bool BeforeMind(NeuronsOverview __instance)
    {
        UnitWindow window = __instance.GetComponentInParent<UnitWindow>();
        return window != null && Prepare(window);
    }

    private static bool Prepare(UnitWindow window)
    {
        if (!Valid(window.actor))
        {
            CloseInvalid(window);
            return false;
        }
        MclslUnitSelectionWatcher watcher = window.GetComponent<MclslUnitSelectionWatcher>();
        if (watcher == null) watcher = window.gameObject.AddComponent<MclslUnitSelectionWatcher>();
        watcher.Window = window;
        return true;
    }

    internal static void StopPageWork(UnitWindow window)
    {
        foreach (MonoBehaviour component in window.GetComponentsInChildren<MonoBehaviour>(true))
            if (component != null) component.StopAllCoroutines();
    }

    internal static void CloseInvalid(UnitWindow window)
    {
        StopPageWork(window);
        MclslActorInfoPanel.OnWindowClosed(window);
        window.scroll_window?.hide("right", false);
        window.gameObject.SetActive(false);
        MclslDiagnostics.Once("unit-window-invalid-selection", "人物选择已失效，请重新选择单位。");
    }
}

internal sealed class MclslUnitSelectionWatcher : MonoBehaviour
{
    internal UnitWindow Window;
    private Actor _previous;
    private void OnDisable() { _previous = null; }
    private void LateUpdate()
    {
        if (Window == null) return;
        Actor current = Window.actor;
        if (!MclslUnitWindowLifecycle.Valid(current))
        {
            MclslUnitWindowLifecycle.CloseInvalid(Window);
            return;
        }
        if (ReferenceEquals(_previous, current)) return;
        bool changed = _previous != null;
        _previous = current;
        if (!changed) return;
        MclslUnitWindowLifecycle.StopPageWork(Window);
        Window.scroll_window?.tabs?.initialTabAction();
        MclslActorInfoPanel.Refresh(Window, true, true);
    }
}
