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

    // 玄黄修士榜使用低饱和灰蓝卷面，减少大面积高亮蓝带来的刺眼感。
    internal static readonly Color RankFrame = new(0.105f, 0.15f, 0.19f, 0.99f);
    internal static readonly Color RankFrameEdge = new(0.37f, 0.50f, 0.58f, 0.96f);
    internal static readonly Color RankSurfaceWindow = new(0.065f, 0.09f, 0.115f, 0.99f);
    internal static readonly Color RankSurfacePanel = new(0.085f, 0.12f, 0.145f, 0.98f);
    internal static readonly Color RankSurfaceDeep = new(0.05f, 0.07f, 0.09f, 0.99f);
    internal static readonly Color RankSurfaceScroll = new(0.035f, 0.055f, 0.075f, 0.96f);
    internal static readonly Color RankAccentPrimary = new(0.46f, 0.63f, 0.71f, 1f);
    internal static readonly Color RankAccentSecondary = new(0.62f, 0.72f, 0.76f, 1f);
    internal static readonly Color RankTextPrimary = new(0.85f, 0.88f, 0.88f, 1f);
    internal static readonly Color RankTextEmphasis = new(0.72f, 0.86f, 0.86f, 1f);
    internal static readonly Color RankTextMuted = new(0.56f, 0.62f, 0.64f, 1f);
    internal static readonly Color RankButton = new(0.15f, 0.22f, 0.26f, 0.98f);
    internal static readonly Color RankButtonActive = new(0.25f, 0.36f, 0.41f, 1f);
    internal static readonly Color RankHighlight = new(0.34f, 0.53f, 0.57f, 0.92f);
    internal static readonly Color RankMedalFirst = new(0.72f, 0.82f, 0.74f, 1f);
    internal static readonly Color RankMedalSecond = new(0.63f, 0.74f, 0.76f, 1f);
    internal static readonly Color RankMedalThird = new(0.60f, 0.68f, 0.66f, 1f);
    internal static readonly Color RankDanger = new(0.39f, 0.24f, 0.25f, 0.96f);
}
