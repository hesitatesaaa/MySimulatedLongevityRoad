using System;
using System.Reflection;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal readonly struct MclslMaobaoCommandResult
{
    internal readonly bool Success;
    internal readonly string Message;

    internal MclslMaobaoCommandResult(bool success, string message)
    {
        Success = success;
        Message = message ?? string.Empty;
    }
}

/// <summary>猫宝窗口的命令层，与鬼谷时光长河的 Commands 层保持同样的调用边界。</summary>
internal static class MclslMaobaoCommands
{
    internal static MclslMaobaoCommandResult SaveSelectedActor()
    {
        return TryGetSelectedActor(out Actor actor)
            ? SaveSelectedActor(actor)
            : new MclslMaobaoCommandResult(false, "未选中可保存的角色。");
    }

    internal static MclslMaobaoCommandResult SaveSelectedActor(Actor actor)
    {
        if (actor?.data == null)
            return new MclslMaobaoCommandResult(false, "未选中可保存的角色。");
        return MclslMaobaoArchiveManager.SaveActor(actor, out string message)
            ? new MclslMaobaoCommandResult(true, message)
            : new MclslMaobaoCommandResult(false, string.IsNullOrWhiteSpace(message) ? "保存失败。" : message);
    }

    internal static MclslMaobaoCommandResult RemoveSavedActor(string actorId)
    {
        return MclslMaobaoArchiveManager.Remove(actorId, out string message)
            ? new MclslMaobaoCommandResult(true, message)
            : new MclslMaobaoCommandResult(false, string.IsNullOrWhiteSpace(message) ? "移除失败。" : message);
    }

    internal static MclslMaobaoCommandResult ToggleSavedActor(Actor actor)
    {
        if (actor?.data == null)
            return new MclslMaobaoCommandResult(false, "未选中角色。");
        return MclslMaobaoArchiveManager.ToggleActor(actor, out string message)
            ? new MclslMaobaoCommandResult(true, message)
            : new MclslMaobaoCommandResult(false, string.IsNullOrWhiteSpace(message) ? "操作失败。" : message);
    }

    internal static bool IsActorSaved(Actor actor)
    {
        return actor?.data != null && MclslMaobaoArchiveManager.IsActorSaved(actor);
    }

    internal static bool TryGetSelectedActor(out Actor actor)
    {
        actor = null;
        Type selectedMetasType = typeof(SelectedMetas);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        string[] names = { "selected_actor", "selected_unit", "selected_creature", "selected_meta_object", "selected" };
        for (int i = 0; i < names.Length; i++)
        {
            FieldInfo field = selectedMetasType.GetField(names[i], flags);
            if (TryResolveActor(field?.GetValue(null), out actor)) return true;
            PropertyInfo property = selectedMetasType.GetProperty(names[i], flags);
            if (property != null && property.CanRead && TryResolveActor(property.GetValue(null, null), out actor)) return true;
        }

        // WorldBox 0.51 的 SelectedUnit 是最稳定的后备入口。
        try
        {
            actor = SelectedUnit.unit;
            return actor?.data != null;
        }
        catch { return false; }
    }

    private static bool TryResolveActor(object value, out Actor actor)
    {
        actor = value as Actor;
        if (actor?.data != null) return true;
        if (value == null) return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        string[] names = { "actor", "unit", "data" };
        for (int i = 0; i < names.Length; i++)
        {
            FieldInfo field = value.GetType().GetField(names[i], flags);
            if (field?.GetValue(value) is Actor fieldActor && fieldActor.data != null)
            {
                actor = fieldActor;
                return true;
            }
            PropertyInfo property = value.GetType().GetProperty(names[i], flags);
            if (property != null && property.CanRead && property.GetValue(value, null) is Actor propertyActor && propertyActor.data != null)
            {
                actor = propertyActor;
                return true;
            }
        }
        return false;
    }
}
