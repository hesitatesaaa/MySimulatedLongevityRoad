using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private string _lineageFocusId = string.Empty;
    private string _lineageFocusTechniqueName = string.Empty;

    internal static void ShowTechniqueForActor(Actor actor)
    {
        Show();
        if (_instance == null || actor?.data == null) return;
        _instance.FocusLineage(actor, false);
    }

    internal static void ShowLineageForActor(Actor actor)
    {
        Show();
        if (_instance == null || actor?.data == null) return;
        _instance.FocusLineage(actor, true);
    }

    private void FocusLineage(Actor actor, bool preferLineageId)
    {
        MclslCodexTab[] tabs = ActiveTabs();
        for (int i = 0; i < tabs.Length; i++)
            if (string.Equals(tabs[i].Title, "传承道法", StringComparison.Ordinal)) { _tab = i; break; }

        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        string techniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        _lineageFocusTechniqueName = techniqueName;
        _lineageFocusId = string.Empty;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        foreach (MclslTechniqueLineageRecord lineage in run?.TechniqueLineages ?? new System.Collections.Generic.List<MclslTechniqueLineageRecord>())
        {
            if (lineage == null) continue;
            bool idMatch = !string.IsNullOrWhiteSpace(techniqueId)
                && (string.Equals(lineage.Id, techniqueId, StringComparison.Ordinal)
                    || string.Equals(lineage.SourceTechniqueId, techniqueId, StringComparison.Ordinal)
                    || string.Equals(lineage.SourceTechniqueId, MclslCultivationCatalog.BaseTechniqueId(techniqueId), StringComparison.Ordinal));
            bool nameMatch = !string.IsNullOrWhiteSpace(techniqueName)
                && string.Equals(lineage.Name, techniqueName, StringComparison.Ordinal);
            if (idMatch || nameMatch)
            {
                _lineageFocusId = lineage.Id;
                break;
            }
        }
        if (preferLineageId && string.IsNullOrWhiteSpace(_lineageFocusId))
            _lineageFocusId = techniqueId;
        _scroll = Vector2.zero;
    }

    private void ClearLineageFocus()
    {
        _lineageFocusId = string.Empty;
        _lineageFocusTechniqueName = string.Empty;
    }

    private bool LineageMatchesFocus(MclslTechniqueLineageRecord lineage)
    {
        if (string.IsNullOrWhiteSpace(_lineageFocusId) && string.IsNullOrWhiteSpace(_lineageFocusTechniqueName)) return true;
        if (!string.IsNullOrWhiteSpace(_lineageFocusId) && string.Equals(lineage?.Id, _lineageFocusId, StringComparison.Ordinal)) return true;
        return !string.IsNullOrWhiteSpace(_lineageFocusTechniqueName)
            && string.Equals(lineage?.Name, _lineageFocusTechniqueName, StringComparison.Ordinal);
    }
}
