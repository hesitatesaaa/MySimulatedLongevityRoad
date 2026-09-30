using System;
using System.IO;
using UnityEngine;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslFpsOverlay : MonoBehaviour
{
    private const float RefreshIntervalSeconds = 0.25f;
    private static MclslFpsOverlay _instance;
    private static GUIStyle _style;
    private static GUIStyle _shadowStyle;

    private float _elapsed;
    private int _frames;
    private int _displayFps;
    private string _displayText = "FPS: 0";
    private bool _visible, _reportVisible, _monitorConfigured;
    private string[] _reportLines = Array.Empty<string>();
    private string _reportText = string.Empty, _exportMessage = string.Empty;
    private int _reportPage;
    private float _nextReportRefresh;
    private Rect _reportRect = new(110, 40, 920, 650);
    private static GUIStyle _reportStyle;
    private const int ReportPageLines = 28;
    internal static void ClearReport()
    {
        if (_instance == null) return;
        _instance._reportLines = Array.Empty<string>(); _instance._reportText = string.Empty;
        _instance._reportPage = 0; _instance._reportVisible = false; _instance._nextReportRefresh = 0;
        _instance._exportMessage = string.Empty;
    }

    internal static void Ensure()
    {
        if ((UnityEngine.Object)(object)_instance != (UnityEngine.Object)null)
        {
            return;
        }

        GameObject host = new GameObject("MclslFpsOverlay");
        UnityEngine.Object.DontDestroyOnLoad(host);
        host.hideFlags = HideFlags.HideAndDontSave;
        _instance = host.AddComponent<MclslFpsOverlay>();
    }

    private void Awake()
    {
        if ((UnityEngine.Object)(object)_instance != (UnityEngine.Object)null && !ReferenceEquals(_instance, this))
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    internal static void SetVisible(bool visible)
    {
        Ensure();
        if (_instance == null) return;
        _instance._visible = visible;
        _instance._elapsed = 0f;
        _instance._frames = 0;
        if (!visible)
        {
            _instance._displayFps = 0;
            _instance._displayText = "FPS: 0";
        }
    }

    private void Update()
    {
        if (_monitorConfigured != MclslRuntimeSettings.PerformanceMonitor)
        {
            _monitorConfigured = MclslRuntimeSettings.PerformanceMonitor;
            MclslPerformanceProbe.SetEnabled(_monitorConfigured);
            if (!_monitorConfigured) _reportVisible = false;
        }
        if (_reportVisible && Time.unscaledTime >= _nextReportRefresh)
        {
            _reportLines = MclslPerformanceProbe.Summary(forceRefresh: true).Split('\n');
            SetReportPage(_reportPage);
            _nextReportRefresh = Time.unscaledTime + 2f;
        }
        if (!_visible) return;
        float delta = Time.unscaledDeltaTime;
        if (delta <= 0f || delta > 2f)
        {
            return;
        }

        _elapsed += delta;
        _frames++;
        if (_elapsed < RefreshIntervalSeconds)
        {
            return;
        }

        int fps = Mathf.Max(0, Mathf.RoundToInt(_frames / _elapsed));
        if (fps != _displayFps)
        {
            _displayFps = fps;
            _displayText = "FPS: " + fps;
        }
        _elapsed = 0f;
        _frames = 0;
    }

    private void OnGUI()
    {
        if (!_visible && !MclslAnnualBackpressure.ShowStatus && !MclslRuntimeSettings.PerformanceMonitor) return;
        EnsureStyles();
        if (MclslAnnualBackpressure.ShowStatus)
        {
            GUI.Label(new Rect(9f, 33f, 900f, 44f), MclslAnnualBackpressure.StatusText, _shadowStyle);
            GUI.Label(new Rect(8f, 32f, 900f, 44f), MclslAnnualBackpressure.StatusText, _style);
        }
        if (MclslRuntimeSettings.PerformanceMonitor)
        {
            if (GUI.Button(new Rect(110, 5, 104, 28), LocalizedTextManager.getText("MCLSL_perf_report")))
            { _reportVisible = !_reportVisible; _nextReportRefresh = 0; }
            if (_reportVisible)
            {
                _reportRect.width = Math.Min(920, Screen.width - 20);
                _reportRect.height = Math.Min(650, Screen.height - 20);
                _reportRect = GUI.Window(2047031, _reportRect, DrawReport, LocalizedTextManager.getText("MCLSL_perf_report"));
            }
        }
        if (!_visible) return;
        const float x = 8f;
        GUI.Label(new Rect(x + 1f, 9f, 96f, 22f), _displayText, _shadowStyle);
        GUI.Label(new Rect(x, 8f, 96f, 22f), _displayText, _style);
    }

    private void SetReportPage(int page)
    {
        int pages = Math.Max(1, (_reportLines.Length + ReportPageLines - 1) / ReportPageLines);
        _reportPage = Math.Clamp(page, 0, pages - 1);
        int start = _reportPage * ReportPageLines;
        _reportText = string.Join("\n", _reportLines, start, Math.Min(ReportPageLines, _reportLines.Length - start));
    }
    private void DrawReport(int id)
    {
        if (_reportStyle == null) _reportStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = false };
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(LocalizedTextManager.getText("MCLSL_perf_reset")))
        { MclslPerformanceProbe.SetEnabled(true); _nextReportRefresh = 0; }
        if (GUILayout.Button(LocalizedTextManager.getText("MCLSL_perf_export"))) ExportReport();
        if (GUILayout.Button(LocalizedTextManager.getText("MCLSL_perf_close"))) _reportVisible = false;
        GUILayout.EndHorizontal();
        if (_exportMessage.Length > 0) GUILayout.Label(_exportMessage, _reportStyle);
        GUILayout.Label(_reportText, _reportStyle);
        GUILayout.FlexibleSpace(); GUILayout.BeginHorizontal();
        if (GUILayout.Button(LocalizedTextManager.getText("MCLSL_ui_page_previous"))) SetReportPage(_reportPage - 1);
        int pages = Math.Max(1, (_reportLines.Length + ReportPageLines - 1) / ReportPageLines);
        GUILayout.Label((_reportPage + 1) + " / " + pages, GUILayout.Width(80));
        if (GUILayout.Button(LocalizedTextManager.getText("MCLSL_ui_page_next"))) SetReportPage(_reportPage + 1);
        GUILayout.EndHorizontal();
        GUI.DragWindow(new Rect(0, 0, _reportRect.width, 22));
    }
    private void ExportReport()
    {
        try
        {
            string directory = Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "ArchitectureV1", "PerformanceReports");
            Directory.CreateDirectory(directory);
            string filename = "MCLS_perf_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + ".txt";
            File.WriteAllText(Path.Combine(directory, filename), MclslPerformanceProbe.Summary(forceRefresh: true), System.Text.Encoding.UTF8);
            string[] reports = Directory.GetFiles(directory, "MCLS_perf_*.txt");
            Array.Sort(reports, StringComparer.Ordinal);
            for (int i = 0; i < reports.Length - 12; i++) File.Delete(reports[i]);
            _exportMessage = string.Format(LocalizedTextManager.getText("MCLSL_perf_exported"), Path.Combine(directory, filename));
        }
        catch (Exception ex)
        { _exportMessage = LocalizedTextManager.getText("MCLSL_perf_export_failed"); MclslDiagnostics.Error("perf-export", "性能报告导出失败：" + ex.Message); }
    }

    private static void EnsureStyles()
    {
        if (_style != null && _shadowStyle != null)
        {
            return;
        }

        _style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white }
        };
        _shadowStyle = new GUIStyle(_style)
        {
            normal = { textColor = new Color(0f, 0f, 0f, 0.65f) }
        };
    }
}
