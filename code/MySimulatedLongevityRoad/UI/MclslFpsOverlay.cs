using UnityEngine;

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

    internal static void Ensure()
    {
        if ((Object)(object)_instance != (Object)null)
        {
            return;
        }

        GameObject host = new GameObject("MclslFpsOverlay");
        Object.DontDestroyOnLoad(host);
        host.hideFlags = HideFlags.HideAndDontSave;
        _instance = host.AddComponent<MclslFpsOverlay>();
    }

    private void Awake()
    {
        if ((Object)(object)_instance != (Object)null && !ReferenceEquals(_instance, this))
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Update()
    {
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

        _displayFps = Mathf.Max(0, Mathf.RoundToInt(_frames / _elapsed));
        _elapsed = 0f;
        _frames = 0;
    }

    private void OnGUI()
    {
        EnsureStyles();
        string text = "FPS: " + _displayFps;
        GUI.Label(new Rect(7f, 3f, 90f, 22f), text, _shadowStyle);
        GUI.Label(new Rect(6f, 2f, 90f, 22f), text, _style);
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
