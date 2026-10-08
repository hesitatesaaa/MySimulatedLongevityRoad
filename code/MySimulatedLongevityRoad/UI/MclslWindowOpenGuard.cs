using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslWindowOpenGuard
{
    internal static void Show(ScrollWindow window, string windowId, bool createdNow)
    {
        if (window == null || string.IsNullOrWhiteSpace(windowId))
        {
            return;
        }

        GameObject gameObject = window.gameObject;
        if (createdNow && gameObject != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        ScrollWindow.showWindow(windowId);
        if (gameObject != null && gameObject.GetComponent<MclslNativeWindowBlocker>() == null)
            gameObject.AddComponent<MclslNativeWindowBlocker>();
        Canvas.ForceUpdateCanvases();
    }
}

internal sealed class MclslNativeWindowBlocker : MonoBehaviour
{
    private GameObject _blocker;

    private void OnEnable()
    {
        if (_blocker != null || transform.parent == null) return;
        _blocker = new GameObject("MclslNativeWindowInputBlocker", typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)_blocker.transform;
        rect.SetParent(transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = _blocker.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;
        rect.SetSiblingIndex(transform.GetSiblingIndex());
        transform.SetAsLastSibling();
    }

    private void OnDisable() => Release();
    private void OnDestroy() => Release();
    private void Release()
    {
        if (_blocker == null) return;
        Destroy(_blocker);
        _blocker = null;
    }
}
