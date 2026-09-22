using System;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// The only boundary used by 0.2.0 spatial features to touch WorldBox objects.
/// Native objects are never serialized; only stable IDs and coordinates cross this boundary.
/// </summary>
internal static class MclslNativeWorldAdapter
{
    private static bool _movementDisabled;
    private static string _movementFailure = string.Empty;

    internal static bool MovementAvailable => !_movementDisabled;
    internal static string MovementFailure => _movementFailure;

    internal static void Reset()
    {
        _movementDisabled = false;
        _movementFailure = string.Empty;
    }

    internal static Actor ResolveActor(long actorId)
    {
        if (actorId <= 0L || World.world?.units == null) return null;
        try { return World.world.units.get(actorId); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("spatial-adapter:resolve-actor", "解析人物失败: " + ex.Message);
            return null;
        }
    }

    internal static bool TryGetTile(int x, int y, out WorldTile tile)
    {
        tile = null;
        if (x < 0 || y < 0 || World.world == null) return false;
        try
        {
            tile = World.world.GetTile(x, y);
            return tile != null;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("spatial-adapter:get-tile", "解析节点地块失败: " + ex.Message);
            return false;
        }
    }

    internal static bool IsAt(Actor actor, int x, int y)
    {
        if (actor?.data == null || x < 0 || y < 0) return false;
        int dx = actor.data.x - x;
        int dy = actor.data.y - y;
        return dx * dx + dy * dy <= 2;
    }

    internal static bool TryMoveTo(Actor actor, int x, int y)
    {
        if (_movementDisabled || actor?.data == null || !MclslActorAccessor.Alive(actor)) return false;
        if (!TryGetTile(x, y, out WorldTile tile)) return false;
        try
        {
            ActorMove.goTo(actor, tile, false, false, false, 0);
            return true;
        }
        catch (Exception ex)
        {
            _movementDisabled = true;
            _movementFailure = ex.Message ?? "native movement failed";
            MclslDiagnostics.Error("spatial-adapter:move", "原版寻路调用失败，空间任务进入降级: " + _movementFailure);
            return false;
        }
    }

    internal static void ReleaseMovement(Actor actor)
    {
        if (actor?.data == null) return;
        try { actor.stopMovement(); }
        catch (Exception ex) { MclslDiagnostics.Error("spatial-adapter:stop", "释放人物移动控制失败: " + ex.Message); }
    }

    internal static string CityName(Actor actor)
    {
        try { return actor?.city?.data?.name ?? string.Empty; }
        catch { return string.Empty; }
    }

    internal static string KingdomName(Actor actor)
    {
        try { return actor?.kingdom?.data?.name ?? string.Empty; }
        catch { return string.Empty; }
    }
}
