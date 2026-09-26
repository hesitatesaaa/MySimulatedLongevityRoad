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
    private string _displayText = "FPS: 0";
    private bool _visible;

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
        if (!_visible) return;
        EnsureStyles();
        const float x = 8f;
        GUI.Label(new Rect(x + 1f, 9f, 96f, 22f), _displayText, _shadowStyle);
        GUI.Label(new Rect(x, 8f, 96f, 22f), _displayText, _style);
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
