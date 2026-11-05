using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslEventLocator
{
    internal static void Locate(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        try
        {
            Focus(actor.data.x, actor.data.y);
            ActionLibrary.openUnitWindow(actor);
        }
        catch (Exception ex) { MclslDiagnostics.Error("locate-actor", ex.Message); }
    }

    internal static bool CanLocate(MclslRunEventRecord record)
    {
        return record != null && (record.ActorId > 0L || (record.MapX >= 0 && record.MapY >= 0));
    }

    internal static void Locate(MclslRunEventRecord record)
    {
        if (record == null) return;
        if (!string.IsNullOrWhiteSpace(record.MapVisualKind) && record.MapX >= 0 && record.MapY >= 0)
        {
            Focus(record.MapX, record.MapY);
            return;
        }
        Actor actor = FindActor(record.ActorId);
        if (MclslActorAccessor.Alive(actor))
        {
            try
            {
                Focus(actor.data.x, actor.data.y);
                ActionLibrary.openUnitWindow(actor);
                return;
            }
            catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslEventLocator-cs-1", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        }
        if (record.MapX < 0 || record.MapY < 0) return;
        try { Focus(record.MapX, record.MapY); }
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslEventLocator-cs-2", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
    }


    internal static bool CanLocate(MclslDeathRecord record)
    {
        return record != null && (record.ActorId > 0L || (record.MapX >= 0 && record.MapY >= 0));
    }

    internal static void Locate(MclslDeathRecord record)
    {
        if (record == null) return;
        Actor actor = FindActor(record.ActorId);
        if (MclslActorAccessor.Alive(actor))
        {
            try
            {
                Focus(actor.data.x, actor.data.y);
                ActionLibrary.openUnitWindow(actor);
                return;
            }
            catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslEventLocator-cs-3", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        }
        if (record.MapX < 0 || record.MapY < 0) return;
        try { Focus(record.MapX, record.MapY); }
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslEventLocator-cs-4", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
    }

    internal static bool CanLocate(MclslWorldCaveRecord record) => record != null && record.MapX >= 0 && record.MapY >= 0;
    internal static bool CanLocate(MclslWorldChangeRecord record) => record != null && record.MapX >= 0 && record.MapY >= 0;
    internal static bool CanLocate(MclslWorldSoulRecord record) => record != null && record.MapX >= 0 && record.MapY >= 0;
    internal static bool CanLocate(MclslSectRuinRecord record) => record != null && record.MapX >= 0 && record.MapY >= 0;

    internal static void Locate(MclslWorldCaveRecord record) { if (CanLocate(record)) Focus(record.MapX, record.MapY); }
    internal static void Locate(MclslWorldChangeRecord record) { if (CanLocate(record)) Focus(record.MapX, record.MapY); }
    internal static void Locate(MclslWorldSoulRecord record) { if (CanLocate(record)) Focus(record.MapX, record.MapY); }
    internal static void Locate(MclslSectRuinRecord record) { if (CanLocate(record)) Focus(record.MapX, record.MapY); }

    private static void Focus(int x, int y)
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        Vector3 position = camera.transform.position;
        camera.transform.position = new Vector3(x, y, position.z);
    }

    private static Actor FindActor(long id)
    {
        if (id <= 0L || World.world?.units == null) return null;
        try { return World.world.units.get(id); }
        catch { return null; }
    }
}
