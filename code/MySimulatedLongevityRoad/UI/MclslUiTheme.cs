using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 玄黄界面统一色板。用户可见名称使用中文；稳定的内部标识继续使用 ASCII，
/// 避免改名破坏存档、按钮绑定和其他模组兼容性。
/// </summary>
internal static class MclslUiTheme
{
    internal static readonly Color Frame = new(0.16f, 0.17f, 0.19f, 0.99f);
    internal static readonly Color FrameGold = new(0.68f, 0.53f, 0.24f, 0.96f);
    internal static readonly Color SurfaceWindow = new(0.045f, 0.043f, 0.04f, 0.99f);
    internal static readonly Color SurfacePanel = new(0.075f, 0.073f, 0.068f, 0.98f);
    internal static readonly Color SurfaceDeep = new(0.055f, 0.052f, 0.048f, 0.99f);
    internal static readonly Color SurfaceScroll = new(0.025f, 0.025f, 0.024f, 0.94f);
    internal static readonly Color AccentGold = new(0.94f, 0.78f, 0.35f, 1f);
    internal static readonly Color AccentBlue = new(0.57f, 0.76f, 0.88f, 1f);
    internal static readonly Color AccentJade = new(0.49f, 0.77f, 0.68f, 1f);
    internal static readonly Color TextPrimary = new(0.91f, 0.92f, 0.90f, 1f);
    internal static readonly Color TextMuted = new(0.61f, 0.63f, 0.61f, 1f);
    internal static readonly Color Danger = new(0.58f, 0.20f, 0.16f, 0.95f);
}
