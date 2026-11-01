using System;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 单线程年度执行上下文。用于阻止境界同步、特质同步或 UI 刷新在同一角色同一逻辑年
/// 重新唤醒年度流水线，保证每个角色每年只有一个 Progression 写入口。
/// </summary>
internal static class MclslAnnualExecutionContext
{
    private static long _actorId;
    private static int _year;
    private static int _depth;

    internal static bool IsActive => _depth > 0;
    internal static long ActorId => _actorId;
    internal static int Year => _year;

    internal static bool TryEnter(Actor actor, int year)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || year <= 0) return false;
        // 年度 Progression 不允许嵌套重入；即使是同一角色同一年，
        // 也必须由当前流水线完成，避免事件或特质回调再次执行增长与突破。
        if (_depth > 0) return false;

        _actorId = actorId;
        _year = year;
        _depth = 1;
        return true;
    }

    internal static void Exit(Actor actor, int year)
    {
        if (_depth <= 0) return;
        long actorId = MclslActorAccessor.Id(actor);
        if (_actorId != actorId || _year != year)
        {
            Clear();
            return;
        }
        _depth--;
        if (_depth <= 0) Clear();
    }

    internal static bool IsExecuting(Actor actor, int year)
    {
        return _depth > 0
            && _actorId == MclslActorAccessor.Id(actor)
            && _year == year;
    }

    internal static bool IsExecutingActor(Actor actor)
    {
        return _depth > 0 && _actorId == MclslActorAccessor.Id(actor);
    }

    internal static int ResolveYear(Actor actor, int fallbackYear)
    {
        return IsExecutingActor(actor) && _year > 0 ? _year : Math.Max(0, fallbackYear);
    }

    internal static void Clear()
    {
        _actorId = 0L;
        _year = 0;
        _depth = 0;
    }
}
