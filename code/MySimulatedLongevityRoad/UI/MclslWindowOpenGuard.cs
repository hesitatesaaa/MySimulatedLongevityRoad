using UnityEngine;

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
        Canvas.ForceUpdateCanvases();
    }
}
