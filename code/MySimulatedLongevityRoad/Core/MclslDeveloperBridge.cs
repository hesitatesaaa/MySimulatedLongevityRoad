using System;
using System.Reflection;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// 可选的本机开发者工具桥接层。
/// 正式源码和玩家包不包含 DeveloperTools/，因此这里不能静态引用调试编辑器。
/// </summary>
internal static class MclslDeveloperBridge
{
    private const string EditorTypeName = "MySimulatedLongevityRoad.UI.MclslDeveloperActorEditor";
    private static Actor _selectedActor;
    private static MethodInfo _showMethod;
    private static bool _resolved;

    internal static Actor SelectedActor => _selectedActor;

    internal static bool IsAvailable
    {
        get
        {
            ResolveEditor();
            return _showMethod != null;
        }
    }

    internal static void Tick()
    {
        if (!IsAvailable) return;
        try
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
                OpenSelectedActorEditor();
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("developer-tools-hotkey", "开发者工具快捷键处理失败: " + ex.Message);
        }
    }

    internal static void SetSelectedActor(Actor actor)
    {
        if (!IsAvailable || actor?.data == null) return;
        _selectedActor = actor;
    }

    internal static bool OpenSelectedActorEditor(Actor actor = null)
    {
        if (actor != null) SetSelectedActor(actor);
        if (!IsAvailable || _selectedActor?.data == null)
        {
            UnityEngine.Debug.Log("[模拟长生路] 请先点击一个人物，再打开开发者工具。");
            return false;
        }

        try
        {
            _showMethod.Invoke(null, new object[] { _selectedActor });
            return true;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("developer-tools-open", "打开开发者角色编辑器失败: " + ex.Message);
            return false;
        }
    }

    internal static void RefreshSelectedActor()
    {
        if (_selectedActor != null) MclslActorInfoPanel.RefreshOpenForActor(_selectedActor);
    }

    private static void ResolveEditor()
    {
        if (_resolved) return;
        _resolved = true;
        try
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length && _showMethod == null; i++)
            {
                Type type = assemblies[i].GetType(EditorTypeName, throwOnError: false);
                MethodInfo method = type?.GetMethod("Show", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null) continue;
                _showMethod = method;
            }
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("developer-tools-resolve", "查找本机开发者工具失败: " + ex.Message);
        }
    }
}
