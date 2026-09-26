using System;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

internal static class MclslVisibleActorRenderLane
{
    private const int NearVisibleCount = 96;
    private const int MediumVisibleCount = 320;
    private const float MediumCameraSize = 55f;
    private const float FarCameraSize = 95f;

    private static readonly FieldInfo PositionsField = typeof(ActorRenderData).GetField(
        "positions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static int _lastFrame = -1;
    private static ActorRenderData _lastRenderData;
    private static Camera _cachedMainCamera;
    private static int _nextCameraRefreshFrame;

    internal static void Apply(ActorManager manager)
    {
        if (manager?.visible_units == null || manager.render_data == null) return;

        long sample = MySimulatedLongevityRoad.Core.MclslPerformanceProbe.Begin();

        int frame = Time.frameCount;
        if (_lastFrame == frame && ReferenceEquals(_lastRenderData, manager.render_data))
        {
            MySimulatedLongevityRoad.Core.MclslPerformanceProbe.End("境界光环", sample);
            return;
        }
        _lastFrame = frame;
        _lastRenderData = manager.render_data;

        bool scanHalo = MclslRealmHaloVisualSystem.BeginRenderFrame(frame);
        Actor[] actors = manager.visible_units.array;
        int count = actors == null ? 0 : Math.Min(manager.visible_units.count, actors.Length);
        int lodLevel = ResolveLodLevel(frame, count);
        if (count <= 0)
        {
            MclslRealmHaloVisualSystem.EndRenderFrame(frame, lodLevel);
            MySimulatedLongevityRoad.Core.MclslPerformanceProbe.End("境界光环", sample);
            return;
        }

        if (scanHalo)
        {
            Vector3[] renderPositions = TryGetRenderPositions(manager.render_data);
            using (MclslUnityProfiler.Sample("MCLS/Visual/HaloVisibleActorScan"))
            {
                for (int i = 0; i < count; i++)
                {
                    Actor actor = actors[i];
                    if (actor?.data == null) continue;
                    if (!TryGetRenderPosition(renderPositions, i, actor, out Vector3 position)) continue;
                    MclslRealmHaloVisualSystem.ObserveVisibleActor(actor, position, frame);
                }
            }
        }

        MclslRealmHaloVisualSystem.EndRenderFrame(frame, lodLevel);
        MySimulatedLongevityRoad.Core.MclslPerformanceProbe.End("境界光环", sample);
    }

    internal static void Clear()
    {
        _lastFrame = -1;
        _lastRenderData = null;
        _cachedMainCamera = null;
        _nextCameraRefreshFrame = 0;
    }

    private static int ResolveLodLevel(int frame, int visibleCount)
    {
        float cameraSize = 0f;
        try
        {
            if (_cachedMainCamera == null || frame >= _nextCameraRefreshFrame)
            {
                _cachedMainCamera = Camera.main;
                _nextCameraRefreshFrame = frame + 120;
            }
            if (_cachedMainCamera != null && _cachedMainCamera.orthographic)
            {
                cameraSize = _cachedMainCamera.orthographicSize;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslVisibleActorRenderLane-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslVisibleActorRenderLane.cs #1: " + mclslEmptyCatchEx.Message); }

        if (visibleCount > MediumVisibleCount || cameraSize >= FarCameraSize) return 2;
        if (visibleCount > NearVisibleCount || cameraSize >= MediumCameraSize) return 1;
        return 0;
    }

    private static Vector3[] TryGetRenderPositions(ActorRenderData renderData)
    {
        try
        {
            return PositionsField?.GetValue(renderData) as Vector3[];
        }
        catch (System.Exception ex)
        {
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error(
                "visible-actor-render-positions",
                "读取可见人物渲染位置失败: " + ex.Message);
            return null;
        }
    }

    private static bool TryGetRenderPosition(Vector3[] positions, int index, Actor actor, out Vector3 position)
    {
        position = default;
        if (positions != null && index >= 0 && index < positions.Length)
        {
            position = positions[index];
            return true;
        }

        try
        {
            position = actor.current_position;
            return true;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslVisibleActorRenderLane-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslVisibleActorRenderLane.cs #3: " + mclslEmptyCatchEx.Message); }

        try
        {
            WorldTile tile = ((BaseSimObject)actor).current_tile;
            if (tile != null)
            {
                position = tile.posV3;
                return true;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslVisibleActorRenderLane-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslVisibleActorRenderLane.cs #4: " + mclslEmptyCatchEx.Message); }
        return false;
    }
}
