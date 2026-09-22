using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSpatialTaskSystem
{
    private const int CommandIntervalFrames = 30;
    private const int FrameTaskBudget = 4;

    internal static void OnWorldLoaded(int year)
    {
        MclslNativeWorldAdapter.Reset();
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run.SpatialTasks ??= new List<MclslSpatialTaskRecord>();
        for (int i = 0; i < run.SpatialTasks.Count; i++)
        {
            MclslSpatialTaskRecord task = run.SpatialTasks[i];
            if (task == null) continue;
            if (task.State == "moving" || task.State == "assigned" || task.State == "arrived")
                task.State = "assigned";
            task.LastCommandFrame = -10000;
        }
    }

    internal static void TickFrame(int frameCounter)
    {
        List<MclslSpatialTaskRecord> tasks = MclslWorldRunRepository.Current?.SpatialTasks;
        if (tasks == null || tasks.Count == 0) return;
        int handled = 0;
        bool changed = false;
        for (int i = 0; i < tasks.Count && handled < FrameTaskBudget; i++)
        {
            MclslSpatialTaskRecord task = tasks[i];
            if (task == null || !IsActive(task.State)) continue;
            handled++;
            Actor actor = MclslNativeWorldAdapter.ResolveActor(task.ActorId);
            MclslMapNodeRecord node = MclslMapNodeSystem.Find(task.TargetNodeId);
            if (!MclslActorAccessor.Alive(actor))
            {
                Fail(task, "人物已死亡或失联", MclslRuntime.CurrentYear());
                changed = true;
                continue;
            }
            if (node == null || node.MapX < 0 || node.MapY < 0 || node.VisibilityState == "lost")
            {
                Fail(task, "节点失联", MclslRuntime.CurrentYear());
                MclslNativeWorldAdapter.ReleaseMovement(actor);
                changed = true;
                continue;
            }
            int year = Math.Max(0, MclslRuntime.CurrentYear());
            if (MclslNativeWorldAdapter.IsAt(actor, node.MapX, node.MapY))
            {
                task.State = "arrived";
                task.Progress = 100;
                task.LastProgressYear = year;
                MclslWorldRunRepository.AddEvent(year, "node_arrived", MclslActorAccessor.DisplayName(actor) + "抵达" + node.Name,
                    MclslActorAccessor.DisplayName(actor) + "已抵达" + node.Name + "，开始执行" + TaskDisplay(task.TaskType) + "。", actor.data.x, actor.data.y,
                    node.LocationName, node.NativeKingdomNameSnapshot, node.Id);
                ResolveArrived(task, actor, node, year);
                changed = true;
                continue;
            }
            if (frameCounter - task.LastCommandFrame < CommandIntervalFrames) continue;
            task.LastCommandFrame = frameCounter;
            task.State = "moving";
            if (!MclslNativeWorldAdapter.TryMoveTo(actor, node.MapX, node.MapY))
            {
                Fail(task, string.IsNullOrWhiteSpace(MclslNativeWorldAdapter.MovementFailure) ? "原版寻路失败" : MclslNativeWorldAdapter.MovementFailure, year);
            }
            changed = true;
        }
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryAssign(Actor actor, MclslMapNodeRecord node, string taskType, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || node == null || node.MapX < 0 || node.MapY < 0 || !MclslNativeWorldAdapter.MovementAvailable)
            return false;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run.SpatialTasks ??= new List<MclslSpatialTaskRecord>();
        int active = 0;
        for (int i = 0; i < run.SpatialTasks.Count; i++)
        {
            MclslSpatialTaskRecord existing = run.SpatialTasks[i];
            if (existing == null) continue;
            if (IsActive(existing.State)) active++;
            if (existing.ActorId == MclslActorAccessor.Id(actor) && IsActive(existing.State)) return false;
        }
        if (active >= MclslWorldRunRepository.MaxActiveSpatialTasks) return false;
        string taskId = "task_" + Math.Max(0, year) + "_" + MclslActorAccessor.Id(actor) + "_" + node.Id;
        MclslSpatialTaskRecord task = new()
        {
            TaskId = taskId,
            ActorId = MclslActorAccessor.Id(actor),
            ActorNameSnapshot = MclslActorAccessor.DisplayName(actor),
            TargetNodeId = node.Id,
            TargetMapX = node.MapX,
            TargetMapY = node.MapY,
            TaskType = taskType ?? string.Empty,
            State = "assigned",
            AssignedYear = year,
            LastProgressYear = year,
            OriginEventId = taskId,
            LastCommandFrame = -10000
        };
        run.SpatialTasks.Add(task);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpatialTaskId, task.TaskId);
        MclslMapNodeSystem.MarkContested(node.Id, year);
        MclslWorldRunRepository.AddEvent(year, "spatial_task_assigned", task.ActorNameSnapshot + "前往" + node.Name,
            task.ActorNameSnapshot + "获得" + node.Name + "的" + TaskDisplay(task.TaskType) + "任务，必须抵达节点后才能执行。", actor.data.x, actor.data.y,
            node.LocationName, node.NativeKingdomNameSnapshot, node.Id);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void InterruptActor(long actorId, int year, string reason)
    {
        List<MclslSpatialTaskRecord> tasks = MclslWorldRunRepository.Current?.SpatialTasks;
        if (tasks == null || actorId <= 0L) return;
        for (int i = 0; i < tasks.Count; i++)
        {
            MclslSpatialTaskRecord task = tasks[i];
            if (task != null && task.ActorId == actorId && IsActive(task.State)) Fail(task, reason, year);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void Clear()
    {
        List<MclslSpatialTaskRecord> tasks = MclslWorldRunRepository.Current?.SpatialTasks;
        if (tasks != null) tasks.Clear();
    }

    private static void ResolveArrived(MclslSpatialTaskRecord task, Actor actor, MclslMapNodeRecord node, int year)
    {
        try
        {
            if (task.TaskType == MclslMapNodeSystem.Ruin)
            {
                MclslSectRuinRecord ruin = MclslWorldRunRepository.FindSectRuin(node.SourceRecordId);
                MclslAdventureSystem.ResolveSpatialTask(actor, ruin, year);
            }
            else if (task.TaskType == MclslMapNodeSystem.Cave)
            {
                MclslWorldCaveSystem.ResolveSpatialTask(actor, node.SourceRecordId, year);
            }
            else if (task.TaskType == MclslMapNodeSystem.WorldChange)
            {
                MclslWorldChangeSystem.ResolveSpatialTask(actor, node.SourceRecordId, year);
            }
            MclslSectSystem.ClaimNode(actor, node, year);
            task.State = "completed";
            task.Progress = 100;
            task.LastProgressYear = year;
            MclslWorldRunRepository.AddEvent(year, "spatial_task_completed", MclslActorAccessor.DisplayName(actor) + "完成" + node.Name + "任务",
                MclslActorAccessor.DisplayName(actor) + "在抵达" + node.Name + "后完成了" + TaskDisplay(task.TaskType) + "。", actor.data.x, actor.data.y,
                node.LocationName, node.NativeKingdomNameSnapshot, node.Id);
        }
        catch (Exception ex)
        {
            Fail(task, "任务结算失败: " + ex.Message, year);
        }
        finally
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpatialTaskId, string.Empty);
            MclslNativeWorldAdapter.ReleaseMovement(actor);
        }
    }

    private static void Fail(MclslSpatialTaskRecord task, string reason, int year)
    {
        if (task == null) return;
        task.State = "failed";
        task.FailureReason = reason ?? string.Empty;
        task.LastProgressYear = year;
        Actor actor = MclslNativeWorldAdapter.ResolveActor(task.ActorId);
        if (MclslActorAccessor.Alive(actor))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpatialTaskId, string.Empty);
            MclslNativeWorldAdapter.ReleaseMovement(actor);
        }
        MclslWorldRunRepository.AddEvent(year, "spatial_task_failed", task.ActorNameSnapshot + "任务中断", task.ActorNameSnapshot + "前往节点的任务中断：" + reason,
            task.TargetMapX, task.TargetMapY, string.Empty, string.Empty, task.TargetNodeId);
    }

    private static bool IsActive(string state) => state == "assigned" || state == "moving" || state == "arrived";

    private static string TaskDisplay(string taskType) => taskType switch
    {
        MclslMapNodeSystem.Cave => "洞天争夺",
        MclslMapNodeSystem.Ruin => "遗迹探索",
        MclslMapNodeSystem.WorldChange => "天地之变争夺",
        _ => "空间任务"
    };
}
