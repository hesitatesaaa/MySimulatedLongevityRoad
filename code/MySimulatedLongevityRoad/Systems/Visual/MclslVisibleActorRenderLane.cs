using System;
using System.Reflection;
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

        int frame = Time.frameCount;
        if (_lastFrame == frame && ReferenceEquals(_lastRenderData, manager.render_data)) return;
        _lastFrame = frame;
        _lastRenderData = manager.render_data;

        bool scanHalo = MclslRealmHaloVisualSystem.BeginRenderFrame(frame);
        Actor[] actors = manager.visible_units.array;
        int count = actors == null ? 0 : Math.Min(manager.visible_units.count, actors.Length);
        int lodLevel = ResolveLodLevel(frame, count);
        if (count <= 0)
        {
            MclslRealmHaloVisualSystem.EndRenderFrame(frame, lodLevel);
            return;
        }

        if (scanHalo)
        {
            for (int i = 0; i < count; i++)
            {
                Actor actor = actors[i];
                if (actor?.data == null) continue;
                if (!TryGetRenderPosition(manager.render_data, i, actor, out Vector3 position)) continue;
                MclslRealmHaloVisualSystem.ObserveVisibleActor(actor, position, frame);
            }
        }

        MclslRealmHaloVisualSystem.EndRenderFrame(frame, lodLevel);
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

    private static bool TryGetRenderPosition(ActorRenderData renderData, int index, Actor actor, out Vector3 position)
    {
        position = default;
        try
        {
            if (PositionsField != null
                && PositionsField.GetValue(renderData) is Vector3[] positions
                && index >= 0
                && index < positions.Length)
            {
                position = positions[index];
                return true;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslVisibleActorRenderLane-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslVisibleActorRenderLane.cs #2: " + mclslEmptyCatchEx.Message); }

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
